using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win6_Staff : Form, ISaveEventForm
    {

        private DbConnector dbCon = new DbConnector();
        private BindingList<DisplayedStaff_TC> _bindingList;

        private List<DisplayedStaff_TC> _changedObjects = new List<DisplayedStaff_TC>();
        private List<DisplayedStaff_TC> _newObjects = new List<DisplayedStaff_TC>();
        private List<DisplayedStaff_TC> _deletedObjects = new List<DisplayedStaff_TC>();

        private int _tcId;
        public Win6_Staff(int tcId)
        {
            InitializeComponent();
            this._tcId = tcId;

            new DGVEvents().AddGragDropEvents(dgvMain);
            new DGVEvents().SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);
        }

        private async void Win6_Staff_Load(object sender, EventArgs e)
        {
            await LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedStaff_TC>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;
        }
        private async Task LoadObjects()
        {
            var tcList = await Task.Run(() => dbCon.GetIntermediateObjectList<Staff_TC, Staff>(_tcId).OrderBy(obj => obj.Order)
                .Select(obj => new DisplayedStaff_TC(obj)).ToList());
            _bindingList = new BindingList<DisplayedStaff_TC>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;
        }
        private async void Win6_Staff_FormClosing(object sender, FormClosingEventArgs e)
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
        public void AddNewObjects(List<Staff> newObjs)
        {
            foreach (var obj in newObjs)
            {
                var newStaffTC = CreateNewObject(obj, _bindingList.Count + 1);

                var displayedStaffTC = new DisplayedStaff_TC(newStaffTC);
                _bindingList.Add(displayedStaffTC);

                _newObjects.Add(displayedStaffTC);
            }
            dgvMain.Refresh();
        }



        /////////////////////////////////////////////// * SaveChanges * ///////////////////////////////////////////
        public async Task SaveChanges()
        {
            // todo - check if in added tech card fulfilled all required fields
            if (_changedObjects.Count == 0 && _newObjects.Count == 0 && _deletedObjects.Count == 0)
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

        private Staff_TC CreateNewObject(DisplayedStaff_TC dObj)
        {
            return new Staff_TC
            {
                ParentId = dObj.ParentId,
                ChildId = dObj.ChildId,
                Order = dObj.Order,
                Symbol = dObj.Symbol
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
            dgvMain.RowHeadersWidth = 25;

            // автоперенос в ячейках
            dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // ширина столбцов по содержанию
            var autosizeColumn = new List<string>
            {
                "Order",
                "Symbol",
                "Id",
                "Name",
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            // autosize only comment column to fill all free space
            dgvMain.Columns["Comment"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dgvMain.Columns["Functions"].Width = 180;
            dgvMain.Columns["CombineResponsibility"].Width = 140;

            dgvMain.Columns["Qualification"].Width = 250;
            dgvMain.Columns["Comment"].Width = 100;

            dgvMain.Columns["Type"].Width = 70;

            // make columns readonly
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                column.ReadOnly = true;
            }
            // make columns editable
            dgvMain.Columns["Order"].ReadOnly = false;
            dgvMain.Columns["Symbol"].ReadOnly = false;

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////// * Events handlers * /////////////////////////////////////////////////////////////////////////////////
        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            // load new form Win7_3_Staff as dictonary
            var newForm = new Win7_3_Staff(this);
            newForm.SetAsAddingForm();
            newForm.ShowDialog();
        }

        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
                _bindingList, _newObjects, _deletedObjects);
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

        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {

        }

        private void dgvMain_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            string symbolName = nameof(Staff_TC.Symbol);
            string orderName = nameof(Staff_TC.Order);

            var row = dgvMain.Rows[e.RowIndex];

            // if changed cell is in Order column then reorder dgv
            if (e.ColumnIndex == dgvMain.Columns[orderName].Index)
            {
                // check if new cell value is a number
                if (int.TryParse(row.Cells[orderName].Value.ToString(), out int orderValue))
                {
                    if (orderValue == e.RowIndex + 1) { return; }
                    MoveRowAndUpdateOrder(e.RowIndex, orderValue - 1); // move row to new position (orderValue - 1 is new index for row
                    //SortAndRefreshDGV();
                    //_bindingList = _bindingList.OrderBy(x => x.Order);

                    //if (orderValue > 0 && orderValue <= dgvMain.Rows.Count)
                    //{ DGVProcessing.ReorderRows(row, orderValue, dgvMain); }
                    //else
                    //{ DGVProcessing.ReorderRows(row, dgvMain.Rows.Count, dgvMain); } // insert row to the end of dgv

                }
                else
                {
                    MessageBox.Show("Ошибка ввода в поле \"№\"");
                    row.Cells[orderName].Value = e.RowIndex + 1;
                }
            }
            //else if (e.ColumnIndex == dgvMain.Columns[symbolName].Index) // todo: - check if symbol is unique
            //{
            //    string symbolValue = (string)row.Cells[symbolName].Value;
            //    string symbolCopyValue = (string)row.Cells[symbolName + "_copy"].Value;
            //    if (symbolValue != symbolCopyValue)
            //    {
            //        if (!DGVProcessing.CheckIfValueIsUnique(dgvMain, nameof(Staff_TC.Symbol), row))
            //        {
            //            MessageBox.Show("Символ должен быть уникальным!");
            //            row.Cells[symbolName].Value = symbolCopyValue;
            //        }
            //    }
            //}
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
        //private void UpdateOrderValues()
        //{
        //    for (int i = 0; i < dgvMain.Rows.Count; i++)
        //    {
        //        var displayedStaff = dgvMain.Rows[i].DataBoundItem as DisplayedStaff_TC;
        //        if (displayedStaff != null)
        //        {
        //            displayedStaff.Order = i + 1; // Обновляем Order на основе позиции строки
        //        }
        //    }
        //}
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
        }



        public class DisplayedStaff_TC : INotifyPropertyChanged, IIntermediateDisplayedEntity
        {
            public Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
            {
                { nameof(ChildId), "ID" },
                { nameof(ParentId), "ID тех. карты" },
                { nameof(Order), "№" },
                { nameof(Symbol), "Символ" },

                { nameof(Name), "Название" },
                { nameof(Type), "Тип" },
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
                    //nameof(ParentId),
                    nameof(Order),

                    nameof(ChildId),
                    nameof(Name),
                    nameof(Type),
                    nameof(Functions),
                    nameof(CombineResponsibility),
                    nameof(Qualification),
                    nameof(Comment),

                    nameof(Symbol),
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
                ChildId = obj.ChildId;
                ParentId = obj.ParentId;
                Order = obj.Order;
                Symbol = obj.Symbol;

                Name = obj.Child.Name;
                Type = obj.Child.Type;
                Functions = obj.Child.Functions;
                CombineResponsibility = obj.Child.CombineResponsibility;
                Qualification = obj.Child.Qualification;
                Comment = obj.Child.Comment;
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
            public string Symbol
            {
                get => symbol;
                set
                {
                    if (symbol != value)
                    {

                        oldValueDict[nameof(Symbol)] = symbol;

                        symbol = value;
                        OnPropertyChanged(nameof(Symbol));
                    }
                }
            }

            public string Name { get; set; }

            public string Type { get; set; }
            public string Functions { get; set; }
            public string? CombineResponsibility { get; set; }
            public string Qualification { get; set; }
            public string? Comment { get; set; }



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
