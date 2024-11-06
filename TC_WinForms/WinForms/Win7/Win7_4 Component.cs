using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Reflection;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Services;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using static TC_WinForms.DataProcessing.AuthorizationService;
using Component = TcModels.Models.TcContent.Component; 

namespace TC_WinForms.WinForms;

public partial class Win7_4_Component : Form, ILoadDataAsyncForm, IPaginationControl//, ISaveEventForm
{
    private readonly User.Role _accessLevel;

    private SelectionService<DisplayedComponent> _selectionService;

    private DbConnector dbCon = new DbConnector();
    private List<DisplayedComponent> _displayedObjects = new();
    private BindingList<DisplayedComponent> _bindingList;

    private readonly bool _isAddingForm = false;
    private readonly bool _isUpdateItemMode = false;// add to UpdateMode
    private Button btnAddSelected;
    private Button btnCancel;

    //private Form _openedForm;
    public readonly bool _newItemCreateActive;
    private readonly int? _tcId;

    public bool _isDataLoaded = false;

    PaginationControlService<DisplayedComponent> paginationService;

    private bool _isFiltered = false;
    private DataGridViewCellEventArgs lastCellEvent;
    private DataGridViewCellEventArgs currentCellEvent;

    public event EventHandler<PageInfoEventArgs> PageInfoChanged;
    public PageInfoEventArgs? PageInfo { get; set; }

    //private ToolTip toolTip;
    //private List<int> _selectedIds = new List<int>();
    public Win7_4_Component(User.Role accessLevel)
    {
        _accessLevel = accessLevel;

        InitializeComponent();
        AccessInitialization();

        InitializeTip();
    }

    public Win7_4_Component(bool activateNewItemCreate = false, int? createdTCId = null, bool isUpdateMode = false)
    {
        _accessLevel = AuthorizationService.CurrentUser.UserRole();

        _isAddingForm = true;
        _tcId = createdTCId;
        _newItemCreateActive = activateNewItemCreate;
        _isUpdateItemMode = isUpdateMode;// add to UpdateMode

        InitializeComponent();


        _selectionService = new SelectionService<DisplayedComponent>(dgvMain, _displayedObjects);
    }

    private  void InitializeTip()
    {
        dgvMain.ShowCellToolTips = false;

        toolTip = new ToolTip();
        toolTip.OwnerDraw = true;
        toolTip.ShowAlways = true;
        toolTip.Popup += ToolTip_Popup;
        toolTip.Draw += ToolTip_Draw;

        dgvMain.CellMouseEnter += new DataGridViewCellEventHandler(dgvMain_CellMouseEnter);
        dgvMain.CellMouseLeave += new DataGridViewCellEventHandler(dgvMain_CellMouseLeave);
    }
    private async void Win7_4_Component_Load(object sender, EventArgs e)
    {
        //progressBar.Visible = true;
        this.Enabled = false;
        dgvMain.Visible = false;

        if (!_isDataLoaded)
        {
            await LoadDataAsync();
        }
        SetDGVColumnsSettings();
        SetupCategoryComboBox();

        DisplayedEntityHelper.SetupDataGridView<DisplayedComponent>(dgvMain);

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
            
            if(_isUpdateItemMode)// add to UpdateMode
            {
                btnAddSelected.Text = "Обновить";
            }

            SetAddingFormEvents();
        }

