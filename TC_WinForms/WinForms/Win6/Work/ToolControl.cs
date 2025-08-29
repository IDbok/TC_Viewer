using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using TC_WinForms.Extensions;
using TC_WinForms.WinForms.Win6.Models;
using TC_WinForms.WinForms.Work;
using TcDbConnector;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TC_WinForms.WinForms.Win6.Work
{
    public partial class ToolControl : UserControl
    {
        #region Fields

        private ILogger _logger = Log.Logger.ForContext<ToolControl>();

        private TechOperationWork? _parentTechOperatinWork = null;
        private readonly TcViewState _tcViewState;
        private readonly MyDbContext _context;
        private List<string> AllFilterInstrument = new List<string>();

        #endregion

        #region Constructor

        /// <summary>
        /// Создаёт новый экземпляр <see cref="ToolControl"/>.
        /// </summary>
        /// <param name="myDbContext">Экземпляр <see cref="MyDbContext"/> для доступа к БД.</param>
        /// <param name="tcViewState">Объект, хранящий текущее состояние ТК и другие настройки.</param>
        public ToolControl(MyDbContext myDbContext, TcViewState tcViewState)
        {
            InitializeComponent();
            _context = myDbContext;
            _tcViewState = tcViewState;

            SubscribeToEvents();
            _logger.Information("Окно ToolControl инициализировано");
            ConfigureCombobox();
        }

        #endregion

        #region Initialization and configuration

        /// <summary>
        /// Подписывается на события элементов управления для обработки взаимодействия с пользователем.
        /// Обрабатывает клики по ячейкам таблиц, редактирование, валидацию и изменения фильтров.
        /// </summary>
        private void SubscribeToEvents()
        {
            dgvToolsGlobal.CellClick += DgvToolsGlobal_CellClick;
            dgvToolsLocal.CellClick += DgvToolsLocal_CellClick;
            dgvToolsLocal.CellEndEdit += DgvToolsLocal_CellEndEdit;
            dgvToolsLocal.CellValidating += CellValidating;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            cmbxFilter.SelectedIndexChanged += CmbxFilter_SelectedIndexChanged;
        }

        /// <summary>
        /// Данный метод заполняет комбобокс для фильтрации списка возможных для добавления инструментов
        /// </summary>
        private void ConfigureCombobox()
        {
            AllFilterInstrument.Add("Всё");
            AllFilterInstrument.AddRange( _context.Tools.Select(t => t.Categoty).Distinct().ToList());
            cmbxFilter.DataSource = AllFilterInstrument;
            cmbxFilter.SelectedIndex = 0;
            _logger.Information($"Список для фильтрации заполнен, всего значений: {AllFilterInstrument.Count}");
        }


        /// <summary>
        /// Устанавливает для данного контрола родительский объект <see cref="TechOperationWork"/>
        /// чтобы впоследствии пользователь мог выбрать,
        /// какие <see cref="ToolWork"/> добавлять/убирать.
        /// </summary>
        /// <param name="parentTOW">Объект класса <see cref="TechOperationWork"/>,
        /// чей список инструментов будет редактироваться</param>
        public void SetParentTechOpetarionWork(TechOperationWork parentTOW)
        {
            _logger.Information("Установка родительского ТO {ParentEWId}", parentTOW?.Id);

            if (parentTOW == null)
            {
                _logger.Error("Технологическая операция не обнаружена");
                MessageBox.Show("Технологическая операция не обнаружена", "ТО не обнаружена!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _parentTechOperatinWork = parentTOW;
            RefreshAllData();
        }

        /// <summary>
        /// Заполняет данными локальную таблицу <see cref="dgvToolsLocal"/>
        /// </summary>
        public void RefreshToolDataLocal()
        {
            _logger.Information("Начато заполнение таблицы dgvToolsLocal");
            var offScroll = dgvToolsLocal.FirstDisplayedScrollingRowIndex;
            dgvToolsLocal.Rows.Clear();

            if(_parentTechOperatinWork == null)
            {
                _logger.Error("Технологическая операция не обнаружена");
                MessageBox.Show("Технологическая операция не обнаружена", "ТО не обнаружена!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var LocalInstrument = _parentTechOperatinWork.ToolWorks.Where(t => t.IsDeleted == false).ToList();

            foreach (var InstrumentWork in LocalInstrument)
            {
                List<object> listItem = new List<object>
                {
                    InstrumentWork,
                    "Удалить",
                    InstrumentWork.tool.Name,
                    InstrumentWork.tool.Type ?? "",
                    InstrumentWork.tool.Unit,
                    InstrumentWork.Quantity,
                    InstrumentWork.Comments ?? ""
                };
                dgvToolsLocal.Rows.Add(listItem.ToArray());
            }

            _logger.Information($"Таблица dgvToolsLocal заполнена, всего записей: {dgvToolsLocal.RowCount}");

            dgvToolsLocal.RestoreScrollPosition(offScroll, _logger);
            dgvToolsLocal.AutoResizeRows();
        }

        /// <summary>
        /// Заполняет данными таблицу <see cref="dgvToolsGlobal"/> со всеми инструментами
        /// </summary>
        public void RefreshToolDataGlobal()
        {
            _logger.Information("Начато заполнение таблицы dgvToolsGlobal");
            var offScroll = dgvToolsGlobal.FirstDisplayedScrollingRowIndex;
            dgvToolsGlobal.Rows.Clear();
            var filteredList = new List<Tool>();
            var work = _parentTechOperatinWork;

            if (work == null)
            {
                _logger.Error("Технологическая операция не обнаружена");
                _logger.Warning("Не выбрана ТО. Обновление инструмента невозможно.");
                return;
            }

            try
            {
                filteredList = FilterTools();
            }
            catch(InvalidOperationException inEx)
            {
                MessageBox.Show($"В процессе фильтрации списка инструментов произошла ошибка: \n {inEx.Message}", "Данные не обнарудены!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch(Exception ex)
            {
                _logger.Error($"В процессе заполнения таблицы dgvToolsGlobal возникла ошибка: {ex.Message}");
                MessageBox.Show($"В процессе фильтрации списка инструментов произошла ошибка: \n {ex.Message}", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (var tool in filteredList)
            {
                var toolInTc = _tcViewState.TechnologicalCard.Tool_TCs.FirstOrDefault(t => t.ChildId == tool.Id);
                List<object> listItem = new List<object>();
                listItem.Add(tool);

                listItem.Add("Добавить");

                listItem.Add(tool.Name);
                listItem.Add(tool.Type ?? "");
                listItem.Add(tool.Unit);
                listItem.Add(toolInTc == null ? "" : toolInTc.Quantity);
                dgvToolsGlobal.Rows.Add(listItem.ToArray());
            }

            _logger.Information($"Таблица dgvToolsGlobal заполнена, всего записей: {dgvToolsGlobal.RowCount}");
            dgvToolsGlobal.RestoreScrollPosition(offScroll, _logger);
        }


        /// <summary>
        /// Вызывает обновление всех <see cref="DataGridView"/> в форме
        /// </summary>
        public void RefreshAllData()
        {
            RefreshToolDataLocal();
            RefreshToolDataGlobal();
        }
        #endregion

        #region Events

        public event EventHandler? DataChanged;
        public event Action<TechOperationWork, ToolWork>? ToolWorkDeleteRequested;

        #endregion

        #region Support methods

        /// <summary>
        /// Возвращает список всех инструментов в базе, оставляя те,
        /// которые еще не добавлены в текущую <see cref="TechOperationWork"/>
        /// и фильтрует их по наличию в списке инструментов Технологической карты.
        /// Инструменты из <see cref="TcModels.Models.IntermediateTables.Tool_TC"/> показываются первыми.
        /// </summary>
        /// <returns>Отфильтрованный и отсортированный список инструментов</returns>
        /// <exception cref="InvalidOperationException">
        /// Возникает, если <see cref="_parentTechOperatinWork"/> равен null
        /// </exception>
        private List<Tool> FilterTools()
        {
            if (_parentTechOperatinWork == null)
            {
                _logger.Error("В процессе фильтрации инструментов _parentTechOperatinWork оказался пуст");
                throw new InvalidOperationException("Техоперация отказалась пустой");
            }

            _logger.Information("Началась фильтрация списка инструментов для добавления в ТО");

            var localTools = _parentTechOperatinWork.ToolWorks.Select(t => t.tool).ToList();
            var allTools = _context.Tools.ToList();
            var remainingTools = allTools.Except(localTools).ToList();

            var searchText = string.IsNullOrWhiteSpace(txtSearch.Text) || txtSearch.Text == "Поиск"
            ? null
            : txtSearch.Text;

            var categoryFilter = cmbxFilter.SelectedItem?.ToString();

            var filteredTools = remainingTools.Where(tool =>
            (searchText == null
                || (tool.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                || (tool.ClassifierCode?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) &&
            (categoryFilter == "Все" || tool.Categoty.ToString() == categoryFilter))).ToList();

            // Теперь редактируем порядок: инструменты из Tool_TCs должны быть первыми
            var toolTcTools = _tcViewState.TechnologicalCard.Tool_TCs
                .Select(t => t.Child)
                .Where(tool => filteredTools.Contains(tool)) // только те, что прошли фильтрацию
                .ToList();

            var otherTools = filteredTools.Except(toolTcTools).ToList();

            _logger.Information("Фильтрация завершена");

            return toolTcTools.Concat(otherTools).ToList();
        }
        #endregion

        #region DGV events

        /// <summary>
        /// Обработчик события изменения выбранного элемента в комбо-боксе фильтрации.
        /// Логирует текущее значение фильтра и обновляет данные в глобальной таблице инструментов.
        /// </summary>
        private void CmbxFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _logger.Information($"Текущее значение CmbxFilter: {cmbxFilter.SelectedItem?.ToString()}");
            RefreshToolDataGlobal();
        }

        /// <summary>
        /// Обработчик события изменения текста в поле поиска.
        /// Обновляет данные в глобальной таблице инструментов при изменении текста поиска.
        /// </summary>
        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            RefreshToolDataGlobal();
        }

        /// <summary>
        /// Обработчик события валидации ячейки DataGridView.
        /// Отменяет валидацию при нажатии клавиш стрелок во время редактирования ячейки,
        /// что позволяет перемещаться между ячейками без завершения редактирования текущей.
        /// </summary>
        /// /// <remarks>
        /// Этот метод предотвращает преждевременное завершение редактирования ячейки
        /// при навигации с помощью клавиш стрелок, улучшая пользовательский опыт.
        /// Использует <see cref="System.Windows.Input.Keyboard"/> для проверки состояния клавиш.
        /// </remarks>
        private void CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            var Left = Keyboard.IsKeyDown(System.Windows.Input.Key.Left);
            var Right = Keyboard.IsKeyDown(System.Windows.Input.Key.Right);
            var Up = Keyboard.IsKeyDown(System.Windows.Input.Key.Up);
            var Down = Keyboard.IsKeyDown(System.Windows.Input.Key.Down);

            var currentGrid = sender as DataGridView;
            if (currentGrid != null && currentGrid.CurrentCell.IsInEditMode && (Left || Right || Up || Down))
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Обрабатывает завершение редактирования ячеек количества и комментариев инструментов.
        /// </summary>
        private void DgvToolsLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvToolsLocal.Columns["countColumn"].Index)
            {
                var tool = dgvToolsLocal.Rows[e.RowIndex].Cells[0].Value as ToolWork;
                var countValue = (object)dgvToolsLocal.Rows[e.RowIndex].Cells[5].Value;

                if (tool == null)
                {
                    _logger.Error("Инструмент для редактирования не обнаружен");
                    MessageBox.Show("Инструмент для редактирования не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                double result = 0;
                if (countValue is double doubleValue)
                {
                    result = doubleValue;
                }
                else if (!double.TryParse((string)countValue, out result))
                {
                    result = 0;
                }

                if (tool.Quantity == result)
                    return;

                tool.Quantity = result;

                _logger.Information($"Инструменту {tool.tool.Name} установлено количество {tool.Quantity}");

                DataChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (e.ColumnIndex == dgvToolsLocal.Columns["commentColumn"].Index)
            {
                var tool = dgvToolsLocal.Rows[e.RowIndex].Cells[0].Value as ToolWork;
                var commentValue = (string)dgvToolsLocal.Rows[e.RowIndex].Cells[6].Value;

                if (tool == null)
                {
                    _logger.Error("Инструмент для редактирования не обнаружен");
                    MessageBox.Show("Инструмент для редактирования не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                tool.Comments = commentValue;

                _logger.Information($"Инструменту {tool.tool.Name} установлен новый комментарий");

                DataChanged?.Invoke(this, EventArgs.Empty);
            }

            dgvToolsLocal.AutoResizeRows();
        }

        /// <summary>
        /// Обрабатывает клик по кнопке удаления инструмента из локальной таблицы.
        /// </summary>
        private void DgvToolsLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            { return; }

            if (e.ColumnIndex == dgvToolsLocal.Columns["deleteBtnColumn"].Index)
            {
                _logger.LogUserAction($"Щелчок по ячейке удаления инструмента в строке {e.RowIndex}.");

                if (_parentTechOperatinWork == null)
                {
                    _logger.Warning("Не выбрана ТО. Удаление инструмента невозможно.");
                    return;
                }

                var tool = (ToolWork)dgvToolsLocal.Rows[e.RowIndex].Cells[0].Value;

                ToolWorkDeleteRequested?.Invoke(_parentTechOperatinWork, tool);
                _logger.Information($"Инструмент {tool.tool.Name} помечен на удаление.");

                RefreshAllData();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Обрабатывает клик по кнопке добавления инструмента из глобальной таблицы.
        /// </summary>
        private void DgvToolsGlobal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex <0)
            { return; }

            if (e.ColumnIndex == dgvToolsLocal.Columns["addBtnColumn"].Index)
            {
                _logger.LogUserAction($"Щелчок по ячейке добавить инструмент в строке {e.RowIndex}.");

                if (_parentTechOperatinWork == null)
                {
                    _logger.Warning("Не выбрана ТО. Добавление инструмента невозможно.");
                    return;
                }

                var tool = dgvToolsGlobal.Rows[e.RowIndex].Cells[0].Value as Tool;

                if (tool == null)
                {
                    _logger.Error("Инструмент для добавления в ТК не обнаружен");
                    MessageBox.Show("Инструмент для добавления в ТК не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var existedTool = _parentTechOperatinWork.ToolWorks.FirstOrDefault(t => t.toolId == tool.Id && t.IsDeleted == true);
                var exustInInstrument = _tcViewState.TechnologicalCard.Tool_TCs.FirstOrDefault(t => t.ChildId == tool.Id);

                
                if (existedTool != null)
                {
                    _logger.Information($"Инструмент {tool.Name} ранее использовался, снимается флаг удаления");
                    existedTool.IsDeleted = false;
                    _parentTechOperatinWork.ToolWorks.Remove(existedTool);
                    _parentTechOperatinWork.ToolWorks.Insert(_parentTechOperatinWork.ToolWorks.Count, existedTool);
                }
                else
                {
                    _logger.Information($"Инструмент {tool.Name} ранее не использовался, создается новый объект");
                   
                    _parentTechOperatinWork.ToolWorks.Add(
                        new ToolWork{
                            tool = tool,
                            toolId = tool.Id,
                            Quantity = exustInInstrument?.Quantity ?? 1
                        });
                }

                if (exustInInstrument == null)
                {
                    _logger.Information($"Инструмент {tool.Name} ранее не использовался и не был указан в таблице инструментов ТК, создается новая запись");
                    
                    _tcViewState.TechnologicalCard.Tool_TCs.Add(
                        new Tool_TC
                        {
                            Parent = _tcViewState.TechnologicalCard,
                            ParentId = _tcViewState.TechnologicalCard.Id,
                            ChildId = tool.Id,
                            Child = tool,
                            Order = _tcViewState.TechnologicalCard.Tool_TCs.Count + 1
                        });
                }


                RefreshAllData();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
