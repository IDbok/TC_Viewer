using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Services;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms;

public partial class Win7_3_Staff : Form, ILoadDataAsyncForm, IPaginationControl//, ISaveEventForm
{
    private readonly User.Role _accessLevel;
    private readonly int _minRowHeight = 20;
    private DbConnector dbCon = new DbConnector();

    private SelectionService<DisplayedStaff> _selectionService;
    private ConcurrencyBlockService<Staff> concurrencyBlockServise;

    private List<DisplayedStaff> _displayedObjects = new();
    private static BindingList<DisplayedStaff> _bindingList;

    private bool _isAddingForm = false;
    private readonly bool _isUpdateItemMode = false; // add to UpdateMode

    private Button btnAddSelected;
    private Button btnCancel;
    private Form _openedForm;
    public readonly bool _newItemCreateActive;
    private readonly int? _tcId;

    public bool isDataLoaded = false;

    private bool isFiltered = false;

    PaginationControlService<DisplayedStaff> paginationService;
    public event EventHandler<PageInfoEventArgs> PageInfoChanged;
    public PageInfoEventArgs? PageInfo { get; set; }

    //private List<int> _selectedIds = new List<int>();

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
        dgvMain.DoubleBuffered(true);
        //this.Enabled = false;
        //dgvMain.Visible = false;
    }

    public Win7_3_Staff(Form openedForm, bool activateNewItemCreate = false, int? createdTCId = null, bool isUpdateMode = false) // this constructor is for adding form in TC editer
    {
        _accessLevel = AuthorizationService.CurrentUser.UserRole();

        _openedForm = openedForm;
        _isAddingForm = true;
        _isUpdateItemMode = isUpdateMode; // add to UpdateMode

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
            if (_isUpdateItemMode)// add to UpdateMode
            {
                btnAddSelected.Text = "Обновить";
            }

            SetAddingFormEvents();

        }
        dgvMain.ResizeRows(_minRowHeight);

        dgvMain.Visible = true;
        this.Enabled = true;
        //progressBar.Visible = false;
    }
    private void HideAllButtons()
    {
        foreach (var button in pnlControlBtns.Controls.OfType<System.Windows.Forms.Button>())
        {
            button.Visible = false;
        }
    }
    public async Task LoadDataAsync()
    {
        _displayedObjects = await Task.Run(() => dbCon.GetObjectList<Staff>(includeRelatedStaffs: true) //.Where(obj => obj.IsReleased == true)
            .Select(obj => new DisplayedStaff(obj)).OrderBy(c => c.Name).ToList());

        if (!_isAddingForm)
            paginationService = new PaginationControlService<DisplayedStaff>(15, _displayedObjects);


        _selectionService = new SelectionService<DisplayedStaff>(dgvMain, _displayedObjects);

        FilteringObjects();

        isDataLoaded = true;
    }
    private void UpdateDisplayedData()
    {
        // Расчет отображаемых записей
        if (!_isAddingForm && paginationService != null)
            _bindingList = new BindingList<DisplayedStaff>(paginationService.GetPageData());

        dgvMain.DataSource = _bindingList;
        dgvMain.ResizeRows(_minRowHeight);

        if (!_isAddingForm && paginationService != null)
        { 
            // Подготовка данных для события
            PageInfo = paginationService.GetPageInfo();
            // Вызов события с подготовленными данными
            RaisePageInfoChanged();
        }
    }

    private void AccessInitialization()
    {
        var controlAccess = new Dictionary<User.Role, Action>
        {
            [User.Role.Lead] = () => { },

            [User.Role.Implementer] = () =>
            {
                HideAllButtons();
            },

            [User.Role.ProjectManager] = () => {},

            [User.Role.User] = () => {}
        };

        controlAccess.TryGetValue(_accessLevel, out var action);
        action?.Invoke();
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
        _bindingList, _displayedObjects);

        FilteringObjects();

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
        //dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        //dgvMain.RowHeadersWidth = 25;
        dgvMain.RowHeadersVisible = false;

        //// автоперенос в ячейках
        dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        //    // ширина столбцов по содержанию

        //var autosizeColumn = new List<string>
        //{
        //    nameof(DisplayedStaff.Id),
        //    nameof(DisplayedStaff.Name),
        //    nameof(DisplayedStaff.Type),
        //    nameof(DisplayedStaff.Functions),
        //    nameof(DisplayedStaff.CombineResponsibility),
        //    nameof(DisplayedStaff.Qualification),
        //    nameof(DisplayedStaff.Comment),
        //};
        //foreach (var column in autosizeColumn)
        //{
        //    dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        //}

        //dgvMain.Columns[nameof(DisplayedStaff.Name)].Width = 200;
        //dgvMain.Columns[nameof(DisplayedStaff.ClassifierCode)].Width = 150;
        //dgvMain.Columns[nameof(DisplayedStaff.Type)].Width = 200;
        //dgvMain.Columns[nameof(DisplayedStaff.Functions)].Width = 290;
        //dgvMain.Columns[nameof(DisplayedStaff.CombineResponsibility)].Width = 290;

        dgvMain.Columns[nameof(DisplayedStaff.Qualification)].Width = 200;
        dgvMain.Columns[nameof(DisplayedStaff.Comment)].Width = 290;


    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////// * isAddingFormMethods and Events * ///////////////////////////////////////////

    void SetAddingFormEvents()
    {
        btnAddSelected.Click += BtnAddSelected_Click;
        btnCancel.Click += BtnCancel_Click;

        dgvMain.CellValueChanged += _selectionService.CellValueChanged;
        dgvMain.CurrentCellDirtyStateChanged += _selectionService.CurrentCellDirtyStateChanged;
    }

    void BtnAddSelected_Click(object sender, EventArgs e)
    {
        if (_isUpdateItemMode)// add to UpdateMode
        {
            UpdateItem();
            return;
        }

        // выделить объекты с id из _selectedIds из списка _displayedObjects
        var selectedObjs = _selectionService.GetSelectedObjects();//_displayedObjects.Where(obj => _selectedIds.Contains(obj.Id)).ToList();

        if (selectedObjs.Count == 0)
        {
            MessageBox.Show("Выберите строки для добавления");
            return;
        }
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

    void UpdateItem()
    {
        var selectedObjs = _selectionService.GetSelectedObjects();

        if (selectedObjs.Count != 1)
        {
            MessageBox.Show("Выберите одну строку для обновления");
            return;
        }

        // find opened form
        var tcEditor = Application.OpenForms.OfType<Win6_Staff>().FirstOrDefault();

        tcEditor.UpdateSelectedObject(CreateNewObject(selectedObjs[0]));

        // close form
        this.Close();
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
            var displayedStaffList = new BindingList<DisplayedStaff>();

            if (string.IsNullOrWhiteSpace(searchText) && !cbxShowUnReleased.Checked)
            {
                displayedStaffList = new BindingList<DisplayedStaff>(_displayedObjects.Where(obj => obj.IsReleased == true).ToList());
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
                        ).ToList();

                displayedStaffList = new BindingList<DisplayedStaff>(filteredList);
                isFiltered = true;

            }

            if(!_isAddingForm && paginationService != null)
                paginationService.SetAllObjectList(displayedStaffList.ToList());
            else
                _bindingList = displayedStaffList;

            UpdateDisplayedData();

            //dgvMain.DataSource = _bindingList;

            DisplayedEntityHelper.SetupDataGridView<DisplayedStaff>(dgvMain);

            // Восстанавливаем выделенные объекты
            if (_isAddingForm)
                _selectionService.RestoreSelectedIds();

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
                var timerInterval = 1000 * 60 * 25;
                concurrencyBlockServise = new ConcurrencyBlockService<Staff>(staff, timerInterval);
                if (concurrencyBlockServise.GetObjectUsedStatus())
                {
                    MessageBox.Show("Данный объект сейчас редактируется другим пользователем. Вы не можете его редактировать.", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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

    public void GoToNextPage()
    {
        paginationService.GoToNextPage();
        UpdateDisplayedData();
    }

    public void GoToPreviousPage()
    {
        paginationService.GoToPreviousPage();
        UpdateDisplayedData();
    }

    public void RaisePageInfoChanged()
    {
        PageInfoChanged?.Invoke(this, PageInfo);
    }

    private void Win7_3_Staff_SizeChanged(object sender, EventArgs e)
    {
        dgvMain.ResizeRows(_minRowHeight);
    }
}
