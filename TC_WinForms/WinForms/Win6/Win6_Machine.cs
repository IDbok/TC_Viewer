using Serilog;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
	// при работе с дизайнером раскоментировать
	//[DesignerCategory("Form")]
	//public partial class Win6_Machine : Win6_Machine_Design

	public partial class Win6_Machine : BaseContentForm<DisplayedMachine_TC, Machine_TC>, IFormWithObjectId
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

			_tcViewState = tcViewState;
			this.context = context;

			_tcId = tcId;

			_logger = Log.Logger
				.ForContext<Win6_Machine>()
				.ForContext("TcId", _tcId);

            _logger.Information("Инициализация формы");

			InitializeComponent();

            InitializeDataGridViewEvents();

			this.FormClosed += (sender, e) => {
				_logger.Information("Форма закрыта");
				this.Dispose();
			};
            dgvMain.CellContentClick += DgvMain_CellContentClick;
		}

        private void DgvMain_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                dgvMain.CommitEdit(DataGridViewDataErrorContexts.Commit);

                var chech = (bool)dgvMain.Rows[e.RowIndex].Cells[0].Value;

                var machineId = (int)dgvMain.Rows[e.RowIndex].Cells[1].Value;
                var updatedMachine = _tcViewState.TechnologicalCard.Machine_TCs.Where(m => m.ChildId == machineId).FirstOrDefault();

                updatedMachine.OutlayCount = chech;
            }
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
            List<Machine> existedMachine = new List<Machine>();

            foreach (var obj in newObjs)
			{
                if (_bindingList.Select(c => c.ChildId).Contains(obj.Id))
                {
                    existedMachine.Add(obj);
                    continue;
                }

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

            if (existedMachine.Count > 0)
            {
                string elements = "";
                foreach (var machine in existedMachine)
                {
                    elements += "Механизм: " + machine.Name + " ID: " + machine.Id + ".\n";
                }

                MessageBox.Show("Часть объектов уже присутствовала в требованиях. Они не были внесены: \n" + elements, "Дублирование элементов", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }



        ////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////
        protected override List<string> GetEditableColumns()
        {
            var baseList = base.GetEditableColumns();

            baseList.Add(nameof(Machine_TC.OutlayCount));
            return baseList;
        }

        public virtual void SetViewMode(bool? isViewMode = null)// todo: можно перенести в BaseForm
        {
            PnlControls.Visible = !_tcViewState.IsViewMode;

            DgvMain.Columns[nameof(BaseDisplayedEntity.ChildId)].Visible = !_tcViewState.IsViewMode;

            // make columns editable
            //DgvMain.Columns[nameof(BaseDisplayedEntity.Formula)].ReadOnly = _tcViewState.IsViewMode;
            DgvMain.Columns[nameof(BaseDisplayedEntity.Note)].ReadOnly = _tcViewState.IsViewMode;
            DgvMain.Columns[nameof(Machine_TC.OutlayCount)].ReadOnly = _tcViewState.IsViewMode;


            DgvMain.Columns[nameof(BaseDisplayedEntity.Formula)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
            DgvMain.Columns[nameof(BaseDisplayedEntity.Quantity)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
            DgvMain.Columns[nameof(BaseDisplayedEntity.Note)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
            DgvMain.Columns[nameof(Machine_TC.OutlayCount)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;

            UpdateDynamicCardParametrs();

            // update form
            RefreshDataGridView();
        }

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
			LogUserAction("Добавление нового объекта");

			var newForm = new Win7_5_Machine(activateNewItemCreate: true, createdTCId: _tcId);
            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();
        }

        private void btnDeleteObj_Click(object sender, EventArgs e)
		{
			LogUserAction("Удаление выбранных объектов");

			DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
                _bindingList, _newObjects, _deletedObjects);

            if (_deletedObjects.Count != 0)
            {
                var deletedObjects = _deletedObjects.Select(dtc => CreateNewObject(dtc)).ToList();

                foreach (var obj in deletedObjects)
                {
					var deletedObj = TargetTable.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();

					if (deletedObj != null)
                    {
						_logger.Debug("Удаление объектов({ObjectType}}: {ComponentName} (ID={Id})",
							deletedObj.GetType(), deletedObj.Child.Name, deletedObj.ChildId);

						TargetTable.Remove(deletedObj);
					}
						
                }

				_logger.Information("Удалено объектов: {Count}", _deletedObjects.Count);
				_deletedObjects.Clear();
            }

        }
        private void btnReplace_Click(object sender, EventArgs e)
		{
			LogUserAction("Замена выбранных объектов");
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

        public int GetObjectId()
        {
            return _tcId;
        }
    }

}
