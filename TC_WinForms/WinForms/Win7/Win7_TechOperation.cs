using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TcDbConnector;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms
{
    public partial class Win7_TechOperation : Form, ISaveEventForm, IPaginationControl
    {
        private readonly User.Role _accessLevel;
        private readonly int _minRowHeight = 20;
        private DbConnector dbCon = new DbConnector();

        private List<DisplayedTechOperation> _displayedObjects;
        private BindingList<DisplayedTechOperation> _bindingList;

        private List<DisplayedTechOperation> _changedObjects = new List<DisplayedTechOperation>();
        private List<DisplayedTechOperation> _newObjects = new List<DisplayedTechOperation>();
        private List<DisplayedTechOperation> _deletedObjects = new List<DisplayedTechOperation>();

        private DisplayedTechOperation _newObject;

        private bool isAddingForm = false;
        private Button btnAddSelected;
        private Button btnCancel;

        PaginationControlService<DisplayedTechOperation> paginationService;
        public event EventHandler<PageInfoEventArgs> PageInfoChanged;
        public PageInfoEventArgs? PageInfo { get; set; }

        public bool CloseFormsNoSave { get; set; } = false;
        public void SetAsAddingForm()
        {
            isAddingForm = true;
        }
        public Win7_TechOperation(User.Role accessLevel)
        {
            _accessLevel = accessLevel;

            InitializeComponent();
            AccessInitialization();
            dgvMain.DoubleBuffered(true);

        }
        public Win7_TechOperation()
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
        private async void Win7_TechOperation_Load(object sender, EventArgs e)
        {
            await LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedTechOperation>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            if (isAddingForm)
            {
                //isAddingFormSetControls();
                WinProcessing.SetAddingFormControls(pnlControlBtns, dgvMain,
                    out btnAddSelected, out btnCancel);
                SetAddingFormEvents();
            }

            SetupCategoryComboBox();
            dgvMain.ResizeRows(_minRowHeight);

        }
        private async Task LoadObjects()
        {
            _displayedObjects = await Task.Run(() => dbCon.GetObjectList<TechOperation>()
                .Select(obj => new DisplayedTechOperation(obj))
                //.OrderBy(obj => obj.Category)
                .OrderBy(obj => obj.Name)
                .ToList());

            paginationService = new PaginationControlService<DisplayedTechOperation>(50, _displayedObjects);

            FilteringObjects();

            //var tcList = await Task.Run(() => dbCon.GetObjectList<TechOperation>()
            //    .Select(obj => new DisplayedTechOperation(obj)).ToList());

            //_bindingList = new BindingList<DisplayedTechOperation>(tcList);
            //_bindingList.ListChanged += BindingList_ListChanged;
            //dgvMain.DataSource = _bindingList;


            SetDGVColumnsSettings();
        }

        private void UpdateDisplayedData()
        {
            // Расчет отображаемых записей

            _bindingList = new BindingList<DisplayedTechOperation>(paginationService.GetPageData());
            dgvMain.DataSource = _bindingList;
            dgvMain.ResizeRows(_minRowHeight);

            // Подготовка данных для события
            PageInfo = paginationService.GetPageInfo();

            // Вызов события с подготовленными данными
            RaisePageInfoChanged();
        }

        private async void Win7_TechOperation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseFormsNoSave)
            {
                return;
            }
            if (HasChanges)
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

            Win7_TechOperation_Window win7_TechOperation_Window = new Win7_TechOperation_Window();
            win7_TechOperation_Window.ShowDialog();

        }

        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            //DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
            //    _bindingList, _newObjects, _deletedObjects);

            if (dgvMain.SelectedRows.Count > 0)
            {
                var selectedDTCs = dgvMain.SelectedRows.Cast<DataGridViewRow>()
                    .Select(row => row.DataBoundItem as DisplayedTechOperation)
                    .Where(dtc => dtc != null)
                    .ToList();

                string message = "Вы действительно хотите удалить?\n";
                DialogResult result = MessageBox.Show(message, "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var context = new MyDbContext();
                    foreach (var row in selectedDTCs)
                    {
                        context.TechOperations.Remove(context.TechOperations.Single(s => s.Id == row.Id));
                    }
                    context.SaveChangesAsync();

                    foreach (var dtc in selectedDTCs)
                    {
                        _bindingList.Remove(dtc);
                        //_deletedObjects.Add(dtc);

                        if (_newObjects.Contains(dtc)) // if new card was deleted, remove it from new cards list
                        {
                            _newObjects.Remove(dtc);
                        }
                    }
                }

                dgvMain.Refresh();
            }

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
                //&& s.Category == newObj.Category
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

            await dbCon.DeleteObjectAsync<TechOperation>(deletedTcIds);
            _deletedObjects.Clear();
        }
        private TechOperation CreateNewObject(DisplayedTechOperation dObj)
        {
            return new TechOperation
            {
                Id = dObj.Id,
                Name = dObj.Name,
                Category = dObj.Category == true ? "Типовая ТО" : "ТО",
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count == 1)
            {
                var selectedRow = dgvMain.SelectedRows[0];
                int id = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                if (id != 0)
                {
                    Win7_TechOperation_Window win71TCsWindow = new Win7_TechOperation_Window(id);
                    win71TCsWindow.Show();
                }
            }
            else
            {
                MessageBox.Show("Выберите строчку для редактирования.");
            }
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
                nameof(DisplayedTechOperation.Id),
                nameof(DisplayedTechOperation.Category),
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            dgvMain.Columns[nameof(DisplayedTechOperation.Id)].ReadOnly = true;
        }
        private void ConfigureDgvWithComboBoxColumn()
        {
            DataGridViewComboBoxColumn cmbColumn = new DataGridViewComboBoxColumn();
            cmbColumn.HeaderText = "Тип карты";
            cmbColumn.Name = nameof(DisplayedTechOperation.Category);

            cmbColumn.DataPropertyName = nameof(DisplayedTechOperation.Category);

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
            DisplayedEntityHelper.ListChangedEventHandler<DisplayedTechOperation>
                (e, _bindingList, _newObjects, _changedObjects, ref _newObject);
        }



        private class DisplayedTechOperation : INotifyPropertyChanged, IDisplayedEntity, IIdentifiable
        {
            public Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
                    {
                        { nameof(Id), "ID" },
                        { nameof(Name), "Наименование" },
                        { nameof(Category), "Типовая ТО" },
                    };
            }
            public List<string> GetPropertiesOrder()
            {
                return new List<string>
                {
                    nameof(Id),
                    nameof(Name),
                    nameof(Category),
                };
            }
            public List<string> GetRequiredFields()
            {
                return new List<string>
                {
                    nameof(Name) ,
                };
            }

            private int id;
            private string name;
            private bool category;

            private bool isReleased;
            private int? createdTCId;

            public DisplayedTechOperation()
            {

            }
            public DisplayedTechOperation(TechOperation obj)
            {
                Id = obj.Id;
                Name = obj.Name;
                Category = obj.Category == "Типовая ТО" ? true : false;

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

            public bool Category
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
                var displayedTechOperationList = new BindingList<DisplayedTechOperation>();

                if (string.IsNullOrWhiteSpace(searchText) && categoryFilter == "Все" && !cbxShowUnReleased.Checked)
                {
                    displayedTechOperationList = new BindingList<DisplayedTechOperation>(_displayedObjects.Where(obj => obj.IsReleased == true).ToList()); ; // Возвращаем исходный список, если строка поиска пуста
                }
                else
                {
                    displayedTechOperationList = FilteredBindingList(searchText, categoryFilter);//new BindingList<DisplayedProtection>(filteredList);
                }
                //dgvMain.DataSource = _bindingList;

                paginationService.SetAllObjectList(displayedTechOperationList.ToList());
                UpdateDisplayedData();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }

        }
        private BindingList<DisplayedTechOperation> FilteredBindingList(string searchText, string categoryFilter)
        {
            var filteredList = _displayedObjects.Where(obj =>
                    (searchText == ""
                        || (obj.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                    && (categoryFilter == "Все" || obj.Category == (categoryFilter == "Типовая ТО"))
                    && (obj.IsReleased == !cbxShowUnReleased.Checked)
                    ).ToList();
            //(categoryFilter != "Все"
            //            ?
            //                (
            //                    (categoryFilter == "Типовая ТО" ? obj.Category == true : false) ||
            //                    (categoryFilter == "ТО" ? obj.Category == false : false)
            //                )
            //            : (obj.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
            //            )

            //            ).ToList();

            return new BindingList<DisplayedTechOperation>(filteredList);
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
            var categories = _displayedObjects.Select(obj => obj).Distinct().ToList();

            cbxCategoryFilter.Items.Add("Все");
            cbxCategoryFilter.Items.Add("ТО");
            cbxCategoryFilter.Items.Add("Типовая ТО");

            cbxCategoryFilter.SelectedIndex = 0;

            cbxCategoryFilter.DropDownWidth = cbxCategoryFilter.Items.Cast<string>().Max(s => TextRenderer.MeasureText(s, cbxCategoryFilter.Font).Width) + 20;
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

        private void Win7_TechOperation_SizeChanged(object sender, EventArgs e)
        {
            dgvMain.ResizeRows(_minRowHeight);
        }
    }
}
