using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win7_TechOperation : Form, ISaveEventForm
    {
        private DbConnector dbCon = new DbConnector();
        private BindingList<DisplayedTechOperation> _bindingList;

        private List<DisplayedTechOperation> _changedObjects = new List<DisplayedTechOperation>();
        private List<DisplayedTechOperation> _newObjects = new List<DisplayedTechOperation>();
        private List<DisplayedTechOperation> _deletedObjects = new List<DisplayedTechOperation>();

        private DisplayedTechOperation _newObject;

        private bool isAddingForm = false;
        private Button btnAddSelected;
        private Button btnCancel;
        public void SetAsAddingForm()
        {
            isAddingForm = true;
        }
        public Win7_TechOperation(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }
        public Win7_TechOperation()
        {
            InitializeComponent();
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
        }
        private async Task LoadObjects()
        {
            var tcList = await Task.Run(() => dbCon.GetObjectList<TechOperation>()
                .Select(obj => new DisplayedTechOperation(obj)).ToList());
            _bindingList = new BindingList<DisplayedTechOperation>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;
        }
        private async void Win7_TechOperation_FormClosing(object sender, FormClosingEventArgs e)
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
        public async Task SaveChanges()
        {
            // todo - check if in added tech card fulfilled all required fields
            if (_changedObjects.Count == 0 && _newObjects.Count == 0 && _deletedObjects.Count == 0)
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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



        private class DisplayedTechOperation : INotifyPropertyChanged, IDisplayedEntity
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
            private bool? category;

            public DisplayedTechOperation()
            {

            }
            public DisplayedTechOperation(TechOperation obj)
            {
                Id = obj.Id;
                Name = obj.Name;
                Category = obj.Category == "Типовая ТО" ? true : false;
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

            public bool? Category
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
