using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Extensions;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms;

public partial class Win6_Staff : Form, IViewModeable
{
	private ILogger _logger;
	private readonly TcViewState _tcViewState;

    private bool _isViewMode;

    private MyDbContext context;

    private BindingList<DisplayedStaff_TC> _bindingList;

    private List<DisplayedStaff_TC> _changedObjects = new List<DisplayedStaff_TC>();
    private List<DisplayedStaff_TC> _newObjects = new List<DisplayedStaff_TC>();
    private List<DisplayedStaff_TC> _deletedObjects = new List<DisplayedStaff_TC>();

    private int _tcId;
    public bool CloseFormsNoSave { get; set; } = false;
    public Win6_Staff(int tcId, TcViewState tcViewState, MyDbContext context) // bool viewerMode = false)
	{
		_tcViewState = tcViewState;
		this.context = context;
		this._tcId = tcId;

		_logger = Log.Logger
			.ForContext<Win6_Staff>()
			.ForContext("TcId", _tcId);
        _logger.Information("Инициализация формы.");


		InitializeComponent();

        AccessInitialization();


        var dgvEventService = new DGVEvents(dgvMain);
        dgvEventService.SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);

        dgvMain.CellFormatting += dgvEventService.dgvMain_CellFormatting;
        dgvMain.CellValidating += dgvEventService.dgvMain_CellValidating;
        dgvMain.CellContentClick += DgvMain_CellContentClick;

