using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win7_TechTransition : Form, ISaveEventForm
    {
        private DbConnector dbCon = new DbConnector();
        private BindingList<DisplayedTechTransition> _bindingList;

        private List<DisplayedTechTransition> _changedObjects = new List<DisplayedTechTransition>();
        private List<DisplayedTechTransition> _newObjects = new List<DisplayedTechTransition>();
        private List<DisplayedTechTransition> _deletedObjects = new List<DisplayedTechTransition>();

        private DisplayedTechTransition _newObject;

        private bool isAddingForm = false;
        private Button btnAddSelected;
        private Button btnCancel;
        public void SetAsAddingForm()
        {
            isAddingForm = true;
        }
        public Win7_TechTransition(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }
        public Win7_TechTransition()
        {
            InitializeComponent();
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
            var tcList = await Task.Run(() => dbCon.GetObjectList<TechTransition>()
                .Select(obj => new DisplayedTechTransition(obj))
                .OrderBy(obj => obj.Category)
                .ThenBy(obj => obj.Name)
                .ToList());
            _bindingList = new BindingList<DisplayedTechTransition>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            SetDGVColumnsSettings();
        }
        private async void Win7_TechTransition_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            DisplayedEntityHelper.AddNewObjectToDGV(ref _newObject,
                _bindingList,
                _newObjects,
                dgvMain);
        }

        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
                _bindingList, _newObjects, _deletedObjects);
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

            var deletedCheck = await dbCon.DeleteObjectAsync<TechTransition>(deletedTcIds);
            if (!deletedCheck)
            {
                // add deleted object to display list if it was not deleted in DB
                foreach (var deletedObj in _deletedObjects)
                {
                    _bindingList.Insert(0, deletedObj);
                }
                dgvMain.Refresh();
            }
            _deletedObjects.Clear();
        }
        private TechTransition CreateNewObject(DisplayedTechTransition dObj)
        {
            return new TechTransition
            {
                Id = dObj.Id,
                Name = dObj.Name,
                Category = dObj.Category,
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


        private class DisplayedTechTransition : INotifyPropertyChanged, IDisplayedEntity
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
        private void FilterTechnologicalCards()
        {
            try
            {
                var searchText = txtSearch.Text == "Поиск" ? "" : txtSearch.Text;
                var categoryFilter = cbxCategoryFilter.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(searchText) && categoryFilter == "Все" )
                {
                    dgvMain.DataSource = _bindingList; // Возвращаем исходный список, если строка поиска пуста
                }
                else
                {
                    dgvMain.DataSource = FilteredBindingList(searchText, categoryFilter);
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }

        }
        private BindingList<DisplayedTechTransition> FilteredBindingList(string searchText, string categoryFilter)
        {
            var filteredList = _bindingList.Where(obj =>
                        (searchText == ""
                        ||

                                (obj.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                            || (obj.Category?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                        )
                        &&
                        (categoryFilter == "Все" || obj.Category?.ToString() == categoryFilter)
                        ).ToList();

            return new BindingList<DisplayedTechTransition>(filteredList);
        }

        private void cbxCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterTechnologicalCards();
            // set combobox width to item length
            var width = TextRenderer.MeasureText(cbxCategoryFilter.SelectedItem.ToString(), cbxCategoryFilter.Font).Width + 20;
            cbxCategoryFilter.Width = width < 160 ? 160 : width;
        }
        private void SetupCategoryComboBox()
        {
            // Set unique categories to combobox from binding list
            var categories = _bindingList.Select(obj => obj.Category).Distinct().ToList();

            cbxCategoryFilter.Items.Add("Все");
            foreach (var category in categories)
            {
                   if (string.IsNullOrWhiteSpace(category)) { continue; }
                cbxCategoryFilter.Items.Add(category);
            }

            cbxCategoryFilter.SelectedIndex = 0;

            cbxCategoryFilter.DropDownWidth = cbxCategoryFilter.Items.Cast<string>().Max(s => TextRenderer.MeasureText(s, cbxCategoryFilter.Font).Width) + 20;
        }
    }
}
