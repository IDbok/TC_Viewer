using System.ComponentModel;
using TC_WinForms.DataProcessing;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms
{
    public partial class Win7_1_TCs : Form, ISaveEventForm
    {
        private DbConnector dbCon = new DbConnector();
        private BindingList<DisplayedTechnologicalCard> _bindingList;

        private List<DisplayedTechnologicalCard> _changedCards = new List<DisplayedTechnologicalCard>();
        private List<DisplayedTechnologicalCard> _newCards = new List<DisplayedTechnologicalCard>();
        private List<DisplayedTechnologicalCard> _deletedCards = new List<DisplayedTechnologicalCard>();

        private DisplayedTechnologicalCard _newCard;

        public Win7_1_TCs(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }


        private async void Win7_1_TCs_Load(object sender, EventArgs e)
        {
            await LoadTechnologicalCards();
            SetupDataGridView();
        }
        private async Task LoadTechnologicalCards()
        {
            var tcList = await Task.Run(()=>dbCon.GetObjectList<TechnologicalCard>()
                .Select(tc => new DisplayedTechnologicalCard
            {
                Id = tc.Id,
                Article = tc.Article,
                Name = tc.Name,
                Description = tc.Description,
                Version = tc.Version,
                Type = tc.Type,
                NetworkVoltage = tc.NetworkVoltage,
                TechnologicalProcessType = tc.TechnologicalProcessType,
                TechnologicalProcessName = tc.TechnologicalProcessName,
                TechnologicalProcessNumber = tc.TechnologicalProcessNumber,
                Parameter = tc.Parameter,
                FinalProduct = tc.FinalProduct,
                Applicability = tc.Applicability,
                Note = tc.Note,
                DamageType = tc.DamageType,
                RepairType = tc.RepairType,
                IsCompleted = tc.IsCompleted

            }).ToList());

            _bindingList = new BindingList<DisplayedTechnologicalCard>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;
           
        }
        private void SetupDataGridView()
        {
            WinProcessing.SetTableHeadersNames(DisplayedTechnologicalCard.GetPropertiesNames(), dgvMain);
            WinProcessing.SetTableColumnsOrder(DisplayedTechnologicalCard.GetPropertiesOrder(), dgvMain);
        }
        private void AccessInitialization(int accessLevel)
        {
        }

        /////////////////////////////// btnNavigation events /////////////////////////////////////////////////////////////////


        private void btnCreateTC_Click(object sender, EventArgs e)
        {
            AddNewTechnologicalCard();
        }

        private void btnUpdateTC_Click(object sender, EventArgs e)
        {
            UpdateSelectedTechnologicalCard();
        }

        private void btnDeleteTC_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count > 0)
            {
                var selectedDTCs = dgvMain.SelectedRows.Cast<DataGridViewRow>()
                    .Select(row => row.DataBoundItem as DisplayedTechnologicalCard)
                    .Where(dtc => dtc != null)
                    .ToList();

                string message = "Вы действительно хотите удалить выбранные карты?\n";
                DialogResult result = MessageBox.Show(message, "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    foreach (var dtc in selectedDTCs)
                    {
                        _bindingList.Remove(dtc);
                        _deletedCards.Add(dtc); 
                    }
                }

                dgvMain.Refresh(); 
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SaveChanges()
        {
            if (_changedCards.Count == 0 && _newCards.Count == 0 && _deletedCards.Count == 0)
            {
                return;
            }
            if (_newCards.Count > 0)
            {
                SaveNewTechnologicalCards();
            }
            if (_changedCards.Count > 0)
            {
                SaveChangedTechnologicalCards();
            }
            if (_deletedCards.Count > 0)
            {
                DeleteDeletedTechnologicalCards();
            }
            // todo - change id in all new cards 
        }
        private async Task SaveNewTechnologicalCards()
        {
            var newTcs = _newCards.Select(dtc => new TechnologicalCard
            {
                Article = dtc.Article,
                Name = dtc.Name,
                Description = dtc.Description,
                Version = dtc.Version,
                Type = dtc.Type,
                NetworkVoltage = dtc.NetworkVoltage,
                TechnologicalProcessType = dtc.TechnologicalProcessType,
                TechnologicalProcessName = dtc.TechnologicalProcessName,
                TechnologicalProcessNumber = dtc.TechnologicalProcessNumber,
                Parameter = dtc.Parameter,
                FinalProduct = dtc.FinalProduct,
                Applicability = dtc.Applicability,
                Note = dtc.Note,
                DamageType = dtc.DamageType,
                RepairType = dtc.RepairType,
                IsCompleted = dtc.IsCompleted
            }).ToList();

            await dbCon.AddTcAsync(newTcs);

            MessageBox.Show("Новые карты сохранены.");
            _newCards.Clear();
        }
        private async Task SaveChangedTechnologicalCards()
        {
            var changedTcs = _changedCards.Select(dtc => new TechnologicalCard
            {
                Id = dtc.Id,
                Article = dtc.Article,
                Name = dtc.Name,
                Description = dtc.Description,
                Version = dtc.Version,
                Type = dtc.Type,
                NetworkVoltage = dtc.NetworkVoltage,
                TechnologicalProcessType = dtc.TechnologicalProcessType,
                TechnologicalProcessName = dtc.TechnologicalProcessName,
                TechnologicalProcessNumber = dtc.TechnologicalProcessNumber,
                Parameter = dtc.Parameter,
                FinalProduct = dtc.FinalProduct,
                Applicability = dtc.Applicability,
                Note = dtc.Note,
                DamageType = dtc.DamageType,
                RepairType = dtc.RepairType,
                IsCompleted = dtc.IsCompleted

            }).ToList();

            await dbCon.UpdateTcListAsync(changedTcs);
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
        private void AddNewTechnologicalCard()
        {
            if (_newCard != null && !IsValidNewCard(_newCard))
            {
                MessageBox.Show("Необходимо заполнить все обязательные поля для новой карты.");
                return;
            }
            var newDtc = new DisplayedTechnologicalCard
            {
                Name = "Новое карта",

            };
            _newCard = newDtc;
            _newCards.Add(_newCard);

            _bindingList.Insert(0, _newCard);
            dgvMain.Refresh();
            // todo - ? highlight new row and all its required fields
            // todo - add check for all required fields (ex. Type can be only as "Ремонтная", "Монтажная", "ТТ")

        }
        private bool IsValidNewCard(DisplayedTechnologicalCard card)
        {
            return !string.IsNullOrEmpty(card.Article)
                && !string.IsNullOrEmpty(card.Type)
                && card.NetworkVoltage > 0;
        }
        private void UpdateSelectedTechnologicalCard()
        {
            if (dgvMain.SelectedRows.Count == 1)
            {
                var selectedRow = dgvMain.SelectedRows[0];
                int id = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                OpenTechnologicalCardEditor(id);
            }
            else
            {
                MessageBox.Show("Выберите одну карту для редактирования.");
            }
        }
        private void OpenTechnologicalCardEditor(int tcId)
        {
            var editorForm = new Win6_new(tcId);
            editorForm.Show();
        }

        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                if (_newCards.Contains(_bindingList[e.NewIndex]))
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
        private void AddComboboxColumn() // must be added before datasourse to DGV
        {
            //comboBoxColumn.DataSource = new string[] { "Значение1", "Значение2", "Значение3" }; // Значения для выбора
            
            DataGridViewComboBoxColumn cmbColumn = new DataGridViewComboBoxColumn();
            cmbColumn.HeaderText = "Тип карты";
            cmbColumn.Name = "cmbType";
            cmbColumn.Items.AddRange("Ремонтная", "Монтажная", "ТТ");
            cmbColumn.DataPropertyName = nameof(DisplayedTechnologicalCard.Type);
            cmbColumn.FlatStyle = FlatStyle.Flat; 
            dgvMain.Columns.Add(cmbColumn);
        }

        private class DisplayedTechnologicalCard : INotifyPropertyChanged
        {
            public static Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
            {
                { nameof(Id), "ID" },
                { nameof(Article), "Артикул" },
                { nameof(Version), "Версия" },
                { nameof(Name), "Название" },
                { nameof(Type), "Тип карты" },
                { nameof(NetworkVoltage), "Сеть, кВ" },
                { nameof(TechnologicalProcessType), "Тип тех. процесса" },
                { nameof(TechnologicalProcessName), "Тех. процесс" },
                { nameof(Parameter), "Параметр" },
                { nameof(FinalProduct), "Конечный продукт" },
                { nameof(Applicability), "Применимость тех. карты" },
                { nameof(Note), "Примечания" },
                { nameof(IsCompleted), "Наличие" }
            };
            }
            public static List<string> GetPropertiesOrder()
            {
                return new List<string>
                {
                    nameof(Article),
                    nameof(Type),
                    nameof(NetworkVoltage),
                    nameof(TechnologicalProcessType),
                    nameof(TechnologicalProcessName),
                    nameof(Parameter),
                    nameof(FinalProduct),
                    nameof(Applicability),
                    nameof(Note),
                    nameof(IsCompleted),
                    nameof(Id),
                    nameof(Version),
                    
                };
            }

            private int id;
            private string article;
            private string name;
            private string? description;
            private string version = "0.0.0.0";
            private string type;
            private int networkVoltage;
            private string? technologicalProcessType;
            private string? technologicalProcessName;
            private string? technologicalProcessNumber;
            private string? parameter;
            private string? finalProduct;
            private string? applicability;
            private string? note;
            private string? damageType;
            private string? repairType;
            private bool isCompleted;

            public int Id { get; set; }
            public string Article
            {
                get => article;
                set
                {
                    if (article != value)
                    {
                        article = value;
                        OnPropertyChanged(nameof(Article));
                    }
                }
            }
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
            public string? Description 
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
            public string Version 
            { 
                get => version;
                set
                {
                    if (version != value)
                    {
                        version = value;
                        OnPropertyChanged(nameof(Version));
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
            public int NetworkVoltage 
            { 
                get => networkVoltage;
                set
                {
                    if (networkVoltage != value)
                    {
                        networkVoltage = value;
                        OnPropertyChanged(nameof(NetworkVoltage));
                    }
                }
            }
            public string? TechnologicalProcessType 
            { 
                get => technologicalProcessType;
                set
                {
                    if (technologicalProcessType != value)
                    {
                        technologicalProcessType = value;
                        OnPropertyChanged(nameof(TechnologicalProcessType));
                    }
                }
            } // Тип тех. процесса
            public string? TechnologicalProcessName 
            {
                get => technologicalProcessName;
                set
                {
                    if (technologicalProcessName != value)
                    {
                        technologicalProcessName = value;
                        OnPropertyChanged(nameof(TechnologicalProcessName));
                    }
                }
            }
            public string? TechnologicalProcessNumber 
            { 
                get => technologicalProcessNumber;
                set
                {
                    if (technologicalProcessNumber != value)
                    {
                        technologicalProcessNumber = value;
                        OnPropertyChanged(nameof(TechnologicalProcessNumber));
                    }
                }
            } 
            public string? Parameter 
            { 
                get => parameter;
                set
                {
                    if (parameter != value)
                    {
                        parameter = value;
                        OnPropertyChanged(nameof(Parameter));
                    }
                }
            } 
            public string? FinalProduct 
            { 
                get => finalProduct;
                set
                {
                    if (finalProduct != value)
                    {
                        finalProduct = value;
                        OnPropertyChanged(nameof(FinalProduct));
                    }
                }
            } 
            public string? Applicability 
            {
                get => applicability;
                set
                {
                    if (applicability != value)
                    {
                        applicability = value;
                        OnPropertyChanged(nameof(Applicability));
                    }
                }
            } 
            public string? Note 
            { 
                get => note;
                set
                {
                    if (note != value)
                    {
                        note = value;
                        OnPropertyChanged(nameof(Note));
                    }
                }
            }
            public string? DamageType 
            { 
                get => damageType;
                set
                {
                    if (damageType != value)
                    {
                        damageType = value;
                        OnPropertyChanged(nameof(DamageType));
                    }
                }
            } 
            public string? RepairType 
            { 
                get => repairType;
                set
                {
                    if (repairType != value)
                    {
                        repairType = value;
                        OnPropertyChanged(nameof(RepairType));
                    }
                }
            } 
            public bool IsCompleted 
            {
                get => isCompleted;
                set
                {
                    if (isCompleted != value)
                    {
                        isCompleted = value;
                        OnPropertyChanged(nameof(IsCompleted));
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
