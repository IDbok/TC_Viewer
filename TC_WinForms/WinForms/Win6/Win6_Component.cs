using Serilog;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcDbConnector;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using static TC_WinForms.DataProcessing.DGVProcessing;
using static TC_WinForms.WinForms.Win6_Component;
using Component = TcModels.Models.TcContent.Component;

namespace TC_WinForms.WinForms
{
	public partial class Win6_Component : BaseContentFormWithFormula<DisplayedComponent_TC>, IViewModeable
    {
		//private readonly ILogger _logger;
		//private readonly TcViewState _tcViewState;
		protected override DataGridView DgvMain => dgvMain;


        //private bool _isViewMode;

        private MyDbContext context;

        private int _tcId;

        //private BindingList<DisplayedComponent_TC> _bindingList;
        //private List<DisplayedComponent_TC> _changedObjects = new ();
        //private List<DisplayedComponent_TC> _newObjects = new ();
        //private List<DisplayedComponent_TC> _deletedObjects = new ();

        private Dictionary<DisplayedComponent_TC, DisplayedComponent_TC> _replacedObjects = new (); // add to UpdateMode // todo: можно перенести в BaseForm

		public bool CloseFormsNoSave { get; set; } = false;

        public Win6_Component(int tcId, TcViewState tcViewState, MyDbContext context)// bool viewerMode = false)
        {

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

        public void SetViewMode(bool? isViewMode = null)// todo: можно перенести в BaseForm
        {

            pnlControls.Visible = !_tcViewState.IsViewMode;

            // make columns editable
            dgvMain.Columns[nameof(DisplayedComponent_TC.Order)].ReadOnly = _tcViewState.IsViewMode;
			dgvMain.Columns[nameof(DisplayedComponent_TC.Formula)].ReadOnly = _tcViewState.IsViewMode;
			dgvMain.Columns[nameof(DisplayedComponent_TC.Quantity)].ReadOnly = _tcViewState.IsViewMode;
            dgvMain.Columns[nameof(DisplayedComponent_TC.Note)].ReadOnly = _tcViewState.IsViewMode;


            dgvMain.Columns[nameof(DisplayedComponent_TC.Order)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
			dgvMain.Columns[nameof(DisplayedComponent_TC.Formula)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
			dgvMain.Columns[nameof(DisplayedComponent_TC.Quantity)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
            dgvMain.Columns[nameof(DisplayedComponent_TC.Note)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;

            // update form
            dgvMain.Refresh();

        }

		private void Win6_Component_Load(object sender, EventArgs e) // todo: можно перенести в BaseForm
		{
            _logger.Information("Загрузка формы Win6_Component");

            LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedComponent_TC>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            SetViewMode();

			RecalculateQuantities();
		}
		protected override void LoadObjects() // todo: можно перенести в BaseForm
		{
            var tcList = _tcViewState.TechnologicalCard.Component_TCs
                .Where(obj => obj.ParentId == _tcId)
                .OrderBy(o => o.Order)
                .Select(obj => new DisplayedComponent_TC(obj))
                .ToList();

            _bindingList = new BindingList<DisplayedComponent_TC>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            //SetDGVColumnsSettings();
            InitializeDataGridViewColumns();

		}
        
        public void AddNewObjects(List<Component> newObjs)
        {
            foreach (var obj in newObjs)
            {
                var newObj_TC = CreateNewObject(obj, _bindingList.Select(o => o.Order).Max() + 1);
                var component = context.Components.Where(s => s.Id == newObj_TC.ChildId).First();

                context.Components.Attach(component);
                _tcViewState.TechnologicalCard.Component_TCs.Add(newObj_TC);

                newObj_TC.Child = component;
                newObj_TC.ChildId = component.Id;

                var displayedObj_TC = new DisplayedComponent_TC(newObj_TC);
                _bindingList.Add(displayedObj_TC);
            }


            dgvMain.Refresh();
        }

        ////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////
        void SetDGVColumnsSettings() // todo: можно перенести в BaseForm. Хотябы частично
		{

            // автоподбор ширины столбцов под ширину таблицы
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.RowHeadersWidth = 25;

            //// автоперенос в ячейках
            dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            int pixels = 35;

            // Минимальные ширины столбцов
            Dictionary<string, int> fixColumnWidths = new Dictionary<string, int>
            {
                { nameof(DisplayedComponent_TC.Order), 1*pixels },
                { nameof(DisplayedComponent_TC.Type), 4*pixels },
                { nameof(DisplayedComponent_TC.Unit), 2*pixels },
                { nameof(DisplayedComponent_TC.TotalPrice), 3*pixels },
				{ nameof(DisplayedComponent_TC.Formula), 3*pixels },
				{ nameof(DisplayedComponent_TC.Quantity), 3*pixels },
                { nameof(DisplayedComponent_TC.ChildId), 2*pixels },

            };
            foreach (var column in fixColumnWidths)
            {
                dgvMain.Columns[column.Key].Width = column.Value;
                dgvMain.Columns[column.Key].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvMain.Columns[column.Key].Resizable = DataGridViewTriState.False;
            }

            dgvMain.Columns[nameof(DisplayedComponent_TC.Type)].Resizable = DataGridViewTriState.True;

            // make columns readonly
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                column.ReadOnly = true;
            }
            var changeableColumn = new List<string>
            {
                nameof(DisplayedComponent_TC.Order),
                //nameof(DisplayedComponent_TC.Quantity),
                nameof(DisplayedComponent_TC.Formula),
                nameof(DisplayedComponent_TC.Note),
            };
            foreach (var column in changeableColumn)
            {
                dgvMain.Columns[column].ReadOnly = false;
                dgvMain.Columns[column].DefaultCellStyle.BackColor = Color.LightGray;
            }
        }

        private void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) // todo: можно перенести в BaseForm
		{
            // Проверяем, что это не заголовок столбца и не новая строка
            if (e.RowIndex >= 0 && e.RowIndex < dgvMain.Rows.Count)
            {
                var row = dgvMain.Rows[e.RowIndex];
                var displayedObject = row.DataBoundItem as DisplayedComponent_TC;
                if (displayedObject != null)
                {
                    // Меняем цвет строки в зависимости от значения свойства IsReleased
                    if (!displayedObject.IsReleased)
                    {
                        row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#d1c6c2"); // Цвет для строк, где IsReleased = false
                    }
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
       

        private void SaveReplacedObjects() // add to UpdateMode
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
                var oldComponent = _tcViewState.TechnologicalCard.Component_TCs.Where(m => m.ChildId == oldObjects[i].ChildId).First();
                _tcViewState.TechnologicalCard.Component_TCs.Remove(oldComponent);
            }

            _tcViewState.TechnologicalCard.Component_TCs.AddRange(newObjects);

            _changedObjects.Clear();

        }

        private Component_TC CreateNewObject(DisplayedComponent_TC dObj)
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

                    var deletedObj = _tcViewState.TechnologicalCard.Component_TCs.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    if (deletedObj != null)
                        _tcViewState.TechnologicalCard.Component_TCs.Remove(deletedObj);
                }

                _deletedObjects.Clear();
            }
        }

        private void dgvMain_CellEndEdit(object sender, DataGridViewCellEventArgs e) 
            // todo: можно перенести в BaseForm
			// todo - fix problem with selection replacing row (error while remove it)
		{
            ReorderRows(dgvMain, e, _bindingList);
        }

        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            DisplayedEntityHelper.ListChangedEventHandlerIntermediate
                (e, _bindingList, _newObjects, _changedObjects, _deletedObjects);

            if (_changedObjects.Count != 0)
            {
                foreach (var obj in _changedObjects)
                {
                    var changedObject = _tcViewState.TechnologicalCard.Component_TCs.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    if (changedObject != null)
                        changedObject.ApplyUpdates(CreateNewObject(obj));
                }
                _changedObjects.Clear();

            }
        }
		protected override void InitializeDataGridViewColumns()
		{
			SetDGVColumnsSettings();
		}