        dgvMain.Visible = true;
        this.Enabled = true;
        //progressBar.Visible = false;
    }
    public async Task LoadDataAsync()
    {
        var displayedObj = await Task.Run(() => DataService.GetComponents() //dbCon.GetObjectList<Component>(includeLinks: true)
            .Select(obj => new DisplayedComponent(obj)).ToList());

        foreach(var obj in displayedObj)
        {
            _displayedObjects.Add(obj);
        }

        paginationService = new PaginationControlService<DisplayedComponent>(50);
        paginationService.SetAllObjectList(_displayedObjects.OrderBy(c => c.Name).ToList());

        FilteringObjects();

        _isDataLoaded = true;
    }

    private void UpdateDisplayedData()
    {
        // Расчет отображаемых записей
        var pageData = paginationService.GetPageData();

        _bindingList = new BindingList<DisplayedComponent>(pageData);
        dgvMain.DataSource = _bindingList;

        // Подготовка данных для события
        PageInfo = paginationService.GetPageInfo();

        // Вызов события с подготовленными данными
        RaisePageInfoChanged();
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
        var objEditor = new Win7_LinkObjectEditor(new Component() { CreatedTCId = _tcId }, isNewObject: true, accessLevel: _accessLevel);

        objEditor.AfterSave = async (createdObj) => AddNewObjectInDataGridView<Component, DisplayedComponent>(createdObj as Component);

        objEditor.ShowDialog();
    }

    private async void btnDeleteObj_Click(object sender, EventArgs e)
    {
        await DisplayedEntityHelper.DeleteSelectedObjectWithLinks<DisplayedComponent, Component>(dgvMain,
            _bindingList, _isFiltered ? _displayedObjects : null);
    }

    /////////////////////////////////////////////// * SaveChanges * ///////////////////////////////////////////

    private Component CreateNewObject(DisplayedComponent dObj)
    {
        return new Component
        {
            Id = dObj.Id,
            Name = dObj.Name,
            Type = dObj.Type,
            Unit = dObj.Unit,
            Price = dObj.Price,
            Description = dObj.Description,
            Manufacturer = dObj.Manufacturer,
            Links = dObj.Links,
            Categoty = dObj.Categoty,
            ClassifierCode = dObj.ClassifierCode,

            IsReleased = dObj.IsReleased,
            CreatedTCId = dObj.CreatedTCId,
        };
    }

    ////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////

    void SetDGVColumnsSettings()
    {
        if (_isAddingForm) { return; }

        // автоподбор ширины столбцов под ширину таблицы
        dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvMain.RowHeadersWidth = 25;


        dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders; //None;
        dgvMain.RowTemplate.Height = 200;

        //// автоперенос в ячейках
        dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        //    // ширина столбцов по содержанию
        var autosizeColumn = new List<string>
        {
            nameof(DisplayedComponent.Id),
            nameof(DisplayedComponent.Unit),
            nameof(DisplayedComponent.Categoty),
            //nameof(DisplayedComponent.ClassifierCode),
        };
        foreach (var column in autosizeColumn)
        {
            dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        }

        dgvMain.Columns[nameof(DisplayedComponent.Name)].Width = 250;
        dgvMain.Columns[nameof(DisplayedComponent.Type)].Width = 200;


        dgvMain.Columns[nameof(DisplayedComponent.Price)].Width = 120;
        dgvMain.Columns[nameof(DisplayedComponent.ClassifierCode)].Width = 150;
        dgvMain.Columns[nameof(DisplayedComponent.ClassifierCode)].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

        dgvMain.Columns[nameof(DisplayedComponent.LinkNames)].Width = 100;
        dgvMain.Columns[nameof(DisplayedComponent.LinkNames)].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
    }
    private void dgvMain_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
    {
        var imageCol = dgvMain.Columns[nameof(DisplayedComponent.Image)] as DataGridViewImageColumn;
        if (imageCol != null)
        {
            imageCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            //var cell = dgvMain.Rows[e.RowIndex].Cells[nameof(DisplayedComponent.Image)];
            //if (cell.Value != null)
            //{
            //    var image = (byte[])cell.Value;
            //    if (image != null)
            //    {
            //        using (var ms = new MemoryStream(image))
            //        {
            //            cell.Value = Image.FromStream(ms);
            //        }
            //    }
            //}
        }

    }


    private void SetupCategoryComboBox()
    {
        var types = _bindingList.Select(obj => obj.Categoty).Distinct().ToList();
        types.Sort();

        cbxCategoryFilter.Items.Add("Все");
        foreach (var type in types)
        {
            if (string.IsNullOrWhiteSpace(type)) { continue; }
            cbxCategoryFilter.Items.Add(type);
        }
        //cbxType.Items.AddRange(new object[] { "Ремонтная", "Монтажная", "Точка Трансформации", "Нет данных" });
        //cbxCategoryFilter.SelectedIndex = 0; // Выбираем "Все" по умолчанию

        cbxCategoryFilter.DropDownWidth = cbxCategoryFilter.Items.Cast<string>().Max(s => TextRenderer.MeasureText(s, cbxCategoryFilter.Font).Width) + 20;
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

        if(selectedObjs.Count != 1)
        {
            MessageBox.Show("Выберите одну строку для обновления");
            return;
        }

        // find opened form
        var tcEditor = Application.OpenForms.OfType<Win6_Component>().FirstOrDefault();
        
        tcEditor.UpdateSelectedObject(CreateNewObject(selectedObjs[0]));

        // close form
        this.Close();
    }

    void AddSelectedItems()
    {
        // выделить объекты с id из _selectedIds из списка _displayedObjects
        var selectedObjs = _selectionService.GetSelectedObjects();//_displayedObjects.Where(obj => _selectedIds.Contains(obj.Id)).ToList();

        if (selectedObjs.Count == 0)
        {
            MessageBox.Show("Выберите строки для добавления");
            return;
        }

        // find opened form
        var tcEditor = Application.OpenForms.OfType<Win6_Component>().FirstOrDefault();
        var newItems = new List<Component>();
        foreach (var obj in selectedObjs)
        {
            newItems.Add(CreateNewObject(obj));
        }

        tcEditor.AddNewObjects(newItems);

        // close form
        this.Close();
    }

    private class DisplayedComponent : INotifyPropertyChanged, IDisplayedEntity, IModelStructure, ICategoryable
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
            { nameof(Price), "Стоимость, руб. без НДС" },
            { nameof(Description), "Описание" },
            { nameof(Manufacturer), "Производители (поставщики)" },
            //{ nameof(Links), "Ссылки" }, // todo - fix problem with Links (load it from DB to DGV)
            { nameof(LinkNames), "Ссылка" },
            { nameof(Categoty), "Категория" },
            { nameof(Image), "Изображение" },
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
                nameof(Price),
                nameof(Description),
                nameof(Manufacturer),
                nameof(LinkNames),
                //nameof(Links),
                nameof(Categoty),
                //nameof(Image),
            };
        }
        public List<string> GetRequiredFields()
        {
            return new List<string>
            {
                nameof(Name) ,
                nameof(Type) ,
                nameof(Unit) ,
                nameof(Categoty) ,
                nameof(ClassifierCode) ,
            };
        }

        private int id;
        private string name;
        private string type;
        private string unit;

        private float? price;
        private string? description;
        private string? manufacturer;
        private List<LinkEntety> links = new();
        private string categoty = "StandComp";
        private string classifierCode;

        private bool isReleased;
        private int? createdTCId;

        public DisplayedComponent()
        {

        }
        public DisplayedComponent(Component obj)
        {
            Id = obj.Id;
            Name = obj.Name;
            Type = obj.Type;
            Unit = obj.Unit;
            Price = obj.Price;
            Description = obj.Description;
            Manufacturer = obj.Manufacturer;
            Links = obj.Links;
            Categoty = obj.Categoty;
            ClassifierCode = obj.ClassifierCode;

            IsReleased = obj.IsReleased;
            CreatedTCId = obj.CreatedTCId;

            Image = obj.Image;
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
            get => manufacturer;
            set
            {
                if (manufacturer != value)
                {
                    manufacturer = value;
                    OnPropertyChanged(nameof(Manufacturer));
                }
            }
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
        public string LinkNames
        {
            get => GetDefaultLinkOrFirst();
        }
        private string GetDefaultLinkOrFirst()
        {
            if (links.Count > 0)
            {
                // Все названия существующих ссылок с новой строки
                var defLink = links.Where(l => l.IsDefault).FirstOrDefault();
                if (defLink == null)
                {
                    return GetLinkName(links[0]);
                }
                return GetLinkName(defLink);
            }
            return string.Empty;

            string GetLinkName(LinkEntety link)
            {
                string linkName;
                if (link.Name != null && link.Name != "")
                {
                    linkName = link.Name;
                }
                else
                {
                    linkName = link.Link;
                }

                return linkName;
            }
        }
        public string Categoty
        {
            get => categoty;
            set
            {
                if (categoty != value)
                {
                    categoty = value;
                    OnPropertyChanged(nameof(Categoty));
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

        private byte[]? image;

        public byte[]? Image
        {
            get => image;
            set
            {
                if (image != value)
                {
                    image = value;
                    OnPropertyChanged(nameof(Image));
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

    private void cbxCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        FilteringObjects();
    }
    private void FilteringObjects()
    {
        try
        {
            var searchText = txtSearch.Text == "Поиск" ? "" : txtSearch.Text;
            var categoryFilter = cbxCategoryFilter.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(searchText) && (categoryFilter == "Все" || string.IsNullOrWhiteSpace(categoryFilter)) && !cbxShowUnReleased.Checked)
            {
                _bindingList = new BindingList<DisplayedComponent>(_displayedObjects.Where(obj => obj.IsReleased == true).ToList());
                _isFiltered = false;

                paginationService.SetAllObjectList(_displayedObjects);
                UpdateDisplayedData();
            }
            else
            {
                _bindingList = FilteredBindingList(searchText);
                _isFiltered = true;

                paginationService.SetAllObjectList(_bindingList.ToList());
                UpdateDisplayedData();

            }
            //dgvMain.DataSource = _bindingList;

            // Восстанавливаем выделенные объекты
            if (_isAddingForm)
                _selectionService.RestoreSelectedIds();
        }
        catch (Exception e)
        {
            //MessageBox.Show(e.Message);
        }

    }
    private BindingList<DisplayedComponent> FilteredBindingList(string searchText)
    {
        var categoryFilter = cbxCategoryFilter.SelectedItem?.ToString();
        var filteredList = _displayedObjects.Where(obj =>
                    (searchText == ""
                        ||
                        (obj.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.Type?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.Unit?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        //(obj.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.Categoty?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (obj.ClassifierCode?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                    ) &&

                    ((categoryFilter == "Все" || string.IsNullOrWhiteSpace(categoryFilter))
                            || obj.Categoty?.ToString() == categoryFilter) &&

                    (obj.IsReleased == !cbxShowUnReleased.Checked) 
                    //&& (!_isAddingForm ||
                    //    (!cbxShowUnReleased.Checked ||
                    //    (cbxShowUnReleased.Checked
                    //    && (obj.CreatedTCId == null || obj.CreatedTCId == _tcId))
                    //    )
                    //)
                    ).ToList();

        return new BindingList<DisplayedComponent>(filteredList);
    }
    private void btnUpdate_Click(object sender, EventArgs e)
    {
        if (dgvMain.SelectedRows.Count != 1)
        {
            MessageBox.Show("Выберите одну строку для редактирования");
            return;
        }

        var selectedObj = dgvMain.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
        var obj = selectedObj?.DataBoundItem as DisplayedComponent;

        if (obj != null)
        {
            var machine = dbCon.GetObjectWithLinks<Component>(obj.Id);

            if (machine != null)
            {
                var objEditor = new Win7_LinkObjectEditor(machine, accessLevel: _accessLevel);

                objEditor.AfterSave = async (updatedObj) => UpdateObjectInDataGridView<Component, DisplayedComponent>(updatedObj as Component);

                objEditor.ShowDialog();
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
            if (displayedObject is ICategoryable objectWithCategory && modelObject is ICategoryable modelWithCategory)
            {
                objectWithCategory.Categoty = modelWithCategory.Categoty;
            }

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
        if (newDisplayedObject is DisplayedComponent displayedObject)
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

    private void dgvMain_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        UrlClick(sender, e);
    }
    private void UrlClick(object sender, DataGridViewCellEventArgs e)
    {
        if (dgvMain.Columns[e.ColumnIndex].Name == nameof(DisplayedComponent.LinkNames) && e.RowIndex >= 0)
        {
            var displayedMachine = dgvMain.Rows[e.RowIndex].DataBoundItem as DisplayedComponent;
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

    private void cbxShowUnReleased_CheckedChanged(object sender, EventArgs e)
    {
        FilteringObjects();
    }


    //private void dgvMain_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
    //{
    //    if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
    //    {
    //        var cell = dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex];
    //        var displayedComponent = dgvMain.Rows[e.RowIndex].DataBoundItem as DisplayedComponent;

    //        if (displayedComponent != null && displayedComponent.Image != null && displayedComponent.Image.Length > 0)
    //        {
    //            var image = Image.FromStream(new MemoryStream(displayedComponent.Image));
    //            toolTip.Tag = image;
    //            toolTip.Show(string.Empty, dgvMain, dgvMain.PointToClient(Cursor.Position), 2000);
    //        }
    //    }
    //}
    //private void dgvMain_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
    //{
    //    if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
    //    {
    //        var cell = dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex];
    //        var displayedComponent = dgvMain.Rows[e.RowIndex].DataBoundItem as DisplayedComponent;

    //        if (displayedComponent != null && displayedComponent.Image != null && displayedComponent.Image.Length > 0)
    //        {
    //            var image = Image.FromStream(new MemoryStream(displayedComponent.Image));
    //            toolTip.Tag = image;
    //            toolTip.Show(" ", dgvMain, dgvMain.PointToClient(Cursor.Position));
                
               
    //        }
    //    }
    //}
    private void dgvMain_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
    {
        currentCellEvent = e;
        if (currentCellEvent.RowIndex != lastCellEvent?.RowIndex || currentCellEvent.ColumnIndex != lastCellEvent?.ColumnIndex)
        {
            lastCellEvent = currentCellEvent;
            dgvMain.MouseHover += dgvMain_MouseHover;
        }
    }

    private void dgvMain_MouseHover(object sender, EventArgs e)
    {
        if (lastCellEvent != null && lastCellEvent.RowIndex >= 0 && lastCellEvent.ColumnIndex >= 0)
        {
            var cell = dgvMain.Rows[lastCellEvent.RowIndex].Cells[lastCellEvent.ColumnIndex];
            var displayedComponent = dgvMain.Rows[lastCellEvent.RowIndex].DataBoundItem as DisplayedComponent;

            if (displayedComponent != null && displayedComponent.Image != null && displayedComponent.Image.Length > 0)
            {
                var image = Image.FromStream(new MemoryStream(displayedComponent.Image));
                toolTip.Tag = image;
                toolTip.Show(" ", dgvMain, dgvMain.PointToClient(Cursor.Position)); // Используем пробел вместо пустой строки
            }
        }
    }

    private void dgvMain_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
    {
        toolTip.Hide(dgvMain);
        dgvMain.MouseHover -= dgvMain_MouseHover;
        lastCellEvent = null;
    }


    //private void dgvMain_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
    //{
    //    toolTip.Hide(dgvMain);
    //}

    private void ToolTip_Draw(object sender, DrawToolTipEventArgs e)
    {
        Debug.WriteLine("ToolTip_Draw called");
        if (toolTip.Tag is Image image)
        {
            e.Graphics.DrawImage(image, new Rectangle(Point.Empty, new Size(200, 200))); // Размер изображения в подсказке
        }
    }
    private void ToolTip_Popup(object sender, PopupEventArgs e)
    {
        Debug.WriteLine("ToolTip_Popup called");
        if (toolTip.Tag is Image image)
        {
            e.ToolTipSize = new Size(200, 200); // Размер всплывающего окна
        }
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
}
