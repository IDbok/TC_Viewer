using Serilog;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.IntermediateTables;
using Component = TcModels.Models.TcContent.Component;


namespace TC_WinForms.WinForms
{
	// при работе с дизайнером раскоментировать
	//[DesignerCategory("Form")]
	//public partial class Win6_Component : Win6_Component_Design
	public partial class Win6_Component : BaseContentForm<DisplayedComponent_TC, Component_TC>, IFormWithObjectId
	{
		protected override DataGridView DgvMain => dgvMain;
        protected override Panel PnlControls => pnlControls;
		protected override IList<Component_TC> TargetTable 
            => _tcViewState.TechnologicalCard.Component_TCs;


		private MyDbContext context;

        private int _tcId;

        public Win6_Component(int tcId, TcViewState tcViewState, MyDbContext context)// bool viewerMode = false)
        {
			if (DesignMode)
				return;

			_logger = Log.Logger
                .ForContext<Win6_Component>()
                .ForContext("TcId", _tcId);

            _logger.Information("Инициализация формы. TcId={TcId}");

            _tcViewState = tcViewState;
            this.context = context;
			_tcId = tcId;

			InitializeComponent();

			InitializeDataGridViewEvents();

			this.FormClosed += (sender, e) => {
                _logger.Information("Форма закрыта");
                this.Dispose();
            };
        }


		protected override void LoadObjects() // todo: можно перенести в BaseForm
		{
            var tcList = TargetTable //_tcViewState.TechnologicalCard.Component_TCs
                //.Where(obj => obj.ParentId == _tcId)
                .OrderBy(o => o.Order)
                .Select(obj => new DisplayedComponent_TC(obj))
                .ToList();

            _bindingList = new BindingList<DisplayedComponent_TC>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            InitializeDataGridViewColumns();
		}

		public void AddNewObjects(List<Component> newObjs)
        {
            foreach (var obj in newObjs)
            {
                var newObj_TC = CreateNewObject(obj, GetNewObjectOrder());
                var component = context.Components.Where(s => s.Id == newObj_TC.ChildId).First();

                context.Components.Attach(component);
				TargetTable //_tcViewState.TechnologicalCard.Component_TCs
                    .Add(newObj_TC);

                newObj_TC.Child = component;
                newObj_TC.ChildId = component.Id;

                var displayedObj_TC = new DisplayedComponent_TC(newObj_TC);
                _bindingList.Add(displayedObj_TC);
            }

            dgvMain.Refresh();
        }

		////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////
		protected override Dictionary<string, int> GetFixedColumnWidths(int pixels)
		{
			return new Dictionary<string, int>
	        {
		        { nameof(BaseDisplayedEntity.Order), 1 * pixels },
		        { nameof(BaseDisplayedEntity.Type), 4 * pixels },
		        { nameof(BaseDisplayedEntity.Unit), 2 * pixels },
                { nameof(DisplayedComponent_TC.TotalPrice), 3*pixels },
                { nameof(BaseDisplayedEntity.Formula), 3 * pixels },
		        { nameof(BaseDisplayedEntity.Quantity), 3 * pixels },
		        { nameof(BaseDisplayedEntity.ChildId), 2 * pixels }
	        };
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		protected override void SaveReplacedObjects() // add to UpdateMode
        {
            if (_replacedObjects.Count == 0)
                return;

            var oldObjects = _replacedObjects.Select(dtc => CreateNewObject(dtc.Key)).ToList();
            var newObjects = _replacedObjects.Select(dtc => CreateNewObject(dtc.Value)).ToList();

            //Присваиваем компонент для обхода ошибки состояний
            foreach (var newObj in newObjects)
            {
                var component = context.Components.Where(m => m.Id == newObj.ChildId).First();
                newObj.Child = component;
            }

            //Заменяем объект в ComponentWorks в TechOperationWorksList, если он присутствует в списке компонентов тех операции
            foreach (var tecjOperationWork in _tcViewState.TechOperationWorksList)
            {
                for (int i = 0; i < oldObjects.Count; i++)
                {
                    var replaceableCompWork = tecjOperationWork.ComponentWorks.Where(m => m.componentId == oldObjects[i].ChildId).FirstOrDefault();
                    if (replaceableCompWork != null)
                    {
                        replaceableCompWork.component = newObjects[i].Child;
                        replaceableCompWork.componentId = newObjects[i].ChildId;
                    }
                }
            }

            //Удялем старые компоненты из ТехКарты
            for (int i = 0; i < oldObjects.Count; i++)
            {
                var oldComponent = TargetTable //_tcViewState.TechnologicalCard.Component_TCs
                    .Where(m => m.ChildId == oldObjects[i].ChildId).First();
				TargetTable //_tcViewState.TechnologicalCard.Component_TCs
                    .Remove(oldComponent);
            }

            foreach (var newObj in newObjects)
            {
                TargetTable 
                    .Add(newObj);
            }

			_replacedObjects.Clear();
        }

		protected override Component_TC CreateNewObject(BaseDisplayedEntity dObj)
        {
            return new Component_TC
            {
                ParentId = dObj.ParentId,
                ChildId = dObj.ChildId,
                Order = dObj.Order,
                Quantity = dObj.Quantity ?? 0,
                Note = dObj.Note,
                Formula = dObj.Formula,
			};
        }
        private Component_TC CreateNewObject(Component obj, int order)
        {
            return new Component_TC
            {
                ParentId = _tcId,
                ChildId = obj.Id,
                Child = obj,
                Order = order,
                Quantity = 0,
                Note = "",
			};
        }
        ///////////////////////////////////////////////////// * Events handlers * /////////////////////////////////////////////////////////////////////////////////
        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            // load new form Win7_3_Component as dictonary
            var newForm = new Win7_4_Component(activateNewItemCreate: true, createdTCId: _tcId);
            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();
        }
        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
                _bindingList, _newObjects, _deletedObjects);

            if(_deletedObjects.Count != 0)
            {
                foreach (var obj in _deletedObjects)
                {
                    foreach (var tecjOperationWork in _tcViewState.TechOperationWorksList)
                    {
                        var deletableCompWork = tecjOperationWork.ComponentWorks.Where(m => m.componentId == obj.ChildId).FirstOrDefault();
                        if (deletableCompWork != null)
                        {
                            tecjOperationWork.ComponentWorks.Remove(deletableCompWork);
                        }

                    }

                    var deletedObj = TargetTable //_tcViewState.TechnologicalCard.Component_TCs
                        .Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    if (deletedObj != null)
						TargetTable //_tcViewState.TechnologicalCard.Component_TCs
                            .Remove(deletedObj);
                }

                _deletedObjects.Clear();
            }
        }
        private void btnReplace_Click(object sender, EventArgs e) // add to UpdateMode
        {
            // Выделение объекта выбранной строки
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return;
            }

            // load new form Win7_3_Component as dictionary
            var newForm = new Win7_4_Component(activateNewItemCreate: true, createdTCId: _tcId, isUpdateMode: true);

            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();
        }
        public bool UpdateSelectedObject(Component updatedObject) // add to UpdateMode
        {
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return false;
            }

            var selectedRow = dgvMain.SelectedRows[0];
            //var displayedComponent = selectedRow.DataBoundItem as DisplayedComponent_TC;

            if (selectedRow.DataBoundItem is DisplayedComponent_TC dObj)
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

                var newDisplayedComponent = new DisplayedComponent_TC(newItem);


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
        }

		public int GetObjectId()
		{
			return _tcId;
		}
	}
}
