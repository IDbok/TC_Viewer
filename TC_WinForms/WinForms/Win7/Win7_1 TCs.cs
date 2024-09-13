
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection.Metadata;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcModels.Models;
using TcModels.Models.Interfaces;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TcModels.Models.TechnologicalCard;

namespace TC_WinForms.WinForms
{
    public partial class Win7_1_TCs : Form, ILoadDataAsyncForm, IPaginationControl//, ISaveEventForm
    {
        private readonly User.Role _accessLevel;

        private DbConnector dbCon = new DbConnector();
        private List<DisplayedTechnologicalCard> _displayedTechnologicalCards;
        private BindingList<DisplayedTechnologicalCard> _bindingList;

        private List<DisplayedTechnologicalCard> _changedObjects = new List<DisplayedTechnologicalCard>();
        private List<DisplayedTechnologicalCard> _newObjects = new List<DisplayedTechnologicalCard>();
        private List<DisplayedTechnologicalCard> _deletedObjects = new List<DisplayedTechnologicalCard>();

        private DisplayedTechnologicalCard _newObject;

        public bool _isDataLoaded = false;
        //public bool CloseFormsNoSave { get; set; } = false;

        private int currentPageIndex = 0;
        private readonly int _pageSize = 50;
        private int totalPageCount;

        private bool isFiltered = false;

        public string setSearch { get => txtSearch.Text;}
        private List<DisplayedTechnologicalCard> _filteredList;
        private int filteredPageIndex = 0;
        private int totalFilteredPageCount;

        public event EventHandler<PageInfoEventArgs> PageInfoChanged;

        public void RaisePageInfoChanged(PageInfoEventArgs e)
        {
            PageInfoChanged?.Invoke(this, e);
        }

        public Win7_1_TCs(User.Role accessLevel)
        {
            _accessLevel = accessLevel;

            InitializeComponent();
            AccessInitialization();


        }
        private void AccessInitialization()
        {
            var controlAccess = new Dictionary<User.Role, Action>
            {
                [User.Role.Lead] = () => { },

                [User.Role.Implementer] = () => { },

                [User.Role.ProjectManager] = () =>
                {
                    HideAllButtonsExcept(new List<System.Windows.Forms.Button> { btnViewMode });
                    btnViewMode.Location = btnDeleteTC.Location;
                },

                [User.Role.User] = () =>
                {
                    HideAllButtonsExcept(new List<System.Windows.Forms.Button> { btnViewMode });
                    btnViewMode.Location = btnDeleteTC.Location;
                }
            };

            controlAccess.TryGetValue(_accessLevel, out var action);
            action?.Invoke();
        }
        private void HideAllButtonsExcept(List<System.Windows.Forms.Button> visibleButtons)
        {
            foreach (var button in pnlControlBtns.Controls.OfType<System.Windows.Forms.Button>())
            {
                button.Visible = visibleButtons.Contains(button);
            }
        }
        private async void Win7_1_TCs_Load(object sender, EventArgs e)
        {
            this.Enabled = false;
            dgvMain.Visible = false;

            progressBar.Visible = true;

            if (!_isDataLoaded)
            {
                await LoadDataAsync();
            }

            SetDGVColumnsSettings();
            DisplayedEntityHelper.SetupDataGridView<DisplayedTechnologicalCard>(dgvMain);

            SetupNetworkVoltageComboBox();
            SetupTypeComboBox();

            progressBar.Visible = false;
            _isDataLoaded = true;

            //dgvMain.CellFormatting += dgvMain_CellFormatting;
            //dgvMain.RowPrePaint += dgvMain_RowPrePaint;
            dgvMain.RowPostPaint += dgvMain_RowPostPaint;

            dgvMain.Visible = true;
            this.Enabled = true;
        }
        public async Task LoadDataAsync()
        {
            if (_accessLevel == User.Role.ProjectManager || _accessLevel == User.Role.User)
            {
                _displayedTechnologicalCards = await Task.Run(() => dbCon.GetObjectList<TechnologicalCard>()
                    .Select(tc => new DisplayedTechnologicalCard(tc))
                    .Where(tc => tc.Status == TechnologicalCardStatus.Approved).ToList());
            }
            else
            {
                _displayedTechnologicalCards = await Task.Run(() => dbCon.GetObjectList<TechnologicalCard>()
                    .Select(tc => new DisplayedTechnologicalCard(tc)).ToList());
            }
            totalPageCount = (int)Math.Ceiling(_displayedTechnologicalCards.Count / (double)_pageSize);
            UpdateDisplayedData();
        }
        private void UpdateDisplayedData()
        {
            // Расчет отображаемых записей
            var displayedData = isFiltered ? _filteredList : _displayedTechnologicalCards;
            int totalRecords = displayedData.Count;
            int startRecord = isFiltered ? filteredPageIndex * _pageSize + 1 : currentPageIndex * _pageSize + 1;
            // Обеспечиваем, что endRecord не превышает общее количество записей
            int endRecord = Math.Min(startRecord + _pageSize - 1, totalRecords);

            int skipedItems = isFiltered ? filteredPageIndex * _pageSize : currentPageIndex * _pageSize;

            // Обновляем данные
            var pageData = displayedData.OrderBy(tc => tc.Article).Skip(skipedItems).Take(_pageSize).ToList();
            _bindingList = new BindingList<DisplayedTechnologicalCard>(pageData);
            dgvMain.DataSource = _bindingList;//.OrderBy(tc => tc.Article);

            // Подготовка данных для события
            PageInfoEventArgs pageInfoEventArgs = new PageInfoEventArgs
            {
                StartRecord = startRecord,
                EndRecord = endRecord,
                TotalRecords = totalRecords
            };

            // Вызов события с подготовленными данными
            RaisePageInfoChanged(pageInfoEventArgs);
        }