		this.FormClosed += (sender, e) =>
		{
			_logger.Information("Форма Win6_Staff закрыта.");
			this.Dispose();
		};

	}

    private void DgvMain_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if(e.ColumnIndex == 6 && e.RowIndex >= 0)
        {
            dgvMain.CommitEdit(DataGridViewDataErrorContexts.Commit);

            var chech = (bool)dgvMain.Rows[e.RowIndex].Cells[6].Value;

            var staffId = (int)dgvMain.Rows[e.RowIndex].Cells[0].Value;
            var updatedStaff = _tcViewState.TechnologicalCard.Staff_TCs.Where(s => s.IdAuto == staffId).FirstOrDefault();

            updatedStaff.OutlayCount = chech;
        }
    }

    private void AccessInitialization()
    {
    }

    public void SetViewMode(bool? isViewMode = null)
    {
        pnlControls.Visible = !_tcViewState.IsViewMode;

        // make columns editable
        dgvMain.Columns[nameof(DisplayedStaff_TC.Order)].ReadOnly = _tcViewState.IsViewMode;
        dgvMain.Columns[nameof(DisplayedStaff_TC.Symbol)].ReadOnly = _tcViewState.IsViewMode;
        dgvMain.Columns[nameof(DisplayedStaff_TC.OutlayCount)].ReadOnly = _tcViewState.IsViewMode;


        dgvMain.Columns[nameof(DisplayedStaff_TC.Order)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
        dgvMain.Columns[nameof(DisplayedStaff_TC.Symbol)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
        dgvMain.Columns[nameof(DisplayedStaff_TC.OutlayCount)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;

        // update form
        dgvMain.Refresh();

    }

    private void Win6_Staff_Load(object sender, EventArgs e)
	{
		_logger.Information("Загрузка формы.");

		LoadObjects();

        dgvMain.AllowUserToDeleteRows = false;

        SetViewMode();
    }
    private void LoadObjects()
    {
		_logger.Information("Загрузка объектов.");

		var tcList = _tcViewState.TechnologicalCard.Staff_TCs
            .Select(obj => new DisplayedStaff_TC(obj)).ToList();
        _bindingList = new BindingList<DisplayedStaff_TC>(tcList);
        _bindingList.ListChanged += BindingList_ListChanged;
        dgvMain.DataSource = _bindingList;

        SetDGVColumnsSettings();
    }

    public void AddNewObjects(List<Staff> newObjs)
    {
    _logger.Information("Добавление новых объектов. Количество: {Count}", newObjs.Count);

        List<Staff> existedStaff = new List<Staff>();
		
		foreach (var obj in newObjs)
        {
            if (_bindingList.Select(c => c.ChildId).Contains(obj.Id))
            {
                existedStaff.Add(obj);
                continue;
            }

            var newStaffTC = CreateNewObject(obj, _bindingList.Count == 0 ? 1 : _bindingList.Select(o => o.Order).Max() + 1);
            var staff = context.Staffs.Where(s => s.Id == newStaffTC.ChildId).First();

            //Прикрепляем для отслеживания в БД объект Staff, используется для обхода ошибки добавления связанных объектов
            //Помогает избежать ошибку попытки повторного добавления в базу уже существующий в ней элементов.
            context.Staffs.Attach(staff);

            _tcViewState.TechnologicalCard.Staff_TCs.Add(newStaffTC);

            //Явно говорим контексту что связываем новый и существуйющий объект
            newStaffTC.Child = staff;
            newStaffTC.ChildId = staff.Id;


            var displayedStaffTC = new DisplayedStaff_TC(newStaffTC);
            _bindingList.Add(displayedStaffTC);
        }

        dgvMain.Refresh();

        if (existedStaff.Count > 0)
        {
            string elements = "";
            foreach (var staff in existedStaff)
            {
                elements += "Сотрудник: " + staff.Name + " ID: " + staff.Id + ".\n";
            }

            MessageBox.Show("Часть объектов уже присутствовала в требованиях. Они не были внесены: \n" + elements, "Дублирование элементов", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        _logger.Information("Добавление новых объектов завершено.");
    }



    /////////////////////////////////////////////// * SaveChanges * ///////////////////////////////////////////
    private Staff_TC CreateNewObject(DisplayedStaff_TC dObj)
    {
        return new Staff_TC
        {
            IdAuto = dObj.IdAuto,
            ParentId = dObj.ParentId,
            ChildId = dObj.ChildId,
            Order = dObj.Order,
            Symbol = dObj.Symbol ?? "-",
            OutlayCount = dObj.OutlayCount,
        };
    }
    private Staff_TC CreateNewObject(Staff obj, int oreder)
    {
        return new Staff_TC
        {
            ParentId = _tcId,
            ChildId = obj.Id,
            Child = obj,
            Order = oreder,
            Symbol = "-"
        };
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////

    void SetDGVColumnsSettings()
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
            { nameof(DisplayedStaff_TC.Order), 1*pixels },//40
            //{ nameof(DisplayedStaff_TC.Name), 10*pixels }, //400
            { nameof(DisplayedStaff_TC.Type), 4*pixels }, // 160 (219 стандарт)
            { nameof(DisplayedStaff_TC.CombineResponsibility), 7*pixels },
            //{ nameof(DisplayedStaff_TC.Qualification), 13*pixels },//82+82+359
            { nameof(DisplayedStaff_TC.Symbol), 3*pixels },
            {nameof(DisplayedStaff_TC.ChildId), 2*pixels }
        };
        foreach (var column in fixColumnWidths)
        {
            dgvMain.Columns[column.Key].Width = column.Value;
            dgvMain.Columns[column.Key].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvMain.Columns[column.Key].Resizable = DataGridViewTriState.False;
        }


        // make columns readonly
        foreach (DataGridViewColumn column in dgvMain.Columns)
        {
            column.ReadOnly = true;
        }

        //if (!_isViewMode)
        //{
        //    // make columns editable
        //    dgvMain.Columns[nameof(DisplayedStaff_TC.Order)].ReadOnly = false;
        //    dgvMain.Columns[nameof(DisplayedStaff_TC.Symbol)].ReadOnly = false;

        //    dgvMain.Columns[nameof(DisplayedStaff_TC.Order)].DefaultCellStyle.BackColor = Color.LightGray;
        //    dgvMain.Columns[nameof(DisplayedStaff_TC.Symbol)].DefaultCellStyle.BackColor = Color.LightGray;
        //}

        DisplayedEntityHelper.SetupDataGridView<DisplayedStaff_TC>(dgvMain);

    }
    //private void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    //{
    //    // Проверяем, что это не заголовок столбца и не новая строка
    //    if (e.RowIndex >= 0 && e.RowIndex < dgvMain.Rows.Count)
    //    {
    //        var row = dgvMain.Rows[e.RowIndex];
    //        var displayedStaff = row.DataBoundItem as IReleasable;
    //        if (displayedStaff != null)
    //        {
    //            // Меняем цвет строки в зависимости от значения свойства IsReleased
    //            if (!displayedStaff.IsReleased)
    //            {
    //                row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#d1c6c2"); // Цвет для строк, где IsReleased = false
    //            }
    //        }
    //    }
    //}
    //private void dgvMain_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
    //{
    //    // Проверяем, редактируется ли столбец "Order"
    //    if (dgvMain.Columns[e.ColumnIndex].Name == nameof(DisplayedStaff_TC.Order))
    //    {
    //        // Проверяем, что значение не пустое и является допустимым целым числом
    //        if (string.IsNullOrWhiteSpace(e.FormattedValue.ToString()) || !int.TryParse(e.FormattedValue.ToString(), out _))
    //        {
    //            e.Cancel = true; // Отменяем редактирование

    //            // Получаем объект, связанный с редактируемой строкой
    //            var row = dgvMain.Rows[e.RowIndex];
    //            var displayedStaff = row.DataBoundItem as IPreviousOrderable;

    //            if (displayedStaff != null)
    //            {
    //                // Восстанавливаем предыдущее значение
    //                dgvMain.CancelEdit(); // Отменяем текущее редактирование
    //                row.Cells[e.ColumnIndex].Value = displayedStaff.PreviousOrder; // Восстанавливаем предыдущее значение
    //            }

    //            MessageBox.Show("Значение в столбце 'Order' должно быть целым числом и не может быть пустым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //        }
    //    }
    //}


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    ///////////////////////////////////////////////////// * Events handlers * /////////////////////////////////////////////////////////////////////////////////
    private void btnAddNewObj_Click(object sender, EventArgs e)
	{
		LogUserAction("Добавление нового объекта");
		// load new form Win7_3_Staff as dictonary
		var newForm = new Win7_3_Staff(this, activateNewItemCreate: true, createdTCId: _tcId);
        newForm.WindowState = FormWindowState.Maximized;
        //newForm.SetAsAddingForm();
        newForm.ShowDialog();
    }

    private void btnDeleteObj_Click(object sender, EventArgs e)
	{
		LogUserAction("Удаление выбранных объектов");

		DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
            _bindingList, _newObjects, _deletedObjects);

        if(_deletedObjects.Count != 0)
        {
            foreach (var obj in _deletedObjects)
            {
                foreach(var techOperationWork in _tcViewState.TechOperationWorksList)
                {
                    foreach(var evecutionWork in techOperationWork.executionWorks)
                    {
                        var staffToRemove = evecutionWork.Staffs.Where(s => s.IdAuto == obj.IdAuto && s.ChildId == obj.ChildId).FirstOrDefault();
                        if (staffToRemove != null)
                        {
							_logger.Debug("Удаление Staff из ExecutionWork: IDAuto={IdAuto}, ChildId={ChildId}", staffToRemove.IdAuto, staffToRemove.ChildId);
							evecutionWork.Staffs.Remove(staffToRemove);
						}
                    }
                }

                var deletedObj = _tcViewState.TechnologicalCard.Staff_TCs.Where(s => s.IdAuto == obj.IdAuto && s.ChildId == obj.ChildId).FirstOrDefault();
                if (deletedObj != null)
                {
					_logger.Debug("Удаление объектов({ObjectType}}: {ObjectName} (ID={Id})",
							deletedObj.GetType(), deletedObj.Child?.Name, deletedObj.ChildId);

					_tcViewState.TechnologicalCard.Staff_TCs.Remove(deletedObj);
				}
            }

			_logger.Information("Удалено объектов: {Count}", _deletedObjects.Count);
			_deletedObjects.Clear();
		}
		else
		{
			_logger.Warning("Удаление не выполнено. Список удаляемых объектов пуст.");
		}
	}


    private bool CheckIfIsNewItems(DataGridViewRow row)
    {
        if (row.Cells["Symbol"].Value.ToString() == "-")
        {
            return true;
        }
        return false;
    }

    private void btnMoveUp_Click(object sender, EventArgs e)
    {
		_logger.Information("Клик по кнопке 'Переместить вверх'.");
	}

    private void btnMoveDown_Click(object sender, EventArgs e)
    {
		_logger.Information("Клик по кнопке 'Переместить вниз'.");
	}

    private void dgvMain_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {

        DGVProcessing.ReorderRows(dgvMain, e, _bindingList);

    }

    private void SortAndRefreshDGV()
    {
        var sortedList = _bindingList.OrderBy(x => x.Order).ToList();
        _bindingList.Clear();
        foreach (var item in sortedList)
        {
            _bindingList.Add(item);
        }
        UpdateOrderValues();
        //_bindingList.ListChanged += BindingList_ListChanged;
        dgvMain.Refresh(); // Обновляем DataGridView, чтобы отразить изменения
    }
    private void MoveRowAndUpdateOrder(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= _bindingList.Count || newIndex < 0 || newIndex >= _bindingList.Count || oldIndex == newIndex)
        {
            return; // Проверка на валидность индексов
        }

        // Шаг 1: Удаление и вставка объекта в новую позицию
        var itemToMove = _bindingList[oldIndex];
        _bindingList.RemoveAt(oldIndex);
        _bindingList.Insert(newIndex, itemToMove);

        UpdateOrderValues();
    }
    private void UpdateOrderValues()
    {
        for (int i = 0; i < _bindingList.Count; i++)
        {
            _bindingList[i].Order = i + 1;
        }
    }
    private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
    {
		DisplayedEntityHelper.ListChangedEventHandlerIntermediate
            (e, _bindingList, _newObjects, _changedObjects, _deletedObjects);

        if(_changedObjects.Count != 0)
        { 
            foreach (var obj in _changedObjects)
            {
                var changedObject = _tcViewState.TechnologicalCard.Staff_TCs.Where(s => s.ChildId == obj.ChildId && s.IdAuto == obj.IdAuto).FirstOrDefault();
                changedObject.ApplyUpdates(CreateNewObject(obj));
            }

            _changedObjects.Clear();
        }
    }


    public class DisplayedStaff_TC : INotifyPropertyChanged, IIntermediateDisplayedEntity, IOrderable, IPreviousOrderable, IReleasable
    {
        public Dictionary<string, string> GetPropertiesNames()
        {
            return new Dictionary<string, string>
            {
                { nameof(ChildId), "ID" },
                { nameof(ParentId), "ID тех. карты" },
                { nameof(Order), "№" },
                { nameof(Symbol), "Обозначение" },
                { nameof(OutlayCount), "Участвует в подсчете затрат" },
                { nameof(Name), "Наименование" },
                { nameof(Type), "Тип (исполнение)" },
                { nameof(Functions), "Функции" },
                { nameof(CombineResponsibility), "Возможность совмещения обязанностей" },
                { nameof(Qualification), "Квалификация" },
                { nameof(Comment), "Комментарии" },
            };
        }
        public List<string> GetPropertiesOrder()
        {
            return new List<string>
            {
                nameof(Order),
                nameof(Name),
                nameof(Type),
                nameof(CombineResponsibility),
                nameof(Qualification),
                nameof(Symbol),
                nameof(OutlayCount),
                nameof(ChildId)
            };
        }
        public List<string> GetRequiredFields()
        {
            return new List<string>
            {
                nameof(ChildId) ,
                nameof(ParentId) ,
                nameof(Order) ,
                nameof(Symbol),
            };
        }
        public List<string> GetKeyFields()
        {
            return new List<string>
            {
                nameof(ChildId),
                nameof(ParentId),
                nameof(Symbol) ,
            };
        }

        private int idAuto;

        private int childId;
        private int parentId;
        private int order;


        private string symbol;

        private string name;
        private string type;
        private string functions;
        private string? combineResponsibility;
        private string qualification;
        private string? comment;


        public DisplayedStaff_TC()
        {

        }
        public DisplayedStaff_TC(Staff_TC obj)
        {
            IdAuto = obj.IdAuto;

            ChildId = obj.ChildId;
            ParentId = obj.ParentId;
            Order = obj.Order;
            Symbol = obj.Symbol;
            OutlayCount = obj.OutlayCount;
            Name = obj.Child.Name;
            Type = obj.Child.Type;
            Functions = obj.Child.Functions;
            CombineResponsibility = obj.Child.CombineResponsibility;
            Qualification = obj.Child.Qualification;
            Comment = obj.Child.Comment;
            IsReleased = obj.Child.IsReleased;

            previousOrder = Order;
        }

        public int IdAuto { get; set; }
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
        public string? Symbol
        {
            get => symbol;
            set
            {
                if (symbol != value)
                {

                    oldValueDict[nameof(Symbol)] = symbol;

                    symbol = value ?? "-";
                    OnPropertyChanged(nameof(Symbol));
                }
            }
        }

        public bool OutlayCount { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }
        public string Functions { get; set; }
        public string? CombineResponsibility { get; set; }
        public string Qualification { get; set; }
        public string? Comment { get; set; }
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

    private void LogUserAction(string message)
    {
        _logger?.LogUserAction(message, _tcId);
	}
    private void AdjustColumnWidths()
    {
        int dgvWidth = dgvMain.ClientSize.Width - dgvMain.RowHeadersWidth;
        int totalMinWidth = 0;
        int totalFixedWidth = 0;
        int availableWidth;
        int extraAdditionalWidth = 0;

        int pixels = 40;

        Dictionary<string, int> minColumnWidths = new Dictionary<string, int>
        {
            { nameof(DisplayedStaff_TC.Order), 1*pixels },//40
            { nameof(DisplayedStaff_TC.Name), 10*pixels }, //400
            { nameof(DisplayedStaff_TC.Type), 4*pixels }, // 160 (219 стандарт)
            { nameof(DisplayedStaff_TC.CombineResponsibility), 7*pixels },
            { nameof(DisplayedStaff_TC.Qualification), 13*pixels },//82+82+359
            { nameof(DisplayedStaff_TC.Symbol), 2*pixels },
            {nameof(DisplayedStaff_TC.ChildId), 1*pixels }
        };
        Dictionary<string, int> maxColumnWidths = new Dictionary<string, int>
        {
            { nameof(DisplayedStaff_TC.Order), 1*pixels },
            { nameof(DisplayedStaff_TC.Type), 6*pixels },
            { nameof(DisplayedStaff_TC.CombineResponsibility), 11*pixels },
            { nameof(DisplayedStaff_TC.Symbol), 2*pixels },
            { nameof(DisplayedStaff_TC.ChildId), 1*pixels }

        };

        // Рассчитайте общую минимальную ширину
        foreach (var minWidth in minColumnWidths)
        {
            totalMinWidth += minWidth.Value;
            if (maxColumnWidths.ContainsKey(minWidth.Key))
            {
                totalFixedWidth += maxColumnWidths[minWidth.Key];
            }
        }

        // Проверяем, достаточно ли места для минимальной ширины столбцов
        if (dgvWidth > totalMinWidth)
        {
            availableWidth = dgvWidth - totalMinWidth;

            // Распределяем доступную ширину между столбцами
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                if (minColumnWidths.ContainsKey(column.Name))
                {
                    int additionalWidth = (int)((double)availableWidth / minColumnWidths.Count);
                    int proposedWidth = minColumnWidths[column.Name] + additionalWidth;

                    if (maxColumnWidths.ContainsKey(column.Name))
                    {
                        if (proposedWidth > maxColumnWidths[column.Name])
                        {
                            extraAdditionalWidth += proposedWidth - maxColumnWidths[column.Name];
                        }
                        // Учитываем максимальную ширину для столбца
                        column.Width = Math.Min(proposedWidth, maxColumnWidths[column.Name]);
                    }
                    else
                    {
                        column.Width = proposedWidth + extraAdditionalWidth;
                    }
                }
            }

            // распределяем оставшееся место между столбцами не имеющими максимальной ширины
            if (extraAdditionalWidth > 0)
            {
                int extraWidth = (int)((double)extraAdditionalWidth / (minColumnWidths.Count - maxColumnWidths.Count));
                foreach (DataGridViewColumn column in dgvMain.Columns)
                {
                    if (minColumnWidths.ContainsKey(column.Name) && !maxColumnWidths.ContainsKey(column.Name))
                    {
                        column.Width += extraWidth;
                    }
                }
            }

        }
        else
        {
            // Если места недостаточно, устанавливаем минимальные ширины
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                if (minColumnWidths.ContainsKey(column.Name))
                {
                    column.Width = minColumnWidths[column.Name];
                }
            }
        }
    }

}
