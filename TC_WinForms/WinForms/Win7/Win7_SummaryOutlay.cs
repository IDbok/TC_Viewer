using ExcelParsing.DataProcessing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml.Style;
using Serilog;
using Serilog.Filters;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.Extensions;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Win7.Work;
using TC_WinForms.WinForms.Work;
using TcDbConnector;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TcModels.Models.TcContent.Outlay;

namespace TC_WinForms.WinForms.Win7
{
    public partial class Win7_SummaryOutlay : Form, IPaginationControl
    {
        #region Fields

        private readonly User.Role _accessLevel;
        private readonly int _minRowHeight = 20;

        private DbConnector dbCon = new DbConnector();

        private readonly ILogger _logger;

        private List<Outlay> _allOutlays = new();
        private readonly int _linesPerPage = 50;
        private List<SummaryOutlayDataGridItem> _allOutlaysList = new List<SummaryOutlayDataGridItem>();
        private List<SummaryOutlayDataGridItem> _displayedList = new List<SummaryOutlayDataGridItem>();
        PaginationControlService<SummaryOutlayDataGridItem> paginationService;


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
        }

        #endregion

        #region FormLoad

        private void Win7_SummaryOutlay_Load(object sender, EventArgs e)
        {
            _logger.Information("Загрузка формы Win7_1_TCs");

            Enabled = false;
            dgvMain.Visible = false;

            // Проверяем наличие файла summaryoutlaysettings.json и создаем его с настройками по умолчанию, если он отсутствует
            EnsureOutlaySettingsExists();
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
                _allOutlays = dbCon.GetObjectList<Outlay>();
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
            _allOutlaysList.Clear();
            var groupedOutlays = _allOutlays.GroupBy(x => x.TcId).ToList();
            foreach (var outlay in groupedOutlays)
            {
                var tcId = outlay.Key;
                var outlayData = outlay.ToList();
                List<(string MachineName, double MachineOutlay, int MachineId)> listMachStr = new List<(string MachineName, double MachineOutlay, int MachineId)>();
                List<(string StaffName, double StaffOutlay)> listStaffStr = new List<(string StaffName, double StaffOutlay)>();

                foreach (var machine in outlayData.Where(x => x.Type == OutlayType.Mechine).ToList())
                    listMachStr.Add((machine.Name, machine.OutlayValue, (int)machine.ChildId));

                foreach (var staff in outlayData.Where(x => x.Type == OutlayType.Staff).ToList())
                    listStaffStr.Add((staff.Name.Split(" ")[0], staff.OutlayValue));

                _allOutlaysList.Add(new SummaryOutlayDataGridItem
                {
                    TcId = tcId,
                    listStaffStr = listStaffStr,
                    listMachStr = listMachStr,
                    ComponentOutlay = outlayData.Where(s => s.Type == OutlayType.Components).Select(s => s.OutlayValue).First(),
                    SummaryOutlay = outlayData.Where(s => s.Type == OutlayType.SummaryTimeOutlay).Select(s => s.OutlayValue).First(),
                });
            }

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
                                x.TechProcessType = techCard.TechnologicalProcessType;
                                x.Parameter = techCard.Parameter;
                                x.UnitType = techCard.OutlayUnit;
                                x.SummaryOutlayCost = CalculateSummaryOutlayCost(x);
                            }
                        }
                    );
            }
        }
        static void EnsureOutlaySettingsExists()
        {
            if (!File.Exists("SummaryOutlaySettings.json"))
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
        }

        #endregion

        #region Filtering

        private void FilterObjects()
        {
            if (!_isDataLoaded) { return; }

            try
            {
                var filteredList = ApplyFilters();
                paginationService.SetAllObjectList(filteredList);
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
                || (outlay.TechProcessType.Contains(searchText, StringComparison.OrdinalIgnoreCase)) || (outlay.Parameter.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                && (typeFilter == "Все" || string.IsNullOrWhiteSpace(typeFilter) || outlay.UnitType.GetDescription() == typeFilter)).ToList();
        }

        #endregion

        #region Pagination

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

        private void UpdateDisplayedData()
        {
            var pageData = paginationService.GetPageData();
            if (pageData != null)
            {
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
            var tcExporter = new SummOutlayExcelExport();

            await tcExporter.SaveSummOutlaytoExcelFile(_allOutlaysList);
        }
        private void btnSettings_Click(object sender, EventArgs e)
        {
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
            // Автоподбор ширины столбцов
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.RowHeadersWidth = 25;

            //// автоперенос в ячейках
            dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            var autosizeColumn = new List<string>
            {
                nameof(SummaryOutlayDataGridItem.TcName),
                nameof(SummaryOutlayDataGridItem.TechProcessType),
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
            dgvMain.Rows.Clear();

            while (dgvMain.Columns.Count > 0)
            {
                dgvMain.Columns.RemoveAt(0);
            }

            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.TcName), "Наименование ТК");
            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.TechProcessType), "Тип тех. процесса");
            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.Parameter), "Параметр");

            foreach (var staff in _allOutlays.Where(s => s.Type == OutlayType.Staff).Select(x => x.Name).Distinct())
            {
                var staffSymbol = staff.Split(" ")[0];

                if (dgvMain.Columns.Contains($"Staff{staffSymbol}"))
                    continue;

                dgvMain.Columns.Add($"Staff{staffSymbol}", $"{staffSymbol}");
            }

            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.ComponentOutlay), "Материалы");

            foreach (var machine in _allOutlays.Where(s => s.Type == OutlayType.Mechine).Distinct())
            {
                if (dgvMain.Columns.Contains($"Machine{machine.ChildId}"))
                    continue;

                dgvMain.Columns.Add($"Machine{machine.ChildId}", $"{machine.Name}");
            }

            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.SummaryOutlay), "Общее время выполнения работ");
            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.UnitType), "Составляющие затрат\n(ед. Измерения)");
            dgvMain.Columns.Add(nameof(SummaryOutlayDataGridItem.SummaryOutlayCost), "Составляющие затрат\n(стоимость)");

            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

        }
        private void AddRowsToGrid()
        {
            int rowCount = 0;
            foreach (var summaryOutlayDataGridItem in _displayedList)
            {
                dgvMain.Rows.Add();

                foreach (DataGridViewColumn column in dgvMain.Columns)
                {
                    var containedStaff = summaryOutlayDataGridItem.listStaffStr.FirstOrDefault(stringToCheck => column.Name.Contains(stringToCheck.StaffName));
                    var containedMachine = summaryOutlayDataGridItem.listMachStr.FirstOrDefault(stringToCheck => column.Name.Equals($"Machine{stringToCheck.MachineId}"));

                    if (containedStaff != (null, 0))
                    {
                        var value = dgvMain.Rows[rowCount].Cells[column.Index].Value == null || dgvMain.Rows[rowCount].Cells[column.Index].Value == " - "
                                        ? 0
                                        : (double)dgvMain.Rows[rowCount].Cells[column.Index].Value;
                        dgvMain.Rows[rowCount].Cells[column.Index].Value = value + containedStaff.StaffOutlay;
                    }
                    else if (containedMachine != (null, 0, 0))
                    {
                        dgvMain.Rows[rowCount].Cells[column.Index].Value = containedMachine.MachineOutlay;
                    }
                    else
                        dgvMain.Rows[rowCount].Cells[column.Index].Value = " - ";
                }

                dgvMain.Rows[rowCount].Cells["TcName"].Value = summaryOutlayDataGridItem.TcName;
                dgvMain.Rows[rowCount].Cells["TechProcessType"].Value = summaryOutlayDataGridItem.TechProcessType;
                dgvMain.Rows[rowCount].Cells["Parameter"].Value = summaryOutlayDataGridItem.Parameter;
                dgvMain.Rows[rowCount].Cells["ComponentOutlay"].Value = summaryOutlayDataGridItem.ComponentOutlay;
                dgvMain.Rows[rowCount].Cells["SummaryOutlay"].Value = summaryOutlayDataGridItem.SummaryOutlay;
                dgvMain.Rows[rowCount].Cells["UnitType"].Value = summaryOutlayDataGridItem.UnitType.GetDescription();
                dgvMain.Rows[rowCount].Cells["SummaryOutlayCost"].Value = CalculateSummaryOutlayCost(summaryOutlayDataGridItem);

                rowCount++;
            }

            
        }

        private double CalculateSummaryOutlayCost(SummaryOutlayDataGridItem summaryOutlayDataGridItem)
        {
            double summaryOutlay = 0;
            int staffCount = 0;
            string jsonString = File.ReadAllText("SummaryOutlaySettings.json");
            SummaryOutlaySettings settings = JsonSerializer.Deserialize<SummaryOutlaySettings>(jsonString);

            foreach (var staff in summaryOutlayDataGridItem.listStaffStr)
            {
                if (staff.StaffName.Contains("ЭР1"))
                    summaryOutlay += summaryOutlayDataGridItem.SummaryOutlay * settings.LeaderSallary;
                else
                    staffCount++;
            }

            using (MyDbContext context = new MyDbContext())
            {
                foreach (var machine in summaryOutlayDataGridItem.listMachStr)
                {
                    var price = context.Machines.Where(s => s.Name == machine.MachineName).Select(s => s.Price).First();
                    summaryOutlay += machine.MachineOutlay * (double)price;
                }
            }

            summaryOutlay += staffCount * summaryOutlayDataGridItem.SummaryOutlay * settings.RegularSallary;
            summaryOutlay += summaryOutlayDataGridItem.ComponentOutlay;

            return summaryOutlay;
        }

        #endregion

    }
}
