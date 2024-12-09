
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Work;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.DGVProcessing;

namespace TC_WinForms.WinForms
{
    public partial class Win6_Protection : Form, IViewModeable
    {
        private readonly TcViewState _tcViewState;

        private bool _isViewMode;

        private MyDbContext context;
        private int _tcId;

        private BindingList<DisplayedProtection_TC> _bindingList;
        private List<DisplayedProtection_TC> _changedObjects = new ();
        private List<DisplayedProtection_TC> _newObjects = new ();
        private List<DisplayedProtection_TC> _deletedObjects = new ();
        private Dictionary<DisplayedProtection_TC, DisplayedProtection_TC> _replacedObjects = new();// add to UpdateMode

        public bool CloseFormsNoSave { get; set; } = false;
        public Win6_Protection(int tcId, TcViewState tcViewState, MyDbContext context)// bool viewerMode = false)
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
            dgvMain.Columns[nameof(DisplayedProtection_TC.Order)].ReadOnly = _tcViewState.IsViewMode;
            dgvMain.Columns[nameof(DisplayedProtection_TC.Quantity)].ReadOnly = _tcViewState.IsViewMode;


            dgvMain.Columns[nameof(DisplayedProtection_TC.Order)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
            dgvMain.Columns[nameof(DisplayedProtection_TC.Quantity)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;

            // update form
            dgvMain.Refresh();

        }
        private void Win6_Protection_Load(object sender, EventArgs e)
        {
            LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedProtection_TC>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            SetViewMode();
        }
        private void LoadObjects()
        {
            var tcList = _tcViewState.TechnologicalCard.Protection_TCs.Where(obj => obj.ParentId == _tcId)
                                                                    .OrderBy(o => o.Order).ToList()
                                                                    .Select(obj => new DisplayedProtection_TC(obj))
                                                                    .ToList();
            _bindingList = new BindingList<DisplayedProtection_TC>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            SetDGVColumnsSettings();
        }

        public void AddNewObjects(List<Protection> newObjs)
        {
            foreach (var obj in newObjs)
            {
                var newObj_TC = CreateNewObject(obj, _bindingList.Select(o => o.Order).Max() + 1);
                var protection = context.Protections.Where(s => s.Id == newObj_TC.ChildId).First();

                context.Protections.Attach(protection);
                _tcViewState.TechnologicalCard.Protection_TCs.Add(newObj_TC);

                newObj_TC.Child = protection;
                newObj_TC.ChildId = protection.Id;

                var displayedObj_TC = new DisplayedProtection_TC(newObj_TC);
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
            //    nameof(DisplayedProtection_TC.Order),
            //    nameof(DisplayedProtection_TC.Name),
            //    nameof(DisplayedProtection_TC.Type),
            //    nameof(DisplayedProtection_TC.Unit),
            //    nameof(DisplayedProtection_TC.Quantity),
            //};
            //foreach (var column in autosizeColumn)
            //{
            //    dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            //}

            int pixels = 35;
            // Минимальные ширины столбцов
            Dictionary<string, int> fixColumnWidths = new Dictionary<string, int>
            {
                { nameof(DisplayedProtection_TC.Order), 1*pixels },
                { nameof(DisplayedProtection_TC.Type), 4*pixels },
                { nameof(DisplayedProtection_TC.Unit), 2*pixels },
                { nameof(DisplayedProtection_TC.Quantity), 3*pixels },
                { nameof(DisplayedProtection_TC.ChildId), 2*pixels },

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
                nameof(DisplayedProtection_TC.Order),
                nameof(DisplayedProtection_TC.Quantity),
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
                var displayedStaff = row.DataBoundItem as DisplayedProtection_TC;
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
            var oldObjects = _replacedObjects.Select(dtc => CreateNewObject(dtc.Key)).ToList();
            var newObjects = _replacedObjects.Select(dtc => CreateNewObject(dtc.Value)).ToList();

            var obj_TCsIds = oldObjects.Select(t => t.ChildId).ToList();

            List<ExecutionWork> executionWorks = new List<ExecutionWork>();

            foreach (var techOperationWork in _tcViewState.TechOperationWorksList)
            {
                var executionWorksToReplace = techOperationWork.executionWorks
                                                               .Where(ew => ew.techOperationWork.TechnologicalCardId == _tcId
                                                                         && ew.Protections.Any(m => obj_TCsIds.Contains(m.ChildId)))
                                                                .ToList();

                if (executionWorksToReplace != null && executionWorksToReplace.Count != 0)
                    executionWorks.AddRange(executionWorksToReplace);
            }

            for (int i = 0; i < newObjects.Count; i++)
            {
                var protection = context.Protections.Where(m => m.Id == newObjects[i].ChildId).First();
                newObjects[i].Child = protection;

                var oldProtection = _tcViewState.TechnologicalCard.Protection_TCs.Where(m => m.ChildId == oldObjects[i].ChildId).FirstOrDefault();
                if(oldProtection != null)
                _tcViewState.TechnologicalCard.Protection_TCs.Remove(oldProtection);
            }

            _tcViewState.TechnologicalCard.Protection_TCs.AddRange(newObjects);

            foreach (var newTc in newObjects)
            {
                executionWorks.ForEach(ew => ew.Protections.Add(newTc));
            }


            _replacedObjects.Clear();
        }
        private Protection_TC CreateNewObject(DisplayedProtection_TC dObj)
        {
            return new Protection_TC
            {
                ParentId = dObj.ParentId,
                ChildId = dObj.ChildId,
                Order = dObj.Order,
                Quantity = dObj.Quantity ?? 0,
                Note = dObj.Note,
            };
        }
        private Protection_TC CreateNewObject(Protection obj, int oreder)
        {
            return new Protection_TC
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
            var newForm = new Win7_7_Protection(activateNewItemCreate: true, createdTCId: _tcId);
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
                    foreach (var techOperation in _tcViewState.TechOperationWorksList)
                    {
                        foreach (var executionWork in techOperation.executionWorks)
                        {
                            var protectionToDelete = executionWork.Protections.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                            if (protectionToDelete != null)
                                executionWork.Protections.Remove(protectionToDelete);
                        }

                    }


                    var deletedObj = _tcViewState.TechnologicalCard.Protection_TCs.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    if(deletedObj != null)
                        _tcViewState.TechnologicalCard.Protection_TCs.Remove(deletedObj);
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
                    var changedObject = _tcViewState.TechnologicalCard.Protection_TCs.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    if (changedObject != null)
                        changedObject.ApplyUpdates(CreateNewObject(obj));
                }

                _changedObjects.Clear();
            }
        }
        private class DisplayedProtection_TC : INotifyPropertyChanged, IIntermediateDisplayedEntity, IOrderable, IPreviousOrderable, IReleasable
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
                    //nameof(Price),
                    //nameof(Description),
                    //nameof(Manufacturer),
                    //nameof(ClassifierCode),

                    nameof(Quantity),
                    //nameof(Note),

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

            public DisplayedProtection_TC()
            {

            }
            public DisplayedProtection_TC(Protection_TC obj)
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
                ClassifierCode = obj.Child.ClassifierCode;

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
            var newForm = new Win7_7_Protection(activateNewItemCreate: true, createdTCId: _tcId, isUpdateMode: true);

            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();

        }// add to UpdateMode
        public bool UpdateSelectedObject(Protection updatedObject)
        {
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return false;
            }

            var selectedRow = dgvMain.SelectedRows[0];
            //var displayedComponent = selectedRow.DataBoundItem as DisplayedTool_TC;

            if (selectedRow.DataBoundItem is DisplayedProtection_TC dObj)
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

                var newDisplayedComponent = new DisplayedProtection_TC(newItem);


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
