using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Extensions;
using TcDbConnector;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TcModels.Models.TcContent.Outlay;
using TC_WinForms.WinForms.Win6.Models;
using TC_WinForms.Services;
using System.DirectoryServices.ActiveDirectory;

namespace TC_WinForms.WinForms.Win6
{
    public partial class Win6_OutlayTable : Form
    {
        enum GroupType
        {
            Single,
            Etap,
            ParallelIndex,
        }
        private CalculateOutlayService _calculateOutlayService;
        public bool _isDataLoaded = false;

        private TcViewState _tcViewState;
        private int _tcId;

        public Win6_OutlayTable(TcViewState tcViewState, CalculateOutlayService calculateOutlayService)
        {
            _tcId = tcViewState.TechnologicalCard.Id;
            _tcViewState = tcViewState;
            _calculateOutlayService = calculateOutlayService;

            Log.Information("Инициализация окна Win6_OutlayTable");

            InitializeComponent();
        }
        private void LoadCalculatedOutlayData() //Метод загрузки рассчитаных данных затрат в таблицу dgvMain
        {
            var bindingList = new BindingList<DisplayedOutlay> //запрашиваем список outlay и сразу преобразовываем его в список DisplayedOutlay
                (
                    _calculateOutlayService.GetOutlayList(_tcViewState)
                                           .Select(outlay => new DisplayedOutlay(outlay))
                                           .ToList()
                );
            dgvMain.DataSource = bindingList;
        }

        private void Win6_OutlayTable_Load(object sender, EventArgs e)
        {
            this.Enabled = false;

            Log.Information("Загрузка формы Win6_OutlayTable");

            SetDGVColumnsSettings();
            LoadCalculatedOutlayData();

            DisplayedEntityHelper.SetupDataGridView<DisplayedOutlay>(dgvMain);

            this.Enabled = true;

            Log.Information("Форма Win6_OutlayTable загружена");
        }
        void SetDGVColumnsSettings()
        {
            // автоподбор ширины столбцов под ширину таблицы
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;

            //dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.RowHeadersWidth = 25;

            //// автоперенос в ячейках
            dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        }
        private class DisplayedOutlay : IDisplayedEntity, IIdentifiable
        {
            private int id;
            private int tcID;
            private string type;
            private string outlayUnitType;
            private string? name;
            private double outlayValue;

            public Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
                {
                    { nameof(Id), "Id" },
                    { nameof(TcID), "Id Тех. карты" },
                    { nameof(Type), "Вид" },
                    { nameof(OutlayUnitType), "Ед. измерения" },
                    { nameof(Name), "Наименование" },
                    { nameof(OutlayValue), "Затраты" }
                };
            }

            public List<string> GetPropertiesOrder()
            {
                return new List<string>
                {
                    nameof(Type),
                    nameof(OutlayUnitType),
                    nameof(OutlayValue),
                };
            }

            public List<string> GetRequiredFields()
            {
                return new List<string>
                {
                    nameof(TcID),
                    nameof(Type),
                    nameof(OutlayUnitType),
                    nameof(OutlayValue),
                };
            }

            public int Id { get; set; }
            public int TcID
            {
                get => tcID;
                set
                {
                    if (tcID != value)
                    {
                        tcID = value;
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
                    }
                }
            }
            public string OutlayUnitType
            {
                get => outlayUnitType;
                set
                {
                    if (outlayUnitType != value)
                    {
                        outlayUnitType = value;
                    }
                }
            }
            public string? Name
            {
                get => name;
                set
                {
                    if (name != value)
                    {
                        name = value;
                    }
                }
            }
            public double OutlayValue
            {
                get => outlayValue;
                set
                {
                    if (outlayValue != value)
                    {
                        outlayValue = value;
                    }
                }
            }
            public DisplayedOutlay() { }

            public DisplayedOutlay(Outlay obj)
            {
                TcID = obj.TcID;
                Type = obj.Name == null
                   ? obj.Type.GetDescription()
                   : $"{obj.Type.GetDescription()} ({obj.Name})";
                Name = obj.Name;
                OutlayUnitType = obj.OutlayUnitType.GetDescription();
                OutlayValue = obj.OutlayValue;
            }
        }//Класс создан для удобного форматирования и объявления таблицы dgvMain с помощью функции SetupDataGridView

    }
}
