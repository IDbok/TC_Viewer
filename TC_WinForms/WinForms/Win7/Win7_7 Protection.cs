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

namespace TC_WinForms.WinForms
{
    public partial class Win7_7_Protection : Form, IPaginationControl//, ISaveEventForm
    {
        private readonly User.Role _accessLevel;
        private readonly int _minRowHeight = 20;
        private SelectionService<DisplayedProtection> _selectionService;

        private DbConnector dbCon = new DbConnector();
        private List<DisplayedProtection> _displayedObjects = new();
        private BindingList<DisplayedProtection> _bindingList;
        private ConcurrencyBlockService<Protection> concurrencyBlockServise;

        private readonly bool _isAddingForm = false;
        private readonly bool _isUpdateItemMode = false; // add to UpdateMode
        private Button btnAddSelected;
        private Button btnCancel;

        public readonly bool _newItemCreateActive;
        private readonly int? _tcId;

        private bool _isFiltered = false;

        PaginationControlService<DisplayedProtection> paginationService;
        public event EventHandler<PageInfoEventArgs> PageInfoChanged;
        public PageInfoEventArgs? PageInfo { get; set; }

        public Win7_7_Protection(User.Role accessLevel)
        {
            _accessLevel = accessLevel;

            InitializeComponent();
            AccessInitialization();

        }
        public Win7_7_Protection(bool activateNewItemCreate = false, int? createdTCId = null, bool isUpdateMode = false) // this constructor is for adding form in TC editer
        {
            _accessLevel = AuthorizationService.CurrentUser.UserRole();

            _isAddingForm = true;
            _isUpdateItemMode = isUpdateMode; // add to UpdateMode
            _newItemCreateActive = activateNewItemCreate;
            _tcId = createdTCId;

            InitializeComponent();

            dgvMain.DoubleBuffered(true);

            _selectionService = new SelectionService<DisplayedProtection>(dgvMain, _displayedObjects);

        }

        private async void Win7_7_Protection_Load(object sender, EventArgs e)
        {
            //progressBar.Visible = true;
            this.Enabled = false;
            dgvMain.Visible = false;

            await LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedProtection>(dgvMain);

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
        private async Task LoadObjects()
        {
            _displayedObjects = await Task.Run(() => dbCon.GetObjectList<Protection>(includeLinks: true)
                .Select(obj => new DisplayedProtection(obj)).OrderBy(c => c.Name).ToList());

            if(!_isAddingForm)
                paginationService = new PaginationControlService<DisplayedProtection>(30, _displayedObjects);
            
            _selectionService = new SelectionService<DisplayedProtection>(dgvMain, _displayedObjects);

            FilteringObjects();
            //_bindingList = new BindingList<DisplayedProtection>(_displayedObjects);
            ////_bindingList.ListChanged += BindingList_ListChanged;
            //dgvMain.DataSource = _bindingList;

            //dgvMain.CellContentClick += dgvMain_CellContentClick;

            SetDGVColumnsSettings();
        }

        private void UpdateDisplayedData()
        {
            // Расчет отображаемых записей
            if(!_isAddingForm && paginationService != null)
                _bindingList = new BindingList<DisplayedProtection>(paginationService.GetPageData());

            dgvMain.DataSource = _bindingList;
            dgvMain.ResizeRows(_minRowHeight);

            // Подготовка данных для события
            if (!_isAddingForm && paginationService != null)
            {
                PageInfo = paginationService.GetPageInfo();
                // Вызов события с подготовленными данными
                RaisePageInfoChanged();
            }

        }

        private async void Win7_7_Protection_FormClosing(object sender, FormClosingEventArgs e)
        {

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
            var objEditor = new Win7_LinkObjectEditor(new Protection() { CreatedTCId = _tcId }, isNewObject: true, accessLevel: _accessLevel);

            objEditor.AfterSave = async (createdObj) => AddNewObjectInDataGridView<Protection, DisplayedProtection>(createdObj as Protection);

            objEditor.ShowDialog();
        }

        private async void btnDeleteObj_Click(object sender, EventArgs e)
        {
            //DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
            //    _bindingList, _newObjects, _deletedObjects);
            await DisplayedEntityHelper.DeleteSelectedObjectWithLinks<DisplayedProtection, Protection>(dgvMain,
                _bindingList, _displayedObjects);

            FilteringObjects();
        }
        /////////////////////////////////////////////// * SaveChanges * ///////////////////////////////////////////

        private Protection CreateNewObject(DisplayedProtection dObj)
        {
            return new Protection
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
                nameof(DisplayedProtection.Id),
                nameof(DisplayedProtection.Unit),
                //nameof(DisplayedProtection.ClassifierCode),
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            dgvMain.Columns[nameof(DisplayedProtection.Name)].Width = 250;
            dgvMain.Columns[nameof(DisplayedProtection.Type)].Width = 200;

            //dgvMain.Columns[nameof(DisplayedProtection.Price)].Width = 120;
            dgvMain.Columns[nameof(DisplayedProtection.ClassifierCode)].Width = 150;
            dgvMain.Columns[nameof(DisplayedProtection.ClassifierCode)].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            dgvMain.Columns[nameof(DisplayedProtection.LinkNames)].Width = 100;
            dgvMain.Columns[nameof(DisplayedProtection.LinkNames)].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
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
            var tcEditor = Application.OpenForms.OfType<Win6_Protection>().FirstOrDefault();

            tcEditor.UpdateSelectedObject(CreateNewObject(selectedObjs[0]));

            // close form
            this.Close();
        }

        void AddSelectedItems()
        {
            // get selected objects
            var selectedObjs = _selectionService.GetSelectedObjects();
            if (selectedObjs.Count == 0)
            {
                MessageBox.Show("Выберите строки для добавления");
                return;
            }
            // find opened form
            var tcEditor = CheckOpenFormService.FindOpenedForm<Win6_Protection>((int)_tcId);
            var newItems = new List<Protection>();
            foreach (var obj in selectedObjs)
            {
                newItems.Add(CreateNewObject(obj));
            }

            tcEditor.AddNewObjects(newItems);

            // close form
            this.Close();
        }
        private class DisplayedProtection : INotifyPropertyChanged, IDisplayedEntity, IModelStructure
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
                { nameof(Manufacturer), "Производители (поставщики)" },
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

            public DisplayedProtection()
            {

            }
            public DisplayedProtection(Protection obj)
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
                        return links[0].Name ?? links[0].Link;
                    }
                    //defLink.Name = linksNames;
                    return defLink.Name ?? defLink.Link;
                }
                return string.Empty;
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
                var displayedProtectionList = new BindingList<DisplayedProtection>();

