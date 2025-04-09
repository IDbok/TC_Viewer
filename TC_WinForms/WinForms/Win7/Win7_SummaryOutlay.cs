using ExcelParsing.DataProcessing;
using Serilog;
using System.Data;
using System.IO;
using System.Text.Json;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.Extensions;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Win7.Work;
using TcDbConnector;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TcModels.Models.TcContent.Outlay;

namespace TC_WinForms.WinForms
{
    public partial class Win7_SummaryOutlay : Form, IPaginationControl
    {
        #region Fields

        //private readonly User.Role _accessLevel;
        private readonly int _minRowHeight = 20;

        private DbConnector dbCon = new DbConnector();

        private bool isMachineViewMode = true;
        private readonly ILogger _logger;
        private User.Role _accessLevel;
        private List<Outlay> _allOutlays = new();
        private readonly int _linesPerPage = 50;
        private List<SummaryOutlayDataGridItem> _allOutlaysList = new List<SummaryOutlayDataGridItem>();
        private List<SummaryOutlayDataGridItem> _displayedList = new List<SummaryOutlayDataGridItem>();
        PaginationControlService<SummaryOutlayDataGridItem> paginationService;

        private SummaryOutlaySettings _settings = new SummaryOutlaySettings { LeaderSallary = 1134, RegularSallary = 794 };//На случай отсуствия доступа к файлу настроек можно считать данные из этого объекта

        public bool _isDataLoaded = false;

        #endregion

        #region EventsAndProperties

        public event EventHandler<PageInfoEventArgs> PageInfoChanged;
        public PageInfoEventArgs? PageInfo { get; set; }
        public void RaisePageInfoChanged()
        {
            PageInfoChanged?.Invoke(this, PageInfo);
        }

        #endregion

        #region Constructor

        public Win7_SummaryOutlay(User.Role accessLevel)
        {
            _logger = Log.Logger.ForContext<Win7_SummaryOutlay>();
            _logger.Information("Инициализация окна Win7_1_TCs для роли {AccessLevel}", accessLevel);

            _accessLevel = accessLevel;

            InitializeComponent();

            // Улучшаем производительность DataGridView, уменьшаем мерцание
            dgvMain.DoubleBuffered(true);
            dgvMain.CellFormatting += DgvMain_CellFormatting;
        }

        #endregion

        #region FormLoad