		public class DisplayedComponent_TC : INotifyPropertyChanged, IIntermediateDisplayedEntity, IOrderable, IPreviousOrderable, IReleasable, IFormulaItem
		{
            public Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
                {
                    { nameof(ChildId), "ID" },
                    { nameof(ParentId), "ID тех. карты" },
                    { nameof(Order), "№" },
                    { nameof(Quantity), "Кол-во" },
				    { nameof(Formula), "Формула" },
				    { nameof(Note), "Примечание" },

                    { nameof(Name), "Наименование" },
                    { nameof(Type), "Тип (исполнение)" },
                    { nameof(Unit), "Ед.изм." },
                    { nameof(Price), "Стоимость за ед., руб. без НДС" },
                    { nameof(TotalPrice), "Стоимость, руб. без НДС" },
                    { nameof(Description), "Описание" },
                    { nameof(Manufacturer), "Производители (поставщики)" },
                    { nameof(Categoty), "Категория" },
                    { nameof(ClassifierCode), "Код в classifier" },
                };
            }
            public List<string> GetPropertiesOrder()
            {
                return new List<string>
                {
                    nameof(Order),

                    nameof(Name),
                    nameof(Type),
                    nameof(Unit),
					nameof(Formula),
					nameof(Quantity),
					nameof(TotalPrice),
                    nameof(Note),
                    nameof(ChildId),

                };
            }
            public List<string> GetRequiredFields()
            {
                return new List<string>
                {
                    nameof(ChildId) ,
                    nameof(ParentId) ,
                    nameof(Order),
                };
            }
            public List<string> GetKeyFields()
            {
                return new List<string>
                {
                    nameof(ChildId),
                    nameof(ParentId),
                };
            }

