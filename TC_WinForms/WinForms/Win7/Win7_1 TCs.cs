using System.ComponentModel;
using System.Reflection.Metadata;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms
{
    public partial class Win7_1_TCs : Form, ISaveEventForm, ILoadDataAsyncForm
    {
        private DbConnector dbCon = new DbConnector();
        private BindingList<DisplayedTechnologicalCard> _bindingList;

        private List<DisplayedTechnologicalCard> _changedObjects = new List<DisplayedTechnologicalCard>();
        private List<DisplayedTechnologicalCard> _newObjects = new List<DisplayedTechnologicalCard>();
        private List<DisplayedTechnologicalCard> _deletedObjects = new List<DisplayedTechnologicalCard>();

        private DisplayedTechnologicalCard _newObject;

        public bool _isDataLoaded = false;
        public Win7_1_TCs(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }

        private async void Win7_1_TCs_Load(object sender, EventArgs e)
        {
            progressBar.Visible = true;
            if (!_isDataLoaded)
            {
                await LoadDataAsync();
            }
            //await LoadDataAsync();
            SetDGVColumnsSettings();
            DisplayedEntityHelper.SetupDataGridView<DisplayedTechnologicalCard>(dgvMain);

            SetupNetworkVoltageComboBox();
            SetupTypeComboBox();

            progressBar.Visible = false;
        }
        public async Task LoadDataAsync()
        {
            var tcList = await Task.Run(() => dbCon.GetObjectList<TechnologicalCard>()
                .Select(tc => new DisplayedTechnologicalCard(tc)).ToList());
            _bindingList = new BindingList<DisplayedTechnologicalCard>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            ConfigureDgvWithComboBoxColumn();

            dgvMain.DataSource = _bindingList;

            _isDataLoaded = true;

        }
        private void AccessInitialization(int accessLevel)
        {
        }

        /////////////////////////////// btnNavigation events /////////////////////////////////////////////////////////////////


        private void btnCreateTC_Click(object sender, EventArgs e)
        {
            DisplayedEntityHelper.AddNewObjectToDGV(ref _newObject,
                _bindingList,
                _newObjects,
                dgvMain);
            //AddNewTechnologicalCard();
        }

        private void btnUpdateTC_Click(object sender, EventArgs e)
        {
            if (_newObjects.Count != 0)
            {
                MessageBox.Show("Сохраните добавленные карты!");
            }
            else
            {
                UpdateSelected();
            }
        }

        private void btnDeleteTC_Click(object sender, EventArgs e)
        {
            DeletSelected();
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool HasChanges => _changedObjects.Count + _newObjects.Count + _deletedObjects.Count != 0;
        public async Task SaveChanges()
        {
            // stop editing cell
            dgvMain.EndEdit();
            // todo- check if in added tech card fulfilled all required fields
            if (!HasChanges)
            {
                return;
            }
            if (_newObjects.Count > 0)
            {
                await SaveNewTechnologicalCards();
            }
            if (_changedObjects.Count > 0)
            {
                await SaveChangedTechnologicalCards();
            }
            if (_deletedObjects.Count > 0)
            {
                await DeleteDeletedTechnologicalCards();
            }
            dgvMain.Refresh();
            // todo - add ids from db to new cards
            // todo - change id in all new cards 
        }
        private async Task SaveNewTechnologicalCards()
        {
            var newTcs = _newObjects.Select(dtc => CreateNewObject(dtc)).ToList();

            await dbCon.AddObjectAsync(newTcs);
            // set new ids to new cards matched them by Articles
            foreach (var newCard in _newObjects)
            {
                var newId = newTcs.Where(s => s.Article == newCard.Article).FirstOrDefault().Id;
                newCard.Id = newId;
            }

            //MessageBox.Show("Новые карты сохранены.");
            _newObjects.Clear();
        }
        private async Task SaveChangedTechnologicalCards()
        {
            var changedTcs = _changedObjects.Select(dtc => CreateNewObject(dtc)).ToList();

            await dbCon.UpdateObjectsListAsync(changedTcs);
            //MessageBox.Show("Изменения сохранены.");
            _changedObjects.Clear();
        }


        private TechnologicalCard CreateNewObject(DisplayedTechnologicalCard dtc)
        {
            return new TechnologicalCard
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

            };
        }

        ////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////

        void SetDGVColumnsSettings()
        {
            //if (dgvMain.InvokeRequired)
            //{
            //    dgvMain.Invoke((MethodInvoker)(() => AutoSizeColumns()));
            //}
            //else
            //{
            //    AutoSizeColumns();
            //}

            // автоподбор ширины столбцов под ширину таблицы
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.RowHeadersWidth = 25;

            //// автоперенос в ячейках
            dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;


            //    // ширина столбцов по содержанию
            var autosizeColumn = new List<string>
            {
                nameof(DisplayedTechnologicalCard.Article),
                nameof(DisplayedTechnologicalCard.Type),
                nameof(DisplayedTechnologicalCard.NetworkVoltage),
                //nameof(DisplayedTechnologicalCard.TechnologicalProcessType),
                //nameof(DisplayedTechnologicalCard.TechnologicalProcessName),
                nameof(DisplayedTechnologicalCard.Parameter),
                //nameof(DisplayedTechnologicalCard.FinalProduct),
                // nameof(DisplayedTechnologicalCard.Applicability),
                // nameof(DisplayedTechnologicalCard.Note),
                nameof(DisplayedTechnologicalCard.IsCompleted),
                nameof(DisplayedTechnologicalCard.Id),
                nameof(DisplayedTechnologicalCard.Version),
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            //    //// make columns readonly
            //    //foreach (DataGridViewColumn column in dgvMain.Columns)
            //    //{
            //    //    column.ReadOnly = true;
            //    //}
            //    //// make columns editable
            //    //dgvMain.Columns["Order"].ReadOnly = false;

            dgvMain.Columns[nameof(DisplayedTechnologicalCard.Id)].ReadOnly = true;
            dgvMain.Columns[nameof(DisplayedTechnologicalCard.Version)].ReadOnly = true;



        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void UpdateSelected()
        {
            if (dgvMain.SelectedRows.Count == 1)
            {
                var selectedRow = dgvMain.SelectedRows[0];
                int id = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                if (id != 0)
                    OpenTechnologicalCardEditor(id);
                else
                {
                    MessageBox.Show("Карта ещё не добавлена в БД.");
                }
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
        private void DeletSelected()
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
                        _deletedObjects.Add(dtc);

                        if (_newObjects.Contains(dtc)) // if new card was deleted, remove it from new cards list
                        {
                            _newObjects.Remove(dtc);
                        }
                    }
                }

                dgvMain.Refresh();
            }
        }
        private async Task DeleteDeletedTechnologicalCards()
        {
            var deletedTcIds = _deletedObjects.Select(dtc => dtc.Id).ToList();

            await dbCon.DeleteObjectAsync<TechnologicalCard>(deletedTcIds);
            //MessageBox.Show("Карты удалены.");
            _deletedObjects.Clear();
        }
        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            // todo - add check of unique article
            DisplayedEntityHelper.ListChangedEventHandler<DisplayedTechnologicalCard>
                (e, _bindingList, _newObjects, _changedObjects, ref _newObject);
        }

        private void ConfigureDgvWithComboBoxColumn()
        {
            DataGridViewComboBoxColumn cmbColumn = new DataGridViewComboBoxColumn();
            cmbColumn.HeaderText = "Тип карты";
            cmbColumn.Name = nameof(DisplayedTechnologicalCard.Type);
            cmbColumn.Items.AddRange(new object[] { "Ремонтная", "Монтажная", "Точка Трансформации", "Нет данных" });

            cmbColumn.DataPropertyName = nameof(DisplayedTechnologicalCard.Type);

            cmbColumn.FlatStyle = FlatStyle.Flat;

            dgvMain.Columns.Add(cmbColumn);

            DataGridViewComboBoxColumn cmbColumn2 = new DataGridViewComboBoxColumn();
            cmbColumn2.HeaderText = "Сеть, кВ";
            cmbColumn2.Name = nameof(DisplayedTechnologicalCard.NetworkVoltage);
            cmbColumn2.Items.AddRange(new object[] { 35f, 10f, 6f, 0.4f, 0f });

            cmbColumn2.DataPropertyName = nameof(DisplayedTechnologicalCard.NetworkVoltage);

            cmbColumn2.FlatStyle = FlatStyle.Flat;

            dgvMain.Columns.Add(cmbColumn2);
        }


        private class DisplayedTechnologicalCard : INotifyPropertyChanged, IDisplayedEntity
        {
            public Dictionary<string, string> GetPropertiesNames()
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
            public List<string> GetPropertiesOrder()
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
            public List<string> GetRequiredFields()
            {
                return new List<string>
                {
                    nameof(Article),
                    nameof(Type),
                    nameof(NetworkVoltage)
                };
            }

            private int id;
            private string article;
            private string name;
            private string? description;
            private string version = "0.0.0.0";
            private string type;
            private float networkVoltage;
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

            public DisplayedTechnologicalCard()
            {

            }
            public DisplayedTechnologicalCard(TechnologicalCard tc)
            {
                Id = tc.Id;
                Article = tc.Article;
                Name = tc.Name;
                Description = tc.Description;
                Version = tc.Version;
                Type = tc.Type;
                NetworkVoltage = tc.NetworkVoltage;
                TechnologicalProcessType = tc.TechnologicalProcessType;
                TechnologicalProcessName = tc.TechnologicalProcessName;
                TechnologicalProcessNumber = tc.TechnologicalProcessNumber;
                Parameter = tc.Parameter;
                FinalProduct = tc.FinalProduct;
                Applicability = tc.Applicability;
                Note = tc.Note;
                DamageType = tc.DamageType;
                RepairType = tc.RepairType;
                IsCompleted = tc.IsCompleted;
            }

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
            public float NetworkVoltage
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

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterTechnologicalCards();
        }

        private void FilterTechnologicalCards()
        {
            try
            {
                var searchText = txtSearch.Text == "Поиск" ? "" : txtSearch.Text;
                var networkVoltageFilter = cbxNetworkVoltageFilter.SelectedItem?.ToString();
                var typeFilter = cbxTypeFilter.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(searchText) && networkVoltageFilter == "Все" && typeFilter == "Все")
                {
                    dgvMain.DataSource = _bindingList; // Возвращаем исходный список, если строка поиска пуста
                }
                else
                {
                    var filteredList = _bindingList.Where(card =>
                        (searchText == ""
                        ||
                            (card.Article?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            //(card.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (card.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            //(card.Type?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (card.TechnologicalProcessType?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (card.TechnologicalProcessName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (card.Parameter?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (card.FinalProduct?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (card.Applicability?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (card.Note?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                        )
                        &&
                        (networkVoltageFilter == "Все" || card.NetworkVoltage.ToString() == networkVoltageFilter)
                        &&
                        (typeFilter == "Все" || card.Type.ToString() == typeFilter)
                        //card.NetworkVoltage.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        //card.Id.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase)
                        ).ToList();

                    dgvMain.DataSource = new BindingList<DisplayedTechnologicalCard>(filteredList);
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }
            
        }

        private void cbxNetworkVoltageFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterTechnologicalCards();
        }
        private void SetupNetworkVoltageComboBox()
        {
            var voltagies = _bindingList.Select(obj => obj.NetworkVoltage).Distinct().ToList();
            
            voltagies.Sort((a, b) => b.CompareTo(a));

            cbxNetworkVoltageFilter.Items.Add("Все");
            foreach (var voltage in voltagies)
            {
                cbxNetworkVoltageFilter.Items.Add(voltage);
            }

            //cbxNetworkVoltageFilter.Items.Add("Все");
            //cbxNetworkVoltageFilter.Items.AddRange(new object[] { 35f, 10f, 6f, 0.4f, 0f });
            cbxNetworkVoltageFilter.SelectedIndex = 0; // Выбираем "Все" по умолчанию

            //cbxNetworkVoltageFilter.DropDownWidth = cbxNetworkVoltageFilter.Items.Cast<string>().Max(s => TextRenderer.MeasureText(s, cbxNetworkVoltageFilter.Font).Width) + 20;
        }
        private void SetupTypeComboBox()
        {
            var types = _bindingList.Select(obj => obj.Type).Distinct().ToList();
            types.Sort();

            cbxTypeFilter.Items.Add("Все");
            foreach (var type in types)
            {
                if (string.IsNullOrWhiteSpace(type)) { continue; }
                    cbxTypeFilter.Items.Add(type);
            }
            //cbxType.Items.AddRange(new object[] { "Ремонтная", "Монтажная", "Точка Трансформации", "Нет данных" });
            cbxTypeFilter.SelectedIndex = 0; // Выбираем "Все" по умолчанию

            cbxTypeFilter.DropDownWidth = cbxTypeFilter.Items.Cast<string>().Max(s => TextRenderer.MeasureText(s, cbxTypeFilter.Font).Width) + 20;
        }

        private void cbxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterTechnologicalCards();
            var width = TextRenderer.MeasureText(cbxTypeFilter.SelectedItem.ToString(), cbxTypeFilter.Font).Width + 20;
            cbxTypeFilter.Width = width < 160 ?
                160
                : width;
        }
    }
}
