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
using TC_WinForms.WinForms.Work;
using TcDbConnector;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Win6.Work.EditorForms
{
    public partial class ProtectionControl : UserControl
    {
        #region Fields

        private ILogger _logger = Log.Logger.ForContext<ProtectionControl>();

        private ExecutionWork? _parentExecutionWork = null;
        private readonly TcViewState _tcViewState;
        private readonly MyDbContext _context;

        #endregion

        #region Constructor

        /// <summary>
        /// Создаёт новый экземпляр <see cref="ProtectionControl"/>.
        /// </summary>
        /// <param name="myDbContext">Экземпляр <see cref="MyDbContext"/> для доступа к БД.</param>
        /// <param name="tcViewState">Объект, хранящий текущее состояние ТК и другие настройки.</param>
        public ProtectionControl(MyDbContext myDbContext, TcViewState tcViewState)
        {
            InitializeComponent();
            _context = myDbContext;
            _tcViewState = tcViewState;

            SubscribeToEvents();
            _logger.Information("Окно ToolControl инициализировано");
        }

        #endregion

        #region Initialization and configuration

        /// <summary>
        /// Подписывается на события элементов управления для обработки взаимодействия с пользователем.
        /// Обрабатывает клики по ячейкам таблиц, редактирование и валидацию.
        /// </summary>
        private void SubscribeToEvents()
        {
            dgvProtectionsGlobal.CellClick += DgvProtectionsGlobal_CellClick;
            dgvProtectionsLocal.CellValidating += CellValidating;
            dgvProtectionsLocal.CellContentClick += DgvProtectionsLocal_CellContentClick;
            dgvProtectionsLocal.CellClick += DgvProtectionsLocal_CellClick;
            txtSearch.TextChanged += TxtSearch_TextChanged;
        }

        /// <summary>
        /// Устанавливает для данного контрола родительский объект <see cref="ExecutionWork"/>
        /// чтобы впоследствии пользователь мог выбрать,
        /// какие <see cref="Protection_TC"/> добавлять/убирать.
        /// </summary>
        /// <param name="parentEw">Объект класса <see cref="ExecutionWork"/>,
        /// чей список СИЗов будет редактироваться</param>
        public void SetParentExecutionWork(ExecutionWork parentEw)
        {
            _logger.Information($"Установка родительского ТП {parentEw?.Id}");

            if (parentEw == null)
            {
                _logger.Error("Технологический переход не обнаружен");
                MessageBox.Show("Технологический переход не обнаружен", "ТП не обнаружен!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _parentExecutionWork = parentEw;
            RefreshAllData();
        }

        /// <summary>
        /// Заполняет данными локальную таблицу <see cref="dgvProtectionsLocal"/>
        /// </summary>
        public void RefreshProtectionDataLocal()
        {
            _logger.Information("Начато заполнение таблицы dgvProtectionsLocal");
            var offScroll = dgvProtectionsLocal.FirstDisplayedScrollingRowIndex;
            dgvProtectionsLocal.Rows.Clear();

            if (_parentExecutionWork == null)
            {
                _logger.Error("Технологический переход не обнаружен");
                MessageBox.Show("Технологический переход не обнаружен", "ТП не обнаружен!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var protectionTcList = _tcViewState.TechnologicalCard.Protection_TCs.OrderBy(x => x.Order).ToList();

            foreach (Protection_TC protectionTc in protectionTcList)
            {
                List<object> listItem = new List<object>();
                listItem.Add(protectionTc);
                listItem.Add("Удалить");

                var vs = _parentExecutionWork.Protections.SingleOrDefault(s => s == protectionTc);

                listItem.Add(vs != null ? true : false);

                if (protectionTc.Child == null)
                {
                    _logger.Warning("Не удалось найти СИЗ с id {Id}", protectionTc.ChildId);
                    continue;
                }

                listItem.Add(protectionTc.Child.Name);
                listItem.Add(protectionTc.Child.Type ?? "");
                listItem.Add(protectionTc.Child.Unit);
                listItem.Add(protectionTc.Quantity);

                dgvProtectionsLocal.Rows.Add(listItem.ToArray());
            }

            _logger.Information($"Таблица dgvProtectionsLocal заполнена, всего записей: {dgvProtectionsLocal.RowCount}");

            dgvProtectionsLocal.RestoreScrollPosition(offScroll, _logger);
            dgvProtectionsLocal.AutoResizeRows();
        }

        /// <summary>
        /// Заполняет данными таблицу <see cref="dgvProtectionsGlobal"/> со всеми инструментами
        /// </summary>
        public void RefreshProtectionDataGlobal()
        {
            _logger.Information("Начато заполнение таблицы dgvProtectionsGlobal");

            var offScroll = dgvProtectionsGlobal.FirstDisplayedScrollingRowIndex;
            dgvProtectionsGlobal.Rows.Clear();

            var filteredProtection = FilterProtection();
            foreach (Protection prot in filteredProtection)
            {
                List<object> staffRow = new List<object>
                {
                    prot,
                    "Добавить",
                    prot.Name,
                    prot.Type ?? "",
                    prot.Unit,
                    ""
                };
                dgvProtectionsGlobal.Rows.Add(staffRow.ToArray());
            }

            _logger.Information($"Таблица dgvProtectionsGlobal заполнена, всего записей: {dgvProtectionsGlobal.RowCount}");
            dgvProtectionsGlobal.RestoreScrollPosition(offScroll, _logger);
        }

        /// <summary>
        /// Вызывает обновление всех <see cref="DataGridView"/> в форме
        /// </summary>
        public void RefreshAllData()
        {
            RefreshProtectionDataLocal();
            RefreshProtectionDataGlobal();
        }
        #endregion

        #region Events

        public event EventHandler? DataChanged;
        public event Action<ExecutionWork, List<Protection_TC>>? ProtectionUpdateRequested;

        #endregion

        #region Support methods

        /// <summary>
        /// Возвращает список всех СИЗов в базе, оставляя те,
        /// которые подходят под текст поля поиска
        /// </summary>
        /// <returns>Отфильтрованный список СИЗов</returns>
        /// <exception cref="InvalidOperationException">
        /// Возникает, если <see cref="_parentExecutionWork"/> равен null
        /// </exception>
        private IEnumerable<Protection> FilterProtection()
        {
            if (_parentExecutionWork == null)
            {
                _logger.Error("В процессе фильтрации компонентов _parentExecutionWork оказался пуст");
                throw new InvalidOperationException("Технологический переход оказался пуст");
            }

            var protectionsList = _context.Protections.ToList();
            var localStaffList = _tcViewState.TechnologicalCard.Protection_TCs.Select(t => t.Child).ToList();
            var remainingProtections = protectionsList.Except(localStaffList).ToList();

            var searchText = string.IsNullOrWhiteSpace(txtSearch.Text) || txtSearch.Text == "Поиск"
            ? null
            : txtSearch.Text;

            return remainingProtections.Where(prt => string.IsNullOrEmpty(searchText)
                                || prt.Name.ToLower().Contains(searchText.ToLower()));
        }

        /// <summary>
        /// Обновляет статус выбора СИЗа на основе состояния чекбоксов в локальной таблице.
        /// Добавляет или удаляет СИЗ из родительского технологического перехода.
        /// </summary>
        private void UpdateProtectionCheckStatus()
        {
            if (_parentExecutionWork == null)
            {
                _logger.Error("Технологический переход не обнаружен");
                MessageBox.Show("Технологический переход не обнаружен", "ТП не обнаружен!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool updateTO = false;
            var protectionLocalList = _tcViewState.TechnologicalCard.Protection_TCs.ToList();

            foreach (DataGridViewRow row in dgvProtectionsLocal.Rows)
            {
                if (row?.Cells[0].Value is not Protection_TC protectionInRow || row.Cells[2].Value is not bool isChecked)
                    continue;

                var protectionInEw = _parentExecutionWork.Protections.SingleOrDefault(s => s == protectionInRow);

                if (isChecked && protectionInEw == null)
                {
                    var addingProtection = protectionLocalList.SingleOrDefault(s => s == protectionInRow);
                    if (addingProtection == null)
                    {
                        _logger.Warning($"Не удалось найти СИЗ с id {protectionInRow.ChildId}");
                        continue;
                    }

                    _parentExecutionWork.Protections.Add(addingProtection);
                    updateTO = true;
                    _logger.Information($"В технологический переход {_parentExecutionWork} добавлен СИЗ {addingProtection.Child?.Name}");

                }
                else if (!isChecked && protectionInEw != null)
                {
                    _parentExecutionWork.Protections.Remove(protectionInEw);
                    updateTO = true;
                    _logger.Information($"Из технологического перехода {_parentExecutionWork} удален СИЗ {protectionInEw.Child?.Name}");
                }
            }

            if (updateTO)
            {
                ProtectionUpdateRequested?.Invoke(_parentExecutionWork, _parentExecutionWork.Protections);
            }
        }

        #endregion

        #region DGV events

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
        /// Обрабатывает клик по кнопке добавления СИЗ из глобальной таблицы.
        /// Добавляет выбранный СИЗ в технологическую карту.
        /// </summary>
        private void DgvProtectionsGlobal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            { return; }

            if (e.ColumnIndex == dgvProtectionsGlobal.Columns["addBtnColumn"].Index)
            {
                _logger.LogUserAction($"Щелчок по ячейке добавить СИЗ в строке {e.RowIndex}.");

                var protection = dgvProtectionsGlobal.Rows[e.RowIndex].Cells[0].Value as Protection;
                var currentProtectionList = _tcViewState.TechnologicalCard.Protection_TCs;

                if (protection == null)
                {
                    _logger.Error("СИЗ для добавления не обнаружен");
                    MessageBox.Show("СИЗ для добавления не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Protection_TC protectionTc = new Protection_TC
                {
                    Child = protection,
                    ParentId = _tcViewState.TechnologicalCard.Id,
                    Parent = _tcViewState.TechnologicalCard,
                    Quantity = 1,
                    Order = _tcViewState.TechnologicalCard.Protection_TCs.Count + 1
                };

                currentProtectionList.Add(protectionTc);

                _logger.Information("Добавление СЗ: {Protection_TC} (id: {Id})",
                    protectionTc.Child.Name, protectionTc.Child);

                RefreshAllData();
            }
        }

        /// <summary>
        /// Обрабатывает клик по содержимому ячейки в локальной таблице.
        /// Фиксирует изменения и обновляет статус выбора СИЗов.
        /// </summary>
        private void DgvProtectionsLocal_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dgvProtectionsLocal.CommitEdit(DataGridViewDataErrorContexts.Commit);
            UpdateProtectionCheckStatus();
        }

        /// <summary>
        /// Обработчик события изменения текста в поле поиска.
        /// Обновляет данные в глобальной таблице компонентов при изменении текста поиска.
        /// </summary>
        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            RefreshProtectionDataGlobal();
        }

        /// <summary>
        /// Обрабатывает клик по ячейке удаления СИЗ в локальной таблице.
        /// Удаляет выбранный СИЗ из технологической карты и всех связанных переходов.
        /// </summary>
        private void DgvProtectionsLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            { return; }

            if (e.ColumnIndex == dgvProtectionsLocal.Columns["deleteBtnColumn"].Index)
            {
                _logger.LogUserAction($"Щелчок по ячейке удаления СЗ в строке {e.RowIndex}.");

                if (_parentExecutionWork == null)
                {
                    _logger.Warning("Не выбрана ТП. Удаление инструмента невозможно.");
                    return;
                }

                var protectionToDelete = dgvProtectionsLocal.Rows[e.RowIndex].Cells[0].Value as Protection_TC;

                if (protectionToDelete == null)
                {
                    _logger.Error("СИЗ для удаления не обнаружен");
                    MessageBox.Show("СИЗ для удаления не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var result = MessageBox.Show("Вы действительно хотите полностью удалить данное СЗ из техкарты?", "Удаление СИЗ", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);

                if (result == DialogResult.Yes)
                {
                    _logger.Information($"Удаление СИЗ: {protectionToDelete.Child?.Name} (id: {protectionToDelete.Child?.Id})");
                    var protectionTcList = _tcViewState.TechnologicalCard.Protection_TCs;
                    protectionTcList.Remove(protectionToDelete);
                    var allExecutionWorkList = _tcViewState.GetAllExecutionWorks();

                    foreach (var executionWork in allExecutionWorkList)
                    {
                        var matchingProtection = executionWork.Protections.FirstOrDefault(p => p.ChildId == protectionToDelete.ChildId);
                        if (matchingProtection != null)
                            executionWork.Protections.Remove(matchingProtection);
                    }

                    RefreshProtectionDataLocal();
                    DataChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        #endregion
    }
}
