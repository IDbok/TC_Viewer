using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win7_3_Staff : Form, ISaveEventForm
    {

        private DbConnector dbCon = new DbConnector();
        private BindingList<DisplayedStaff> _bindingList;

        private List<DisplayedStaff> _changedObjects = new List<DisplayedStaff>();
        private List<DisplayedStaff> _newObjects = new List<DisplayedStaff>();
        private List<DisplayedStaff> _deletedObjects = new List<DisplayedStaff>();

        private DisplayedStaff _newObject;

        private bool isAddingForm = false;
        private Button btnAddSelected;
        private Button btnCancel;
        private Form _openedForm;
        public void SetAsAddingForm()
        {
            isAddingForm = true;
        }
        
        public Win7_3_Staff(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }

        public Win7_3_Staff(Form openedForm) // this constructor is for adding form in TC editer
        {
            _openedForm= openedForm;
            InitializeComponent();
        }

        private async void Win7_3_Staff_Load(object sender, EventArgs e)
        {
            progressBar.Visible = true;

            await LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedStaff>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false; // TODO: change it when add deleting by "del" Key press

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
            var tcList = await Task.Run(() => dbCon.GetObjectList<Staff>()
                .Select(obj => new DisplayedStaff(obj)).ToList());
            _bindingList = new BindingList<DisplayedStaff>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            SetDGVColumnsSettings();
        }
        private void AccessInitialization(int accessLevel)
        {
            //var controlAccess = new Dictionary<int, Action>
            //{
            //    [0] = () => {  },
            //    [1] = () => {  },
            //    [2] = () => {  },
            //};
            //controlAccess.TryGetValue(accessLevel, out var action);
            //action?.Invoke();
        }

        private async void Win7_3_Staff_FormClosing(object sender, FormClosingEventArgs e)
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

        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            //AddNewObject();
            DisplayedEntityHelper.AddNewObjectToDGV(ref _newObject,
                _bindingList,
                _newObjects,
                dgvMain);
        }
        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            //DeletSelected();
            DisplayedEntityHelper.DeleteSelectedObject(dgvMain, _bindingList, _newObjects, _deletedObjects);
        }
        
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
            var newTcs = _newObjects.Select(dtc => CreateNewObject(dtc)).ToList();

            await dbCon.AddObjectAsync(newTcs);

            // set new ids to new objects matched them by all params
            foreach (var newCard in _newObjects)
            {
                var newId = newTcs.Where(s => s.Name == newCard.Name 
                && s.Type == newCard.Type 
                && s.Functions == newCard.Functions
                && s.Qualification == newCard.Qualification
                && s.CombineResponsibility == newCard.CombineResponsibility
                && s.Comment == newCard.Comment
                ).FirstOrDefault().Id;
                newCard.Id = newId;
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

            await dbCon.DeleteObjectAsync<Staff>(deletedTcIds);
            _deletedObjects.Clear();
        }
        private Staff CreateNewObject(DisplayedStaff dtc)
        {
            return new Staff
            {
                Id = dtc.Id,
                Name = dtc.Name,
                Type = dtc.Type,
                Functions = dtc.Functions,
                CombineResponsibility = dtc.CombineResponsibility,
                Qualification = dtc.Qualification,
                Comment = dtc.Comment,
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
                nameof(DisplayedStaff.Id),
                //nameof(DisplayedStaff.Name),
                //nameof(DisplayedStaff.Type),
                //nameof(DisplayedStaff.Functions),
                //nameof(DisplayedStaff.CombineResponsibility),
                //nameof(DisplayedStaff.Qualification),
                //nameof(DisplayedStaff.Comment),
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }


            dgvMain.Columns[nameof(DisplayedStaff.Id)].ReadOnly = true;
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
            var selectedObjs = selectedRows.Select(r => r.DataBoundItem as DisplayedStaff).ToList();
            var newItems = new List<Staff>();
            foreach ( var obj in selectedObjs)
            {
                newItems.Add(CreateNewObject(obj));
            }
            // find opened form
            if (_openedForm is Win6_Staff tcEditor)
            {
                tcEditor.AddNewObjects(newItems);
            }

            Close();
        }
        void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            DisplayedEntityHelper.ListChangedEventHandler<DisplayedStaff>
                (e, _bindingList, _newObjects, _changedObjects, ref _newObject);
        }


        private class DisplayedStaff : INotifyPropertyChanged, IDisplayedEntity
        {
            public Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
            {
                { nameof(Id), "ID" },
                { nameof(Name), "Название" },
                { nameof(Type), "Тип" },
                { nameof(Functions), "Функции" },
                { nameof(CombineResponsibility), "Возможность совмещения обязанностей" },
                { nameof(Qualification), "Квалификация" },
                { nameof(Comment), "Комментарии" },
            };
            }
            public List<string> GetPropertiesOrder()
            {
                return new List<string>
                {
                    nameof(Id),
                    nameof(Name),
                    nameof(Type),
                    nameof(Functions),
                    nameof(CombineResponsibility),
                    nameof(Qualification),
                    nameof(Comment),
                };
            }
            public List<string> GetRequiredFields()
            {
                return new List<string>
                {
                    nameof(Name) ,
                    nameof(Type) ,
                    nameof(Functions) ,
                    nameof(Qualification),
                };
            }

            private int id;
            private string name;
            private string type;
            private string functions;
            private string? combineResponsibility;
            private string qualification;
            private string? comment;

            public DisplayedStaff()
            {

            }
            public DisplayedStaff(Staff obj)
            {
                Id = obj.Id;
                Name = obj.Name;
                Type = obj.Type;
                Functions = obj.Functions;
                CombineResponsibility = obj.CombineResponsibility;
                Qualification = obj.Qualification;
                Comment = obj.Comment;
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
            public string Functions
            {
                get => functions;
                set
                {
                    if (functions != value)
                    {
                        functions = value;
                        OnPropertyChanged(nameof(functions));
                    }
                }
            }
            public string? CombineResponsibility
            {
                get => combineResponsibility;
                set
                {
                    if (combineResponsibility != value)
                    {
                        combineResponsibility = value;
                        OnPropertyChanged(nameof(CombineResponsibility));
                    }
                }
            }
            public string Qualification
            {
                get => qualification;
                set
                {
                    if (qualification != value)
                    {
                        qualification = value;
                        OnPropertyChanged(nameof(Qualification));
                    }
                }
            }
            public string? Comment
            {
                get => comment;
                set
                {
                    if (comment != value)
                    {
                        comment = value;
                        OnPropertyChanged(nameof(Comment));
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