                if (string.IsNullOrWhiteSpace(searchText) && !cbxShowUnReleased.Checked)
                {
                    // Возвращаем исходный список, если строка поиска пуста
                    displayedProtectionList = new BindingList<DisplayedProtection>(_displayedObjects.Where(obj => obj.IsReleased == true).ToList());
                    _isFiltered = false;
                }
                else
                {
                    displayedProtectionList = FilteredBindingList(searchText);
                    _isFiltered = true;
                }
                //dgvMain.DataSource = _bindingList;
                if(!_isAddingForm && paginationService != null)
                    paginationService.SetAllObjectList(displayedProtectionList.ToList());
                else
                {
                    _bindingList = displayedProtectionList;
                }

                UpdateDisplayedData();

                // Восстанавливаем выделенные объекты
                if (_isAddingForm)
                    _selectionService.RestoreSelectedIds();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }

        }
        private BindingList<DisplayedProtection> FilteredBindingList(string searchText)
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

            return new BindingList<DisplayedProtection>(filteredList);
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return;
            }

            var selectedObj = dgvMain.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            var obj = selectedObj?.DataBoundItem as DisplayedProtection;

            if (obj != null)
            {
                var machine = dbCon.GetObjectWithLinks<Protection>(obj.Id);

                if (machine != null)
                {
                    var timerInterval = 1000 * 60 * 25;
                    concurrencyBlockServise = new ConcurrencyBlockService<Protection>(machine, timerInterval);
                    if (concurrencyBlockServise.GetObjectUsedStatus())
                    {
                        MessageBox.Show("Данный объект сейчас редактируется другим пользователем. Вы не можете его редактировать.", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var objEditor = new Win7_LinkObjectEditor(machine, accessLevel: _accessLevel);

                    objEditor.AfterSave = async (updatedObj) => UpdateObjectInDataGridView<Protection, DisplayedProtection>(updatedObj as Protection);

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
            if (newDisplayedObject is DisplayedProtection displayedObject)
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
            if (dgvMain.Columns[e.ColumnIndex].Name == nameof(DisplayedProtection.LinkNames) && e.RowIndex >= 0)
            {
                var displayedMachine = dgvMain.Rows[e.RowIndex].DataBoundItem as DisplayedProtection;
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

        private void cbxShowUnReleased_CheckedChanged(object sender, EventArgs e)
        {
            FilteringObjects();
        }

        private void Win7_7_Protection_SizeChanged(object sender, EventArgs e)
        {
            dgvMain.ResizeRows(_minRowHeight);
        }
    }
}
