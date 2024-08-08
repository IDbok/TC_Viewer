using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms;

public partial class Win7_3_Staff : Form, ILoadDataAsyncForm//, ISaveEventForm
{
    private readonly User.Role _accessLevel;

    private DbConnector dbCon = new DbConnector();

    private List<DisplayedStaff> _displayedObjects;
    private static BindingList<DisplayedStaff> _bindingList;

    private bool _isAddingForm = false;
    private Button btnAddSelected;
    private Button btnCancel;
    private Form _openedForm;
    public readonly bool _newItemCreateActive;
    private readonly int? _tcId;

    public bool isDataLoaded = false;

    private bool isFiltered = false;

    public void SetAsAddingForm()
    {
        _isAddingForm = true;
    }

    //public bool CloseFormsNoSave { get; set; } = false;

    //public bool GetDontSaveData()
    //{
    //    if (_newObjects.Count + _changedObjects.Count + _deletedObjects.Count != 0)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}
    public Win7_3_Staff(User.Role accessLevel)
    {
        _accessLevel = accessLevel;

        InitializeComponent();
        AccessInitialization();

        //this.Enabled = false;
        //dgvMain.Visible = false;
    }

    public Win7_3_Staff(Form openedForm, bool activateNewItemCreate = false, int? createdTCId = null) // this constructor is for adding form in TC editer
    {
        _accessLevel = AuthorizationService.CurrentUser.UserRole();

        _openedForm = openedForm;
        _isAddingForm = true;
        _newItemCreateActive = activateNewItemCreate;
        _tcId = createdTCId;
        InitializeComponent();

    }

    private async void Win7_3_Staff_Load(object sender, EventArgs e)
    {
        //progressBar.Visible = true;
        this.Enabled = false;
        dgvMain.Visible = false;

        if (!isDataLoaded)
            await LoadDataAsync();

        SetDGVColumnsSettings();

        //DisplayedEntityHelper.SetupDataGridView<DisplayedStaff>(dgvMain);

        //dgvMain.AllowUserToDeleteRows = false; // TODO: change it when add deleting by "del" Key press

        if (_isAddingForm)
        {

            //isAddingFormSetControls();
            WinProcessing.SetAddingFormControls(pnlControlBtns, dgvMain,
                out btnAddSelected, out btnCancel);
            //////////////////////////////////////////////////////////////////////////////////////
            if (_newItemCreateActive)
            {
                btnAddNewObj.Visible = true;
                btnAddNewObj.Dock = DockStyle.Right;
                btnAddNewObj.Text = "Создать новый объект";
            }
            //////////////////////////////////////////////////////////////////////////////////////////

            SetAddingFormEvents();
        }

        dgvMain.Visible = true;
        this.Enabled = true;
        //progressBar.Visible = false;
    }
    public async Task LoadDataAsync()
    {
        _displayedObjects = await Task.Run(() => dbCon.GetObjectList<Staff>(includeRelatedStaffs: true) //.Where(obj => obj.IsReleased == true)
            .Select(obj => new DisplayedStaff(obj)).ToList());

        FilteringObjects();

        isDataLoaded = true;
    }
    private void AccessInitialization()
    {
        //var controlAccess = new Dictionary<int, Action>
        //{
        //    [0] = () => {  },
        //    [1] = () => {  },
        //    [2] = () => {  },
        //};
        //controlAccess.TryGetValue(accessLevel, out var action);
        //action?.Invoke();
    }

    private async void Win7_3_Staff_FormClosing(object sender, FormClosingEventArgs e)
    {
    }

    private void btnAddNewObj_Click(object sender, EventArgs e)
    {
        var objEditor = new Win7_StaffEditor(new Staff() { CreatedTCId = _tcId }, isNewObject: true, accessLevel: _accessLevel);

        objEditor.AfterSave = async (createdObj) => AddNewObjectInDataGridView(createdObj);

        objEditor.ShowDialog();
    }

    private async void btnDeleteObj_Click(object sender, EventArgs e)
    {
        await DisplayedEntityHelper.DeleteSelectedObject<DisplayedStaff, Staff>(dgvMain,
        _bindingList, isFiltered ? _displayedObjects : null);
    }

    private Staff CreateNewObject(DisplayedStaff dObj)
    {
        return new Staff
        {
            Id = dObj.Id,
            Name = dObj.Name,
            Type = dObj.Type,
            Functions = dObj.Functions,
            CombineResponsibility = dObj.CombineResponsibility,
            Qualification = dObj.Qualification,
            Comment = dObj.Comment,

            IsReleased = dObj.IsReleased,
            CreatedTCId = dObj.CreatedTCId,
        };
    }

    ////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////