            private int childId;
            private int parentId;
            private int order;
            private double quantity;
            private string? note;

            private string? formula;

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
                Categoty = obj.Child.Categoty;
                ClassifierCode = obj.Child.ClassifierCode;
                Note = obj.Note;
                IsReleased = obj.Child.IsReleased;

                previousOrder = Order;
            }

            public int ChildId { get; set; }
            public int ParentId { get; set; }
            private int previousOrder;
            public int Order
            {
                get => order;
                set
                {
                    if (order != value)
                    {
                        previousOrder = order;
                        order = value;
                        OnPropertyChanged(nameof(Order));
                    }
                }
            }

            public int PreviousOrder => previousOrder;
            public double? Quantity
            {
                get => quantity;
                set
                {
                    if (quantity != value)
                    {
                        quantity = value ?? 0;
                        OnPropertyChanged(nameof(Quantity));
                    }
                }
            }

			public string? Formula
			{
				get => formula;
				set
				{
					if (formula != value)
					{
						formula = value;
						OnPropertyChanged(nameof(Formula));
					}
					// todo:  пересчёт количества по формуле
				}
			}

			public string? Note
            {
                get => note;
                set
                {
                    if (note != value)
                    {
                        note = value;
                        OnPropertyChanged(nameof(Note));
                    }
                }
            }

            public string Name { get; set; }
            public string? Type { get; set; }
            public string Unit { get; set; }
            public float Price { get; set; } = 0;

            public double TotalPrice => (int)(Price * Quantity);
            public string? Description { get; set; }
            public string? Manufacturer { get; set; }
            //public List<LinkEntety> Links { get; set; } = new();
            public string Categoty { get; set; } = "StandComp";
            public string ClassifierCode { get; set; }

            public bool IsReleased { get; set; } = false;


            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private Dictionary<string, object> oldValueDict = new Dictionary<string, object>();

            public object GetOldValue(string propertyName)
            {
                if (oldValueDict.ContainsKey(propertyName))
                {
                    return oldValueDict[propertyName];
                }

                return null;
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

		private void dgvMain_CellValueChanged(object sender, DataGridViewCellEventArgs e) // todo: можно перенести в BaseForm
		{
			if (e.RowIndex >= 0 && dgvMain.Columns[e.ColumnIndex].Name == nameof(DisplayedComponent_TC.Formula))
			{
				var row = dgvMain.Rows[e.RowIndex];
				if (row.DataBoundItem is DisplayedComponent_TC displayedComponent)
				{
					RecalculateQuantityForObject(displayedComponent);
				}
			}
		}

