using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TcModels.Models.TcContent.RoadMap;
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Controls.Control;
using Binding = System.Windows.Data.Binding;
using TC_WinForms.Converters;
using TcModels.Models.TcContent;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Win6.Models;
using TC_WinForms.WinForms.Diagram;
using System.ComponentModel;

namespace TC_WinForms.WinForms.Win6.RoadMap
{
    public partial class RoadMapControl : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        #region Fields

        public ObservableCollection<RoadMapItem> RoadmapItems { get; } = new ObservableCollection<RoadMapItem>();
        private List<TechOperationWork> _techOperationWorks = new List<TechOperationWork>();
        private List<OperationGroup> _operationGroups = new List<OperationGroup>();
        private TcViewState _tcViewState;
        private int _maxColumns;

        public bool IsViewMode => _tcViewState.IsViewMode;

        #endregion

        #region Constructors

        public RoadMapControl(List<TechOperationWork> techOperationWorks, TcViewState tcViewState)
      : this(tcViewState)
        {
            _techOperationWorks = techOperationWorks;
            DetermineColumnsData();
            CountMaxColumns();
        }

        public RoadMapControl(List<RoadMapItem> roadmapItems, TcViewState tcViewState)
            : this(tcViewState)
        {
            RoadmapItems = new ObservableCollection<RoadMapItem>(roadmapItems);
            CountMaxColumns();
        }

        private RoadMapControl(TcViewState tcViewState)
        {
            _tcViewState = tcViewState;
            _tcViewState.ViewModeChanged += OnViewModeChanged;

            InitializeComponent();
            DataContext = this;
        }

        #endregion

        #region EventsAndProperties

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnViewModeChanged()
        {
            OnPropertyChanged(nameof(IsViewMode));
            if (!_tcViewState.RoadmapInfo.IsRoadMapUpdate && !_tcViewState.IsViewMode)
                _tcViewState.RoadmapInfo = (true, _tcViewState.RoadmapInfo.RoadMapItems);
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region FormLoad

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (RoadmapItems.Count == 0)
                WriteRoadMapData();

            //AddMergedHeader();
            AddColumnsToGrid();
            UpdateHeaderGridWidth();

            GenerateDataStructure();
        }
        private void WriteRoadMapData()
        {
            int colIndex = 1;
            var maxColInd = colIndex;
            var outlayService = new OutlayService();
            foreach (var group in _operationGroups)
            {
                colIndex = maxColInd;
                foreach (var tow in group.Items)
                {
                    var roadmapItem = new RoadMapItem
                    {
                        TowId = tow.Id,
                        TOName = tow.techOperation.Name,
                        Staffs = string.Join(", ", tow.executionWorks.SelectMany(ew => ew.Staffs).Select(s => s.Symbol).Distinct()),
                        Note = tow.Note ?? "",
                        Order = tow.Order,
                        techOperationWork = tow
                    };

                    roadmapItem.SequenceCells = new SequenceCell
                    {
                        RoadmapItemId = roadmapItem.Id,
                        Column = !group.IsParallel ? colIndex : CalculateColumn(group, tow, colIndex),
                        ColumnSpan = !group.IsParallel ? 1 : CalculateColumnSpan(group, tow, colIndex),
                        Order = tow.Order,
                        Value = outlayService.GetToOutlay(tow)
                    };
                    RoadmapItems.Add(roadmapItem);
                    colIndex = !group.IsParallel ? colIndex : roadmapItem.SequenceCells.Column;
                    colIndex++;
                    maxColInd = roadmapItem.SequenceCells.Column + roadmapItem.SequenceCells.ColumnSpan >= colIndex //ищем какую максимальную колонку займет эта запись в ДК, проверяем больше ли номер колонки вместе с объединением индекса колонки
                        ? roadmapItem.SequenceCells.Column + roadmapItem.SequenceCells.ColumnSpan > maxColInd ? roadmapItem.SequenceCells.Column + roadmapItem.SequenceCells.ColumnSpan : maxColInd //если больше, проверяем больше ли он последней наибольшей колонки записи, если да - перезаписываем
                        : colIndex;
                }
            }
        }
        private void AddColumnsToGrid()
        {

            for (int i = 0; i < _maxColumns; i++)
            {
                // Create a style for the cell
                // Создаем стиль для ячейки
                Style cellStyle = new Style(typeof(DataGridCell));

                // Создаем стиль для текста внутри ячейки (TextBlock)
                Style elementStyle = new Style(typeof(TextBlock));
                elementStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Black)); // Черный цвет по умолчанию