    void SetDGVColumnsSettings()
    {
        // автоподбор ширины столбцов под ширину таблицы
        dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        //dgvMain.RowHeadersWidth = 25;
        dgvMain.RowHeadersVisible = false;

        //// автоперенос в ячейках
        dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        //    // ширина столбцов по содержанию
        var autosizeColumn = new List<string>
        {
            nameof(DisplayedStaff.Id),
            //nameof(DisplayedStaff.Name),
            //nameof(DisplayedStaff.Type),
            //nameof(DisplayedStaff.Functions),
            //nameof(DisplayedStaff.CombineResponsibility),
            //nameof(DisplayedStaff.Qualification),
            //nameof(DisplayedStaff.Comment),
        };
        foreach (var column in autosizeColumn)
        {
            dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        }

        dgvMain.Columns[nameof(DisplayedStaff.Name)].Width = 80;
        dgvMain.Columns[nameof(DisplayedStaff.ClassifierCode)].Width = 30;
        dgvMain.Columns[nameof(DisplayedStaff.Type)].Width = 80;
        dgvMain.Columns[nameof(DisplayedStaff.Functions)].Width = 100;
        dgvMain.Columns[nameof(DisplayedStaff.CombineResponsibility)].Width = 90;
        dgvMain.Columns[nameof(DisplayedStaff.Qualification)].Width = 200;

    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////// * isAddingFormMethods and Events * ///////////////////////////////////////////

    void SetAddingFormEvents()
    {
        btnAddSelected.Click += BtnAddSelected_Click;
        btnCancel.Click += BtnCancel_Click;
    }

    void BtnAddSelected_Click(object sender, EventArgs e)
    {
        // get selected rows
        var selectedRows = dgvMain.Rows.Cast<DataGridViewRow>().Where(r => Convert.ToBoolean(r.Cells["Selected"].Value) == true).ToList();
        if (selectedRows.Count == 0)
        {
            MessageBox.Show("Выберите строки для добавления");
            return;
        }


        // get selected objects
        var selectedObjs = selectedRows.Select(r => r.DataBoundItem as DisplayedStaff).ToList();
        var newItems = new List<Staff>();
        foreach (var obj in selectedObjs)
        {
            newItems.Add(CreateNewObject(obj));
        }
        // find opened form
        if (_openedForm is Win6_Staff tcEditor)
        {
            tcEditor.AddNewObjects(newItems);
        }

        if (_openedForm is Win7_StaffEditor editor2)
        {
            // todo: добавить проверку на выпущенность объекта перед добавлением
            editor2.AddNewObjects(newItems);
        }

        Close();
    }
    void BtnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
    //{
    //    DisplayedEntityHelper.ListChangedEventHandler
    //        (e, _bindingList, _newObjects, _changedObjects, ref _newObject);
    //}

    private class DisplayedStaff : INotifyPropertyChanged, IDisplayedEntity, IIdentifiable
    {
        public Dictionary<string, string> GetPropertiesNames()
        {
            return new Dictionary<string, string>
        {
            { nameof(Id), "ID" },
            { nameof(Name), "Наименование" },
            { nameof(Type), "Тип (исполнение)" },
            { nameof(ClassifierCode), "Код в classifier" },
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
                nameof(Id),
                nameof(Name),
                nameof(Type),
                nameof(ClassifierCode),
                nameof(Functions),
                nameof(CombineResponsibility),
                nameof(Qualification),
                nameof(Comment),
            };
        }
        public List<string> GetRequiredFields()
        {
            return new List<string>
            {
                nameof(Name) ,
                nameof(Type) ,
                nameof(Functions) ,
                nameof(Qualification),
                nameof(ClassifierCode),
            };
        }

        private int id;
        private string name;
        private string type;
        private string functions;
        private string? combineResponsibility;
        private string qualification;
        private string? comment;

        private bool isReleased;

        private int? createdTCId;


        //private List<DisplayedStaff> relatedStaffs;
        private string? classifierCode; 

        public DisplayedStaff()
        {

        }
        public DisplayedStaff(Staff obj)
        {
            Id = obj.Id;
            Name = obj.Name;
            Type = obj.Type;
            ClassifierCode = obj.ClassifierCode;
            Functions = obj.Functions;
            CombineResponsibility = obj.CombineResponsibility;
            Qualification = obj.Qualification;
            Comment = obj.Comment;
            IsReleased = obj.IsReleased;

            CreatedTCId = obj.CreatedTCId;
            //relatedStaffs = obj.RelatedStaffs.Select(st => new DisplayedStaff(st)).ToList();
        }

        public int Id { get; set; }

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Type
        {
            get => type;
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }
        public string? ClassifierCode
        {
            get => classifierCode;
            set
            {
                if (classifierCode != value)
                {
                    classifierCode = value;
                    OnPropertyChanged(nameof(ClassifierCode));
                }
            }
        }
        public string Functions
        {
            get => functions;
            set
            {
                if (functions != value)
                {
                    functions = value;
                    OnPropertyChanged(nameof(functions));
                }
            }
        }
        public string? CombineResponsibility
        {
            get => combineResponsibility;
            set
            {
                if (combineResponsibility != value)
                {
                    combineResponsibility = value;
                    OnPropertyChanged(nameof(CombineResponsibility));
                }
            }
        }
        public string Qualification
        {
            get => qualification;
            set
            {
                if (qualification != value)
                {
                    qualification = value;
                    OnPropertyChanged(nameof(Qualification));
                }
            }
        }
        public string? Comment
        {
            get => comment;
            set
            {
                if (comment != value)
                {
                    comment = value;
                    OnPropertyChanged(nameof(Comment));
                }
            }
        }
        public bool IsReleased
        {
            get => isReleased;
            set
            {
                if (isReleased != value)
                {
                    isReleased = value;
                    OnPropertyChanged(nameof(IsReleased));
                }
            }
        }
        public int? CreatedTCId
        {
            get => createdTCId;
            set
            {
                if (createdTCId != value)
                {
                    createdTCId = value;
                    OnPropertyChanged(nameof(CreatedTCId));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    private void txtSearch_TextChanged(object sender, EventArgs e)
    {
        FilteringObjects();
    }
    private void FilteringObjects()
    {
        try
        {
            var searchText = txtSearch.Text == "Поиск" ? "" : txtSearch.Text;

            //dgvMain.DataSource = null;

            if (string.IsNullOrWhiteSpace(searchText) && !cbxShowUnReleased.Checked)
            {
                _bindingList = new BindingList<DisplayedStaff>(_displayedObjects.Where(obj => obj.IsReleased == true).ToList());
                //dgvMain.DataSource = _bindingList; // Возвращаем исходный список, если строка поиска пуста
                isFiltered = false;
            }
            else
            {
                var filteredList = _displayedObjects.Where(obj =>
                        ((obj.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.Type?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.Functions?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.CombineResponsibility?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.Qualification?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.Comment?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                        (obj.IsReleased == !cbxShowUnReleased.Checked) 
                        //&&
                        //(!_isAddingForm ||
                        //    (!cbxShowUnReleased.Checked ||
                        //    (cbxShowUnReleased.Checked &&
                        //    (obj.CreatedTCId == null || obj.CreatedTCId == _tcId)))
                        //)
                        ).ToList();

                _bindingList = new BindingList<DisplayedStaff>(filteredList);
                // dgvMain.DataSource = new BindingList<DisplayedStaff>(filteredList);
                isFiltered = true;
            }

            dgvMain.DataSource = _bindingList;

            DisplayedEntityHelper.SetupDataGridView<DisplayedStaff>(dgvMain);

        }
        catch (Exception e)
        {
            //MessageBox.Show(e.Message);
        }

    }
    private void btnUpdate_Click(object sender, EventArgs e)
    {
        if (dgvMain.SelectedRows.Count != 1)
        {
            MessageBox.Show("Выберите одну строку для редактирования");
            return;
        }

        var selectedObj = dgvMain.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
        var obj = selectedObj?.DataBoundItem as DisplayedStaff;

        if (obj != null)
        {
            var staff = dbCon.GetObject<Staff>(obj.Id);

            if (staff != null)
            {
                var objEditor = new Win7_StaffEditor(staff, accessLevel: _accessLevel);

                objEditor.AfterSave = async (updatedObj) => UpdateObjectInDataGridView(updatedObj as Staff);

                objEditor.ShowDialog();
            }
        }
    }

    public void UpdateObjectInDataGridView(Staff modelObject)
    {
        // Обновляем объект в DataGridView
        var editedObject = _displayedObjects.FirstOrDefault(obj => obj.Id == modelObject.Id);
        if (editedObject != null)
        {
            editedObject.Name = modelObject.Name;
            editedObject.Type = modelObject.Type;
            editedObject.Functions = modelObject.Functions;
            editedObject.CombineResponsibility = modelObject.CombineResponsibility;
            editedObject.Qualification = modelObject.Qualification;
            editedObject.Comment = modelObject.Comment;

            editedObject.IsReleased = modelObject.IsReleased;
            editedObject.ClassifierCode = modelObject.ClassifierCode;

            FilteringObjects();
        }

    }

    public void AddNewObjectInDataGridView(Staff modelObject)
    {
        var newDisplayedObject = Activator.CreateInstance<DisplayedStaff>();
        if (newDisplayedObject is DisplayedStaff displayedObject)
        {
            displayedObject.Id = modelObject.Id;
            displayedObject.Name = modelObject.Name;
            displayedObject.Type = modelObject.Type;
            displayedObject.Functions = modelObject.Functions;
            displayedObject.CombineResponsibility = modelObject.CombineResponsibility;
            displayedObject.Qualification = modelObject.Qualification;
            displayedObject.Comment = modelObject.Comment;

            displayedObject.IsReleased = modelObject.IsReleased;
            displayedObject.ClassifierCode = modelObject.ClassifierCode;

            _displayedObjects.Insert(0, displayedObject);
            FilteringObjects();

        }
    }


    private void cbxShowUnReleased_CheckedChanged(object sender, EventArgs e)
    {
        FilteringObjects();
    }
}
