using Serilog;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
	// при работе с дизайнером раскоментировать
	//[DesignerCategory("Form")]
	//public partial class Win6_Machine : Win6_Machine_Design

	public partial class Win6_Machine : BaseContentForm<DisplayedMachine_TC, Machine_TC>
	{
		protected override DataGridView DgvMain => dgvMain;
		protected override Panel PnlControls => pnlControls;
		protected override IList<Machine_TC> TargetTable
			=> _tcViewState.TechnologicalCard.Machine_TCs;

        private int _tcId;
        private MyDbContext context;

        public Win6_Machine(int tcId, TcViewState tcViewState, MyDbContext context)// bool viewerMode = false)
		{
			if (DesignMode)
				return;

			_logger = Log.Logger
				.ForContext<Win6_Machine>()
				.ForContext("TcId", _tcId);

			_tcViewState = tcViewState;
            this.context = context;

            _tcId = tcId;

            InitializeComponent();

            var dgvEventService = new DGVEvents(dgvMain);
            dgvEventService.SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);

            dgvMain.CellFormatting += dgvEventService.dgvMain_CellFormatting;
            dgvMain.CellValidating += dgvEventService.dgvMain_CellValidating;
			dgvMain.CellValueChanged += dgvMain_CellValueChanged;

			this.FormClosed += (sender, e) => {
				_logger.Information("Форма закрыта");
				this.Dispose();
			};
		}
		protected override void LoadObjects()
		{
			var tcList = TargetTable
				.OrderBy(o => o.Order)
                .Select(obj => new DisplayedMachine_TC(obj)).ToList();

			_bindingList = new BindingList<DisplayedMachine_TC>(tcList);
			_bindingList.ListChanged += BindingList_ListChanged;
			dgvMain.DataSource = _bindingList;

			InitializeDataGridViewColumns();
		}

        public void AddNewObjects(List<Machine> newObjs)
        {
            foreach (var obj in newObjs)
			{
				var newObj_TC = CreateNewObject(obj, GetNewObjectOrder());
				var machine = context.Machines.Where(s => s.Id == newObj_TC.ChildId).First();

				context.Machines.Attach(machine);
				TargetTable.Add(newObj_TC);

				newObj_TC.Child = machine;
				newObj_TC.ChildId = machine.Id;

				var displayedObj_TC = new DisplayedMachine_TC(newObj_TC);
				_bindingList.Add(displayedObj_TC);

			}
			dgvMain.Refresh();
        }



		////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////


		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		protected override void SaveReplacedObjects() // add to UpdateMode
        {
            if (_replacedObjects.Count == 0)
                return;

            var oldObjects = _replacedObjects.Select(dtc => CreateNewObject(dtc.Key)).ToList();
            var newObjects = _replacedObjects.Select(dtc => CreateNewObject(dtc.Value)).ToList();

            var obj_TCsIds = oldObjects.Select(t => t.ChildId).ToList();

            List<ExecutionWork> executionWorks = new List<ExecutionWork>();

            //Получаем список ExecutionWorks где будет происходить замена Machine
            foreach (var techOperationWork in _tcViewState.TechOperationWorksList)
            {
                var executionWorksToReplace = techOperationWork.executionWorks
                                                               .Where(ew => ew.techOperationWork.TechnologicalCardId == _tcId
                                                                         && ew.Machines.Any(m => obj_TCsIds.Contains(m.ChildId)))
                                                                .ToList();

                if(executionWorksToReplace != null && executionWorksToReplace.Count != 0)
                    executionWorks.AddRange(executionWorksToReplace);
            }

            //Присваиваем Machine новым объектам и удаляем старые объекты Machine_TCs под замену
            for (int i = 0; i < oldObjects.Count; i++)
            {
                var mach = context.Machines.Where(m => m.Id == newObjects[i].ChildId).First();
                newObjects[i].Child = mach;

                var oldMach = TargetTable.Where(m => m.ChildId == oldObjects[i].ChildId).FirstOrDefault();
               
                if (oldMach != null)
                {
                    context.Machine_TCs.Attach(oldMach);
                    TargetTable.Remove(oldMach);
                }
            }

			foreach (var newObj in newObjects)
			{
				TargetTable
					.Add(newObj);
			}
			//TargetTable.AddRange(newObjects);

            foreach (var newTc in newObjects)
            {
                executionWorks.ForEach(ew => ew.Machines.Add(newTc));
            }

            _replacedObjects.Clear();
        }
		protected override Machine_TC CreateNewObject(BaseDisplayedEntity dObj)
        {
            return new Machine_TC
            {
                ParentId = dObj.ParentId,
                ChildId = dObj.ChildId,
                Order = dObj.Order,
                Quantity = dObj.Quantity ?? 0,
                Note = dObj.Note,
                Formula = dObj.Formula,
			};
        }
        private Machine_TC CreateNewObject(Machine obj, int oreder)
        {
            return new Machine_TC // todo: создать конструктор с параметрами от родительского объекта (для базового класса)
			{
                ParentId = _tcId,
                ChildId = obj.Id,
                Child = obj,
                Order = oreder,
                Quantity = 0,
                Note = "",
            };
        }

        ///////////////////////////////////////////////////// * Events handlers * /////////////////////////////////////////////////////////////////////////////////
        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            var newForm = new Win7_5_Machine(activateNewItemCreate: true, createdTCId: _tcId);
            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();
        }

        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
                _bindingList, _newObjects, _deletedObjects);

            if (_deletedObjects.Count != 0)
            {
                var deletedObjects = _deletedObjects.Select(dtc => CreateNewObject(dtc)).ToList();

                foreach (var obj in deletedObjects)
                {
                    var deletedObj = TargetTable.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    TargetTable.Remove(deletedObj);
                }

                _deletedObjects.Clear();

            }

        }
        private void btnReplace_Click(object sender, EventArgs e)
        {
            // Выделение объекта выбранной строки
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return;
            }

            // load new form Win7_3_Component as dictonary
            var newForm = new Win7_5_Machine(activateNewItemCreate: true, createdTCId: _tcId, isUpdateMode: true);

            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();

        }// add to UpdateMode
        public bool UpdateSelectedObject(Machine updatedObject)
        {
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return false;
            }

            var selectedRow = dgvMain.SelectedRows[0];
            //var displayedComponent = selectedRow.DataBoundItem as DisplayedTool_TC;

            if (selectedRow.DataBoundItem is DisplayedMachine_TC dObj)
            {

                if (dObj.ChildId == updatedObject.Id)
                {
                    MessageBox.Show("Ошибка обновления объекта: ID объекта совпадает");
                    return false;
                }
                // проверка на наличие объекта в списке существующих объектов
                if (_bindingList.Any(obj => obj.ChildId == updatedObject.Id))
                {
                    MessageBox.Show("Ошибка обновления объекта: объект с таким ID уже существует");
                    return false;
                }
                var newItem = CreateNewObject(updatedObject, dObj.Order);
                newItem.Quantity = dObj.Quantity ?? 0;
                newItem.Note = dObj.Note;

                var newDisplayedComponent = new DisplayedMachine_TC(newItem);


                // замена displayedComponent в dgvMain на newDisplayedComponent
                var index = _bindingList.IndexOf(dObj);
                _bindingList[index] = newDisplayedComponent;

                // проверяем наличие объекта в списке измененных объектов в значениях replacedObjects
                if (_replacedObjects.ContainsKey(dObj))
                {
                    _replacedObjects[dObj] = newDisplayedComponent;
                }
                else
                {
                    _replacedObjects.Add(dObj, newDisplayedComponent);
                }

                SaveReplacedObjects();

                return true;
            }

            return false;
        }// add to UpdateMode
    }

}
