using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms;

public partial class Win6_Staff : Form, ISaveEventForm
{

    private DbConnector dbCon = new DbConnector();
    private BindingList<DisplayedStaff_TC> _bindingList;

    private List<DisplayedStaff_TC> _changedObjects = new List<DisplayedStaff_TC>();
    private List<DisplayedStaff_TC> _newObjects = new List<DisplayedStaff_TC>();
    private List<DisplayedStaff_TC> _deletedObjects = new List<DisplayedStaff_TC>();

    private int _tcId;
    public bool CloseFormsNoSave { get; set; } = false;
    public Win6_Staff(int tcId)
    {
        InitializeComponent();
        this._tcId = tcId;

        // new DGVEvents().AddGragDropEvents(dgvMain);
        new DGVEvents().SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);
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
    private async void Win6_Staff_Load(object sender, EventArgs e)
    {
        await LoadObjects();

        dgvMain.AllowUserToDeleteRows = false;
    }
    private async Task LoadObjects()
    {
        var tcList = await Task.Run(() => dbCon.GetIntermediateObjectList<Staff_TC, Staff>(_tcId).OrderBy(obj => obj.Order)
            .Select(obj => new DisplayedStaff_TC(obj)).ToList());
        _bindingList = new BindingList<DisplayedStaff_TC>(tcList);
        _bindingList.ListChanged += BindingList_ListChanged;
        dgvMain.DataSource = _bindingList;

        SetDGVColumnsSettings();
    }
    private async void Win6_Staff_FormClosing(object sender, FormClosingEventArgs e)
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

    private Staff_TC CreateNewObject(DisplayedStaff_TC dObj)
    {
        return new Staff_TC
        {
            IdAuto = dObj.IdAuto,
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
        dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvMain.RowHeadersWidth = 25;

        //// автоперенос в ячейках
        dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        // ширина столбцов по содержанию
        //var autosizeColumn = new List<string>
        //{
        //    //nameof(DisplayedStaff_TC.Order),
        //    //nameof(DisplayedStaff_TC.Name),
        //    //nameof(DisplayedStaff_TC.Type),
        //    //nameof(DisplayedStaff_TC.CombineResponsibility),
        //    //nameof(DisplayedStaff_TC.Symbol),
        //    //nameof(DisplayedStaff_TC.ChildId),
        //};
        //foreach (var column in autosizeColumn)
        //{
        //    dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        //}
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
        // make columns editable
        dgvMain.Columns[nameof(DisplayedStaff_TC.Order)].ReadOnly = false;
        dgvMain.Columns[nameof(DisplayedStaff_TC.Symbol)].ReadOnly = false;

        dgvMain.Columns[nameof(DisplayedStaff_TC.Order)].DefaultCellStyle.BackColor = Color.LightGray;
        dgvMain.Columns[nameof(DisplayedStaff_TC.Symbol)].DefaultCellStyle.BackColor = Color.LightGray;

        DisplayedEntityHelper.SetupDataGridView<DisplayedStaff_TC>(dgvMain);

    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    ///////////////////////////////////////////////////// * Events handlers * /////////////////////////////////////////////////////////////////////////////////
    private void btnAddNewObj_Click(object sender, EventArgs e)
    {
        // load new form Win7_3_Staff as dictonary
        var newForm = new Win7_3_Staff(this, activateNewItemCreate: true, createdTCId: _tcId);
        //newForm.SetAsAddingForm();
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
    }



    public class DisplayedStaff_TC : INotifyPropertyChanged, IIntermediateDisplayedEntity, IOrderable
    {
        public Dictionary<string, string> GetPropertiesNames()
        {
            return new Dictionary<string, string>
        {
            { nameof(ChildId), "ID" },
            { nameof(ParentId), "ID тех. карты" },
            { nameof(Order), "№" },
            { nameof(Symbol), "Обозначение" },

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

            Name = obj.Child.Name;
            Type = obj.Child.Type;
            Functions = obj.Child.Functions;
            CombineResponsibility = obj.Child.CombineResponsibility;
            Qualification = obj.Child.Qualification;
            Comment = obj.Child.Comment;
        }

        public int IdAuto { get; set; }
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
