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
using System.Linq;

namespace TC_WinForms.WinForms.Win6
{
    public partial class Win6_OutlayTable : Form
    {
        private List<Machine_TC> _calculatedMachines;
        private List<Staff_TC> _calculatedStaffs;
        private List<Component_TC> _calculatedComponent;
        private List<ExecutionWork> _calculatedEtaps = new List<ExecutionWork>();

        public bool _isDataLoaded = false;

        private BindingList<DisplayedOutlay> _bindingList;
        private List<DisplayedOutlay> _displayedOutlay;

        private int _tcId;

        public Win6_OutlayTable(int tcId)
        {
            _tcId = tcId;

            Log.Information("Инициализация окна Win6_OutlayTable");

            InitializeComponent();
        }

        private async Task LoadDataAsync()
        {
            using (MyDbContext context = new MyDbContext())
            {
                _displayedOutlay = await Task.Run
                (
                    () => context.Set<Outlay>()
                                 .Where(o => o.TcID == _tcId)
                                 .Select(outlay => new DisplayedOutlay(outlay))
                                 .ToList()
                );


                var a = await context.TechOperationWorks.Where(t => t.TechnologicalCardId == _tcId).Include(e => e.executionWorks).ToListAsync();
                foreach(var tow in a)
                {
                    _calculatedEtaps.AddRange(tow.executionWorks);
                }
                _calculatedMachines = await context.Set<Machine_TC>().Where(o => o.ParentId == _tcId && o.OutlayCount).Include(e => e.Child).Include(m => m.ExecutionWorks).ThenInclude(e => e.techOperationWork).ToListAsync();
                _calculatedStaffs = await context.Set<Staff_TC>().Where(o => o.ParentId == _tcId && o.OutlayCount).Include(e => e.Child).Include(m => m.ExecutionWorks).ToListAsync();
                _calculatedComponent = await context.Set<Component_TC>().Where(o => o.ParentId == _tcId).Include(c => c.Child).ToListAsync();
                
            }
        }

        private void CalculateOutlay()
        {
            CalculateStaff();
            CalculateComponents();
            CalculateMachine();
            CalculateEtaps();
            dgvMain.DataSource = _displayedOutlay;
        }

