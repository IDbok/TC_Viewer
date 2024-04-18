using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using Component = TcModels.Models.TcContent.Component; 

namespace TC_WinForms.WinForms
{
    public partial class Win7_4_Component : Form, ISaveEventForm, ILoadDataAsyncForm
    {
        private DbConnector dbCon = new DbConnector();
        private BindingList<DisplayedComponent> _bindingList;

        private List<DisplayedComponent> _changedObjects = new List<DisplayedComponent>();
        private List<DisplayedComponent> _newObjects = new List<DisplayedComponent>();
        private List<DisplayedComponent> _deletedObjects = new List<DisplayedComponent>();

        private DisplayedComponent _newObject;

        private bool isAddingForm = false;
        private Button btnAddSelected;
        private Button btnCancel;

        public bool _isDataLoaded = false;
        public bool CloseFormsNoSave { get; set; } = false;
        public void SetAsAddingForm()
        {
            isAddingForm = true;
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
        public Win7_4_Component(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }
        public Win7_4_Component()
        {
            InitializeComponent();
        }

        private async void Win7_4_Component_Load(object sender, EventArgs e)
        {
            //progressBar.Visible = true;

            if (!_isDataLoaded)
            {
                await LoadDataAsync();
            }
            SetDGVColumnsSettings();
            SetupCategoryComboBox();

            DisplayedEntityHelper.SetupDataGridView<DisplayedComponent>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            if (isAddingForm)
            {
                //isAddingFormSetControls();
                WinProcessing.SetAddingFormControls(pnlControlBtns, dgvMain,
                    out btnAddSelected, out btnCancel);
                SetAddingFormEvents();
            }

            //progressBar.Visible = false;
        }
        public async Task LoadDataAsync()
        {
            var tcList = await Task.Run(() => dbCon.GetObjectList<Component>(includeLinks: true)
                .Select(obj => new DisplayedComponent(obj)).ToList());

            _bindingList = new BindingList<DisplayedComponent>(tcList);

            dgvMain.DataSource = null; // cancel update of dgv while data is loading
            _bindingList.ListChanged += BindingList_ListChanged;

            dgvMain.DataSource = _bindingList;

            dgvMain.CellContentClick += dgvMain_CellContentClick;

            _isDataLoaded = true;
        }
        private async void Win7_4_Component_FormClosing(object sender, FormClosingEventArgs e)
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
                Close();
            }
        }
        private void AccessInitialization(int accessLevel)
        {

        }



        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            var objEditor = new Win7_LinkObjectEditor(new Component(), isNewObject: true);

            objEditor.AfterSave = async (createdObj) => AddNewObjectInDataGridView<Component, DisplayedComponent>(createdObj as Component);

            objEditor.ShowDialog();
        }

        private async void btnDeleteObj_Click(object sender, EventArgs e)
        {
            await DisplayedEntityHelper.DeleteSelectedObjectWithLinks<DisplayedComponent, Component>(dgvMain,
                _bindingList);
        }

        /////////////////////////////////////////////// * SaveChanges * ///////////////////////////////////////////
        public bool HasChanges => _changedObjects.Count + _newObjects.Count + _deletedObjects.Count != 0;
        public async Task SaveChanges()
        {
            // stop editing cell
            dgvMain.EndEdit();
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
            // todo - change id in all new cards 
            dgvMain.Refresh();
        }
        private async Task SaveNewObjects()
        {
            var newObjects = _newObjects.Select(dtc => CreateNewObject(dtc)).ToList();

            await dbCon.AddObjectAsync(newObjects);

            // set new ids to new objects matched them by all params
            foreach (var newObj in _newObjects)
            {
                var newId = newObjects.Where(s =>
                s.Name == newObj.Name
                && s.Type == newObj.Type
                && s.Unit == newObj.Unit
                && s.Price == newObj.Price
                && s.Description == newObj.Description
                && s.Manufacturer == newObj.Manufacturer
                && s.Links == newObj.Links
                && s.Categoty == newObj.Categoty
                && s.ClassifierCode == newObj.ClassifierCode
                ).FirstOrDefault().Id;
                newObj.Id = newId;
            }

            _newObjects.Clear();
        }
        private async Task SaveChangedObjects()
        {
            var changedTcs = _changedObjects.Select(dtc => CreateNewObject(dtc)).ToList();

            await dbCon.UpdateObjectsListAsync(changedTcs);

            _changedObjects.Clear();
        }

        private async Task DeleteDeletedObjects()
        {
            var deletedTcIds = _deletedObjects.Select(dtc => dtc.Id).ToList();

            await dbCon.DeleteObjectAsync<Component>(deletedTcIds);
            _deletedObjects.Clear();
        }
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
            var selectedObjs = selectedRows.Select(r => r.DataBoundItem as DisplayedComponent).ToList();
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
        void BtnCancel_Click(object sender, EventArgs e)
        {
            // close form
            this.Close();
        }


        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            DisplayedEntityHelper.ListChangedEventHandler<DisplayedComponent>
                (e, _bindingList, _newObjects, _changedObjects, ref _newObject);
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
            private string? type;
            private string unit;
            private float? price;
            private string? description;
            private string? manufacturer;
            private List<LinkEntety> links = new();
            private string categoty = "StandComp";
            private string classifierCode;


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

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterTechnologicalCards();
        }

        private void cbxCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterTechnologicalCards();
        }
        private void FilterTechnologicalCards()
        {
            try
            {
                var searchText = txtSearch.Text == "Поиск" ? "" : txtSearch.Text;
                var categoryFilter = cbxCategoryFilter.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(searchText) && (categoryFilter == "Все" || string.IsNullOrWhiteSpace(categoryFilter)))
                {
                    dgvMain.DataSource = _bindingList; // Возвращаем исходный список, если строка поиска пуста
                }
                else
                {
                    var filteredList = _bindingList.Where(obj =>
                        (searchText == ""
                            ||
                            (obj.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.Type?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.Unit?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            //(obj.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.Categoty?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.ClassifierCode?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                        ) &&
                        (categoryFilter == "Все" || obj.Categoty?.ToString() == categoryFilter)
                        ).ToList();

                    dgvMain.DataSource = new BindingList<DisplayedComponent>(filteredList);
                }
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
            var obj = selectedObj?.DataBoundItem as DisplayedComponent;

            if (obj != null)
            {
                var machine = dbCon.GetObjectWithLinks<Component>(obj.Id);

                if (machine != null)
                {
                    var objEditor = new Win7_LinkObjectEditor(machine);

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
            var displayedObject = _bindingList.OfType<TDisplayed>().FirstOrDefault(obj => obj.Id == modelObject.Id);
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
                if (displayedObject is ICategoryable objectWithCategory && modelObject is ICategoryable modelWithCategory)
                {
                    objectWithCategory.Categoty = modelWithCategory.Categoty;
                }

                _bindingList.Insert(0, displayedObject);
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

    }
}
