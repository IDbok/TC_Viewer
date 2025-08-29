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
    public partial class StaffControl : UserControl
    {
        #region Fields

        private ILogger _logger = Log.Logger.ForContext<StaffControl>();

        private ExecutionWork? _parentExecutionWork = null;
        private readonly TcViewState _tcViewState;
        private readonly MyDbContext _context;

        #endregion

        #region Constructor

        /// <summary>
        /// Создаёт новый экземпляр <see cref="StaffControl"/>.
        /// </summary>
        /// <param name="myDbContext">Экземпляр <see cref="MyDbContext"/> для доступа к БД.</param>
        /// <param name="tcViewState">Объект, хранящий текущее состояние ТК и другие настройки.</param>
        public StaffControl(MyDbContext myDbContext, TcViewState tcViewState)
        {
            InitializeComponent();
            _context = myDbContext;
            _tcViewState = tcViewState;

            SubscribeToEvents();
            _logger.Information("Окно ToolControl инициализировано");
        }

        #endregion

        #region Initialization and configuration

        // <summary>
        /// Подписывается на события элементов управления для обработки взаимодействия с пользователем.
        /// Обрабатывает клики по ячейкам таблиц, редактирование и валидацию.
        /// </summary>
        private void SubscribeToEvents()
        {
            dgvStaffsGlobal.CellClick += DgvStaffsGlobal_CellClick;
            dgvStaffsLocal.CellContentClick += DgvStaffsLocal_CellContentClick;
            dgvStaffsLocal.CellClick += DgvStaffsLocal_CellClick;
            dgvStaffsLocal.CellBeginEdit += DgvStaffsLocal_CellBeginEdit;
            dgvStaffsLocal.CellEndEdit += DgvStaffsLocal_CellEndEdit;
            dgvStaffsLocal.CellValidating += CellValidating;
            txtSearch.TextChanged += TxtSearch_TextChanged;
        }

        /// <summary>
        /// Устанавливает для данного контрола родительский объект <see cref="ExecutionWork"/>
        /// чтобы впоследствии пользователь мог выбрать,
        /// какие <see cref="Staff_TC"/> добавлять/убирать.
        /// </summary>
        /// <param name="parentEw">Объект класса <see cref="ExecutionWork"/>,
        /// чей список компонентов будет редактироваться</param>
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
        /// Заполняет данными локальную таблицу <see cref="dgvStaffsLocal"/>
        /// </summary>
        public void RefreshStaffDataLocal()
        {
            _logger.Information("Начато заполнение таблицы dgvStaffsLocal");
            var offScroll = dgvStaffsLocal.FirstDisplayedScrollingRowIndex;
            dgvStaffsLocal.Rows.Clear();

            if (_parentExecutionWork == null)
            {
                _logger.Error("Технологический переход не обнаружен");
                MessageBox.Show("Технологический переход не обнаружен", "ТП не обнаружен!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var staffTcList = _tcViewState.TechnologicalCard.Staff_TCs.OrderBy(x => x.Symbol);

            foreach (Staff_TC staffTc in staffTcList)
            {
                List<object> listItem = new List<object>();
                listItem.Add(staffTc);
                listItem.Add("Удалить");

                var vs = _parentExecutionWork.Staffs.SingleOrDefault(s => s == staffTc);

                listItem.Add(vs != null ? true : false);

                if (staffTc.Child == null)
                {
                    _logger.Warning("Не удалось найти персонал с id {Id}", staffTc.IdAuto);
                    continue;
                }

                listItem.Add(staffTc.Symbol);
                listItem.Add(staffTc.Child.Name);
                listItem.Add(staffTc.Child.Type);
                listItem.Add(staffTc.Child.Functions);
                listItem.Add(staffTc.Child.CombineResponsibility ?? "");
                listItem.Add(staffTc.Child.Qualification);
                listItem.Add(staffTc.Child.Comment ?? "");
                dgvStaffsLocal.Rows.Add(listItem.ToArray());
            }

            _logger.Information($"Таблица dgvStaffsLocal заполнена, всего записей: {dgvStaffsLocal.RowCount}");

            dgvStaffsLocal.RestoreScrollPosition(offScroll, _logger);
            dgvStaffsLocal.AutoResizeRows();
        }

        /// <summary>
        /// Заполняет данными таблицу <see cref="dgvStaffsGlobal"/> со всеми инструментами
        /// </summary>
        public void RefreshStaffDataGlobal()
        {
            _logger.Information("Начато заполнение таблицы dgvStaffsGlobal");

            var offScroll = dgvStaffsGlobal.FirstDisplayedScrollingRowIndex;
            dgvStaffsGlobal.Rows.Clear();

            var filteredPersonal = FilterStaff();
            foreach (Staff staff in filteredPersonal)
            {
                List<object> staffRow = new List<object>
                {
                    staff,
                    "Добавить",
                    staff.Name,
                    staff.Type,
                    staff.Functions,
                    staff.CombineResponsibility ?? "",
                    staff.Qualification,
                    staff.Comment ?? ""
                };
                dgvStaffsGlobal.Rows.Add(staffRow.ToArray());
            }

            _logger.Information($"Таблица dgvStaffsGlobal заполнена, всего записей: {dgvStaffsGlobal.RowCount}");
            dgvStaffsGlobal.RestoreScrollPosition(offScroll, _logger);
        }

        /// <summary>
        /// Вызывает обновление всех <see cref="DataGridView"/> в форме
        /// </summary>
        public void RefreshAllData()
        {
            RefreshStaffDataLocal();
            RefreshStaffDataGlobal();
        }
        #endregion

        #region Events

        public event EventHandler? DataChanged;
        public event Action<ExecutionWork, List<Staff_TC>>? StaffUpdateRequested;

        #endregion

        #region Support methods

        /// <summary>
        /// Возвращает список всего персонала в базе, оставляя те,
        /// которые подходят под текст поля поиска
        /// </summary>
        /// <returns>Отфильтрованный список персонала</returns>
        /// <exception cref="InvalidOperationException">
        /// Возникает, если <see cref="_parentExecutionWork"/> равен null
        /// </exception>
        private IEnumerable<Staff> FilterStaff()
        {
            if (_parentExecutionWork == null)
            {
                _logger.Error("В процессе фильтрации компонентов _parentExecutionWork оказался пуст");
                throw new InvalidOperationException("Технологический переход оказался пуст");
            }

            var staffList = _context.Staffs.ToList();

            var searchText = string.IsNullOrWhiteSpace(txtSearch.Text) || txtSearch.Text == "Поиск"
            ? null
            : txtSearch.Text;

            return staffList.Where(stf => string.IsNullOrEmpty(searchText)
                                || stf.Name.ToLower().Contains(searchText.ToLower()));
        }

        /// <summary>
        /// Обновляет статус выбора персонала на основе состояния чекбоксов в локальной таблице.
        /// Добавляет или удаляет персонал из родительского технологического перехода.
        /// </summary>
        private void UpdateStaffCheckStatus()
        {
            if (_parentExecutionWork == null)
            {
                _logger.Error("Технологический переход не обнаружен");
                MessageBox.Show("Технологический переход не обнаружен", "ТП не обнаружен!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool updateTO = false;
            var staffLocalList = _tcViewState.TechnologicalCard.Staff_TCs.ToList();

            foreach (DataGridViewRow row in dgvStaffsLocal.Rows)
            {
                if (row?.Cells[0].Value is not Staff_TC staffInRow || row.Cells[2].Value is not bool isChecked)
                    continue;

                var staffInEw = _parentExecutionWork.Staffs.SingleOrDefault(s => s == staffInRow);
                if (isChecked && staffInEw == null)
                {
                    var addingStaff = staffLocalList.SingleOrDefault(s => s == staffInRow);
                    if (addingStaff == null)
                    {
                        _logger.Warning($"Не удалось найти персонал с id {staffInRow.IdAuto}");
                        continue;
                    }

                    _parentExecutionWork.Staffs.Add(addingStaff);
                    updateTO = true;
                    _logger.Information($"В технологический переход {_parentExecutionWork} добавлен персонал {addingStaff.Child?.Name}");

                }
                else if (!isChecked && staffInEw != null)
                {
                    _parentExecutionWork.Staffs.Remove(staffInEw);
                    updateTO = true;
                    _logger.Information($"Из технологического перехода {_parentExecutionWork} удален персонал {staffInEw.Child?.Name}");
                }
            }

            if (updateTO)
            {
                StaffUpdateRequested?.Invoke(_parentExecutionWork, _parentExecutionWork.Staffs);
            }
        }

        #endregion

        #region DGV events

        /// <summary>
        /// Обрабатывает завершение редактирования ячейки с обозначением персонала в локальной таблице.
        /// Обновляет символ персонала и инициирует обновление данных.
        /// </summary>
        private void DgvStaffsLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvStaffsLocal.Columns["symbolColumn"].Index)
            {
                var staffToUpdate = dgvStaffsLocal.Rows[e.RowIndex].Cells[0].Value as Staff_TC;

                if (staffToUpdate == null)
                {
                    _logger.Error("Персонал для редактирования не обнаружен");
                    MessageBox.Show("Персонал для редактирования не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var value = dgvStaffsLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                staffToUpdate.Symbol = value ?? " ";

                Task.Run(() =>
                {
                    this.BeginInvoke((Action)(() => RefreshStaffDataLocal()));
                });

                if (_parentExecutionWork != null)
                    StaffUpdateRequested?.Invoke(_parentExecutionWork, _parentExecutionWork.Staffs);

            }
        }

        /// <summary>
        /// Обрабатывает начало редактирования ячейки с чекбоксом в локальной таблице.
        /// Проверяет возможность изменения состояния чекбокса на основе существующих обозначений.
        /// </summary>
        private void DgvStaffsLocal_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex == dgvStaffsLocal.Columns["checkBoxColumn"].Index)  // Проверка, что это столбец с чекбоксами
            {
                if (_parentExecutionWork == null)
                {
                    _logger.Warning("Не выбрана ТП. Удаление инструмента невозможно.");
                    return;
                }

                var editedStaff = dgvStaffsLocal.Rows[e.RowIndex].Cells[0].Value as Staff_TC;

                if (editedStaff == null)
                {
                    _logger.Error("Персонал для редактирования не обнаружен");
                    MessageBox.Show("Персонал для редактирования не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var symbol = editedStaff.Symbol;
                var executionWorkStaffList = _parentExecutionWork.Staffs.Where(w => w.Symbol == symbol).ToList();

                // Проверяем, есть ли уже такой символ среди выбранных
                if (executionWorkStaffList.Count >= 1
                    && dgvStaffsLocal.Rows[e.RowIndex].Cells["checkBoxColumn"].Value is bool AreCheked
                    && AreCheked == false)
                {
                    MessageBox.Show("Роль с таким обозначением уже добавлена в переход");
                    e.Cancel = true;
                }
            }
        }

        // <summary>
        /// Обработчик события изменения текста в поле поиска.
        /// Обновляет данные в глобальной таблице компонентов при изменении текста поиска.
        /// </summary>
        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            RefreshStaffDataGlobal();
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
        /// Обрабатывает клик по ячейке удаления персонала в локальной таблице.
        /// Удаляет выбранный персонал из технологической карты и всех связанных переходов.
        /// </summary>
        private void DgvStaffsLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            { return; }

            if (e.ColumnIndex == dgvStaffsLocal.Columns["deleteBtnColumn"].Index)
            {
                _logger.LogUserAction($"Щелчок по ячейке удаления персонала в строке {e.RowIndex}.");

                if (_parentExecutionWork == null)
                {
                    _logger.Warning("Не выбрана ТП. Удаление инструмента невозможно.");
                    return;
                }

                var staffToDelete = dgvStaffsLocal.Rows[e.RowIndex].Cells[0].Value as Staff_TC;

                if (staffToDelete == null)
                {
                    _logger.Error("Персонал для удаления не обнаружен");
                    MessageBox.Show("Персонал для удаления не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var result = MessageBox.Show("Вы действительно хотите удалить данную роль из техкарты?", "Удаление персонала", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);

                if(result == DialogResult.Yes) 
                {
                    _logger.Information($"Удаление персонала: {staffToDelete.Child?.Name} (symbol: {staffToDelete.Symbol}, id: {staffToDelete.IdAuto})");
                    var staffTcList = _tcViewState.TechnologicalCard.Staff_TCs;
                    staffTcList.Remove(staffToDelete);
                    var allExecutionWorkList = _tcViewState.GetAllExecutionWorks();

                    foreach (var executionWork in allExecutionWorkList)
                    {
                        var matchingStaff = executionWork.Staffs.FirstOrDefault(s => s.IdAuto == staffToDelete.IdAuto && s.ChildId == staffToDelete.ChildId);
                        if (matchingStaff != null)
                            executionWork.Staffs.Remove(matchingStaff);
                    }

                    RefreshStaffDataLocal();
                    DataChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Обрабатывает клик по содержимому ячейки в локальной таблице.
        /// Фиксирует изменения и обновляет статус выбора персонала.
        /// </summary>
        private void DgvStaffsLocal_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dgvStaffsLocal.CommitEdit(DataGridViewDataErrorContexts.Commit);
            UpdateStaffCheckStatus();
        }

        /// <summary>
        /// Обрабатывает клик по кнопке добавления персонала из глобальной таблицы.
        /// Добавляет выбранный персонал в технологическую карту.
        /// </summary>
        private void DgvStaffsGlobal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            { return; }

            if (e.ColumnIndex == dgvStaffsGlobal.Columns["addBtnColumn"].Index)
            {
                _logger.LogUserAction($"Щелчок по ячейке добавить персонал в строке {e.RowIndex}.");

                var staff = dgvStaffsGlobal.Rows[e.RowIndex].Cells[0].Value as Staff;
                var currentStaffList = _tcViewState.TechnologicalCard.Staff_TCs;

                if (staff == null)
                {
                    _logger.Error("Персонал для добавления не обнаружен");
                    MessageBox.Show("Персонал для добавления не обнаружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Staff_TC staffTc = new Staff_TC
                {
                    Order = _tcViewState.TechnologicalCard.Staff_TCs.Count + 1,
                    Child = staff,
                    Symbol = " ",
                };

                currentStaffList.Add(staffTc);

                _logger.Information($"Добавление персонала: {staffTc.Child?.Name} (symbol: {staffTc.Symbol}, id: {staffTc.IdAuto})");

                Task.Run(() =>
                {
                    this.BeginInvoke((Action)(() => RefreshStaffDataLocal()));
                });
            }
        }

        #endregion
    }
}