        private void CalculateComponents()
        {
            double componentOutlay = 0;
            foreach (var component in _calculatedComponent)
            {
                componentOutlay += component.Child.Price == null
                    ? component.Quantity * 0
                    : component.Quantity * (double)component.Child.Price;
            }

            var updatedOutlay = _displayedOutlay.Where(o => o.Type == OutlayType.Components.GetDescription()).FirstOrDefault();
            if (updatedOutlay != null)
            {
                updatedOutlay.OutlayValue = (int)componentOutlay;
            }
            else
            {
                var componentTypeOutlay = new Outlay
                {
                    TcID = _tcId,
                    Type = OutlayType.Components,
                    Name = null,
                    OutlayUnitType = UnitType.Сurrency,
                    OutlayValue = (int)componentOutlay
                };

                _displayedOutlay.Add(new DisplayedOutlay(componentTypeOutlay));
            }
        }
        private void CalculateStaff()
        {
            double staffOutlay = 0;
            foreach(var staff in _calculatedStaffs)
            {
                foreach(var ew in staff.ExecutionWorks)
                {
                    staffOutlay += ew.Value;
                }

                staffOutlay = staffOutlay / 60;
                var updatedOutlay = _displayedOutlay.Where(o => o.Type == OutlayType.Staff.GetDescription()).FirstOrDefault();

                if (updatedOutlay != null)
                {
                    updatedOutlay.OutlayValue = (int)staffOutlay;
                }
                else
                {
                    var componentTypeOutlay = new Outlay
                    {
                        TcID = _tcId,
                        Type = OutlayType.Staff,
                        Name = staff.Symbol + " " + staff.Child.Name,
                        OutlayUnitType = UnitType.Hours,
                        OutlayValue = staffOutlay
                    };

                    _displayedOutlay.Add(new DisplayedOutlay(componentTypeOutlay));
                }

                staffOutlay = 0;
            }
        }
        private void CalculateMachine()
        {
            double machineOutlay = 0;
            foreach (var machine in _calculatedMachines)
            {
                machineOutlay = CalculateEtapTimes(machine.ExecutionWorks);

                machineOutlay = machineOutlay / 60;
                var updatedOutlay = _displayedOutlay.Where(o => o.Type == OutlayType.Mechine.GetDescription()).FirstOrDefault();

                if (updatedOutlay != null)
                {
                    updatedOutlay.OutlayValue = (int)machineOutlay;
                }
                else
                {
                    var componentTypeOutlay = new Outlay
                    {
                        TcID = _tcId,
                        Type = OutlayType.Mechine,
                        Name = machine.Child.Name,
                        OutlayUnitType = UnitType.Hours,
                        OutlayValue = machineOutlay
                    };

                    _displayedOutlay.Add(new DisplayedOutlay(componentTypeOutlay));
                }

                machineOutlay = 0;
            }
        }
        private void CalculateEtaps()
        {
            double etapOutlay = 0;
            etapOutlay = CalculateEtapTimes(_calculatedEtaps);

            etapOutlay = etapOutlay / 60;
            var updatedOutlay = _displayedOutlay.Where(o => o.Type == OutlayType.SummaryTimeOutlay.GetDescription()).FirstOrDefault();

            if (updatedOutlay != null)
            {
                updatedOutlay.OutlayValue = (int)etapOutlay;
            }
            else
            {
                var summTypeOutlay = new Outlay
                {
                    TcID = _tcId,
                    Type = OutlayType.SummaryTimeOutlay,
                    Name = null,
                    OutlayUnitType = UnitType.Hours,
                    OutlayValue = etapOutlay
                };

                _displayedOutlay.Add(new DisplayedOutlay(summTypeOutlay));
            }

        }
        private async void Win6_OutlayTable_Load(object sender, EventArgs e)
        {
            Log.Information("Загрузка формы Win6_OutlayTable");

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
                CalculateOutlay();

                DisplayedEntityHelper.SetupDataGridView<DisplayedOutlay>(dgvMain);

                this.Enabled = true;
            }
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
                   : ($"{obj.Type.GetDescription()} ({obj.Name})");
                Name = obj.Name;
                OutlayUnitType = obj.OutlayUnitType.GetDescription();
                OutlayValue = obj.OutlayValue;
            }
        }

        private async void btnCalculateOutlay_Click_1(object sender, EventArgs e)
        {
            await LoadDataAsync();
            CalculateOutlay();
        }
        enum GroupType
        {
            Single,
            Etap,
            ParallelIndex,
        }
        private Stack<(GroupType, List<ExecutionWork>, List<ExecutionWork>)> GroupByParallelIndex(List<ExecutionWork> items)
        {
            var parallelGroups = new Stack<(GroupType, List<ExecutionWork>, List<ExecutionWork>)>();

            var currentParallelIndex = 0;
            var currentEtap = "";

            foreach (var item in items)
            {
                int? parallelIndex = item.techOperationWork.GetParallelIndex();

                if (parallelIndex == null)
                {
                    if(item.Etap == "" || item.Etap == "0")
                    {
                        // обнуляем этап
                        currentEtap = "";

                        parallelGroups.Push((GroupType.Single,
                            new List<ExecutionWork> { item },
                            new List<ExecutionWork>()));

                        continue;
                    }
                    else
                    {
                        // проверка на смену этапа
                        if (currentEtap != item.Etap)
                        {
                            currentEtap = item.Etap;

                            parallelGroups.Push((GroupType.Etap,
                                new List<ExecutionWork> { item },
                                new List<ExecutionWork>()));

                            continue;
                        }
                        else
                        {
                            // добавляем в текущий список
                            parallelGroups.Peek().Item2.Add(item);
                        }
                    }
                }
                else
                {
                    // обнуляем этап ????
                    currentEtap = "";

                    if (currentParallelIndex != parallelIndex)
                    {

                        currentParallelIndex = parallelIndex.Value;

                        parallelGroups.Push((GroupType.ParallelIndex,
                            new List<ExecutionWork> { item },
                            new List<ExecutionWork>()));
                    }
                    else
                    {
                        parallelGroups.Peek().Item2.Add(item);
                    }
                }
            }

            return parallelGroups;
        }

        private double CalculateEtapTimes(List<ExecutionWork> machineExecutionWork)
        {
            // Group items by ParallelIndex
            var parallelGroups = GroupByParallelIndex(machineExecutionWork);
            double timeOutlay = 0;
            // Process each group
            foreach (var group in parallelGroups)
            {
                var currentGroupType = group.Item1;
                var currentGroupList = group.Item2;
                var currentGroupProducts = group.Item3;

                switch (currentGroupType)
                {
                    case GroupType.Single:
                    {
                        timeOutlay += currentGroupList[0].Value;
                        break;
                    }
                    case GroupType.Etap:
                    {
                        timeOutlay += ProcessEtapGroup(currentGroupList);
                        break;
                    }
                    //case GroupType.ParallelIndex:
                    //    ProcessParallelGroup(currentGroupList);
                    //    break;
                }
            }

            var a = 0;
            return timeOutlay;
        }

        private double ProcessEtapGroup(List<ExecutionWork> executionWorks)
        {
            // Process ExecutionWorks based on Posled
            foreach (var executionWork in executionWorks)
            {
                if (!string.IsNullOrEmpty(executionWork.Posled) && executionWork.Posled != "0")
                {
                    var allSum = executionWorks
                        .Where(w => w.Posled == executionWork.Posled && w.Value != -1)
                        .Sum(s => s.Value);
                    executionWork.TempTimeExecution = allSum;
                }
                else
                {
                    executionWork.TempTimeExecution = executionWork.Value == -1 ? 0 : executionWork.Value;
                }
            }

            // Calculate the maximum TempTimeExecution among ExecutionWorks
            double maxTime = CalculateMaxEtapTime(executionWorks); //executionWorks.Max(m => m.TempTimeExecution);

            return maxTime;
        }

        private double CalculateMaxEtapTime(List<ExecutionWork> etapGroup)
        {
            // todo: проверка на наличие различных этапов внутри группы
            // Если есть разные этапы, то группируем по этапам и выполняем расчёт времени рекурсивно
            var etapGroups = etapGroup.GroupBy(g => g.Etap).ToList();
            if (etapGroups.Count > 1)
            {
                var times = new List<double>();
                foreach (var group in etapGroups)
                {
                    times.Add(CalculateMaxEtapTime(group.ToList()));
                }

                return times.Max();
            }

            // Если этап у всех операций равен 0 или "",
            // то суммировать время всех операций в группе
            if (etapGroup.All(a => a.Etap == "" || a.Etap == "0"
                //|| a.Etap == etapGroup[0].Etap
                ))
            {
                return etapGroup.Sum(s => s.Value);
            }

            var executionWorks = etapGroup.ToList();

            var executionTimes = new List<double>();

            var sequenceTimes = new List<double>();

            foreach (var item in etapGroup)
            {
                
                var executionWork = item;

                // Если у шага нет группы параллельности, то считаем его выполнение последовательным
                if (executionWork.Etap == "" || executionWork.Etap == "0")
                {
                    executionTimes.Add(executionWork.Value);
                    continue;
                }

                if (!string.IsNullOrEmpty(executionWork.Posled) && executionWork.Posled != "0")
                {
                    var allSum = executionWorks
                        .Where(w => w.Posled == executionWork.Posled && w.Value != -1
                            && w.Etap == executionWork.Etap && w.Etap != "0" && w.Etap != "")
                        .Sum(s => s.Value);

                    executionTimes.Add(allSum);
                }
                else
                {
                    executionTimes.Add(executionWork.Value);
                }
                
            }

            executionTimes.Add(sequenceTimes.Sum());

            return executionTimes.Max();
        }


    }
}
