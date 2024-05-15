using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.DGVProcessing;

namespace TC_WinForms.WinForms
{
    public partial class Win6_Tool : Form, ISaveEventForm, IViewModeable
    {
        private bool _isViewMode;

        private DbConnector dbCon = new DbConnector();
        private int _tcId;

        private BindingList<DisplayedTool_TC> _bindingList;
        private List<DisplayedTool_TC> _changedObjects = new List<DisplayedTool_TC>();
        private List<DisplayedTool_TC> _newObjects = new List<DisplayedTool_TC>();
        private List<DisplayedTool_TC> _deletedObjects = new List<DisplayedTool_TC>();
        public bool CloseFormsNoSave { get; set; } = false;
        public Win6_Tool(int tcId, bool viewerMode = false)
        {
            _isViewMode= viewerMode;
            _tcId = tcId;

            InitializeComponent();
            

            // new DGVEvents().AddGragDropEvents(dgvMain);
            new DGVEvents().SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);
        }
        public void SetViewMode(bool? isViewMode = null)
        {
            if (isViewMode != null)
            {
                _isViewMode = (bool)isViewMode;
            }

            pnlControls.Visible = !_isViewMode;

            // make columns editable
            dgvMain.Columns[nameof(DisplayedTool_TC.Order)].ReadOnly = _isViewMode;
            dgvMain.Columns[nameof(DisplayedTool_TC.Quantity)].ReadOnly = _isViewMode;


            dgvMain.Columns[nameof(DisplayedTool_TC.Order)].DefaultCellStyle.BackColor = _isViewMode ? Color.White : Color.LightGray;
            dgvMain.Columns[nameof(DisplayedTool_TC.Quantity)].DefaultCellStyle.BackColor = _isViewMode ? Color.White : Color.LightGray;

            // update form
            dgvMain.Refresh();

        }

        public bool GetDontSaveData()
        {
            if (_newObjects.Count + _changedObjects.Count + _deletedObjects.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private async void Win6_Tool_Load(object sender, EventArgs e)
        {
            await LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedTool_TC>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            SetViewMode();
        }
        private async Task LoadObjects()
        {
            var tcList = await Task.Run(() => dbCon.GetIntermediateObjectList<Tool_TC, Tool>(_tcId).OrderBy(obj => obj.Order)
                .Select(obj => new DisplayedTool_TC(obj)).ToList());
            _bindingList = new BindingList<DisplayedTool_TC>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            SetDGVColumnsSettings();
        }

        private async void Win6_Tool_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseFormsNoSave)
            {
                return;
            }
            if (_newObjects.Count + _changedObjects.Count + _deletedObjects.Count != 0)
            {
                e.Cancel = true;
                var result = MessageBox.Show("Сохранить изменения перед закрытием?", "Сохранение", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    await SaveChanges();
                }
                e.Cancel = false;
            }
        }
        public void AddNewObjects(List<Tool> newObjs)
        {
            foreach (var obj in newObjs)
            {
                var newObj_TC = CreateNewObject(obj, _bindingList.Count + 1);

                var displayedObj_TC = new DisplayedTool_TC(newObj_TC);
                _bindingList.Add(displayedObj_TC);

                _newObjects.Add(displayedObj_TC);
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
            };
            foreach (var column in changeableColumn)
            {
                dgvMain.Columns[column].ReadOnly = false;
                dgvMain.Columns[column].DefaultCellStyle.BackColor = Color.LightGray;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool HasChanges => _changedObjects.Count + _newObjects.Count + _deletedObjects.Count != 0;
        public async Task SaveChanges()
        {
            if (!HasChanges)
            {
                return;
            }
            if (_newObjects.Count > 0)
            {
                await SaveNewObjects();
            }
            if (_changedObjects.Count > 0)
            {
                await SaveChangedObjects();
            }
            if (_deletedObjects.Count > 0)
            {
                await DeleteDeletedObjects();
            }

            dgvMain.Refresh();
        }
        private async Task SaveNewObjects()
        {
            var newObjects = _newObjects.Select(dObj => CreateNewObject(dObj)).ToList();

            await dbCon.AddIntermediateObjectAsync(newObjects);

            _newObjects.Clear();
        }
        private async Task SaveChangedObjects()
        {
            var changedTcs = _changedObjects.Select(dtc => CreateNewObject(dtc)).ToList();

            await dbCon.UpdateIntermediateObjectAsync(changedTcs);

            _changedObjects.Clear();
        }

        private async Task DeleteDeletedObjects()
        {
            var deletedObjects = _deletedObjects.Select(dtc => CreateNewObject(dtc)).ToList();
            await dbCon.DeleteIntermediateObjectAsync(deletedObjects);
            _deletedObjects.Clear();
        }

        private Tool_TC CreateNewObject(DisplayedTool_TC dObj)
        {
            return new Tool_TC
            {
                ParentId = dObj.ParentId,
                ChildId = dObj.ChildId,
                Order = dObj.Order,
                Quantity = dObj.Quantity,
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

        }

        private void dgvMain_CellEndEdit(object sender, DataGridViewCellEventArgs e) // todo - fix problem with selection replacing row (error while remove it)
        {
            ReorderRows(dgvMain, e, _bindingList);
        }

        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            DisplayedEntityHelper.ListChangedEventHandlerIntermediate
                (e, _bindingList, _newObjects, _changedObjects, _deletedObjects);
        }
        private class DisplayedTool_TC : INotifyPropertyChanged, IIntermediateDisplayedEntity, IOrderable
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
                    //nameof(Price),
                    //nameof(Description),
                    //nameof(Manufacturer),
                    //nameof(Categoty),
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
                Price = obj.Child.Price;
                Description = obj.Child.Description;
                Manufacturer = obj.Child.Manufacturer;
                Categoty = obj.Child.Categoty;
                ClassifierCode = obj.Child.ClassifierCode;
            }

            public int ChildId { get; set; }
            public int ParentId { get; set; }
            public int Order
            {
                get => order;
                set
                {
                    if (order != value)
                    {
                        order = value;
                        OnPropertyChanged(nameof(Order));
                    }
                }
            }
            public double Quantity
            {
                get => quantity;
                set
                {
                    if (quantity != value)
                    {
                        quantity = value;
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

    }

}
