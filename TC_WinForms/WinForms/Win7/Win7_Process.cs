using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms
{
    public partial class Win7_Process : Form, ISaveEventForm, ILoadDataAsyncForm
    {
        private readonly User.Role _accessLevel;

        private DbConnector dbCon = new DbConnector();
        private BindingList<DisplayedTool> _bindingList;

        private List<DisplayedTool> _changedObjects = new List<DisplayedTool>();
        private List<DisplayedTool> _newObjects = new List<DisplayedTool>();
        private List<DisplayedTool> _deletedObjects = new List<DisplayedTool>();

        private DisplayedTool _newObject;

        private bool isAddingForm = false;
        private Button btnAddSelected;
        private Button btnCancel;

        public bool _isDataLoaded = false;
        public bool CloseFormsNoSave { get; set; } = false;
        public void SetAsAddingForm()
        {
            isAddingForm = true;
        }

        public Win7_Process(User.Role accessLevel)
        {
            _accessLevel = accessLevel;

            InitializeComponent();
            AccessInitialization();
        }
        public Win7_Process()
        {
            InitializeComponent();
        }


        private void AccessInitialization()
        {
            var controlAccess = new Dictionary<User.Role, Action>
            {
                //[User.Role.Lead] = () => { },

                [User.Role.Implementer] = () => 
                { 
                    btnAddNew.Visible = false;
                    btnDeleteObj.Visible = false;

                    btnUpdate.Text = "Просмотр"; 
                    btnUpdate.Location = btnDeleteObj.Location;
                },

                //[User.Role.ProjectManager] = () =>
                //{
                //    updateToolStripMenuItem.Visible = false;
                //},

                //[User.Role.User] = () =>
                //{
                //    updateToolStripMenuItem.Visible = false;
                //}
            };

            controlAccess.TryGetValue(_accessLevel, out var action);
            action?.Invoke();
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
        private async void Win7_6_Tool_Load(object sender, EventArgs e)
        {
            progressBar.Visible = true;

            if (!_isDataLoaded)
                await LoadDataAsync();

            SetDGVColumnsSettings();

            DisplayedEntityHelper.SetupDataGridView<DisplayedTool>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            if (isAddingForm)
            {
                //isAddingFormSetControls();
                WinProcessing.SetAddingFormControls(pnlControlBtns, dgvMain,
                    out btnAddSelected, out btnCancel);
                SetAddingFormEvents();
            }

            progressBar.Visible = false;
        }
        private async Task LoadObjects()
        {
            var tcList = await Task.Run(() => dbCon.GetObjectList<TechnologicalProcess>()
                .Select(obj => new DisplayedTool(obj)).ToList());
            _bindingList = new BindingList<DisplayedTool>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            SetDGVColumnsSettings();
        }
        public async Task LoadDataAsync()
        {
            var tcList = await Task.Run(() => dbCon.GetObjectList<TechnologicalProcess>()
                .Select(obj => new DisplayedTool(obj)).ToList());

            _bindingList = new BindingList<DisplayedTool>(tcList);

            dgvMain.DataSource = null; // cancel update of dgv while data is loading
            _bindingList.ListChanged += BindingList_ListChanged;

            dgvMain.DataSource = _bindingList;

            _isDataLoaded = true;
        }
        private async void Win7_6_Tool_FormClosing(object sender, FormClosingEventArgs e)
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

        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            //DisplayedEntityHelper.AddNewObjectToDGV(ref _newObject,
            //    _bindingList,
            //    _newObjects,
            //    dgvMain);

            Win7_ProcessEdit win7_ProcessEdit = new Win7_ProcessEdit();
            win7_ProcessEdit.ShowDialog();
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
            var newObjects = _newObjects.Select(dObj => CreateNewObject(dObj)).ToList();

            await dbCon.AddObjectAsync(newObjects);

            // set new ids to new objects matched them by all params
            foreach (var newObj in _newObjects)
            {
                var newId = newObjects.Where(s =>
                s.Name == newObj.Name
                && s.Type == newObj.Type
                && s.Description == newObj.Description
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

            await dbCon.DeleteObjectAsync<TechnologicalProcess>(deletedTcIds);
            _deletedObjects.Clear();
        }
        private TechnologicalProcess CreateNewObject(DisplayedTool dObj)
        {
            return new TechnologicalProcess
            {
                Id = dObj.Id,
                Name = dObj.Name,
                Type = dObj.Type,
                Description = dObj.Description
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
                nameof(DisplayedTool.Id)
                //nameof(DisplayedTool.ClassifierCode),
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            //dgvMain.Columns[nameof(DisplayedTool.Price)].Width = 120;
            //dgvMain.Columns[nameof(DisplayedTool.ClassifierCode)].Width = 150;

            //dgvMain.Columns[nameof(DisplayedTool.Id)].ReadOnly = true;
            //dgvMain.Columns[nameof(DisplayedTool.ClassifierCode)].ReadOnly = true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /////////////////////////////////////////// * isAddingFormMethods and Events * ///////////////////////////////////////////

        void SetAddingFormEvents()
        {
            //btnAddSelected.Click += BtnAddSelected_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        //void BtnAddSelected_Click(object sender, EventArgs e)
        //{
        //    // get selected rows
        //    var selectedRows = dgvMain.Rows.Cast<DataGridViewRow>()
        //        .Where(r => Convert.ToBoolean(r.Cells["Selected"].Value) == true).ToList();
        //    if (selectedRows.Count == 0)
        //    {
        //        MessageBox.Show("Выберите строки для добавления");
        //        return;
        //    }
        //    // get selected objects
        //    var selectedObjs = selectedRows.Select(r => r.DataBoundItem as DisplayedTool).ToList();
        //    var newItems = new List<Tool>();
        //    foreach (var obj in selectedObjs)
        //    {
        //        newItems.Add(CreateNewObject(obj));
        //    }

        //    // find opened form
        //    var tcEditor = Application.OpenForms.OfType<Win6_Tool>().FirstOrDefault();

        //    tcEditor.AddNewObjects(newItems);

        //    // close form
        //    this.Close();
        //}
        void BtnCancel_Click(object sender, EventArgs e)
        {
            // close form
            this.Close();
        }

        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            DisplayedEntityHelper.ListChangedEventHandler<DisplayedTool>
                (e, _bindingList, _newObjects, _changedObjects, ref _newObject);
        }



        private class DisplayedTool : INotifyPropertyChanged, IDisplayedEntity
        {
            public Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
            {
                { nameof(Id), "ID" },
                { nameof(Name), "Наименование" },
                { nameof(Type), "Тип" },
                { nameof(Description), "Описание" }
            };
            }
            public List<string> GetPropertiesOrder()
            {
                return new List<string>
                {
                    nameof(Id),
                    nameof(Name),
                    nameof(Type),
                    nameof(Description)
                };
            }
            public List<string> GetRequiredFields()
            {
                return new List<string>
                {
                    nameof(Name) ,
                    nameof(Type)
                };
            }

            private int id;
            private string name;
            private string? type;
            private string? description;

            public DisplayedTool()
            {

            }
            public DisplayedTool(TechnologicalProcess obj)
            {
                Id = obj.Id;
                Name = obj.Name;
                Type = obj.Type;
                Description = obj.Description;
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

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    dgvMain.DataSource = _bindingList; // Возвращаем исходный список, если строка поиска пуста
                }
                else
                {
                    var filteredList = _bindingList.Where(obj =>
                            (obj.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.Type?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                        ).ToList();

                    dgvMain.DataSource = new BindingList<DisplayedTool>(filteredList);
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count == 1)
            {
                var selectedRow = dgvMain.SelectedRows[0];
                int id = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                if (id != 0)
                {
                    Win7_ProcessEdit win71TCsWindow = new Win7_ProcessEdit(id, _accessLevel);
                    win71TCsWindow.Show();
                }
            }
            else
            {
                MessageBox.Show("Выберите одну карту для редактирования.");
            }
        }
    }
}