        private void Win7_SummaryOutlay_Load(object sender, EventArgs e)
        {
            _logger.Information("Загрузка формы Win7_1_TCs");

            Enabled = false;
            dgvMain.Visible = false;

            _logger.Information($"Проверка наличия файла SummaryOutlaySettings.json");
            // Проверяем наличие файла summaryoutlaysettings.json и создаем его с настройками по умолчанию, если он отсутствует
            EnsureOutlaySettingsExists();
            _logger.Information($"Файл SummaryOutlaySettings.json существует, либо успешно создан");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (!_isDataLoaded)
                {
                    _logger.Information("Начало загрузки данных из базы");
                    LoadData();
                    _logger.Information("Данные успешно загружены");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка при загрузке данных: {ExceptionMessage}", ex.Message);
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                _logger.Information("Данные загружены за {ElapsedMilliseconds} мс", stopwatch.ElapsedMilliseconds);

                //Инициализируем и заполняем таблицу
                SetupDGV();

                // Инициализация комбобоксов фильтра
                SetupTypeFilterComboBox();

                // Настраиваем отрисовку DataGridView
                dgvMain.ResizeRows(_minRowHeight);

                Enabled = true;
                dgvMain.Visible = true;
                _isDataLoaded = true;
            }
        }
        public void LoadData()
        {
            try
            {
                _logger.Information($"Начинается загрузка данных затрат из БД");

                if (_accessLevel == User.Role.User || _accessLevel == User.Role.ProjectManager)
                {
                    using (MyDbContext context = new MyDbContext())
                    {
                        var a = context.OutlaysTable.Select(s => s.TcId).Distinct().ToList();
                        var aprovedTc = context.TechnologicalCards
                            .Where(t => a.Contains(t.Id) && t.Status == TcModels.Models.TechnologicalCard.TechnologicalCardStatus.Approved)
                            .Select(s => s.Id).ToList();
                        _allOutlays = context.OutlaysTable.Where(o => aprovedTc.Contains(o.TcId)).ToList();
                    }
                }
                else
                {
                    _allOutlays = dbCon.GetObjectList<Outlay>();
                }

                _logger.Information($"Загружены данные из БД для пользователя {_accessLevel}");
                WriteSummaryOutlayData();
                paginationService = new PaginationControlService<SummaryOutlayDataGridItem>(_linesPerPage, _allOutlaysList);
                UpdateDisplayedData();
            }
            catch (Exception e)
            {
                _logger.Error("Ошибка при загрузке данных: {ExceptionMessage}", e.Message);
                MessageBox.Show("Ошибка загрузки данных: " + e.Message);
            }
        }
        private void WriteSummaryOutlayData()
        {
            _logger.Information($"Инициализация заполнения списка _allOutlaysList элементами SummaryOutlayDataGridItem");

            _allOutlaysList.Clear();
            var groupedOutlays = _allOutlays.GroupBy(x => x.TcId).ToList();
            foreach (var outlay in groupedOutlays)
            {
                var tcId = outlay.Key;
                var outlayData = outlay.ToList();
                List<SummaryOutlayMachine> listMachStr = new List<SummaryOutlayMachine>();
                List<SummaryOutlayStaff> listStaffStr = new List<SummaryOutlayStaff>();

                foreach (var machineOutlay in outlayData.Where(x => x.Type == OutlayType.Mechine).ToList())
                    listMachStr.Add(new SummaryOutlayMachine
                    {
                        MachineName = machineOutlay.Name,
                        MachineOutlay = machineOutlay.OutlayValue,
                        MachineId = (int)machineOutlay.ChildId,
                    });

                foreach (var staffOutlay in outlayData.Where(x => x.Type == OutlayType.Staff).ToList())
                    listStaffStr.Add(new SummaryOutlayStaff
                    {
                        StaffName = staffOutlay.Name.Split(" ")[0],
                        StaffOutlay = staffOutlay.OutlayValue,
                        StaffId = (int)staffOutlay.ChildId,
                    });

                _allOutlaysList.Add(new SummaryOutlayDataGridItem
                {
                    TcId = tcId,
                    listStaffStr = listStaffStr,
                    listMachStr = listMachStr,
                    ComponentOutlay = outlayData.Where(s => s.Type == OutlayType.Components).Select(s => s.OutlayValue).First(),
                    SummaryOutlay = outlayData.Where(s => s.Type == OutlayType.SummaryTimeOutlay).Select(s => s.OutlayValue).First(),
                });
            }

            _logger.Information($"Основные данные списка _allOutlaysList заполнены");

            try
            {
                _logger.Information($"Инициализация контекста для заполнения оставшихся данных _allOutlaysList");

                using (MyDbContext context = new MyDbContext())
                {
                    _allOutlaysList.ForEach
                        (
                            x =>
                            {
                                var techCard = context.TechnologicalCards.Where(t => t.Id == x.TcId).FirstOrDefault();
                                if (techCard != null)
                                {
                                    x.TcName = techCard.Article;
                                    x.TechProcess = techCard.TechnologicalProcessName;
                                    x.Parameter = techCard.Parameter;
                                    x.UnitType = techCard.OutlayUnit;
                                    x.listMachStr.ForEach(
                                        m =>
                                        {
                                            m.MachineCost = context.Machines.Where(mc => mc.Id == m.MachineId).Select(s => s.Price).Cast<double?>().FirstOrDefault();
                                        });
                                    x.listStaffStr.ForEach(
                                        m =>
                                        {
                                            m.StaffCost = context.Staffs.Where(mc => mc.Id == m.StaffId).Select(s => s.Price).Cast<double?>().FirstOrDefault();
                                        });
                                    x.SummaryOutlayCost = CalculateSummaryOutlayCost(x);
                                }
                            }
                        );
                }

                _logger.Information($"Список _allOutlaysList заполнен записями {_allOutlaysList.Count}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при заполнении _allOutlaysList:\n {ex.Message}");
                MessageBox.Show($"Ошибка запроса данных:\n {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
        }
        private static void EnsureOutlaySettingsExists()
        {
            if (!File.Exists("SummaryOutlaySettings.json"))
            {
                try
                {
                    var defaultConfig = new SummaryOutlaySettings
                    {
                        LeaderSallary = 1134,
                        RegularSallary = 794
                    };

                    // Сериализация в JSON
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(defaultConfig, options);

                    // Запись в файл
                    File.WriteAllText("SummaryOutlaySettings.json", json);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Ошибка создания файла настроек:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Filtering

        private void FilterObjects()
        {
            _logger.Information($"Инициализован процесс фильтрации информации на странице");

            if (!_isDataLoaded) 
            {
                _logger.Information($"Данные не загружены, фильтрация страницы отменена");
                return; 
            }

            try
            {
                var filteredList = ApplyFilters();
                _logger.Information($"Применены фильтры, получен список из {filteredList.Count} элементов");

                paginationService.SetAllObjectList(filteredList);
                _logger.Information($"Задан список всех объектов для сервиса пагинации");

                UpdateDisplayedData();

            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка фильтрации: {ExceptionMessage}", ex.Message);
            }
        }

        private List<SummaryOutlayDataGridItem> ApplyFilters()
        {
            var searchText = txtSearch.Text == "Поиск" ? "" : txtSearch.Text;
            var typeFilter = cmbxUnit.SelectedItem?.ToString();

            return _allOutlaysList.Where(outlay => (searchText == "" || string.IsNullOrWhiteSpace(searchText) || (outlay.TcName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                || (outlay.TechProcess.Contains(searchText, StringComparison.OrdinalIgnoreCase)) || (outlay.Parameter.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                && (typeFilter == "Все" || string.IsNullOrWhiteSpace(typeFilter) || outlay.UnitType.GetDescription() == typeFilter)).ToList();
        }

        #endregion

        #region Pagination

        public void GoToNextPage()
        {
            _logger.Information($"Попытка перехода на следующую страницу");
            paginationService.GoToNextPage();
            UpdateDisplayedData();
        }

        public void GoToPreviousPage()
        {
            _logger.Information($"Попытка перехода на предыдущую страницу");
            paginationService.GoToPreviousPage();
            UpdateDisplayedData();
        }

        private void UpdateDisplayedData()
        {
            _logger.Information($"Инициализировано обновление выводимых данных таблицы");

            var pageData = paginationService.GetPageData();
            if (pageData != null)
            {
                _logger.Information($"Получена информация о данных на странице, происходит обновление. На старнице будет отображено {pageData.Count} записей");

                _displayedList = pageData;
                SetupDGV();
                dgvMain.ResizeRows(_minRowHeight);

                PageInfo = paginationService.GetPageInfo();
                RaisePageInfoChanged();
            }
            else
            {
                _logger.Warning("Не удалось получить данные для отображения на странице");
                _displayedList = new List<SummaryOutlayDataGridItem>();
                SetupDGV();
            }
        }

        #endregion

        #region UIEventHandlers
        private void DgvMain_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvMain.Columns[e.ColumnIndex].Name.Contains("Machine"))
            {
                _logger.Information($"Инициализовано событие форматирования ячеек столбца механизма");
                var machineId = Convert.ToInt32(dgvMain.Columns[e.ColumnIndex].Name.Split(' ')[1]);
                var currentMachine = _allOutlaysList.Where(s => s.listMachStr.Any(m => m.MachineId == machineId))//Берем любой список, где участвует нужный механизм
                                                    .SelectMany(
                                                                s => s.listMachStr
                                                                        .Where(s => s.MachineId == machineId)
                                                                )//берем из списка объект с механизмом, у которого ID совпадает с machineId для просмотра его цены
                                                    .FirstOrDefault();
                if (currentMachine != null && (currentMachine.MachineCost == null || currentMachine.MachineCost == 0))
                {
                    dgvMain.Columns[e.ColumnIndex].DefaultCellStyle.BackColor = Color.Yellow;
                    _logger.Information($"У объекта {currentMachine.MachineName} отсутсвует цена. Колонка {e.ColumnIndex} таблицы отмечена цветом");
                }
            }
        }
        private void btnShowMachine_Click(object sender, EventArgs e)
        {
            isMachineViewMode = !isMachineViewMode;

            _logger.Information($"Видимость колонок механизмов установлена: {isMachineViewMode}");

            btnShowMachine.Text = isMachineViewMode ?
                 "Скрыть механизмы" : "Показать механизмы";

            SetMachineViewMode();
        }

        public void SetMachineViewMode()
        {
            foreach (DataGridViewColumn col in dgvMain.Columns)
            {
                if (col.Name.Contains("Machine"))
                {
                    col.Visible = isMachineViewMode;
                }
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterObjects();
        }
        private void Win7_SummaryOutlay_SizeChanged(object sender, EventArgs e)
        {
            dgvMain.ResizeRows(_minRowHeight);
        }
        private async void btnPrint_Click(object sender, EventArgs e)
        {
            _logger.Information($"Инициализована печать сводной таблицы");

            var tcExporter = new SummOutlayExcelExport();

            await tcExporter.SaveSummOutlaytoExcelFile(_allOutlaysList);
        }
        private void btnSettings_Click(object sender, EventArgs e)
        {
            _logger.Information($"Вызвано открытие формы Win7_OutlaySettings");

            var openedForm = CheckOpenFormService.FindOpenedForm<Win7_OutlaySettings>();
            if (openedForm != null)
            {
                openedForm.BringToFront();
                return;
            }

            var settingsForm = new Win7_OutlaySettings();
            settingsForm.Show();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterObjects();
        }

        #endregion

        #region ComboBoxSetup

        private void SetupTypeFilterComboBox()
        {
            var types = _allOutlaysList.Select(obj => obj.UnitType).Distinct().ToList();

            types.Sort((a, b) => b.CompareTo(a));

            cmbxUnit.Items.Add("Все");

            foreach (var type in types)
            {
                cmbxUnit.Items.Add(type.GetDescription());
            }

            cmbxUnit.SelectedIndex = 0;
        }


        #endregion

        #region DgvSettings
        void SetDGVColumnsSettings()
        {
            _logger.Information($"Настойка отображения таблицы");

            // Автоподбор ширины столбцов
            //dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.RowHeadersWidth = 25;

            //// автоперенос в ячейках
            dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            var autosizeColumn = new List<string>
            {
                nameof(SummaryOutlayDataGridItem.TcName),
                nameof(SummaryOutlayDataGridItem.TechProcess),
                nameof(SummaryOutlayDataGridItem.Parameter),
                nameof(SummaryOutlayDataGridItem.ComponentOutlay),
                nameof(SummaryOutlayDataGridItem.UnitType),
                nameof(SummaryOutlayDataGridItem.SummaryOutlay),
                nameof(SummaryOutlayDataGridItem.SummaryOutlayCost),
            };

            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                if (autosizeColumn.Contains(column.Name))
                    dgvMain.Columns[column.Name].SortMode = DataGridViewColumnSortMode.Automatic;
                else
                    dgvMain.Columns[column.Name].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            foreach(DataGridViewColumn col in dgvMain.Columns)
            {
                col.FillWeight = col.Name.Contains("Staff") || col.Name.Contains("Machine")
                    ? GetFillWeight(col.Name.Split(" ")[0])
                    : GetFillWeight(col.Name);
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                col.MinimumWidth = col.Name.Contains("Staff") ? 60 : (int)col.FillWeight;
                col.MinimumWidth = col.Name.Contains("Machine") ? 180 : (int)col.FillWeight;

                col.Resizable = DataGridViewTriState.True;
            }
            dgvMain.Columns[nameof(SummaryOutlayDataGridItem.TcName)].Frozen = true;
            dgvMain.Columns[nameof(SummaryOutlayDataGridItem.TechProcess)].Frozen = true;
            dgvMain.Columns[nameof(SummaryOutlayDataGridItem.Parameter)].Frozen = true;
            dgvMain.ScrollBars = ScrollBars.Both;
        }

        private float GetFillWeight(string FieldName)
        {
            switch (FieldName)
            {
                case "Staff":
                    return 75;
                case "Machine":
                    return 220;
                case "ComponentOutlay":
                case "SummaryOutlay":
                case "UnitType":
                case "SummaryOutlayCost":
                    return 190;
                case "TcName":
                case "TechProcess":
                case "Parameter":
                    return 170;
                default:
                    return 100;
            }
        }
        #endregion

        #region SetupDGV

        private void SetupDGV()
        {
            InitializeDataGrid();
            AddRowsToGrid();
            SetDGVColumnsSettings();
        }
        private void InitializeDataGrid()
        {
            _logger.Information($"Проиходит инициализация таблицы dgvMain");

            dgvMain.Rows.Clear();

            while (dgvMain.Columns.Count > 0)
            {
                dgvMain.Columns.RemoveAt(0);
            }

            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.TcName), "Наименование ТК");
            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.TechProcess), "Тех. процесс");
            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.Parameter), "Параметр");

            foreach (var staff in _allOutlays.Where(s => s.Type == OutlayType.Staff).Select(x => x.Name).Distinct().OrderBy(s => s))
            {
                var staffSymbol = staff.Split(" ")[0];

                if (dgvMain.Columns.Contains($"Staff{staffSymbol}"))
                    continue;

                dgvMain.Columns.Add($"Staff{staffSymbol}", $"{staffSymbol}, ч.");
            }
            _logger.Information($"Добавлены столбцы персонала");

            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.ComponentOutlay), "Материалы, Руб. без НДС");

