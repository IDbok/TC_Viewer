using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms
{
    public partial class Win7_TechTransition : Form, IPaginationControl//, ISaveEventForm
    {

        private readonly User.Role _accessLevel;

        private DbConnector dbCon = new DbConnector();


        private List<DisplayedTechTransition> _displayedObjects;
        private BindingList<DisplayedTechTransition> _bindingList;

        private List<DisplayedTechTransition> _changedObjects = new List<DisplayedTechTransition>();
        private List<DisplayedTechTransition> _newObjects = new List<DisplayedTechTransition>();
        private List<DisplayedTechTransition> _deletedObjects = new List<DisplayedTechTransition>();

        private DisplayedTechTransition _newObject;

        private bool isAddingForm = false;
        private Button btnAddSelected;
        private Button btnCancel;
        public bool CloseFormsNoSave { get; set; } = false;

        private bool _isFiltered = false;

        PaginationControlService<DisplayedTechTransition> paginationService;
        public event EventHandler<PageInfoEventArgs> PageInfoChanged;
        public PageInfoEventArgs? PageInfo { get; set; }

        public void SetAsAddingForm()
        {
            isAddingForm = true;
        }
        public Win7_TechTransition(User.Role accessLevel)
        {
            _accessLevel = accessLevel;

            InitializeComponent();
            AccessInitialization();
            dgvMain.DoubleBuffered(true);

        }
        public Win7_TechTransition()
        {
            InitializeComponent();
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
        private async void Win7_TechTransition_Load(object sender, EventArgs e)
        {
            await LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedTechTransition>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            if (isAddingForm)
            {
                WinProcessing.SetAddingFormControls(pnlControlBtns, dgvMain,
                    out btnAddSelected, out btnCancel);
                SetAddingFormEvents();
            }

            SetupCategoryComboBox();
        }
        private async Task LoadObjects()
        {
            _displayedObjects = await Task.Run(() => dbCon.GetObjectList<TechTransition>()
                .Select(obj => new DisplayedTechTransition(obj))
                .OrderBy(obj => obj.Category)
                .ThenBy(obj => obj.Name)
                .ToList());

            paginationService = new PaginationControlService<DisplayedTechTransition>(30, _displayedObjects);

            FilteringObjects();
            //_bindingList = new BindingList<DisplayedTechTransition>(_displayedObjects);
            ////_bindingList.ListChanged += BindingList_ListChanged;

            //dgvMain.DataSource = null;
            //dgvMain.DataSource = _bindingList;

            SetDGVColumnsSettings();
        }

        private void UpdateDisplayedData()
        {
            // Расчет отображаемых записей

            _bindingList = new BindingList<DisplayedTechTransition>(paginationService.GetPageData());
            dgvMain.DataSource = _bindingList;
            dgvMain.ResizeRows(20);

            // Подготовка данных для события
            PageInfo = paginationService.GetPageInfo();

            // Вызов события с подготовленными данными
            RaisePageInfoChanged();
        }

        private async void Win7_TechTransition_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (CloseFormsNoSave)
            //{
            //    return;
            //}
            //if (_newObjects.Count + _changedObjects.Count + _deletedObjects.Count != 0)
            //{
            //    e.Cancel = true;
            //    var result = MessageBox.Show("Сохранить изменения перед закрытием?", "Сохранение", MessageBoxButtons.YesNo);

            //    if (result == DialogResult.Yes)
            //    {
            //        await SaveChanges();
            //    }
            //    e.Cancel = false;
            //    Close();
            //}
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
            //    _bindingList,
            //    _newObjects,
            //    dgvMain);

            var objEditor = new Win7_TechTransitionEditor(new TechTransition(), isNewObject: true);

            objEditor.AfterSave = async (createdObj) => AddNewObjectInDataGridView(createdObj);

            objEditor.ShowDialog();
        }

        private async void btnDeleteObj_Click(object sender, EventArgs e)
        {
            await DisplayedEntityHelper.DeleteSelectedObject<DisplayedTechTransition, TechTransition>(dgvMain,
                _bindingList, _isFiltered ? _displayedObjects : null);
        }
        /////////////////////////////////////////////// * SaveChanges * ///////////////////////////////////////////
        //public bool HasChanges => _changedObjects.Count + _newObjects.Count + _deletedObjects.Count != 0;
        //public async Task SaveChanges()
        //{
        //    // stop editing cell
        //    dgvMain.EndEdit();
        //    if (!HasChanges)
        //    {
        //        return;
        //    }
        //    if (_newObjects.Count > 0)
        //    {
        //        await SaveNewObjects();
        //    }
        //    if (_changedObjects.Count > 0)
        //    {
        //        await SaveChangedObjects();
        //    }
        //    if (_deletedObjects.Count > 0)
        //    {
        //        await DeleteDeletedObjects();
        //    }
        //    // todo - change id in all new cards 
        //    dgvMain.Refresh();
        //}
        //private async Task SaveNewObjects()
        //{
        //    var newObjects = _newObjects.Select(dtc => CreateNewObject(dtc)).ToList();

        //    await dbCon.AddObjectAsync(newObjects);

        //    // set new ids to new objects matched them by all params
        //    foreach (var newObj in _newObjects)
        //    {
        //        var newId = newObjects.Where(s =>
        //        s.Name == newObj.Name
        //        //&& s.Category == newObj.Category
        //        ).FirstOrDefault().Id;
        //        newObj.Id = newId;
        //    }


        //    _newObjects.Clear();
        //}
        //private async Task SaveChangedObjects()
        //{
        //    var changedTcs = _changedObjects.Select(dtc => CreateNewObject(dtc)).ToList();

        //    await dbCon.UpdateObjectsListAsync(changedTcs);

        //    _changedObjects.Clear();
        //}

        //private async Task DeleteDeletedObjects()
        //{
        //    var deletedTcIds = _deletedObjects.Select(dtc => dtc.Id).ToList();

        //    var deletedCheck = await dbCon.DeleteObjectAsync<TechTransition>(deletedTcIds);
        //    if (!deletedCheck)
        //    {
        //        // add deleted object to display list if it was not deleted in DB
        //        foreach (var deletedObj in _deletedObjects)
        //        {
        //            _bindingList.Insert(0, deletedObj);
        //        }
        //        dgvMain.Refresh();
        //    }
        //    _deletedObjects.Clear();
        //}
        //private TechTransition CreateNewObject(DisplayedTechTransition dObj)
        //{
        //    return new TechTransition
        //    {
        //        Id = dObj.Id,
        //        Name = dObj.Name,
        //        Category = dObj.Category,
        //    };
        //}

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
                nameof(DisplayedTechTransition.Id),
                nameof(DisplayedTechTransition.Category),
                nameof(DisplayedTechTransition.Name),
                nameof(DisplayedTechTransition.TimeExecution),
                nameof(DisplayedTechTransition.TimeExecutionChecked),
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            dgvMain.Columns[nameof(DisplayedTechTransition.Id)].ReadOnly = true;
        }
        private void ConfigureDgvWithComboBoxColumn()
        {
            DataGridViewComboBoxColumn cmbColumn = new DataGridViewComboBoxColumn();
            cmbColumn.HeaderText = "Тип карты";
            cmbColumn.Name = nameof(DisplayedTechTransition.Category);

            cmbColumn.DataPropertyName = nameof(DisplayedTechTransition.Category);

            cmbColumn.FlatStyle = FlatStyle.Flat;

            dgvMain.Columns.Add(cmbColumn);
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
            //var selectedRows = dgvMain.Rows.Cast<DataGridViewRow>().Where(r => Convert.ToBoolean(r.Cells["Selected"].Value) == true).ToList();
            //if (selectedRows.Count == 0)
            //{
            //    MessageBox.Show("Выберите строки для добавления");
            //    return;
            //}
            //// get selected objects
            //var selectedObjs = selectedRows.Select(r => r.DataBoundItem as TechOperation).ToList();
            //// find opened form
            //var tcEditor = Application.OpenForms.OfType<Win6_TechOperation>().FirstOrDefault();

            //tcEditor.AddNewObjects(selectedObjs);

            //// close form
            //this.Close();
        }
        void BtnCancel_Click(object sender, EventArgs e)
        {
            // close form
            this.Close();
        }

        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            DisplayedEntityHelper.ListChangedEventHandler<DisplayedTechTransition>
                (e, _bindingList, _newObjects, _changedObjects, ref _newObject);
        }


        private class DisplayedTechTransition : INotifyPropertyChanged, IDisplayedEntity, IIdentifiable
        {
            public Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
                    {
                        { nameof(Id), "ID" },
                        { nameof(Name), "Наименование" },
                        { nameof(Category), "Категория" },
                        { nameof(TimeExecution), "Время выполнения" },
                        { nameof(TimeExecutionChecked), "Время проверено" },
                        { nameof(CommentName), "Комментарий к наименованию" },
                        { nameof(CommentTimeExecution), "Комментарий к времени выполнения" },
                    };
            }
            public List<string> GetPropertiesOrder()
            {
                return new List<string>
                {
                    nameof(Id),
                    nameof(Category),
                    nameof(Name),
                    nameof(TimeExecution),
                    nameof(TimeExecutionChecked),
                    nameof(CommentName),
                    nameof(CommentTimeExecution),
                };
            }
            public List<string> GetRequiredFields()
            {
                return new List<string>
                {
                    nameof(Name) ,
                    nameof(Category),
                    // nameof(TimeExecution),
                };
            }

            private int id;
            private string name;
            private string category;
            private double timeExecution = 0;
            private bool timeExecutionChecked;
            private string? commentName;
            private string? commentTimeExecution;

            private bool isReleased;
            private int? createdTCId;

            public DisplayedTechTransition()
            {

            }
            public DisplayedTechTransition(TechTransition obj)
            {
                Id = obj.Id;
                Name = obj.Name;
                TimeExecution = obj.TimeExecution;
                Category = obj.Category;
                TimeExecutionChecked = obj.TimeExecutionChecked ?? false;
                CommentName = obj.CommentName;
                CommentTimeExecution = obj.CommentTimeExecution;

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

            public string? Category
            {
                get => category;
                set
                {
                    if (category != value)
                    {
                        category = value;
                        OnPropertyChanged(nameof(Category));
                    }
                }
            }

            public double TimeExecution
            {
                get => timeExecution;
                set
                {
                    if (timeExecution != value)
                    {
                        timeExecution = value;
                        OnPropertyChanged(nameof(TimeExecution));
                    }
                }
            }

            public bool TimeExecutionChecked
            {
                get => timeExecutionChecked;
                set
                {
                    if (timeExecutionChecked != value)
                    {
                        timeExecutionChecked = value;
                        OnPropertyChanged(nameof(TimeExecutionChecked));
                    }
                }
            }

            public string? CommentName
            {
                get => commentName;
                set
                {
                    if (commentName != value)
                    {
                        commentName = value;
                        OnPropertyChanged(nameof(commentName));
                    }
                }
            }

            public string? CommentTimeExecution
            {
                get => commentTimeExecution;
                set
                {
                    if (commentTimeExecution != value)
                    {
                        commentTimeExecution = value;
                        OnPropertyChanged(nameof(commentTimeExecution));
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
                var categoryFilter = cbxCategoryFilter.SelectedItem?.ToString();
                var displayedTechTransitionList = new BindingList<DisplayedTechTransition>();

                if (string.IsNullOrWhiteSpace(searchText) && categoryFilter == "Все" && !cbxShowUnReleased.Checked)
                {
                    displayedTechTransitionList = new BindingList<DisplayedTechTransition>(_displayedObjects.Where(obj => obj.IsReleased == true).ToList()); // Возвращаем исходный список, если строка поиска пуста
                }
                else
                {
                    displayedTechTransitionList = FilteredBindingList(searchText, categoryFilter);
                }
                //dgvMain.DataSource = _bindingList;

                paginationService.SetAllObjectList(displayedTechTransitionList.ToList());
                UpdateDisplayedData();

            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }

        }
        private BindingList<DisplayedTechTransition> FilteredBindingList(string searchText, string categoryFilter)
        {
            var filteredList = _displayedObjects.Where(obj =>
                        (searchText == ""
                        ||
                            (obj.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.Category?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                        )
                        &&
                        (categoryFilter == "Все" || obj.Category?.ToString() == categoryFilter) &&

                            (obj.IsReleased == !cbxShowUnReleased.Checked)
                        ).ToList();

            return new BindingList<DisplayedTechTransition>(filteredList);
        }

        private void cbxCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilteringObjects();
            // set combobox width to item length
            var width = TextRenderer.MeasureText(cbxCategoryFilter.SelectedItem.ToString(), cbxCategoryFilter.Font).Width + 20;
            cbxCategoryFilter.Width = width < 160 ? 160 : width;
        }
        private void SetupCategoryComboBox()
        {
            // Set unique categories to combobox from binding list
            var categories = _displayedObjects.Select(obj => obj.Category).Distinct().ToList();
            categories.Sort();

            cbxCategoryFilter.Items.Add("Все");
            foreach (var category in categories)
            {
                if (string.IsNullOrWhiteSpace(category)) { continue; }
                cbxCategoryFilter.Items.Add(category);
            }

            cbxCategoryFilter.SelectedIndex = 0;

            cbxCategoryFilter.DropDownWidth = cbxCategoryFilter.Items.Cast<string>().Max(s => TextRenderer.MeasureText(s, cbxCategoryFilter.Font).Width) + 20;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return;
            }

            var selectedObj = dgvMain.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            var obj = selectedObj?.DataBoundItem as DisplayedTechTransition;

            if (obj != null)
            {
                var TP = dbCon.GetObject<TechTransition>(obj.Id);

                if (TP != null)
                {
                    var objEditor = new Win7_TechTransitionEditor(TP);

                    objEditor.AfterSave = async (updatedObj) => UpdateObjectInDataGridView(updatedObj);

                    objEditor.ShowDialog();
                }
            }
        }
        private void UpdateObjectInDataGridView(TechTransition modelObject)
        {
            // Обновляем объект в DataGridView
            var editedObject = _displayedObjects.FirstOrDefault(obj => obj.Id == modelObject.Id);
            if (editedObject != null)
            {
                editedObject.Name = modelObject.Name;
                editedObject.TimeExecution = modelObject.TimeExecution;
                editedObject.Category = modelObject.Category;
                editedObject.TimeExecutionChecked = modelObject.TimeExecutionChecked ?? false;
                editedObject.CommentName = modelObject.CommentName;
                editedObject.CommentTimeExecution = modelObject.CommentTimeExecution;

                editedObject.IsReleased = modelObject.IsReleased;
                editedObject.CreatedTCId = modelObject.CreatedTCId;

                FilteringObjects();
            }

        }

        private void AddNewObjectInDataGridView(TechTransition modelObject)
        {
            var newDisplayedObject = Activator.CreateInstance<DisplayedTechTransition>();
            if (newDisplayedObject is DisplayedTechTransition displayedObject)
            {
                displayedObject.Id = modelObject.Id;
                displayedObject.Name = modelObject.Name;
                displayedObject.TimeExecution = modelObject.TimeExecution;
                displayedObject.Category = modelObject.Category;
                displayedObject.TimeExecutionChecked = modelObject.TimeExecutionChecked ?? false;
                displayedObject.CommentName = modelObject.CommentName;
                displayedObject.CommentTimeExecution = modelObject.CommentTimeExecution;

                displayedObject.IsReleased = modelObject.IsReleased;
                displayedObject.CreatedTCId = modelObject.CreatedTCId;

                _displayedObjects.Insert(0, displayedObject);
                FilteringObjects();

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

        private void Win7_TechTransition_SizeChanged(object sender, EventArgs e)
        {
            dgvMain.ResizeRows(20);
        }
    }
}