        /////////////////////////////// btnNavigation events /////////////////////////////////////////////////////////////////

        private void btnViewMode_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count == 1)
            {
                var selectedRow = dgvMain.SelectedRows[0];
                int id = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                if (id != 0)
                {
                    var win6 = new Win6_new(id,role: _accessLevel, viewMode:true);
                    win6.Show();
                }
                else
                {
                    MessageBox.Show("Карта ещё не добавлена в БД.");
                }
            }
            else
            {
                MessageBox.Show("Выберите одну карту.");
            }
        }

        private void btnCreateTC_Click(object sender, EventArgs e)
        {
            var objEditor = new Win7_1_TCs_Window(role: _accessLevel);

            objEditor.AfterSave = async (createObj) => AddObjectInDataGridView(createObj);

            objEditor.Show();
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

        private async void btnDeleteTC_Click(object sender, EventArgs e)
        {
            //DeleteSelected();
            await DisplayedEntityHelper.DeleteSelectedObject<DisplayedTechnologicalCard, TechnologicalCard>(dgvMain,
                _bindingList, isFiltered ? _displayedTechnologicalCards : null);
        }

        //private void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        //{
        //    if (dgvMain.Columns[e.ColumnIndex].Name == nameof(DisplayedTechnologicalCard.Status))
        //    {
        //        if (e.Value != null && Enum.TryParse(typeof(TechnologicalCardStatus), e.Value.ToString(), out var status))
        //        {
        //            switch ((TechnologicalCardStatus)status!)
        //            {
        //                case TechnologicalCardStatus.Created:
        //                    e.CellStyle.BackColor = Color.LightGray;
        //                    break;
        //                case TechnologicalCardStatus.Draft:
        //                    e.CellStyle.BackColor = Color.Yellow;
        //                    break;
        //                case TechnologicalCardStatus.Remarked:
        //                    e.CellStyle.BackColor = Color.Orange;
        //                    break;
        //                case TechnologicalCardStatus.Approved:
        //                    e.CellStyle.BackColor = Color.LightGreen;
        //                    break;
        //                case TechnologicalCardStatus.Rejected:
        //                    e.CellStyle.BackColor = Color.LightCoral;
        //                    break;
        //                case TechnologicalCardStatus.Completed:
        //                    e.CellStyle.BackColor = Color.Red;
        //                    break;
        //            }
        //            e.Value = ""; // Устанавливаем текст ячейки в пустую строку
        //        }
        //    }
        //}
        private void dgvMain_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var row = dgvMain.Rows[e.RowIndex];
            var displayedCard = row.DataBoundItem as DisplayedTechnologicalCard;

            if (displayedCard != null)
            {
                var headerCell = row.HeaderCell;

                if (_accessLevel != User.Role.User && _accessLevel != User.Role.ProjectManager)
                {
                    switch (displayedCard.Status)
                    {
                        case TechnologicalCardStatus.Created:
                            headerCell.Style.BackColor = Color.LightGray;
                            break;
                        case TechnologicalCardStatus.Draft:
                            headerCell.Style.BackColor = Color.Yellow;
                            break;
                        case TechnologicalCardStatus.Remarked:
                            headerCell.Style.BackColor = Color.Orange;
                            break;
                        case TechnologicalCardStatus.Approved:
                            headerCell.Style.BackColor = Color.LightGreen;
                            break;
                        case TechnologicalCardStatus.Rejected:
                            headerCell.Style.BackColor = Color.LightCoral;
                            break;
                        case TechnologicalCardStatus.Completed:
                            headerCell.Style.BackColor = Color.SteelBlue;
                            break;
                    }
                }

                // Рисуем заново заголовок строки, чтобы применить стиль
                var rect = e.RowBounds;
                var gridBrush = new SolidBrush(dgvMain.GridColor);
                var backColorBrush = new SolidBrush(headerCell.Style.BackColor);
                var foreColorBrush = new SolidBrush(headerCell.Style.ForeColor);

                e.Graphics.FillRectangle(backColorBrush, rect.Left, rect.Top, dgvMain.RowHeadersWidth, rect.Height);
                e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, foreColorBrush, rect.Left + 5, rect.Top + ((rect.Height - e.InheritedRowStyle.Font.Height) / 2));
                gridBrush.Dispose();
                backColorBrush.Dispose();
                foreColorBrush.Dispose();
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //public bool HasChanges => _changedObjects.Count + _newObjects.Count + _deletedObjects.Count != 0;
        //public async Task SaveChanges()
        //{
        //    // stop editing cell
        //    dgvMain.EndEdit();
        //    // todo- check if in added tech card fulfilled all required fields
        //    if (!HasChanges)
        //    {
        //        return;
        //    }
        //    if (_newObjects.Count > 0)
        //    {
        //        await SaveNewTechnologicalCards();
        //    }
        //    if (_changedObjects.Count > 0)
        //    {
        //        await SaveChangedTechnologicalCards();
        //    }
        //    if (_deletedObjects.Count > 0)
        //    {
        //        await DeleteDeletedTechnologicalCards();
        //    }
        //    dgvMain.Refresh();
        //    // todo - add ids from db to new cards
        //    // todo - change id in all new cards 
        //}
        //private async Task SaveNewTechnologicalCards()
        //{
        //    var newTcs = _newObjects.Select(dtc => CreateNewObject(dtc)).ToList();

        //    await dbCon.AddObjectAsync(newTcs);
        //    // set new ids to new cards matched them by Articles
        //    foreach (var newCard in _newObjects)
        //    {
        //        var newId = newTcs.Where(s => s.Article == newCard.Article).FirstOrDefault().Id;
        //        newCard.Id = newId;
        //    }

        //    //MessageBox.Show("Новые карты сохранены.");
        //    _newObjects.Clear();
        //}
        //private async Task SaveChangedTechnologicalCards()
        //{
        //    var changedTcs = _changedObjects.Select(dtc => CreateNewObject(dtc)).ToList();

        //    await dbCon.UpdateObjectsListAsync(changedTcs);
        //    //MessageBox.Show("Изменения сохранены.");
        //    _changedObjects.Clear();
        //}


        //private TechnologicalCard CreateNewObject(DisplayedTechnologicalCard dtc)
        //{
        //    return new TechnologicalCard
        //    {
        //        Id = dtc.Id,
        //        Article = dtc.Article,
        //        Name = dtc.Name,
        //        Description = dtc.Description,
        //        Version = dtc.Version,
        //        Type = dtc.Type,
        //        NetworkVoltage = dtc.NetworkVoltage,
        //        TechnologicalProcessType = dtc.TechnologicalProcessType,
        //        TechnologicalProcessName = dtc.TechnologicalProcessName,
        //        TechnologicalProcessNumber = dtc.TechnologicalProcessNumber,
        //        Parameter = dtc.Parameter,
        //        FinalProduct = dtc.FinalProduct,
        //        Applicability = dtc.Applicability,
        //        Note = dtc.Note,
        //        DamageType = dtc.DamageType,
        //        RepairType = dtc.RepairType,
        //        IsCompleted = dtc.IsCompleted

        //    };
        //}

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

            dgvMain.Columns[nameof(DisplayedTechnologicalCard.Status)].Width = 35;
            dgvMain.Columns[nameof(DisplayedTechnologicalCard.Status)].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;


        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void UpdateSelected()
        {
            if (dgvMain.SelectedRows.Count == 1)
            {
                var selectedObj = dgvMain.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
                var obj = Convert.ToInt32(selectedObj?.Cells["Id"].Value);

                if (obj != 0)
                {
                    var objEditor = new Win7_1_TCs_Window(obj, role: _accessLevel);
                    objEditor.AfterSave = async (updatedObj) => UpdateObjectInDataGridView(updatedObj);
                    objEditor.Show();
                }
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
        
        //private void OpenTechnologicalCardEditor(int tcId)
        //{
        //    var editorForm = new Win6_new(tcId);
        //    editorForm.Show();
        //}
        private void DeleteSelected()
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

            var types = _bindingList.Select(obj => obj.Type).Distinct().ToList();
            types.Sort();
            cmbColumn.Items.AddRange(types.ToArray());

            cmbColumn.DataPropertyName = nameof(DisplayedTechnologicalCard.Type);

            cmbColumn.FlatStyle = FlatStyle.Flat;

            dgvMain.Columns.Add(cmbColumn);

            DataGridViewComboBoxColumn cmbColumn2 = new DataGridViewComboBoxColumn();
            cmbColumn2.HeaderText = "Сеть, кВ";
            cmbColumn2.Name = nameof(DisplayedTechnologicalCard.NetworkVoltage);

            List<float> voltages = _bindingList.Select(obj => obj.NetworkVoltage).Distinct().ToList();
            voltages.Sort((a, b) => b.CompareTo(a));
            cmbColumn2.Items.AddRange(voltages.Cast<object>().ToArray());

            cmbColumn2.DataPropertyName = nameof(DisplayedTechnologicalCard.NetworkVoltage);

            cmbColumn2.FlatStyle = FlatStyle.Flat;

            dgvMain.Columns.Add(cmbColumn2);
        }


        private class DisplayedTechnologicalCard : INotifyPropertyChanged, IDisplayedEntity, IIdentifiable
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
                { nameof(IsCompleted), "Наличие" },
                { nameof(Status), "Ст." }
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
                    //nameof(Status)
                    // nameof(IsCompleted),
                    // nameof(Id),
                    // nameof(Version),

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

            private TechnologicalCardStatus status;

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

                Status = tc.Status;
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

            public TechnologicalCardStatus Status
            {
                get => status;
                set
                {
                    if (status != value)
                    {
                        status = value;
                        OnPropertyChanged(nameof(Status));
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
            if (!_isDataLoaded) { return; }

            try
            {
                var searchText = txtSearch.Text == "Поиск" ? "" : txtSearch.Text;
                var networkVoltageFilter = cbxNetworkVoltageFilter.SelectedItem?.ToString();
                var typeFilter = cbxTypeFilter.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(searchText) && (
                    (networkVoltageFilter == "Все" && typeFilter == "Все") ||
                    (string.IsNullOrWhiteSpace(networkVoltageFilter) && string.IsNullOrWhiteSpace(typeFilter))))
                {
                    isFiltered = false;
                    filteredPageIndex = 0;

                    UpdateDisplayedData();
                    //dgvMain.DataSource = _bindingList; // Возвращаем исходный список, если строка поиска пуста
                }
                else
                {

                    isFiltered = true;
                    filteredPageIndex = 0;

                    var filteredList = _displayedTechnologicalCards.Where(card =>
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
                        ).ToList();

                    totalFilteredPageCount = (int)Math.Ceiling(filteredList.Count / (double)_pageSize);
                    _filteredList = filteredList;

                    UpdateDisplayedData();
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }

        }
        public void UpdateObjectInDataGridView(TechnologicalCard modelObject)
        {
            // Обновляем объект в DataGridView
            var displayedObject = _displayedTechnologicalCards.FirstOrDefault(obj => obj.Id == modelObject.Id);
            if (displayedObject != null)
            {
                displayedObject.Article = modelObject.Article;
                displayedObject.Version = modelObject.Version;
                displayedObject.Name = modelObject.Name;
                displayedObject.Type = modelObject.Type;
                displayedObject.NetworkVoltage = modelObject.NetworkVoltage;
                displayedObject.TechnologicalProcessType = modelObject.TechnologicalProcessType;
                displayedObject.TechnologicalProcessName = modelObject.TechnologicalProcessName;
                displayedObject.TechnologicalProcessNumber = modelObject.TechnologicalProcessNumber;
                displayedObject.Parameter = modelObject.Parameter;
                displayedObject.FinalProduct = modelObject.FinalProduct;
                displayedObject.Applicability = modelObject.Applicability;
                displayedObject.Note = modelObject.Note;
                displayedObject.DamageType = modelObject.DamageType;
                displayedObject.RepairType = modelObject.RepairType;
                displayedObject.IsCompleted = modelObject.IsCompleted;
                displayedObject.Status = modelObject.Status;
                displayedObject.Description = modelObject.Description;
                
                dgvMain.Refresh();

                FilterTechnologicalCards();
            }

        }

        public void AddObjectInDataGridView(TechnologicalCard modelObject)
        {
            var newDisplayedObject = new DisplayedTechnologicalCard();

            newDisplayedObject.Article = modelObject.Article;
            newDisplayedObject.Version = modelObject.Version;
            newDisplayedObject.Name = modelObject.Name;
            newDisplayedObject.Type = modelObject.Type;
            newDisplayedObject.NetworkVoltage = modelObject.NetworkVoltage;
            newDisplayedObject.TechnologicalProcessType = modelObject.TechnologicalProcessType;
            newDisplayedObject.TechnologicalProcessName = modelObject.TechnologicalProcessName;
            newDisplayedObject.TechnologicalProcessNumber = modelObject.TechnologicalProcessNumber;
            newDisplayedObject.Parameter = modelObject.Parameter;
            newDisplayedObject.FinalProduct = modelObject.FinalProduct;
            newDisplayedObject.Applicability = modelObject.Applicability;
            newDisplayedObject.Note = modelObject.Note;
            newDisplayedObject.DamageType = modelObject.DamageType;
            newDisplayedObject.RepairType = modelObject.RepairType;
            newDisplayedObject.IsCompleted = modelObject.IsCompleted;
            newDisplayedObject.Status = modelObject.Status;
            newDisplayedObject.Description = modelObject.Description;

            _displayedTechnologicalCards.Insert(0, newDisplayedObject);

            FilterTechnologicalCards();
        }
        private void cbxNetworkVoltageFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterTechnologicalCards();
        }
        private void SetupNetworkVoltageComboBox()
        {
            var voltagies = _displayedTechnologicalCards.Select(obj => obj.NetworkVoltage).Distinct().ToList();

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
            var types = _displayedTechnologicalCards.Select(obj => obj.Type).Distinct().ToList();
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



        public void GoToNextPage()
        {
            if (isFiltered)
            {
                if (filteredPageIndex < totalFilteredPageCount - 1)
                {
                    filteredPageIndex++;
                    UpdateDisplayedData();
                }
            }
            else
            {
                if (currentPageIndex < totalPageCount - 1)
                {
                    currentPageIndex++;
                    UpdateDisplayedData();
                }
            }
        }

        public void GoToPreviousPage()
        {
            if (isFiltered)
            {
                if (filteredPageIndex > 0)
                {
                    filteredPageIndex--;
                    UpdateDisplayedData();
                }
            }
            else
            {
                if (currentPageIndex > 0)
                {
                    currentPageIndex--;
                    UpdateDisplayedData();
                }
            }
        }

    }
}
