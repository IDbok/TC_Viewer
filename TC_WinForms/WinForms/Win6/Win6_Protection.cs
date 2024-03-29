﻿
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.DGVProcessing;

namespace TC_WinForms.WinForms
{
    public partial class Win6_Protection : Form, ISaveEventForm
    {
        private DbConnector dbCon = new DbConnector();
        private int _tcId;

        private BindingList<DisplayedProtection_TC> _bindingList;
        private List<DisplayedProtection_TC> _changedObjects = new List<DisplayedProtection_TC>();
        private List<DisplayedProtection_TC> _newObjects = new List<DisplayedProtection_TC>();
        private List<DisplayedProtection_TC> _deletedObjects = new List<DisplayedProtection_TC>();

        public Win6_Protection(int tcId)
        {
            InitializeComponent();
            this._tcId = tcId;

            // new DGVEvents().AddGragDropEvents(dgvMain);
            new DGVEvents().SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);
        }

        private async void Win6_Protection_Load(object sender, EventArgs e)
        {
            await LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedProtection_TC>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;
        }
        private async Task LoadObjects()
        {
            var tcList = await Task.Run(() => dbCon.GetIntermediateObjectList<Protection_TC, Protection>(_tcId).OrderBy(obj => obj.Order)
                .Select(obj => new DisplayedProtection_TC(obj)).ToList());
            _bindingList = new BindingList<DisplayedProtection_TC>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            SetDGVColumnsSettings();
        }

        private async void Win6_Protection_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_newObjects.Count + _changedObjects.Count + _deletedObjects.Count != 0)
            {
                e.Cancel = true;
                var result = MessageBox.Show("Сохранить изменения перед закрытием?", "Сохранение", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    await SaveChanges();
                }
                e.Cancel = false;
                Close();
            }
        }
        public void AddNewObjects(List<Protection> newObjs)
        {
            foreach (var obj in newObjs)
            {
                var newObj_TC = CreateNewObject(obj, _bindingList.Count + 1);

                var displayedObj_TC = new DisplayedProtection_TC(newObj_TC);
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

            dgvMain.Columns["Order"].DefaultCellStyle.BackColor = Color.LightGray;
            dgvMain.Columns["Quantity"].DefaultCellStyle.BackColor = Color.LightGray; //Color.LightBlue;
            dgvMain.Columns["Note"].DefaultCellStyle.BackColor = Color.LightGray;

            // ширина столбцов по содержанию
            var autosizeColumn = new List<string>
            {
                "Order",
                "Quantity",
                "Note",
                "ChildId",
                "Name",
                "Type",
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            // make columns readonly
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                column.ReadOnly = true;
            }
            var changeableColumn = new List<string>
            {
                "Order",
                "Quantity",
                "Note",
            };
            foreach (var column in changeableColumn)
            {
                dgvMain.Columns[column].ReadOnly = false;
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

        private Protection_TC CreateNewObject(DisplayedProtection_TC dObj)
        {
            return new Protection_TC
            {
                ParentId = dObj.ParentId,
                ChildId = dObj.ChildId,
                Order = dObj.Order,
                Quantity = dObj.Quantity,
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
            var newForm = new Win7_7_Protection();
            newForm.SetAsAddingForm();
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
        private class DisplayedProtection_TC : INotifyPropertyChanged, IIntermediateDisplayedEntity, IOrderable
        {
            public Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
            {
                { nameof(ChildId), "ID" },
                { nameof(ParentId), "ID тех. карты" },
                { nameof(Order), "№" },
                { nameof(Quantity), "Количество" },
                { nameof(Note), "Примечание" },

                { nameof(Name), "Наименование" },
                { nameof(Type), "Тип" },
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
                    nameof(Price),
                    nameof(Description),
                    nameof(Manufacturer),
                    nameof(ClassifierCode),

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
                Price = obj.Child.Price;
                Description = obj.Child.Description;
                Manufacturer = obj.Child.Manufacturer;
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