            foreach (var machine in _allOutlays.Where(s => s.Type == OutlayType.Mechine).Distinct())
            {
                if (dgvMain.Columns.Contains($"Machine {machine.ChildId}"))
                    continue;

                dgvMain.Columns.Add($"Machine {machine.ChildId}", $"{machine.Name}, ч.");
            }
            _logger.Information($"Добавлены столбцы механизмов");

            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.SummaryOutlay), "Общее время выполнения работ, ч");
            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.UnitType), "Составляющие затрат\n(ед. Измерения)");
            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.SummaryOutlayCost), "Составляющие затрат\n(стоимость), Руб. без НДС");

            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

        }
        private void AddRowsToGrid()
        {
            _logger.Information($"Иницирован процесс добавления строк с таблицу");

            int rowCount = 0;

            foreach (var summaryOutlayDataGridItem in _displayedList)
            {
                dgvMain.Rows.Add();

                foreach (DataGridViewColumn column in dgvMain.Columns)
                {
                    var containedStaff = summaryOutlayDataGridItem.listStaffStr.FirstOrDefault(stringToCheck => column.Name.Contains(stringToCheck.StaffName));
                    var containedMachine = summaryOutlayDataGridItem.listMachStr.FirstOrDefault(stringToCheck => column.Name.Equals($"Machine {stringToCheck.MachineId}"));

                    if (containedStaff != null)
                    {
                        var value = dgvMain.Rows[rowCount].Cells[column.Index].Value == null || dgvMain.Rows[rowCount].Cells[column.Index].Value == " - "
                                        ? 0
                                        : (double)dgvMain.Rows[rowCount].Cells[column.Index].Value;
                        dgvMain.Rows[rowCount].Cells[column.Index].Value = value + containedStaff.StaffOutlay;
                    }
                    else if (containedMachine != null)
                    {
                        dgvMain.Rows[rowCount].Cells[column.Index].Value = containedMachine.MachineOutlay;
                    }
                    else
                        dgvMain.Rows[rowCount].Cells[column.Index].Value = " - ";
                }

                dgvMain.Rows[rowCount].Cells["TcName"].Value = summaryOutlayDataGridItem.TcName;
                dgvMain.Rows[rowCount].Cells["TechProcess"].Value = summaryOutlayDataGridItem.TechProcess;
                dgvMain.Rows[rowCount].Cells["Parameter"].Value = summaryOutlayDataGridItem.Parameter;
                dgvMain.Rows[rowCount].Cells["ComponentOutlay"].Value = summaryOutlayDataGridItem.ComponentOutlay;
                dgvMain.Rows[rowCount].Cells["SummaryOutlay"].Value = summaryOutlayDataGridItem.SummaryOutlay;
                dgvMain.Rows[rowCount].Cells["UnitType"].Value = summaryOutlayDataGridItem.UnitType.GetDescription();

                dgvMain.Rows[rowCount].Cells["SummaryOutlayCost"].Value = CalculateSummaryOutlayCost(summaryOutlayDataGridItem).ToString("0.00");

                rowCount++;
                _logger.Information($"Добавлена строка {rowCount}, она содержит информацию о ТК: {summaryOutlayDataGridItem.TcName}");
            }


        }

        private double CalculateSummaryOutlayCost(SummaryOutlayDataGridItem summaryOutlayDataGridItem)
        {
            _logger.Information($"Иницирован расчет сводных затрат ТК {summaryOutlayDataGridItem.TcName}");

            double summaryOutlay = 0;
            int staffCount = 0;
            try
            {
                string jsonString = File.ReadAllText("SummaryOutlaySettings.json");
                _settings = JsonSerializer.Deserialize<SummaryOutlaySettings>(jsonString);
            }
            catch (FileNotFoundException fe)
            {
                _logger.Error($"Ошибка при загрузке данных настроек, файл не найден: {fe.Message}");
                MessageBox.Show("Ошибка при загрузке данных настроек, файл не найден: \n" + fe.Message);
            }
            catch (IOException io)
            {
                _logger.Error($"Ошибка ввода-вывода. Файл может быть недоступен: {io.Message}");
                MessageBox.Show("Ошибка ввода-вывода. Файл может быть недоступен: \n" + io.Message);
            }
            catch (UnauthorizedAccessException un)
            {
                _logger.Error($"Нет прав доступа к файлу: {un.Message}");
                MessageBox.Show("Нет прав доступа к файлу.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при загрузке данных настроек: {ex.Message}");
                MessageBox.Show("Ошибка загрузки данных настроек: " + ex.Message);
            }

            var groupedStaff = summaryOutlayDataGridItem.listStaffStr.GroupBy(s => s.StaffName.Split(" ")[0]).ToList();

            foreach(var list in groupedStaff)
            {
                var key = list.Key;
                var staffList = list.ToList();

                var maxCost = staffList.Max(m => m.StaffCost);
                if(maxCost == 0 || maxCost == null)
                {
                    summaryOutlay += key.Equals("ЭР1")
                        ? summaryOutlayDataGridItem.SummaryOutlay * _settings.LeaderSallary
                        : _settings.RegularSallary * summaryOutlayDataGridItem.SummaryOutlay;
                }
                else
                {
                    summaryOutlay += summaryOutlayDataGridItem.SummaryOutlay * (double)maxCost;
                }
            }

            foreach (var machine in summaryOutlayDataGridItem.listMachStr)
            {
                summaryOutlay += machine.MachineCost == null
                    ? 0 * machine.MachineOutlay
                    : machine.MachineOutlay * (double)machine.MachineCost;
            }

            //summaryOutlay += staffCount * summaryOutlayDataGridItem.SummaryOutlay * settings.RegularSallary;
            summaryOutlay += summaryOutlayDataGridItem.ComponentOutlay;

            return summaryOutlay;
        }

        #endregion
    }
}