		//private void RecalculateQuantityForComponent(DisplayedComponent_TC displayedComponent)
		//{
		//	if (displayedComponent == null || string.IsNullOrEmpty(displayedComponent.Formula))
		//		return;

		//	try
		//	{
		//		var coefDict = GetCoefficientDictionary();
		//		displayedComponent.Quantity = MathScript.EvaluateCoefficientExpression(displayedComponent.Formula, coefDict);
		//		dgvMain.Refresh();
		//	}
		//	catch (Exception ex)
		//	{
		//		LogAndShowError(
		//			$"Ошибка вычисления формулы: {ex.Message}",
		//			$"Ошибка вычисления формулы для компонента '{displayedComponent.Name} - {displayedComponent.Type}'. Проверьте формулу.");
		//	}
		//}

		//private Dictionary<string, double> GetCoefficientDictionary()
		//{
		//	return _tcViewState.TechnologicalCard.Coefficients.ToDictionary(c => c.Code, c => c.Value);
		//}

		///// <summary>
		///// Пересчёт значения Quantity для всех объектов с заданной формулой.
		///// </summary>
		//private void RecalculateQuantities()
		//{
  //          if (_bindingList == null || _bindingList.Count == 0)
  //              return;

  //          try
  //          {
  //              ApplyFormulasToQuantities();

		//		dgvMain.Refresh(); // Обновляем отображение данных в DataGridView
  //          }
  //          catch (Exception ex)
  //          {
		//		LogAndShowError($"Ошибка при пересчёте значений Quantity: {ex.Message}",
		//			"Ошибка пересчёта значений. Проверьте корректность данных.",
		//			ex.InnerException?.Message);
  //          }
  //      }

		///// <summary>
		///// Асинхронный пересчёт значения Quantity для всех объектов с заданной формулой.
		///// </summary>
		//private async Task RecalculateQuantitiesAsync()
		//{
		//	if (_bindingList == null || _bindingList.Count == 0)
		//		return;

		//	try
		//	{
		//		// Выполняем пересчёт в отдельной задаче
		//		await Task.Run(() =>
		//		{
		//			ApplyFormulasToQuantities();
		//		});

  //              // Обновляем интерфейс в основном потоке
  //              Invoke(new Action(() => dgvMain.Refresh()));

  //          }
		//	catch (Exception ex)
		//	{
  //              LogAndShowError($"Ошибка при пересчёте значений Quantity: {ex.Message}",
		//			"Ошибка пересчёта значений. Проверьте корректность данных.",
		//			ex.InnerException?.Message);
		//	}
		//}

		//private void ApplyFormulasToQuantities()
		//{
		//	var coefDict = GetCoefficientDictionary();

		//	foreach (var displayedComponent in _bindingList)
		//	{
		//		if (!string.IsNullOrWhiteSpace(displayedComponent.Formula))
		//		{
		//			try
		//			{
		//				// Пересчёт значения Quantity на основе формулы и коэффициентов
		//				displayedComponent.Quantity = MathScript.EvaluateCoefficientExpression(displayedComponent.Formula, coefDict);
		//			}
		//			catch (Exception ex)
		//			{
		//				_logger.Error($"Ошибка вычисления формулы для компонента ID={displayedComponent.ChildId}: {ex.Message}. Формула: {displayedComponent.Formula}");
		//				throw new InvalidOperationException($"Ошибка вычисления формулы для компонента '{displayedComponent.Name} - {displayedComponent.Type}'.", ex);
		//			}
		//		}
		//	}
		//}

  //      private void LogAndShowError(string logMessage, string userMessage, string? innerError = null)
  //      {
  //          _logger.Error(logMessage);

  //          if (!string.IsNullOrWhiteSpace(innerError))
  //              userMessage += "\n\n" + innerError;

		//	MessageBox.Show(userMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
		//}
	}


}
