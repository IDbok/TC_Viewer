using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.DGVProcessing;

namespace TC_WinForms.WinForms
{
    public partial class Win6_Tool : Form, IViewModeable
    {
        private readonly TcViewState _tcViewState;

        private bool _isViewMode;

        private MyDbContext context;

        private int _tcId;

        private BindingList<DisplayedTool_TC> _bindingList;
        private List<DisplayedTool_TC> _changedObjects = new ();
        private List<DisplayedTool_TC> _newObjects = new ();
        private List<DisplayedTool_TC> _deletedObjects = new ();

        private Dictionary<DisplayedTool_TC, DisplayedTool_TC> _replacedObjects = new ();// add to UpdateMode
        public Win6_Tool(int tcId, TcViewState tcViewState, MyDbContext context)// bool viewerMode = false)
        {
            _tcViewState = tcViewState;
            this.context = context;

            _tcId = tcId;

            InitializeComponent();


            var dgvEventService = new DGVEvents(dgvMain);
            dgvEventService.SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);

            dgvMain.CellFormatting += dgvEventService.dgvMain_CellFormatting;
            dgvMain.CellValidating += dgvEventService.dgvMain_CellValidating;

            this.FormClosed += (sender, e) => this.Dispose();
        }

        public void SetViewMode(bool? isViewMode = null)
        {
            pnlControls.Visible = !_tcViewState.IsViewMode;

            // make columns editable
            dgvMain.Columns[nameof(DisplayedTool_TC.Order)].ReadOnly = _tcViewState.IsViewMode;
            dgvMain.Columns[nameof(DisplayedTool_TC.Quantity)].ReadOnly = _tcViewState.IsViewMode;
            dgvMain.Columns[nameof(DisplayedTool_TC.Note)].ReadOnly = _tcViewState.IsViewMode;


            dgvMain.Columns[nameof(DisplayedTool_TC.Order)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
            dgvMain.Columns[nameof(DisplayedTool_TC.Quantity)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
            dgvMain.Columns[nameof(DisplayedTool_TC.Note)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;

            // update form
            dgvMain.Refresh();

        }

        private void Win6_Tool_Load(object sender, EventArgs e)
        {
            LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedTool_TC>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            SetViewMode();
        }
        private void LoadObjects()
        {
            var tcList = _tcViewState.TechnologicalCard.Tool_TCs.Where(obj => obj.ParentId == _tcId)
                                                                .OrderBy(o => o.Order).ToList()
                                                                .Select(obj => new DisplayedTool_TC(obj))
                                                                .ToList();
            _bindingList = new BindingList<DisplayedTool_TC>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            SetDGVColumnsSettings();
        }

        public void AddNewObjects(List<Tool> newObjs)
        {
            foreach (var obj in newObjs)
            {
                var newObj_TC = CreateNewObject(obj, _bindingList.Select(o => o.Order).Max() + 1);
                var tool = context.Tools.Where(s => s.Id == newObj_TC.ChildId).First();

                context.Tools.Attach(tool);
                _tcViewState.TechnologicalCard.Tool_TCs.Add(newObj_TC);

                newObj_TC.Child = tool;
                newObj_TC.ChildId = tool.Id;

                var displayedObj_TC = new DisplayedTool_TC(newObj_TC);
                _bindingList.Add(displayedObj_TC);
            }


            dgvMain.Refresh();
        }

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

            //// ширина столбцов по содержанию
            //var autosizeColumn = new List<string>
            //{
            //    nameof(DisplayedTool_TC.Order),
            //    nameof(DisplayedTool_TC.Name),
            //    nameof(DisplayedTool_TC.Type),
            //    nameof(DisplayedTool_TC.Unit),
            //    nameof(DisplayedTool_TC.Quantity),
            //    nameof(DisplayedTool_TC.ChildId),
            //};
            //foreach (var column in autosizeColumn)
            //{
            //    dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            //}

            int pixels = 35;
            // Минимальные ширины столбцов
            Dictionary<string, int> fixColumnWidths = new Dictionary<string, int>
            {
                { nameof(DisplayedTool_TC.Order), 1*pixels },
                { nameof(DisplayedTool_TC.Type), 4*pixels },
                { nameof(DisplayedTool_TC.Unit), 2*pixels },
                { nameof(DisplayedTool_TC.Quantity), 3*pixels },
                { nameof(DisplayedTool_TC.ChildId), 2*pixels },

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
            var changeableColumn = new List<string>
            {
                nameof(DisplayedTool_TC.Order),
                nameof(DisplayedTool_TC.Quantity),
                nameof(DisplayedTool_TC.Note),
            };
            foreach (var column in changeableColumn)
            {
                dgvMain.Columns[column].ReadOnly = false;
                dgvMain.Columns[column].DefaultCellStyle.BackColor = Color.LightGray;
            }
        }
        private void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Проверяем, что это не заголовок столбца и не новая строка
            if (e.RowIndex >= 0 && e.RowIndex < dgvMain.Rows.Count)
            {
                var row = dgvMain.Rows[e.RowIndex];
                var displayedStaff = row.DataBoundItem as DisplayedTool_TC;
                if (displayedStaff != null)
                {
                    // Меняем цвет строки в зависимости от значения свойства IsReleased
                    if (!displayedStaff.IsReleased)
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
                var tool = context.Tools.Where(m => m.Id == newObj.ChildId).First();
                newObj.Child = tool;
            }

            //Заменяем объект в ToolWorks в TechOperationWorksList, если он присутствует в списке компонентов тех операции
            foreach (var tecjOperationWork in _tcViewState.TechOperationWorksList)
            {
                for (int i = 0; i < oldObjects.Count; i++)
                {
                    var replaceableToolWork = tecjOperationWork.ToolWorks.Where(m => m.toolId == oldObjects[i].ChildId).FirstOrDefault();
                    if (replaceableToolWork != null)
                    {
                        replaceableToolWork.tool = newObjects[i].Child;
                        replaceableToolWork.toolId = newObjects[i].ChildId;
                    }
                }
            }

            //Удялем старые компоненты из ТехКарты
            for (int i = 0; i < oldObjects.Count; i++)
            {
                var oldTool = _tcViewState.TechnologicalCard.Tool_TCs.Where(m => m.ChildId == oldObjects[i].ChildId).First();
                _tcViewState.TechnologicalCard.Tool_TCs.Remove(oldTool);
            }

            _tcViewState.TechnologicalCard.Tool_TCs.AddRange(newObjects);

            _changedObjects.Clear();
        }
        private Tool_TC CreateNewObject(DisplayedTool_TC dObj)
        {
            return new Tool_TC
            {
                ParentId = dObj.ParentId,
                ChildId = dObj.ChildId,
                Order = dObj.Order,
                Quantity = dObj.Quantity ?? 0,
                Note = dObj.Note,
            };
        }
        private Tool_TC CreateNewObject(Tool obj, int oreder)
        {
            return new Tool_TC
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
            // load new form Win7 Tool as dictonary
            var newForm = new Win7_6_Tool(activateNewItemCreate: true, createdTCId: _tcId);
            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();
        }

        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
                _bindingList, _newObjects, _deletedObjects);

            if (_deletedObjects.Count != 0)
            {
                foreach (var obj in _deletedObjects)
                {
                    foreach (var tecjOperationWork in _tcViewState.TechOperationWorksList)
                    {
                        var deletableCompWork = tecjOperationWork.ToolWorks.Where(m => m.toolId == obj.ChildId).FirstOrDefault();
                        if (deletableCompWork != null)
                        {
                            tecjOperationWork.ToolWorks.Remove(deletableCompWork);
                        }

                    }
                    var deletedObj = _tcViewState.TechnologicalCard.Tool_TCs.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    if (deletedObj != null)
                        _tcViewState.TechnologicalCard.Tool_TCs.Remove(deletedObj);
                }

                _deletedObjects.Clear();
            }
        }

        private void dgvMain_CellEndEdit(object sender, DataGridViewCellEventArgs e) // todo - fix problem with selection replacing row (error while remove it)
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
                    var changedObject = _tcViewState.TechnologicalCard.Tool_TCs.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    if (changedObject != null)
                        changedObject.ApplyUpdates(CreateNewObject(obj));
                }

