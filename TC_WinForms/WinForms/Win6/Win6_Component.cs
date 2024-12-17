using Serilog;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcDbConnector;
using TcModels.Models.IntermediateTables;
using static TC_WinForms.WinForms.Win6_Component;
using Component = TcModels.Models.TcContent.Component;

namespace TC_WinForms.WinForms
{
	[DesignerCategory("Form")]
#if DEBUG
	public partial class Win6_Component : Win6_Component_Design
#else
    public partial class Win6_Component : BaseContentFormWithFormula<DisplayedComponent_TC, Component_TC>
#endif
	{
		protected override DataGridView DgvMain => dgvMain;
        protected override Panel PnlControls => pnlControls;
		protected override IList<Component_TC> TargetTable 
            => _tcViewState.TechnologicalCard.Component_TCs;


		private MyDbContext context;

        private int _tcId;
		public bool CloseFormsNoSave { get; set; } = false;

        public Win6_Component(int tcId, TcViewState tcViewState, MyDbContext context)// bool viewerMode = false)
        {
			if (DesignMode)
				return;

			_logger = Log.Logger
                .ForContext<Win6_Component>()
                .ForContext("TcId", _tcId);

            _logger.Information("Инициализация формы Win6_Component TcId={TcId}");

            _tcViewState = tcViewState;
            this.context = context;

            InitializeComponent();

			_tcId = tcId;

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

		protected override void LoadObjects() // todo: можно перенести в BaseForm
		{
            var tcList = TargetTable //_tcViewState.TechnologicalCard.Component_TCs
                .Where(obj => obj.ParentId == _tcId)
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
                var newObj_TC = CreateNewObject(obj, _bindingList.Select(o => o.Order).Max() + 1);
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
			//_tcViewState.TechnologicalCard.Component_TCs
   //             .AddRange(newObjects);

            _changedObjects.Clear();

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
            var displayedComponent = selectedRow.DataBoundItem as DisplayedComponent_TC;

            if (displayedComponent != null)
            {

                if (displayedComponent.ChildId == updatedObject.Id)
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
                var newItem = CreateNewObject(updatedObject, displayedComponent.Order);
                newItem.Quantity = displayedComponent.Quantity ?? 0;
                newItem.Note = displayedComponent.Note;

                var newDisplayedComponent = new DisplayedComponent_TC(newItem);


                // замена displayedComponent в dgvMain на newDisplayedComponent
                var index = _bindingList.IndexOf(displayedComponent);
                _bindingList[index] = newDisplayedComponent;

                // проверяем наличие объекта в списке измененных объектов в значениях replacedObjects
                if (_replacedObjects.ContainsKey(displayedComponent))
                {
                    _replacedObjects[displayedComponent] = newDisplayedComponent;
                }
                else
                {
                    _replacedObjects.Add(displayedComponent, newDisplayedComponent);
                }

                SaveReplacedObjects();

                return true;
            }

            return false;
        }
		public class DisplayedComponent_TC : BaseDisplayedEntity
		{
			public override Dictionary<string, string> GetPropertiesNames()
			{
				var baseDict = base.GetPropertiesNames();

				baseDict.Add(nameof(Category), "Категория");
				baseDict.Add(nameof(TotalPrice), "Стоимость, руб. без НДС");

				return baseDict;
			}
			public override List<string> GetPropertiesOrder()
			{
				var baseList = base.GetPropertiesOrder();

				baseList.Insert(5, nameof(TotalPrice));

				return baseList;
			}

			public DisplayedComponent_TC()
			{

			}
			public DisplayedComponent_TC(Component_TC obj)
			{
				ChildId = obj.ChildId;
				ParentId = obj.ParentId;
				Order = obj.Order;

				Name = obj.Child.Name;
				Type = obj.Child.Type;

				Unit = obj.Child.Unit;
				Quantity = obj.Quantity;
				Formula = obj.Formula;
				Price = obj.Child.Price ?? 0;
				Description = obj.Child.Description;
				Manufacturer = obj.Child.Manufacturer;
				Category = obj.Child.Categoty;
				ClassifierCode = obj.Child.ClassifierCode;
				Note = obj.Note;
				IsReleased = obj.Child.IsReleased;

				//previousOrder = Order; // устанавливается вместе с Order
			}

			public double TotalPrice => (int)(Price * Quantity);
			public string Category { get; set; } = "StandComp";

		}
	}
}
