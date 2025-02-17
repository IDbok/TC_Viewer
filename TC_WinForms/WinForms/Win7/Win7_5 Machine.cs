﻿using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Services;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms;

public partial class Win7_5_Machine : Form, ILoadDataAsyncForm, IPaginationControl//, ISaveEventForm
{
    private readonly User.Role _accessLevel;
    private readonly int _minRowHeight = 20;
    private SelectionService<DisplayedMachine> _selectionService;

    private DbConnector dbCon = new DbConnector();

    private List<DisplayedMachine> _displayedObjects = new();
    private BindingList<DisplayedMachine> _bindingList;
    private List<Machine> _objects = new List<Machine>();
    private ConcurrencyBlockService<Machine> concurrencyBlockServise;

    private readonly bool _isAddingForm = false;
    private readonly bool _isUpdateItemMode = false; // add to UpdateMode
    private Button btnAddSelected;
    private Button btnCancel;

    public readonly bool _newItemCreateActive;
    private readonly int? _tcId;

    public bool _isDataLoaded = false;
    private bool _isFiltered = false;

    PaginationControlService<DisplayedMachine> paginationService;
    public event EventHandler<PageInfoEventArgs> PageInfoChanged;
    public PageInfoEventArgs? PageInfo { get; set; }

    public Win7_5_Machine(User.Role accessLevel)
    {
        _accessLevel = accessLevel;

        InitializeComponent();
        AccessInitialization();

    }
    public Win7_5_Machine(bool activateNewItemCreate = false, int? createdTCId = null, bool isUpdateMode = false)
    {
        _accessLevel = AuthorizationService.CurrentUser.UserRole();

        _isAddingForm = true;
        _isUpdateItemMode = isUpdateMode; // add to UpdateMode
        _tcId = createdTCId;
        _newItemCreateActive = activateNewItemCreate;

        InitializeComponent();

        dgvMain.DoubleBuffered(true);
        _selectionService = new SelectionService<DisplayedMachine>(dgvMain, _displayedObjects);

    }

    private async void Win7_5_Machine_Load(object sender, EventArgs e)
    {
        //progressBar.Visible = true;
        this.Enabled = false;
        dgvMain.Visible = false;

        if (!_isDataLoaded)
            await LoadDataAsync();

        SetDGVColumnsSettings();
        DisplayedEntityHelper.SetupDataGridView<DisplayedMachine>(dgvMain);

        dgvMain.AllowUserToDeleteRows = false;

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
    public async Task LoadDataAsync()
    {

        _displayedObjects = await Task.Run(() => dbCon.GetObjectList<Machine>(includeLinks: true)
            .Select(obj => new DisplayedMachine(obj)).OrderBy(c => c.Name).ToList());

        if (!_isAddingForm)
            paginationService = new PaginationControlService<DisplayedMachine>(30, _displayedObjects.OrderBy(c => c.Name).ToList());
        

        _selectionService = new SelectionService<DisplayedMachine>(dgvMain, _displayedObjects);

        FilteringObjects();

        _isDataLoaded = true;
    }

    private void UpdateDisplayedData()
    {
        // Расчет отображаемых записей
        if (!_isAddingForm && paginationService != null)
            _bindingList = new BindingList<DisplayedMachine>(paginationService.GetPageData());

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

            [User.Role.ProjectManager] = () => { },

            [User.Role.User] = () => { }
        };

        controlAccess.TryGetValue(_accessLevel, out var action);
        action?.Invoke();
    }
    private void HideAllButtons()
    {
        foreach (var button in pnlControlBtns.Controls.OfType<System.Windows.Forms.Button>())
        {
            button.Visible = false;
        }
    }

