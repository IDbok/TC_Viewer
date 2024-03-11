using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win8_TechTransition : Form, ISaveEventForm
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
        public Win8_TechTransition(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }
        public Win8_TechTransition()
        {
            InitializeComponent();
        }

        private async void Win8_TechTransition_Load(object sender, EventArgs e)
        {
            await LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedTechTransition>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            if (isAddingForm)
            {
                //isAddingFormSetControls();
                WinProcessing.SetAddingFormControls(pnlControlBtns, dgvMain,
                    out btnAddSelected, out btnCancel);
                SetAddingFormEvents();
            }
        }
        private async Task LoadObjects()
        {
            var tcList = await Task.Run(() => dbCon.GetObjectList<TechTransition>()
                .Select(obj => new DisplayedTechTransition(obj)).ToList());
            _bindingList = new BindingList<DisplayedTechTransition>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            SetDGVColumnsSettings();
            // ConfigureDgvWithComboBoxColumn();
        }
        private async void Win8_TechTransition_FormClosing(object sender, FormClosingEventArgs e)
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

            await dbCon.DeleteObjectAsync<TechOperation>(deletedTcIds);
            _deletedObjects.Clear();
        }
        private TechOperation CreateNewObject(DisplayedTechTransition dObj)
        {
            return new TechOperation
            {
                Id = dObj.Id,
                Name = dObj.Name,
                Category = dObj.Category == true ? "Типовая ТО" : "ТО",
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
                        { nameof(Category), "Типовое ТО" },
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
                    nameof(Category),
                };
            }

            private int id;
            private string name;
            private bool category;

            public DisplayedTechTransition()
            {

            }
            public DisplayedTechTransition(TechTransition obj)
            {
                Id = obj.Id;
                Name = obj.Name;
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

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
