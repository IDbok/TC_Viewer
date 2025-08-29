using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using TC_WinForms.Extensions;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using Component = TcModels.Models.TcContent.Component;

namespace TC_WinForms.WinForms.Win6.Work.EditorForms
{
    public partial class ComponentControl : UserControl
    {
        #region Fields

        private ILogger _logger = Log.Logger.ForContext<ToolControl>();

        private TechOperationWork? _parentTechOperatinWork = null;
        private readonly TcViewState _tcViewState;
        private readonly MyDbContext _context;
        private List<string> AllFilterComponents = new List<string>();

        #endregion

        #region Constructor

        /// <summary>
        /// Создаёт новый экземпляр <see cref="ComponentControl"/>.
        /// </summary>
        /// <param name="myDbContext">Экземпляр <see cref="MyDbContext"/> для доступа к БД.</param>
        /// <param name="tcViewState">Объект, хранящий текущее состояние ТК и другие настройки.</param>
        public ComponentControl(MyDbContext myDbContext, TcViewState tcViewState)
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
            dgvComponentsGlobal.CellClick += DgvComponentsGlobal_CellClick;
            dgvComponentsLocal.CellClick += DgvComponentsLocal_CellClick;
            dgvComponentsLocal.CellEndEdit += DgvComponentsLocal_CellEndEdit;
            dgvComponentsLocal.CellValidating += CellValidating;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            cmbxFilter.SelectedIndexChanged += CmbxFilter_SelectedIndexChanged;
        }

        /// <summary>
        /// Данный метод заполняет комбобокс для фильтрации списка возможных для добавления инструментов
        /// </summary>
        private void ConfigureCombobox()
        {
            AllFilterComponents.Add("Всё");
            AllFilterComponents.AddRange(_context.Tools.Select(t => t.Categoty).Distinct().ToList());
            cmbxFilter.DataSource = AllFilterComponents;
            cmbxFilter.SelectedIndex = 0;
            _logger.Information($"Список для фильтрации заполнен, всего значений: {AllFilterComponents.Count}");
        }