    private void btnAddNewObj_Click(object sender, EventArgs e)
    {
        //DisplayedEntityHelper.AddNewObjectToDGV(ref _newObject,
        //                _bindingList,
        //                _newObjects,
        //                dgvMain);
        var objEditor = new Win7_LinkObjectEditor(new Machine() { CreatedTCId = _tcId }, isNewObject: true, accessLevel: _accessLevel);

        objEditor.AfterSave = async (createdObj) => AddNewObjectInDataGridView<Machine, DisplayedMachine>(createdObj as Machine);

        objEditor.ShowDialog();
    }
    private void btnUpdate_Click(object sender, EventArgs e)
    {
        if (dgvMain.SelectedRows.Count != 1)
        {
            MessageBox.Show("Выберите одну строку для редактирования");
            return;
        }

        var selectedObj = dgvMain.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
        var obj = selectedObj?.DataBoundItem as DisplayedMachine;

        if (obj != null)
        {
            var machine = dbCon.GetObjectWithLinks<Machine>(obj.Id);

            if (machine != null)
            {
                var timerInterval = 1000 * 60 * 25;
                concurrencyBlockServise = new ConcurrencyBlockService<Machine>(machine, timerInterval);
                if (concurrencyBlockServise.GetObjectUsedStatus())
                {
                    MessageBox.Show("Данный объект сейчас редактируется другим пользователем. Вы не можете его редактировать.", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var objEditor = new Win7_LinkObjectEditor(machine, accessLevel: _accessLevel);

                objEditor.AfterSave = async (updatedObj) => UpdateObjectInDataGridView<Machine, DisplayedMachine>(updatedObj as Machine);

                objEditor.ShowDialog();
            }
        }
    }

    private async void btnDeleteObj_Click(object sender, EventArgs e)
    {
        await DisplayedEntityHelper.DeleteSelectedObjectWithLinks<DisplayedMachine, Machine>(dgvMain,
            _bindingList, _displayedObjects);

        FilteringObjects();

    }
    /////////////////////////////////////////////// * SaveChanges * ///////////////////////////////////////////

    private Machine CreateNewObject(DisplayedMachine dObj)
    {
        return new Machine
        {
            Id = dObj.Id,
            Name = dObj.Name,
            Type = dObj.Type,
            Unit = dObj.Unit,
            Price = dObj.Price,
            Description = dObj.Description,
            Manufacturer = dObj.Manufacturer,
            Links = dObj.Links,
            ClassifierCode = dObj.ClassifierCode,

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
        dgvMain.RowHeadersWidth = 25;

        //// автоперенос в ячейках
        dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        //    // ширина столбцов по содержанию
        var autosizeColumn = new List<string>
        {
            nameof(DisplayedMachine.Id),
            nameof(DisplayedMachine.Unit),
            nameof(DisplayedMachine.Type),
            //nameof(DisplayedMachine.ClassifierCode),
        };
        foreach (var column in autosizeColumn)
        {
            dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        }

        dgvMain.Columns[nameof(DisplayedMachine.Price)].Width = 120;
        dgvMain.Columns[nameof(DisplayedMachine.ClassifierCode)].Width = 150;
        dgvMain.Columns[nameof(DisplayedMachine.ClassifierCode)].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        dgvMain.Columns[nameof(DisplayedMachine.LinkNames)].Width = 100;
        dgvMain.Columns[nameof(DisplayedMachine.LinkNames)].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
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
        if (_isUpdateItemMode) // add to UpdateMode
        {
            UpdateItem();
        }
        else
        {
            AddSelectedItems();
        }
    }
    void BtnCancel_Click(object sender, EventArgs e)
    {
        // close form
        this.Close();
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
        var tcEditor = Application.OpenForms.OfType<Win6_Machine>().FirstOrDefault();

        tcEditor.UpdateSelectedObject(CreateNewObject(selectedObjs[0]));

        // close form
        this.Close();
    }

    void AddSelectedItems()
    {
        // get selected objects
        var selectedObjs = _selectionService.GetSelectedObjects(); //selectedRows.Select(r => r.DataBoundItem as DisplayedMachine).ToList();
        if (selectedObjs.Count == 0)
        {
            MessageBox.Show("Выберите строки для добавления");
            return;
        }
        var newItems = new List<Machine>();
        foreach (var obj in selectedObjs)
        {
            newItems.Add(CreateNewObject(obj));
        }
        // find opened form
        var tcEditor = CheckOpenFormService.FindOpenedForm<Win6_Machine>((int)_tcId);

        tcEditor.AddNewObjects(newItems);

        // close form
        this.Close();
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private class DisplayedMachine : INotifyPropertyChanged, IDisplayedEntity, IModelStructure
    {
        public Dictionary<string, string> GetPropertiesNames()
        {
            return new Dictionary<string, string>
        {
            { nameof(Id), "ID" },
            { nameof(Name), "Наименование" },
            { nameof(Type), "Тип (исполнение)" },
            { nameof(Unit), "Ед.изм." },
            { nameof(ClassifierCode), "Код в classifier" },
            //{ nameof(Price), "Стоимость, руб. без НДС" },
            { nameof(Description), "Описание" },
            { nameof(Manufacturer), "Производитель" },
            //{ nameof(Links), "Ссылки" },
            { nameof(LinkNames), "Ссылка" },
        };
        }
        public List<string> GetPropertiesOrder()
        {
            return new List<string>
            {
                nameof(Id),
                nameof(Name),
                nameof(Type),
                nameof(Unit),
                nameof(ClassifierCode),
                //nameof(Price),
                nameof(Description),
                nameof(Manufacturer),
                nameof(LinkNames),
            };
        }
        public List<string> GetRequiredFields()
        {
            return new List<string>
            {
                nameof(Name) ,
                nameof(Type) ,
                nameof(Unit) ,
                nameof(ClassifierCode) ,
            };
        }

        private int id;
        private string name;
        private string? type;
        private string unit;
        private float? price;
        private string? description;
        private string? manufacturer;
        private List<LinkEntety> links = new();
        private string classifierCode;

        private bool isReleased;
        private int? createdTCId;

        private string GetDefaultLinkOrFirst()
        {
            if (links.Count > 0)
            {
                // Все названия существующих ссылок с новой строки
                //var linksNames = string.Join("\n", links.Select(l => l.Name).ToList());
                var defLink = links.Where(l => l.IsDefault).FirstOrDefault();
                if (defLink == null)
                {
                    return links[0].Name ?? links[0].Link;
                }
                //defLink.Name = linksNames;
                return defLink.Name ?? defLink.Link;
            }
            return string.Empty;
        }
        public DisplayedMachine()
        {

        }
        public DisplayedMachine(Machine obj)
        {
            Id = obj.Id;
            Name = obj.Name;
            Type = obj.Type;
            Unit = obj.Unit;
            Price = obj.Price;
            Description = obj.Description;
            Manufacturer = obj.Manufacturer;
            Links = obj.Links;
            ClassifierCode = obj.ClassifierCode;

            IsReleased = obj.IsReleased;
            CreatedTCId = obj.CreatedTCId;
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
        public string Unit
        {
            get => unit;
            set
            {
                if (unit != value)
                {
                    unit = value;
                    OnPropertyChanged(nameof(unit));
                }
            }
        }
        public float? Price
        {
            get => price;
            set
            {
                if (price != value)
                {
                    price = value;
                    OnPropertyChanged(nameof(Price));
                }
            }
        }
        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }
        public string Manufacturer
        {
            get => manufacturer ?? string.Empty;
            set
            {
                if (manufacturer != value)
                {
                    manufacturer = value;
                    OnPropertyChanged(nameof(Manufacturer));
                }
            }
        }
        public string LinkNames
        {
            get => GetDefaultLinkOrFirst();
        }

        public List<LinkEntety> Links
        {
            get => links;
            set
            {
                if (links != value)
                {
                    links = value;
                    OnPropertyChanged(nameof(Links));
                }
            }
        }
        public string ClassifierCode
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
            var displayedMachineList = new BindingList<DisplayedMachine>();

            if (string.IsNullOrWhiteSpace(searchText) && !cbxShowUnReleased.Checked)
            {
                displayedMachineList = new BindingList<DisplayedMachine>(_displayedObjects.Where(obj => obj.IsReleased == true).ToList());
                _isFiltered = false;
            }
            else
            {
                displayedMachineList = FilteredBindingList(searchText);
                _isFiltered = true;
            }

            if (!_isAddingForm && paginationService != null)
                paginationService.SetAllObjectList(displayedMachineList.ToList());
            else
                _bindingList = displayedMachineList;

            UpdateDisplayedData();

            if (_isAddingForm)
                _selectionService.RestoreSelectedIds();
        }
        catch (Exception e)
        {
            //MessageBox.Show(e.Message);
        }

    }
    private BindingList<DisplayedMachine> FilteredBindingList(string searchText)
    {
        var filteredList = _displayedObjects.Where(obj =>
                    (searchText == ""
                        ||
                        (obj.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.Type?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.Unit?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        //(obj.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        //(obj.Categoty?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.ClassifierCode?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                    ) &&
                    (obj.IsReleased == !cbxShowUnReleased.Checked)
                    //&&
                    //(!_isAddingForm ||
                    //    (!cbxShowUnReleased.Checked ||
                    //    (cbxShowUnReleased.Checked &&
                    //    (obj.CreatedTCId == null || obj.CreatedTCId == _tcId)))
                    //)
                    ).ToList();

        return new BindingList<DisplayedMachine>(filteredList);
    }
    private void dgvMain_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        UrlClick(sender, e);
    }
    private void UrlClick(object sender, DataGridViewCellEventArgs e)
    {
        if (dgvMain.Columns[e.ColumnIndex].Name == nameof(DisplayedMachine.LinkNames) && e.RowIndex >= 0)
        {
            var displayedMachine = dgvMain.Rows[e.RowIndex].DataBoundItem as DisplayedMachine;
            if (displayedMachine != null)
            {
                var link = displayedMachine.Links.FirstOrDefault(l => l.IsDefault) ?? displayedMachine.Links.FirstOrDefault();
                if (link != null && Uri.TryCreate(link.Link, UriKind.Absolute, out var uri))
                {
                    var result = MessageBox.Show($"Открыть ссылку в браузере?\n{link}", "Ссылка", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
                    }
                }
            }
        }
    }


    public void UpdateObjectInDataGridView<TModel, TDisplayed>(TModel modelObject)
                where TModel : IModelStructure
                where TDisplayed : class, IModelStructure
    {
        // Обновляем объект в DataGridView
        var displayedObject = _displayedObjects.OfType<TDisplayed>().FirstOrDefault(obj => obj.Id == modelObject.Id);
        if (displayedObject != null)
        {
            displayedObject.Name = modelObject.Name;
            displayedObject.Type = modelObject.Type;
            displayedObject.Unit = modelObject.Unit;
            displayedObject.Price = modelObject.Price;
            displayedObject.Description = modelObject.Description;
            displayedObject.Manufacturer = modelObject.Manufacturer;
            displayedObject.Links = modelObject.Links;
            displayedObject.ClassifierCode = modelObject.ClassifierCode;

            dgvMain.Refresh();

            displayedObject.IsReleased = modelObject.IsReleased;

            FilteringObjects();
        }

    }

    public void AddNewObjectInDataGridView<TModel, TDisplayed>(TModel modelObject)
        where TModel : IModelStructure
        where TDisplayed : class, IModelStructure
    {

        var newDisplayedObject = Activator.CreateInstance<TDisplayed>();
        if (newDisplayedObject is DisplayedMachine displayedObject)
        {
            displayedObject.Id = modelObject.Id;
            displayedObject.Name = modelObject.Name;
            displayedObject.Type = modelObject.Type;
            displayedObject.Unit = modelObject.Unit;
            displayedObject.Price = modelObject.Price;
            displayedObject.Description = modelObject.Description;
            displayedObject.Manufacturer = modelObject.Manufacturer;
            displayedObject.Links = modelObject.Links;
            displayedObject.ClassifierCode = modelObject.ClassifierCode;
            displayedObject.IsReleased = modelObject.IsReleased;
            if (displayedObject is ICategoryable objectWithCategory && modelObject is ICategoryable modelWithCategory)
            {
                objectWithCategory.Categoty = modelWithCategory.Categoty;
            }
            // добавляем в список всех объектов новый объект
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

    private void Win7_5_Machine_SizeChanged(object sender, EventArgs e)
    {
        dgvMain.ResizeRows(_minRowHeight);
    }
}
