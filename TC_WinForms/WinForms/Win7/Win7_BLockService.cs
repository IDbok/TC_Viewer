using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms;

public partial class Win7_BLockService : Form
{
    private readonly User.Role _accessLevel;
    private DbConnector dbCon = new DbConnector();

    private BindingList<DisplayedObjectLocker> _bindingList;
    private List<DisplayedObjectLocker> _displayedObjectLockers;
    private bool isFiltered = false;
    public bool _isDataLoaded = false;

    public Win7_BLockService(User.Role accessLevel)
    {
        Log.Information("Инициализация окна Win7_BLockService для роли {AccessLevel}", accessLevel);
        _accessLevel = accessLevel;

        InitializeComponent();
    }

    private async Task LoadDataAsync()
    {
        Log.Information("Начало асинхронной загрузки данных для Win7_BLockService");

        try
        {
            using (MyDbContext context = new MyDbContext())
            {
                _displayedObjectLockers = await Task.Run
                (
                    () => context.Set<ObjectLocker>()
                                 .Select(locker => new DisplayedObjectLocker(locker))
                                 .ToList()
                );

                _displayedObjectLockers.ForEach
                    (
                        e =>
                        {
                            var entityType = context.Model.GetEntityTypes()
                                .FirstOrDefault(t => t.ClrType.Name == e.ObjectType)?.ClrType;

                            var obj = context.Find(entityType, e.ObjectId);

                            if (obj != null && obj is INameable objN)
                                e.ObjectName = objN is TechnologicalCard tc
                                ? tc.Article
                                : objN.Name;
                        }
                    );
            }

            dgvMain.DataSource = _displayedObjectLockers;

            FilterBlockedObjects();
        }
        catch (Exception e)
        {
            Log.Error("Ошибка при загрузке данных: {ExceptionMessage}", e.Message);
            MessageBox.Show("Ошибка загрузки данных: " + e.Message);
        }
    }

    private void txtSearch_TextChanged(object sender, EventArgs e)
    {
        FilterBlockedObjects();
    }

    private void FilterBlockedObjects()
    {
        if (!_isDataLoaded) { return; }

        try
        {
            var searchText = txtSearch.Text == "Поиск" ? "" : txtSearch.Text;
            var typeFilter = cmbType.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(searchText) && (
                (typeFilter == "Все") ||
                (string.IsNullOrWhiteSpace(typeFilter))))
            {
                _bindingList = new BindingList<DisplayedObjectLocker>(_displayedObjectLockers);
                dgvMain.DataSource = _bindingList; // Возвращаем исходный список, если строка поиска пуста
                isFiltered = false;
            }
            else
            {
                var filteredList = _displayedObjectLockers.Where(locker => (searchText == "" || (locker.ObjectName.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                && (typeFilter == "Все" || locker.ObjectType == typeFilter)).ToList();
                _bindingList = new BindingList<DisplayedObjectLocker>(filteredList);
                dgvMain.DataSource = _bindingList;
                isFiltered = true;
            }

        }
        catch (Exception ex)
        {

        }
    }

    private void SetupTypeFilterComboBox()
    {
        var types = _displayedObjectLockers.Select(obj => obj.ObjectType).Distinct().ToList();

        types.Sort((a, b) => b.CompareTo(a));

        cmbType.Items.Add("Все");

        foreach (var type in types)
        {
            cmbType.Items.Add(type);
        }

        cmbType.SelectedIndex = 0;
    }

    private class DisplayedObjectLocker : IDisplayedEntity, IIdentifiable
    {
        public Dictionary<string, string> GetPropertiesNames()
        {
            return new Dictionary<string, string>
        {
            { nameof(Id), "Id" },
            { nameof(ObjectId), "Id Объекта" },
            { nameof(ObjectType), "Вид объекта" },
            { nameof(ObjectName), "Наименование" },
            { nameof(TimeStamp), "Время блокировки" }
        };
        }

        public List<string> GetPropertiesOrder()
        {
            return new List<string>
            {
                //nameof(ObjectId),
                nameof(ObjectType),
                nameof(ObjectName),
                nameof(TimeStamp),
                // nameof(Id),

            };
        }

        public List<string> GetRequiredFields()
        {
            return new List<string>
            {
                nameof(ObjectId),
                nameof(ObjectType),
                nameof(ObjectName),
                nameof(TimeStamp),
                nameof(Id)
            };
        }

        private int id;
        private int objectId;
        private string objectType;
        private string objectName;
        private DateTime timeStamp;

        public int Id { get; set; }
        public int ObjectId
        {
            get => objectId;
            set
            {
                if (objectId != value)
                {
                    objectId = value;
                }
            }
        }
        public string ObjectType
        {
            get => objectType;
            set
            {
                if (objectType != value)
                {
                    objectType = value;
                }
            }
        }
        public string ObjectName
        {
            get => objectName;
            set
            {
                if (objectName != value)
                {
                    objectName = value;
                }
            }
        }
        public DateTime TimeStamp
        {
            get => timeStamp;
            set
            {
                if (timeStamp != value)
                {
                    timeStamp = value;
                }
            }
        }

        public DisplayedObjectLocker() { }

        public DisplayedObjectLocker(ObjectLocker locker)
        {
            Id = locker.Id;
            ObjectId = locker.ObjectId;
            ObjectType = locker.ObjectType;
            ObjectName = locker.ObjectName;
            TimeStamp = locker.TimeStamp;
        }

    }

    private async void Win7_BLockService_Load(object sender, EventArgs e)
    {
        Log.Information("Загрузка формы Win7_BLockService");

        this.Enabled = false;
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

            _isDataLoaded = true;

            SetDGVColumnsSettings();
            DisplayedEntityHelper.SetupDataGridView<DisplayedObjectLocker>(dgvMain);
            dgvMain.DataSource = _displayedObjectLockers;
            SetupTypeFilterComboBox();

            this.Enabled = true;
        }

    }

    void SetDGVColumnsSettings()
    {
        // автоподбор ширины столбцов под ширину таблицы
        dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        //dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvMain.RowHeadersWidth = 25;

        //// автоперенос в ячейках
        dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

    }

    private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
    {
        FilterBlockedObjects();
    }

    private async void btnUpdate_Click(object sender, EventArgs e)
    {
        await LoadDataAsync();
    }

    private async void btnDelete_Click(object sender, EventArgs e)
    {
        await DisplayedEntityHelper.DeleteSelectedObject<DisplayedObjectLocker, ObjectLocker>(dgvMain,
                _bindingList, isFiltered ? _displayedObjectLockers : null);
    }
}