        /// <summary>
        /// Устанавливает для данного контрола родительский объект <see cref="TechOperationWork"/>
        /// чтобы впоследствии пользователь мог выбрать,
        /// какие <see cref="ComponentWork"/> добавлять/убирать.
        /// </summary>
        /// <param name="parentTOW">Объект класса <see cref="TechOperationWork"/>,
        /// чей список компонентов будет редактироваться</param>
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
        /// Заполняет данными локальную таблицу <see cref="dgvComponentsLocal"/>
        /// </summary>
        public void RefreshComponentDataLocal()
        {
            _logger.Information("Начато заполнение таблицы dgvComponentsLocal");
            var offScroll = dgvComponentsLocal.FirstDisplayedScrollingRowIndex;
            dgvComponentsLocal.Rows.Clear();

            if (_parentTechOperatinWork == null)
            {
                _logger.Error("Технологическая операция не обнаружена");
                MessageBox.Show("Технологическая операция не обнаружена", "ТО не обнаружена!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var LocalComponents = _parentTechOperatinWork.ComponentWorks.Where(t => t.IsDeleted == false).ToList();

            foreach (var ComponentWork in LocalComponents)
            {
                List<object> listItem = new List<object>
                {
                    ComponentWork,
                    "Удалить",
                    ComponentWork.component.Name,
                    ComponentWork.component.Type ?? "",
                    ComponentWork.component.Unit,
                    ComponentWork.Quantity,
                    ComponentWork.Comments ?? ""
                };
                dgvComponentsLocal.Rows.Add(listItem.ToArray());
            }

            _logger.Information($"Таблица dgvComponentsLocal заполнена, всего записей: {dgvComponentsLocal.RowCount}");

            dgvComponentsLocal.RestoreScrollPosition(offScroll, _logger);
            dgvComponentsLocal.AutoResizeRows();
        }

        /// <summary>
        /// Заполняет данными таблицу <see cref="dgvComponentsGlobal"/> со всеми инструментами
        /// </summary>
        public void RefreshComponentDataGlobal()
        {
            _logger.Information("Начато заполнение таблицы dgvComponentsGlobal");
            var offScroll = dgvComponentsGlobal.FirstDisplayedScrollingRowIndex;
            dgvComponentsGlobal.Rows.Clear();
            var filteredList = new List<Component>();
            var work = _parentTechOperatinWork;

            if (work == null)
            {
                _logger.Error("Технологическая операция не обнаружена");
                _logger.Warning("Не выбрана ТО. Обновление инструмента невозможно.");
                return;
            }

            try
            {
                filteredList = FilterComponents();
            }
            catch (InvalidOperationException inEx)
            {
                MessageBox.Show($"В процессе фильтрации списка инструментов произошла ошибка: \n {inEx.Message}", "Данные не обнарудены!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"В процессе заполнения таблицы dgvComponentsGlobal возникла ошибка: {ex.Message}");
                MessageBox.Show($"В процессе фильтрации списка инструментов произошла ошибка: \n {ex.Message}", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (var component in filteredList)
            {
                var toolInTc = _tcViewState.TechnologicalCard.Component_TCs.FirstOrDefault(t => t.ChildId == component.Id);
                List<object> listItem = new List<object>();
                listItem.Add(component);

                listItem.Add("Добавить");

                listItem.Add(component.Name);
                listItem.Add(component.Type ?? "");
                listItem.Add(component.Unit);
                listItem.Add(toolInTc?.Quantity.ToString() ?? "");
                dgvComponentsGlobal.Rows.Add(listItem.ToArray());
            }

            _logger.Information($"Таблица dgvComponentsGlobal заполнена, всего записей: {dgvComponentsGlobal.RowCount}");
            dgvComponentsGlobal.RestoreScrollPosition(offScroll, _logger);
        }


        /// <summary>
        /// Вызывает обновление всех <see cref="DataGridView"/> в форме
        /// </summary>
        public void RefreshAllData()
        {
            RefreshComponentDataLocal();
            RefreshComponentDataGlobal();
        }
        #endregion

        #region Events

        public event EventHandler? DataChanged;
        public event Action<TechOperationWork, ComponentWork>? ComponentWorkDeleteRequested;

        #endregion

        #region Support methods

        /// <summary>
        /// Возвращает список всех компонентов в базе, оставляя те,
        /// которые еще не добавлены в текущую <see cref="TechOperationWork"/>
        /// и фильтрует их по наличию в списке компонентов Технологической карты.
        /// Инструменты из <see cref="TcModels.Models.IntermediateTables.Component_TC"/> показываются первыми.
        /// </summary>
        /// <returns>Отфильтрованный и отсортированный список компонентов</returns>
        /// <exception cref="InvalidOperationException">
        /// Возникает, если <see cref="_parentTechOperatinWork"/> равен null
        /// </exception>
        private List<Component> FilterComponents()
        {
            if (_parentTechOperatinWork == null)
            {
                _logger.Error("В процессе фильтрации компонентов _parentTechOperatinWork оказался пуст");
                throw new InvalidOperationException("Техоперация отказалась пустой");
            }

            _logger.Information("Началась фильтрация списка компонентов для добавления в ТО");

            var localComponents = _parentTechOperatinWork.ComponentWorks.Select(t => t.component).ToList();
            var allComponents = _context.Components.ToList();
            var remainingComponents = allComponents.Except(localComponents).ToList();

            var searchText = string.IsNullOrWhiteSpace(txtSearch.Text) || txtSearch.Text == "Поиск"
            ? null
            : txtSearch.Text;

            var categoryFilter = cmbxFilter.SelectedItem?.ToString();

            var filteredComponents = remainingComponents.Where(tool =>
            (searchText == null
                || (tool.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                || (tool.ClassifierCode?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) &&
            (categoryFilter == "Все" || tool.Categoty.ToString() == categoryFilter))).ToList();

            // Теперь редактируем порядок: инструменты из Tool_TCs должны быть первыми
            var componentTcComponents = _tcViewState.TechnologicalCard.Component_TCs
                .Select(t => t.Child)
                .Where(comp => filteredComponents.Contains(comp)) // только те, что прошли фильтрацию
                .ToList();

            var otherComponents = filteredComponents.Except(componentTcComponents).ToList();

            _logger.Information("Фильтрация завершена");

            return componentTcComponents.Concat(otherComponents).ToList();
        }
        #endregion

        #region DGV events

        /// <summary>
        /// Обработчик события изменения выбранного элемента в комбо-боксе фильтрации.
        /// Логирует текущее значение фильтра и обновляет данные в глобальной таблице компонентов.
        /// </summary>
        private void CmbxFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _logger.Information($"Текущее значение CmbxFilter: {cmbxFilter.SelectedItem?.ToString()}");
            RefreshComponentDataGlobal();
        }

        /// <summary>
        /// Обработчик события изменения текста в поле поиска.
        /// Обновляет данные в глобальной таблице компонентов при изменении текста поиска.
        /// </summary>
        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            RefreshComponentDataGlobal();
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
        /// Обрабатывает завершение редактирования ячеек количества и комментариев компонентов.
        /// </summary>
        private void DgvComponentsLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvComponentsLocal.Columns["countColumn"].Index)
            {
                var component = dgvComponentsLocal.Rows[e.RowIndex].Cells[0].Value as ComponentWork;
                var countValue = (object)dgvComponentsLocal.Rows[e.RowIndex].Cells[5].Value;

                if (component == null)
                {
                    _logger.Error("Компонент для редактирования не обнаружен");
                    MessageBox.Show("Компонент для редактирования не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                if (component.Quantity == result)
                    return;

                component.Quantity = result;

                _logger.Information($"Компонентe {component.component.Name} установлено количество {component.Quantity}");

                DataChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (e.ColumnIndex == dgvComponentsLocal.Columns["commentColumn"].Index)
            {
                var component = dgvComponentsLocal.Rows[e.RowIndex].Cells[0].Value as ComponentWork;
                var commentValue = (string)dgvComponentsLocal.Rows[e.RowIndex].Cells[6].Value;

                if (component == null)
                {
                    _logger.Error("Компонент для редактирования не обнаружен");
                    MessageBox.Show("Компонент для редактирования не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                component.Comments = commentValue;

                _logger.Information($"Компонентe {component.component.Name} установлен новый комментарий");

                DataChanged?.Invoke(this, EventArgs.Empty);
            }

            dgvComponentsLocal.AutoResizeRows();
        }

        /// <summary>
        /// Обрабатывает клик по кнопке удаления компонента из локальной таблицы.
        /// </summary>
        private void DgvComponentsLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            { return; }

            if (e.ColumnIndex == dgvComponentsLocal.Columns["deleteBtnColumn"].Index)
            {
                _logger.LogUserAction($"Щелчок по ячейке удаления компонента в строке {e.RowIndex}.");

                if (_parentTechOperatinWork == null)
                {
                    _logger.Warning("Не выбрана ТО. Удаление инструмента невозможно.");
                    return;
                }

                var component = dgvComponentsLocal.Rows[e.RowIndex].Cells[0].Value as ComponentWork;

                if (component == null)
                {
                    _logger.Error("Компонент для редактирования не обнаружен");
                    MessageBox.Show("Компонент для редактирования не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ComponentWorkDeleteRequested?.Invoke(_parentTechOperatinWork, component);
                _logger.Information($"Компонент {component.component.Name} помечен на удаление.");

                RefreshAllData();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Обрабатывает клик по кнопке добавления компонента из глобальной таблицы.
        /// </summary>
        private void DgvComponentsGlobal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            { return; }

            if (e.ColumnIndex == dgvComponentsGlobal.Columns["addBtnColumn"].Index)
            {
                _logger.LogUserAction($"Щелчок по ячейке добавить компонент в строке {e.RowIndex}.");

                if (_parentTechOperatinWork == null)
                {
                    _logger.Warning("Не выбрана ТО. Добавление компонента невозможно.");
                    return;
                }

                var component = dgvComponentsGlobal.Rows[e.RowIndex].Cells[0].Value as Component;

                if (component == null)
                {
                    _logger.Error("Компонент для добавления в ТК не обнаружен");
                    MessageBox.Show("Компонент для добавления в ТК не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var existedComponent = _parentTechOperatinWork.ComponentWorks.FirstOrDefault(t => t.componentId == component.Id && t.IsDeleted == true);
                var existInComponent = _tcViewState.TechnologicalCard.Component_TCs.FirstOrDefault(t => t.ChildId == component.Id);


                if (existedComponent != null)
                {
                    _logger.Information($"Компонент {component.Name} ранее использовался, снимается флаг удаления");
                    existedComponent.IsDeleted = false;
                    _parentTechOperatinWork.ComponentWorks.Remove(existedComponent);
                    _parentTechOperatinWork.ComponentWorks.Insert(_parentTechOperatinWork.ComponentWorks.Count, existedComponent);
                }
                else
                {
                    _logger.Information($"Компонент {component.Name} ранее не использовался, создается новый объект");

                    _parentTechOperatinWork.ComponentWorks.Add(
                        new ComponentWork
                        {
                            component = component,
                            componentId = component.Id,
                            Quantity = existInComponent?.Quantity ?? 1
                        });
                }

                if (existInComponent == null)
                {
                    _logger.Information($"Компонент {component.Name} ранее не использовался и не был указан в таблице компонентов ТК, создается новая запись");

                    _tcViewState.TechnologicalCard.Component_TCs.Add(
                        new Component_TC
                        {
                            Parent = _tcViewState.TechnologicalCard,
                            ParentId = _tcViewState.TechnologicalCard.Id,
                            ChildId = component.Id,
                            Child = component,
                            Order = _tcViewState.TechnologicalCard.Component_TCs.Count + 1
                        });
                }


                RefreshAllData();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
