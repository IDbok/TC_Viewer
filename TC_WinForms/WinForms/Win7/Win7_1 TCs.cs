
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;
using System.Reflection.Metadata;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TcModels.Models;
using TcModels.Models.Interfaces;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TcModels.Models.TechnologicalCard;

namespace TC_WinForms.WinForms
{
    public partial class Win7_1_TCs : Form, ILoadDataAsyncForm, IPaginationControl//, ISaveEventForm
    {
        private readonly User.Role _accessLevel;
        private readonly int _minRowHeight = 20;

        private DbConnector dbCon = new DbConnector();
        private List<DisplayedTechnologicalCard> _displayedTechnologicalCards;
        private BindingList<DisplayedTechnologicalCard> _bindingList;

        private List<DisplayedTechnologicalCard> _changedObjects = new List<DisplayedTechnologicalCard>();
        private List<DisplayedTechnologicalCard> _newObjects = new List<DisplayedTechnologicalCard>();
        private List<DisplayedTechnologicalCard> _deletedObjects = new List<DisplayedTechnologicalCard>();

        private DisplayedTechnologicalCard _newObject;

        public bool _isDataLoaded = false;
        //public bool CloseFormsNoSave { get; set; } = false;

        private bool isFiltered = false;

        public string setSearch { get => txtSearch.Text;}

        PaginationControlService<DisplayedTechnologicalCard> paginationService;

        public event EventHandler<PageInfoEventArgs> PageInfoChanged;
        public PageInfoEventArgs? PageInfo { get; set; }
        public void RaisePageInfoChanged()
        {
            PageInfoChanged?.Invoke(this, PageInfo);
        }

        public Win7_1_TCs(User.Role accessLevel)
        {
            Log.Information("Инициализация окна Win7_1_TCs для роли {AccessLevel}", accessLevel);

            _accessLevel = accessLevel;

            InitializeComponent();
            AccessInitialization();

            dgvMain.DoubleBuffered(true);
        }
        private void AccessInitialization()
        {
            var controlAccess = new Dictionary<User.Role, Action>
            {
                [User.Role.Lead] = () => { },

                [User.Role.Implementer] = () =>
                {
                    HideAllButtonsExcept(new List<System.Windows.Forms.Button> { btnViewMode });
                    btnViewMode.Location = btnDeleteTC.Location;
                },

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
            Log.Information("Загрузка формы Win7_1_TCs");

            this.Enabled = false;
            dgvMain.Visible = false;

            progressBar.Visible = true;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (!_isDataLoaded)
                {
                    Log.Information("Начало загрузки данных из базы");
                    await LoadDataAsync();
                    Log.Information("Данные успешно загружены");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка при загрузке данных: {ExceptionMessage}", ex.Message);
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                Log.Information("Данные загружены за {ElapsedMilliseconds} мс", stopwatch.ElapsedMilliseconds);

                progressBar.Visible = false;
                _isDataLoaded = true;

                SetDGVColumnsSettings();
                DisplayedEntityHelper.SetupDataGridView<DisplayedTechnologicalCard>(dgvMain);

                SetupNetworkVoltageComboBox();
                SetupTypeComboBox();

                dgvMain.RowPostPaint += dgvMain_RowPostPaint;


            dgvMain.ResizeRows(_minRowHeight);
            dgvMain.Visible = true;
            this.Enabled = true;

        }
        public async Task LoadDataAsync()
        {
            Log.Information("Начало асинхронной загрузки данных для Win7_1_TCs");

            try
            {
                if (_accessLevel == User.Role.ProjectManager || _accessLevel == User.Role.User)
                {
                    _displayedTechnologicalCards = await Task.Run(() => dbCon.GetObjectList<TechnologicalCard>()
                        .Select(tc => new DisplayedTechnologicalCard(tc))
                        .Where(tc => tc.Status == TechnologicalCardStatus.Approved).OrderBy(tc => tc.Article).ToList());
                }
                else
                {
                    _displayedTechnologicalCards = await Task.Run(() => dbCon.GetObjectList<TechnologicalCard>()
                        .Select(tc => new DisplayedTechnologicalCard(tc)).OrderBy(tc => tc.Article).ToList());
                }

                paginationService = new PaginationControlService<DisplayedTechnologicalCard>(50, _displayedTechnologicalCards);

                UpdateDisplayedData();
            }
            catch (Exception e)
            {
                Log.Error("Ошибка при загрузке данных: {ExceptionMessage}", e.Message);
                MessageBox.Show("Ошибка загрузки данных: " + e.Message);
            }


        }
        private void UpdateDisplayedData()
        {
            // Расчет отображаемых записей

            _bindingList = new BindingList<DisplayedTechnologicalCard>(paginationService.GetPageData());
            dgvMain.DataSource = _bindingList;//.OrderBy(tc => tc.Article);
            dgvMain.ResizeRows(20);
            // Подготовка данных для события
            PageInfo = paginationService.GetPageInfo();

            // Вызов события с подготовленными данными
            RaisePageInfoChanged();
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
                    var win6 = new Win6_new(id, role: _accessLevel, viewMode: true);
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
                    paginationService.SetAllObjectList(_displayedTechnologicalCards);

                    //dgvMain.DataSource = _bindingList; // Возвращаем исходный список, если строка поиска пуста
                }
                else
                {

                    isFiltered = true;

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

                    paginationService.SetAllObjectList(filteredList);

                }

                UpdateDisplayedData();

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

            newDisplayedObject.Id = modelObject.Id;
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
            paginationService.GoToNextPage();
            UpdateDisplayedData();
        }

        public void GoToPreviousPage()
        {
            paginationService.GoToPreviousPage();
            UpdateDisplayedData();
            
        }

        private void Win7_1_TCs_SizeChanged(object sender, EventArgs e)
        {
            dgvMain.ResizeRows(_minRowHeight);
        }
    }

    
}