                _changedObjects.Clear();
            }
        }
        private class DisplayedTool_TC : INotifyPropertyChanged, IIntermediateDisplayedEntity, IOrderable, IPreviousOrderable, IReleasable
        {
            public Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
            {
                { nameof(ChildId), "ID" },
                { nameof(ParentId), "ID тех. карты" },
                { nameof(Order), "№" },
                { nameof(Quantity), "Кол-во" },
                { nameof(Note), "Примечание" },

                { nameof(Name), "Наименование" },
                { nameof(Type), "Тип (исполнение)" },
                { nameof(Unit), "Ед.изм." },
                { nameof(Price), "Стоимость, руб. без НДС" },
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
                    //nameof(ParentId),
                    nameof(Order),

                    nameof(Name),
                    nameof(Type),
                    nameof(Unit),
                    //nameof(TotalPrice),
                    //nameof(Description),
                    //nameof(Manufacturer),
                    //nameof(Categoty),
                    //nameof(ClassifierCode),

                    nameof(Quantity),
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

            public DisplayedTool_TC()
            {

            }
            public DisplayedTool_TC(Tool_TC obj)
            {
                ChildId = obj.ChildId;
                ParentId = obj.ParentId;
                Order = obj.Order;

                Name = obj.Child.Name;
                Type = obj.Child.Type;

                Unit = obj.Child.Unit;
                Quantity = obj.Quantity;
                Price = obj.Child.Price;
                Description = obj.Child.Description;
                Manufacturer = obj.Child.Manufacturer;
                Categoty = obj.Child.Categoty;
                ClassifierCode = obj.Child.ClassifierCode;
                Note = obj.Note;

                IsReleased = obj.Child.IsReleased;

                previousOrder = obj.Order;
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
            public float? Price { get; set; }
            public string? Description { get; set; }
            public string? Manufacturer { get; set; }
            //public List<LinkEntety> Links { get; set; } = new();
            public string Categoty { get; set; } = "Tool";
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

        private void btnReplace_Click(object sender, EventArgs e)
        {
            // Выделение объекта выбранной строки
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return;
            }

            // load new form Win7_3_Component as dictonary
            var newForm = new Win7_6_Tool(activateNewItemCreate: true, createdTCId: _tcId, isUpdateMode: true);

            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();

        }// add to UpdateMode
        public bool UpdateSelectedObject(Tool updatedObject)
        {
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return false;
            }

            var selectedRow = dgvMain.SelectedRows[0];
            //var displayedComponent = selectedRow.DataBoundItem as DisplayedTool_TC;

            if (selectedRow.DataBoundItem is DisplayedTool_TC dObj)
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

                var newDisplayedComponent = new DisplayedTool_TC(newItem);


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