                // Создание тригера для подсветки ячейки цветом если она не пустая
                DataTrigger triggerBrush = new DataTrigger
                {
                    Binding = new Binding($"SequenceData[{i}]") { Converter = new NotZeroToBoolConverter() },
                    Value = true
                };

                // Создание тригера для окрашивание текста в цвет ячейки, если она она объеденена с помощью ColumnSpan
                DataTrigger triggerTextColor = new DataTrigger
                {
                    Binding = new Binding($"SequenceData[{i}]"),
                    Value = -1d // Указываем значение -1
                };

                triggerBrush.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Yellow));
                triggerTextColor.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.Yellow));

                cellStyle.Triggers.Add(triggerBrush);
                cellStyle.Triggers.Add(triggerTextColor);
                // Добавляем триггер в стиль текста
                elementStyle.Triggers.Add(triggerTextColor);

                // Добавление колонок для отображения данных паралелльности/последовательности выполнения ТО
                MainDataGrid.Columns.Add(new DataGridTextColumn
                {
                    //Header = $"Время действий, мин",
                    Binding = new Binding($"SequenceData[{i}]")
                    {
                        Converter = new ZeroToEmptyConverter()
                    },
                    MinWidth = 75,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellStyle = cellStyle,
                    ElementStyle = elementStyle, // Установка ElementStyle
                    IsReadOnly = true
                });
            }

        }
        private void UpdateHeaderGridWidth()
        {
            for (int i = 0; i < MainDataGrid.Columns.Count - _maxColumns; i++)//Настраиваем первые 3 хэдера столбцов MainDataGrid
            {
                var mainColumn = MainDataGrid.Columns[i];
                var headerColumn = HeaderGrid.Columns[i];

                // Устанавливаем начальную ширину
                headerColumn.Width = new DataGridLength(mainColumn.ActualWidth);

                // Подписываемся на изменения ActualWidth колонок MainDataGrid
                var descriptor = DependencyPropertyDescriptor.FromProperty(
                    DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
                descriptor.AddValueChanged(mainColumn, (s, args) =>
                {
                    headerColumn.Width = new DataGridLength(mainColumn.ActualWidth);
                });
            }
        }
        private void GenerateDataStructure()
        {
            if (RoadmapItems.All(s => s.SequenceData != null))
                return;

            foreach (var item in RoadmapItems)
            {
                item.SequenceData = new double[_maxColumns];
                int columnIndex = item.SequenceCells.Column - 1;
                int rowIndex = item.SequenceCells.Order - 1;

                int startCol = columnIndex;
                int endCol = columnIndex + item.SequenceCells.ColumnSpan - 1;

                for (int col = startCol; col <= endCol; col++)
                {
                    if (rowIndex >= 0 && rowIndex <= RoadmapItems.Count &&
                    col >= 0 && col < _maxColumns)
                    {
                        item.SequenceData[col] = col == startCol ? item.SequenceCells.Value : -1d;
                        System.Diagnostics.Debug.WriteLine($"Set value {item.SequenceCells.Value} at row {rowIndex}, column {col}");
                    }
                }
            }

            _tcViewState.RoadmapInfo = (true, RoadmapItems.ToList());
        }
        #endregion

        #region SupportMethods
        private void DetermineColumnsData()
        {
            var orderedTOWs = _techOperationWorks.OrderBy(o => o.Order).ToList();
            OperationGroup? currentGroup = null;

            foreach (var tow in orderedTOWs)
            {
                var parallelIndex = tow.GetParallelIndex();
                var sequenceGroupIndex = tow.GetSequenceGroupIndex() ?? 0;

                if (currentGroup == null ||
                (currentGroup.IsParallel != (parallelIndex != null)) ||
                (currentGroup.IsParallel && currentGroup.ParallelIndex != parallelIndex) ||
                (currentGroup.SequenceGroupIndex != sequenceGroupIndex))
                {
                    currentGroup = new OperationGroup
                    {
                        IsParallel = parallelIndex != null,
                        ParallelIndex = parallelIndex,
                        SequenceGroupIndex = sequenceGroupIndex
                    };
                    _operationGroups.Add(currentGroup);
                }

                currentGroup.Items.Add(tow);
            }
        }
        private void CountMaxColumns()
        {
            if (RoadmapItems.Count != 0)
            {
                _maxColumns = RoadmapItems.First().SequenceData.Length;
                return;
            }

            var groupedData = _operationGroups.GroupBy(g => g.ParallelIndex);
            foreach (var group in groupedData)
            {
                if (group.Key == null)
                {
                    _maxColumns += group.Sum(g => g.Items.Count);
                }
                else
                {
                    if (group.ToList().Count > 1)
                    {
                        var maxGroup = group.Where(s => s.SequenceGroupIndex != 0).Max(s => s.Items.Count);
                        _maxColumns += maxGroup;
                    }
                    else
                        _maxColumns++;
                }
            }

        }
        private int CalculateColumnSpan(OperationGroup operationGroup, TechOperationWork currentTow, int colIndex)
        {
            if (operationGroup.ParallelIndex == null || operationGroup.SequenceGroupIndex != 0)
                return 1;

            if (_operationGroups.Where(o => o.ParallelIndex == operationGroup.ParallelIndex).ToList().Count > 1)
            {
                var parallelGroups = _operationGroups.Where(o => o.ParallelIndex == operationGroup.ParallelIndex && o.SequenceGroupIndex != 0).Select(o => o.Items).Max(i => i.Count);
                return parallelGroups;
            }
            else
                return 1;
            //throw new NotImplementedException();
        }
        private int CalculateColumn(OperationGroup operationGroup, TechOperationWork currentTow, int colIndex)
        {
            if (operationGroup.ParallelIndex == null)
                return colIndex;

            if (operationGroup.SequenceGroupIndex == 0)
            {
                int? existedColIndex = RoadmapItems.Where(item => operationGroup.Items.Select(t => t.Id).Contains(item.TowId)).Where(s => s.SequenceCells.Column != 0).Select(s => s.SequenceCells.Column).FirstOrDefault();
                if (existedColIndex != null && existedColIndex != 0)
                    return (int)existedColIndex;
                else
                    return colIndex;
            }
            else
            {
                if (operationGroup.Items.IndexOf(currentTow) != 0)
                {
                    var previousElement = operationGroup.Items[operationGroup.Items.IndexOf(currentTow) - 1];
                    var previousCol = RoadmapItems.Where(item => item.TowId == previousElement.Id).Select(s => s.SequenceCells.Column).FirstOrDefault();
                    return previousCol + 1;
                }
                TechOperationWork? previousTow = null;
                var previousGroup = _operationGroups.Where(o => o.ParallelIndex == operationGroup.ParallelIndex && _operationGroups.IndexOf(o) < _operationGroups.IndexOf(operationGroup)).FirstOrDefault();

                if (previousGroup != null)
                    previousTow = previousGroup.Items.Where(o => o.Order < currentTow.Order).FirstOrDefault();

                if (previousTow != null)
                {
                    var previousCol = RoadmapItems.Where(item => item.TowId == previousTow.Id).Select(s => s.SequenceCells.Column).FirstOrDefault();
                    return previousCol;
                }
                else
                    return colIndex;
            }
        }
        #endregion

        #region UIEventHandlers

        private void TextBox_NoteTextChanged(object sender, TextChangedEventArgs e)
        {
            RoadMapItem gridItem = (RoadMapItem)(((System.Windows.Controls.TextBox)sender).DataContext);

            if (gridItem != null)
            {
                gridItem.Note = ((System.Windows.Controls.TextBox)sender).Text;
                gridItem.techOperationWork.Note = gridItem.Note;
            }
        }

        #endregion

    }

    public class OperationGroup
    {
        public bool IsParallel { get; set; } // true - параллельная, false - последовательная
        public int? ParallelIndex { get; set; } // null для последовательных
        public int? SequenceGroupIndex { get; set; } // индекс группы внутри последовательности
        public List<TechOperationWork> Items { get; } = new List<TechOperationWork>();
    }
}