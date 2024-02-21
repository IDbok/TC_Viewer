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

        private List<DisplayedStaff> _changedCards = new List<DisplayedStaff>();
        private List<DisplayedStaff> _newCards = new List<DisplayedStaff>();
        private List<DisplayedStaff> _deletedCards = new List<DisplayedStaff>();

        private DisplayedStaff _newCard;


        private List<Staff> objList = new List<Staff>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<Staff> changedObjs = new List<Staff>();
        private Staff newObj;

        private bool isAddingForm = false;
        private Button btnAddSelected;
        private Button btnCancel;
        public void SetAsAddingForm()
        {
            isAddingForm = true;
        }
        
        public Win7_3_Staff(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }

        public Win7_3_Staff() // this constructor is for adding form in TC editer
        {
            InitializeComponent();
        }

        private async void Win7_3_Staff_Load(object sender, EventArgs e)
        {
            await LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedStaff>(dgvMain);

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
            var tcList = await Task.Run(() => dbCon.GetObjectList<Staff>()
                .Select(obj => new DisplayedStaff(obj)).ToList());
            _bindingList = new BindingList<DisplayedStaff>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;
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

        private void Win7_3_Staff_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (newObj != null)
            {
                if (MessageBox.Show("Сохранить новую запись?", "Сохранение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show("Сохраняю всё чт нужно в Staff ", "Сохранение");
                    //dbCon.AddObject(newObj);
                }
            }
        }

        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            AddNewObject();
        }
        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            DeletSelected();
        }
        private void AddNewObject()
        {

            if(DisplayedEntityHelper.AddNewObject(ref _newCard))
            {
                _newCards.Add(_newCard);
                _bindingList.Insert(0, _newCard);
                dgvMain.Refresh();
            }
            
        }
        private void DeletSelected()
        {
            DisplayedEntityHelper.DeleteSelectedObject(dgvMain, _bindingList, _newCards, _deletedCards);
            //if (dgvMain.SelectedRows.Count > 0)
            //{
            //    string message = "Вы действительно хотите удалить выбранные объекты?\n";
            //    DialogResult result = MessageBox.Show(message, "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            //    if (result == DialogResult.Yes)
            //    {
            //        var selectedDTCs = dgvMain.SelectedRows.Cast<DataGridViewRow>()
            //        .Select(row => row.DataBoundItem as DisplayedStaff)
            //        .Where(dtc => dtc != null)
            //        .ToList();

            //        foreach (var dtc in selectedDTCs)
            //        {
            //            _bindingList.Remove(dtc);
            //            _deletedCards.Add(dtc);

            //            if (_newCards.Contains(dtc)) // if new card was deleted, remove it from new cards list
            //            {
            //                _newCards.Remove(dtc);
            //            }
            //        }
            //    }

            //    dgvMain.Refresh();
            //}
        }
        public async Task SaveChanges()
        {
            // todo- check if in added tech card fulfilled all required fields
            if (_changedCards.Count == 0 && _newCards.Count == 0 && _deletedCards.Count == 0)
            {
                return;
            }
            if (_newCards.Count > 0)
            {
                await SaveNewTechnologicalCards();
            }
            if (_changedCards.Count > 0)
            {
                await SaveChangedTechnologicalCards();
            }
            if (_deletedCards.Count > 0)
            {
                await DeleteDeletedTechnologicalCards();
            }
            // todo - change id in all new cards 
        }
        private async Task SaveNewTechnologicalCards()
        {
            var newTcs = _newCards.Select(dtc => CreateNewObject(dtc)).ToList();

            await dbCon.AddObjectAsync(newTcs);

            MessageBox.Show("Новые карты сохранены.");
            _newCards.Clear();
        }
        private async Task SaveChangedTechnologicalCards()
        {
            var changedTcs = _changedCards.Select(dtc => CreateNewObject(dtc)).ToList();

            //await dbCon.UpdateTcListAsync(changedTcs);
            MessageBox.Show("Изменения сохранены.");
            _changedCards.Clear();
        }

        private async Task DeleteDeletedTechnologicalCards()
        {
            var deletedTcIds = _deletedCards.Select(dtc => dtc.Id).ToList();

            await dbCon.DeleteTcAsync(deletedTcIds);
            MessageBox.Show("Карты удалены.");
            _deletedCards.Clear();
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
        private void ColorizeEmptyRequiredCells() // todo - change call collore after value changed to non empty
        {
            DataGridViewRow row = dgvMain.Rows[0];
            var colNames = Staff.GetPropertiesRequired;
            foreach (var colName in colNames)
            {
                // get collumn index by name
                var colIndex = dgvMain.Columns[colName].Index;

                DGVProcessing.ColorizeCell(dgvMain, colIndex, row.Index, "Red");
            }


        }

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
            var selectedObjs = selectedRows.Select(r => r.DataBoundItem as Staff).ToList();
            // find opened form
            var tcEditor = Application.OpenForms.OfType<Win6_Staff>().FirstOrDefault();

            tcEditor.AddNewObjects(selectedObjs);

            // close form
            this.Close();
        }
        void BtnCancel_Click(object sender, EventArgs e)
        {
            // close form
            this.Close();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                if (_newCard != null && e.NewIndex == 0) // if changed _newCard check if all required fields are filled
                {
                    if (!DisplayedEntityHelper.IsValidNewCard(_newCard))
                    {
                        return;
                    }
                    _newCard = null;
                }

                if (_newCards.Contains(_bindingList[e.NewIndex])) // if changed new Objects don't add it to changed list
                {
                    return;
                }

                var changedItem = _bindingList[e.NewIndex];
                if (!_changedCards.Contains(changedItem))
                {
                    _changedCards.Add(changedItem);
                }
            }
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
