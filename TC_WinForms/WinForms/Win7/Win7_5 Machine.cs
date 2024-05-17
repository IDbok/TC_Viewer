using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms;

public partial class Win7_5_Machine : Form, ILoadDataAsyncForm//, ISaveEventForm
{
    private readonly User.Role _accessLevel;

    private DbConnector dbCon = new DbConnector();

    private List<DisplayedMachine> _displayedObjects;
    private BindingList<DisplayedMachine> _bindingList;
    private List<Machine> _objects = new List<Machine>();

    private readonly bool _isAddingForm = false;
    private Button btnAddSelected;
    private Button btnCancel;

    public readonly bool _newItemCreateActive;
    private readonly int? _tcId;

    public bool _isDataLoaded = false;
    private bool _isFiltered = false;

    public Win7_5_Machine(User.Role accessLevel)
    {
        _accessLevel = accessLevel;

        InitializeComponent();
        AccessInitialization();

    }
    public Win7_5_Machine(bool activateNewItemCreate = false, int? createdTCId = null)
    {
        _accessLevel = AuthorizationService.CurrentUser.UserRole();

        _isAddingForm = true;
        _tcId = createdTCId;
        _newItemCreateActive = activateNewItemCreate;

        InitializeComponent();

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
            SetAddingFormEvents();
        }

        dgvMain.Visible = true;
        this.Enabled = true;
        //progressBar.Visible = false;
    }
    public async Task LoadDataAsync()
    {

        _displayedObjects = await Task.Run(() => dbCon.GetObjectList<Machine>(includeLinks: true)
            .Select(obj => new DisplayedMachine(obj)).ToList());

        FilteringObjects();

        //_bindingList = new BindingList<DisplayedMachine>(_displayedObjects);

        //dgvMain.DataSource = null; // cancel update of dgv while data is loading
        ////_bindingList.ListChanged += BindingList_ListChanged;

        //dgvMain.DataSource = _bindingList;
        ////SetLinkColumn();
        //dgvMain.CellContentClick += dgvMain_CellContentClick;

        _isDataLoaded = true;
    }
    private async void Win7_5_Machine_FormClosing(object sender, FormClosingEventArgs e)
    {
        
    }



    private void AccessInitialization()
    {
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
                var objEditor = new Win7_LinkObjectEditor(machine, accessLevel: _accessLevel);

                objEditor.AfterSave = async (updatedObj) => UpdateObjectInDataGridView<Machine, DisplayedMachine>(updatedObj as Machine);

                objEditor.ShowDialog();
            }
        }
    }

    private async void btnDeleteObj_Click(object sender, EventArgs e)
    {
        await DisplayedEntityHelper.DeleteSelectedObjectWithLinks<DisplayedMachine, Machine>(dgvMain,
            _bindingList, _isFiltered ? _displayedObjects : null);
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
        dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
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
        var selectedObjs = selectedRows.Select(r => r.DataBoundItem as DisplayedMachine).ToList();
        var newItems = new List<Machine>();
        foreach (var obj in selectedObjs)
        {
            newItems.Add(CreateNewObject(obj));
        }
        // find opened form
        var tcEditor = Application.OpenForms.OfType<Win6_Machine>().FirstOrDefault();

        tcEditor.AddNewObjects(newItems);

        // close form
        this.Close();
    }
    void BtnCancel_Click(object sender, EventArgs e)
    {
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

            if (string.IsNullOrWhiteSpace(searchText) && !cbxShowUnReleased.Checked)
            {
                _bindingList = new BindingList<DisplayedMachine>(_displayedObjects.Where(obj => obj.IsReleased == true).ToList());
                _isFiltered = false;
            }
            else
            {
                _bindingList = FilteredBindingList(searchText);
                _isFiltered = true;
            }
            dgvMain.DataSource = _bindingList;
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
                    (obj.IsReleased == !cbxShowUnReleased.Checked) &&

                    (!_isAddingForm ||
                        (!cbxShowUnReleased.Checked ||
                        (cbxShowUnReleased.Checked &&
                        (obj.CreatedTCId == null || obj.CreatedTCId == _tcId)))
                    )
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
}
