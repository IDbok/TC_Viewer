using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Serilog;
using System.Data;
using System.Linq;
using System.Windows.Input;
using TC_WinForms.DataProcessing;
using TC_WinForms.Extensions;
using TC_WinForms.Extensions;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Win6.Models;
using TC_WinForms.WinForms.Win6.Work;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static Antlr4.Runtime.Atn.SemanticContext;
using Component = TcModels.Models.TcContent.Component;
using Machine = TcModels.Models.TcContent.Machine;

namespace TC_WinForms.WinForms.Work
{
    public partial class AddEditTechOperationForm : Form, IOnActivationForm
    {
        private ILogger _logger;

        public TechOperationForm TechOperationForm { get; }
		private RepeatExecutionControl repeatAsInTcExecutionControl;
		private RepeatExecutionControl repeatExecutionControl;
		private ToolControl toolControl;
        private readonly TcViewState _tcViewState;
        private List<TechOperation> allTO;
        private List<TechTransition> allTP;
        private List<Staff> AllStaff;
        (int TOWID, int TOWOrder, int? EWID, int? EWOrder) highlightRowData;
        //private List<ExecutionWork> listExecutionWork; // не используется

        private TechOperationWork? SelectedTO => (TechOperationWork)comboBoxTO.SelectedItem; // todo: сделать nullable
		private ExecutionWork? SelectedTP => comboBoxTT.SelectedItem as ExecutionWork;// (ExecutionWork)comboBoxTT.SelectedItem;

		public AddEditTechOperationForm()
        {
            Log.Information("Инициализация AddEditTechOperationForm без параметров.");
            InitializeComponent();
        }

        public AddEditTechOperationForm(TechOperationForm techOperationForm, TcViewState tcViewState)
        {
            TechOperationForm = techOperationForm;
            _tcViewState = tcViewState;

            _logger = Log.Logger
                .ForContext<AddEditTechOperationForm>()
                .ForContext("TcId", _tcViewState.TechnologicalCard.Id);

            _logger.Information("Инициализация формы.");

            if (_tcViewState.IsViewMode) 
            {
				_logger.Warning("Закрытие формы. Попытка открыть формы в режиме редактирования.");
				this.Close();
			}

            InitializeComponent();

			//         // Создать экземпляр
			//         repeatExecutionControl = new RepeatExecutionControl(TechOperationForm.context, _tcViewState);//.GetAllExecutionWorks());
			//repeatExecutionControl.Dock = DockStyle.Fill;

			//// Подписаться на событие
			//repeatExecutionControl.DataChanged += (s, e) =>
			//{
			//	TechOperationForm.UpdateGrid();
			//};

			//// Добавить на вкладку
			//tabPageRepeat.Controls.Add(repeatExecutionControl);

			// Создать экземпляр
			repeatExecutionControl = new RepeatExecutionControl(TechOperationForm.context, _tcViewState);//.GetAllExecutionWorks());
			repeatExecutionControl.Dock = DockStyle.Fill;
            _ = repeatExecutionControl.SetSearchBoxParamsAsync();

			// Подписаться на событие
			repeatExecutionControl.DataChanged += (s, e) =>
            {
                TechOperationForm.UpdateGrid();
            };

            // Добавить на вкладку
            tabPageRepeat.Controls.Add(repeatExecutionControl);


            toolControl = new ToolControl(techOperationForm.context, _tcViewState);
            toolControl.Dock = DockStyle.Fill;

            toolControl.DataChanged += (s, e) =>
            {
                TechOperationForm.UpdateGrid();
            };

            toolControl.ToolWorkDeleteRequested += (work, toolWork) =>
            {
                TechOperationForm.MarkToDeleteToolWork(work, toolWork);
            };

            tabPageTool.Controls.Add(toolControl);

            this.Text = $"{TechOperationForm.TehCarta.Name} ({TechOperationForm.TehCarta.Article}) - Редактор хода работ";

            _logger.Information("Настройка событий DataGridView и ComboBox.");
            SetupEventHandlers();

            _logger.Information("Загрузка всех ТО и локальных ТО.");
            UpdateAllTO();
            UpdateLocalTO();
        }

        private void SetupEventHandlers()
        {
            // dataGridViewAllTO.CellContentClick += DataGridViewAllTO_CellContentClick;
            dataGridViewAllTO.CellClick += DataGridViewAllTO_CellClick;

            dataGridViewTO.CellClick += DataGridViewTO_CellClick;
            dataGridViewTO.CellValidating += CellValidating;
            dataGridViewTO.CellEndEdit += DataGridViewTO_CellEndEdit;
            dataGridViewTO.CellFormatting += DataGridViewTO_CellFormatting;
            dataGridViewTO.SelectionChanged += DataGridViewTO_SelectionChanged; // если комбобокс не отображать, то не нужное событие

            //dataGridViewTPAll.CellContentClick += DataGridViewTPAll_CellContentClick;
            dataGridViewTPAll.CellClick += DataGridViewTPAll_CellClick;
            dataGridViewTPAll.SelectionChanged += DataGridViewTPAll_SelectionChanged;

            dataGridViewTPLocal.CellClick += DataGridViewTPLocal_CellClick;
            dataGridViewTPLocal.CellEndEdit += DataGridViewTPLocal_CellEndEdit;
            dataGridViewTPLocal.CellFormatting += DataGridViewTPLocal_CellFormatting;
            dataGridViewTPLocal.SelectionChanged += DataGridViewTPLocal_SelectionChanged;
            dataGridViewTPLocal.CellValidating += CellValidating;
            dataGridViewTPLocal.RowHeightChanged += dataGridViewTPLocal_RowHeightChanged;


            dataGridViewStaff.CellContentClick += DataGridViewStaff_CellContentClick;
            dataGridViewStaff.CellClick += DataGridViewStaff_CellClick;
            dataGridViewStaff.CellEndEdit += DataGridViewStaff_CellEndEdit;
            dataGridViewStaff.CellBeginEdit += DataGridViewStaff_CellBeginEdit;
            dataGridViewStaff.CellValidating += CellValidating;

            dataGridViewStaffAll.CellClick += DataGridViewStaffAll_CellClick;

            dataGridViewComponentAll.CellClick += DataGridViewComponentAll_CellClick;
            dataGridViewComponentLocal.CellClick += DataGridViewComponentLocal_CellClick;
            dataGridViewComponentLocal.CellEndEdit += DataGridViewComponentLocal_CellEndEdit;
            dataGridViewComponentLocal.CellValidating += CellValidating;


            //dataGridViewInstumentAll.CellClick += DataGridViewInstumentAll_CellClick;
            //dataGridViewInstrumentLocal.CellClick += DataGridViewInstrumentLocal_CellClick;
            //dataGridViewInstrumentLocal.CellEndEdit += DataGridViewInstrumentLocal_CellEndEdit;
            //dataGridViewInstrumentLocal.CellValidating += CellValidating;

            dataGridViewAllSZ.CellClick += DataGridViewAllSZ_CellClick;
            dataGridViewLocalSZ.CellClick += DataGridViewLocalSZ_CellClick;

            dataGridViewLocalSZ.CellValidating += CellValidating;
            dataGridViewLocalSZ.CellContentClick += DataGridViewSZ_CellContentClick;

            dataGridViewEtap.CellEndEdit += DataGridViewEtap_CellEndEdit;
            dataGridViewEtap.CellContentClick += DataGridViewEtap_CellContentClick;
            dataGridViewEtap.CellValidating += CellValidating;
            dataGridViewEtap.CellClick += DataGridViewEtap_CellClick;

            dataGridViewMeha.CellContentClick += DataGridViewMeha_CellContentClick;
            dataGridViewMeha.CellValidating += CellValidating;

            //dataGridViewPovtor.CellContentClick += DataGridViewPovtor_CellContentClick;
            //dataGridViewPovtor.CellValueChanged += DataGridViewPovtor_CellValueChanged;
            //dataGridViewPovtor.CellFormatting += dataGridViewPovtor_CellFormatting;
            //dataGridViewPovtor.CellEndEdit += DataGridViewPovtor_CellEndEdit;
			//dataGridViewPovtor.CellValidating += CellValidating;

			//comboBoxFiltrCategor.SelectedIndexChanged += ComboBoxFiltrCategor_SelectedIndexChanged;
            comboBoxFilterComponent.SelectedIndexChanged += ComboBoxFilterComponent_SelectedIndexChanged;

            textBoxPoiskTo.TextChanged += TextBoxPoiskTo_TextChanged;
            textBoxPoiskComponent.TextChanged += TextBoxPoiskComponent_TextChanged;
            textBoxPoiskTP.TextChanged += TextBoxPoiskTP_TextChanged;
            textBoxPoiskSZ.TextChanged += TextBoxPoiskSZ_TextChanged;
            textBoxPoiskMach.TextChanged += TextBoxPoiskMach_TextChanged;
            PoiskPersonal.TextChanged += TextBoxPersonalPoisk_TextChanged;

            comboBoxTPCategoriya.SelectedIndexChanged += ComboBoxTPCategoriya_SelectedIndexChanged;
            comboBoxTO.SelectedIndexChanged += ComboBoxTO_SelectedIndexChanged;
            comboBoxTT.SelectedIndexChanged += ComboBoxTT_SelectedIndexChanged;

            //var Even = new DGVEvents();
            //Even.EventsObj = this;
            //Even.Table = 1;
            //Even.AddGragDropEvents(dataGridViewTO);

            //var Even = new DGVEvents();
            //Even.EventsObj = this;
            //Even.Table = 2;
            //Even.AddGragDropEvents(dataGridViewTPLocal);

            comboBoxTO.Format += (s, e) =>
            {
                if (e.ListItem is TechOperationWork tow)
                {
                    // Задайте формат отображения текста
                    e.Value = $"№{tow.Order} {tow.techOperation.Name}";
                }
            };

            comboBoxTT.Format += (s, e) =>
            {
                if (e.ListItem is ExecutionWork ew)
                {
                    // Задайте формат отображения текста
                    e.Value = $"№{ew.Order} {ew.techTransition?.Name ?? ""}";
                }
            };

            _logger.Information("Все события для элементов интерфейса настроены.");
        }

        

        private void ComboBoxTT_SelectedIndexChanged(object? sender, EventArgs e)
        {
            HighlightTOTTRow(true);

			if (tabControl1.SelectedTab == null) return;

			switch (tabControl1.SelectedTab.Name)
            {
                case "tabPageStaff":
                    UpdateGridStaff();
                    UpdateGridStaffAll();
                    break;
                case "tabPageProtection":
                    UpdateGridLocalSZ();
                    UpdateGridAllSZ();
                    break;
            }
        }

        private void ComboBoxTO_SelectedIndexChanged(object? sender, EventArgs e)
        {
            //HighlightTOTTRow();

            if (tabControl1.SelectedTab == null) return;

            switch (tabControl1.SelectedTab.Name)
            {
                //case "tabPageTO": // у нас просто нет комбабокса на этой вкладке
                //    // UpdateLocalTO(); // обновление произойдет в момент перехода на вкладку
                //    break;
                case "tabPageTP":
                    UpdateComboBoxTT();
                    UpdateLocalTP();
                    break;
                case "tabPageStaff":
                    UpdateComboBoxTT();
                    break;
                case "tabPageComponent":
                    UpdateComponentLocal();
                    UpdateComponentAll();//чтобы обновлялся список добавленных в ТО компонентов
                    break;
                case "tabPageTool":
                    toolControl.SetParentTechOpetarionWork(SelectedTO ?? TechOperationForm.TechOperationWorksList.First());
                    break;
                case "tabPageProtection":
                    UpdateComboBoxTT();
                    UpdateGridLocalSZ();
                    UpdateGridAllSZ();//чтобы обновлялся список добавленных в ТО СЗ
                    break;
            }


        }

		public void RefreshActiveTabData()
		{
			// Определяем текущую активную вкладку
			var activeTab = tabControl1.SelectedTab;
			if (activeTab == null)
				return;

			string tabName = activeTab.Name;
			_logger.Information($"Обновление данных для активной вкладки: {tabName}");

			// Выбираем действия в зависимости от имени активной вкладки
			switch (tabName)
			{
				case "tabPageTO":
					// Обновление данных для вкладки "Все ТО" (технологические операции)
					//UpdateAllTO();
					//UpdateLocalTO();
					//_logger.Information("Вкладка ТО обновлена (список всех и локальных ТО).");
					break;
				case "tabPageTP":
					// Обновление данных для вкладки "ТП" (технологические переходы)
					UpdateGridAllTP();
					UpdateLocalTP();
					_logger.Information("Вкладка ТП обновлена (список всех и локальных переходов).");
					break;
				case "tabPageStaff":
					// Обновление данных для вкладки "Персонал"
					UpdateGridStaff();
					UpdateGridStaffAll();
					_logger.Information("Вкладка персонала обновлена (локальный и общий списки).");
					break;
				case "tabPageComponent":
					// Обновление данных для вкладки "Комплектующие/Компоненты"
					UpdateComponentLocal();
					UpdateComponentAll();
					_logger.Information("Вкладка компонентов обновлена (локальный и общий списки).");
					break;
				case "tabPageTool":
					// Обновление данных для вкладки "Инструменты"
					_logger.Information("Вкладка инструментов обновлена (локальный и общий списки).");
					break;
				case "tabPageProtection":
					// Обновление данных для вкладки "Средства защиты"
					UpdateGridLocalSZ();
					UpdateGridAllSZ();
					_logger.Information("Вкладка средств защиты обновлена (локальный и общий списки).");
					break;
				case "tabPageStage":
					// Обновление данных для вкладки "Этапы"
					dataGridViewEtapUpdate();
					_logger.Information("Вкладка этапов обновлена.");
					break;
				case "tabPageRepeat":
					// Обновление данных для вкладки "Повторить" (если требуется логика обновления)
					//dataGridViewPovtor.Rows.Clear();
					//_logger.Information("Вкладка повторов обновлена.");
					break;
				default:
					// На случай непредвиденного имени вкладки
					_logger.Warning($"Неизвестная вкладка: {tabName}. Обновление пропущено.");
					break;
			}
		}


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

        #region TO


        private void TextBoxPoiskTo_TextChanged(object? sender, EventArgs e)
        {
            UpdateAllTO();
        }


        private void DataGridViewAllTO_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                _logger.LogUserAction($"Щелчок по ячейке добавления ТО в строке {e.RowIndex}.");

                var IddGuid = (TechOperation)dataGridViewAllTO.Rows[e.RowIndex].Cells[0].Value;

                _logger.Information("Добавление ТО: {TechOperation} (id: {Id})", IddGuid.Name, IddGuid.Id);

                AddTOWToGridLocalTO(IddGuid);
            }
        }

        private void AddTOWToGridLocalTO(TechOperation techOperationWork, bool Update = false)
        {
            if (Update)
            {
                var temp = TechOperationForm.context.TechOperations.Single(s => s.Id == techOperationWork.Id);
                techOperationWork = temp;
            }

            TechOperationForm.AddTechOperation(techOperationWork);
            UpdateLocalTO();

            // Обновляем номера ImageOwner
            UpdateImageOwnerNumbers(TechOperationForm.TehCarta);

            TechOperationForm.UpdateGrid();
        }

        private void DataGridViewTO_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                _logger.LogUserAction($"Щелчок по ячейке удаления ТО в строке {e.RowIndex}.");

                var result = MessageBox.Show("Вы уверены, что хотите удалить ТО?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var IddGuid = (TechOperationWork)dataGridViewTO.Rows[e.RowIndex].Cells[0].Value;

                    _logger.Information("Удаление ТО: {TechOperationWork} (id: {Id})", IddGuid.techOperation.Name, IddGuid.Id);
                    TechOperationForm.DeleteTechOperation(IddGuid);
                    UpdateLocalTO();

                    foreach (DataGridViewRow dataGridViewRow in dataGridViewTO.Rows)
                    {
                        var techOperationWorkRow = (TechOperationWork)dataGridViewRow.Cells[0].Value;
                        techOperationWorkRow.Order = dataGridViewRow.Index + 1; // Обновляем свойство Order
                    }

                    // Обновляем номера ImageOwner
                    UpdateImageOwnerNumbers(TechOperationForm.TehCarta);

                    TechOperationForm.UpdateGrid();
                }
            }
        }

        public void UpdateAllTO()
        {
            _logger.Information("Обновление списка всех технологических операций.");

            try
            {
                var offScroll = dataGridViewAllTO.FirstDisplayedScrollingRowIndex;

                dataGridViewAllTO.Rows.Clear();

                _logger.Information("Загрузка данных из контекста.");
                var context = TechOperationForm.context;
                allTO = context
                    .TechOperations
                    .Include(t => t.techTransitionTypicals.OrderBy(o => o.Order))
                    .ToList();

                _logger.Information("Фильтрация ТО по строке поиска: {SearchText}", textBoxPoiskTo.Text);
                var filteredOperations = FilterTechOperations(textBoxPoiskTo.Text);
                DisplayTechOperations(filteredOperations);

                RestoreScrollPosition(dataGridViewAllTO, offScroll);
                _logger.Information("Список всех ТО обновлен успешно.");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка при обновлении списка всех технологических операций.");
            }
        }
        private IEnumerable<TechOperation> FilterTechOperations(string searchText)
        {
            return allTO.Where(to => (string.IsNullOrEmpty(searchText) || to.Name.ToLower().Contains(searchText.ToLower())));
            /*&& (to.IsReleased == true || to.CreatedTCId == TechOperationForm.tcId || to.IsReleased == false))*/ //закомментирована фильтрация то, выводятся все ТО
        }
        private void RestoreScrollPosition(DataGridView dataGridView, int position)
        {
            try
            {
                if (position > 0 && position < dataGridView.Rows.Count)
                {
                    dataGridView.FirstDisplayedScrollingRowIndex = position;
                }
            }
            catch (Exception e)
            {
                // ignored
                // добавить логирование, посмотреть, в каких случаях возникает исключение.
                // если не возникает убрать try-catch
            }
        }
        private void DisplayTechOperations(IEnumerable<TechOperation> operations)
        {
            foreach (var operation in operations)
            {
                dataGridViewAllTO.Rows.Add(
                    operation,
                    "Добавить",
                    operation.Name,
                    operation.Category == "Типовая ТО",
                    string.Empty,
                    string.Empty
                );

                if (!operation.IsReleased)
                {
                    dataGridViewAllTO.Rows[^1].DefaultCellStyle.BackColor = Color.LightGray;
                }
            }
        }

        public void UpdateLocalTO()
        {
            _logger.Information("Обновление списка всех технологических переходов.");
            try
            {
                // Сохранение текущего выделения
                var selectedTO = SelectedTO;

                var offScroll = dataGridViewTO.FirstDisplayedScrollingRowIndex;
                dataGridViewTO.Rows.Clear();


                _logger.Information("Загрузка данных.");
                List<TechOperationWork> list = TechOperationForm.TechOperationWorksList
                    .Where(w => w.Delete == false)
                    .OrderBy(o => o.Order)
                    .ToList();

                foreach (TechOperationWork techOperationWork in list)
                {
                    AddTechOperationToGridLocalTO(techOperationWork);
                }

                RestoreScrollPosition(dataGridViewTO, offScroll);

                // Выделить строку соотвествующую активному ТО из комбобокс
                if (selectedTO != null)
                {
                    var selectedRowIndex = dataGridViewTO.Rows.Cast<DataGridViewRow>()
                        .Select(r => r.Cells[0].Value)
                        .ToList()
                        .IndexOf(selectedTO);

                    if (selectedRowIndex >= 0 && selectedRowIndex < dataGridViewTO.Rows.Count)
                    {
                        dataGridViewTO.Rows[selectedRowIndex].Selected = true;
                    }
                }

                _logger.Information("Список всех ТП обновлен успешно.");

                UpdateComboBoxTO();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка при обновлении списка всех технологических переходов.");
            }
        }
        private void AddTechOperationToGridLocalTO(TechOperationWork techOperationWork)
        {

            var row = new List<object>
            {
                techOperationWork,
                "Удалить",
                $"№{techOperationWork.Order} {techOperationWork.techOperation.Name}",
                techOperationWork.Order,
                techOperationWork.techOperation.Category == "Типовая ТО",
                techOperationWork.GetParallelIndex().ToString() ?? "",
                techOperationWork.GetSequenceGroupIndex().ToString() ?? ""
            };
            dataGridViewTO.Rows.Add(row.ToArray());
        }

        private void DataGridViewTO_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridViewTO.Columns["ParallelIndex"].Index)
            {
                var newValue = (string)dataGridViewTO.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var techOperationWork = (TechOperationWork)dataGridViewTO.Rows[e.RowIndex].Cells[0].Value;

                if (techOperationWork != null)
                {
                    if (newValue != techOperationWork.GetParallelIndex().ToString())
                    {
                        if (string.IsNullOrEmpty(newValue))
                        {
                            techOperationWork.SetParallelIndex(0);
                        }
                        else
                        {
                            if (int.TryParse(newValue, out var result))
                            {
                                techOperationWork.SetParallelIndex(result);
                            }
                        }

                        TechOperationForm.UpdateGrid();
                    }
                }
            }
            else if (e.ColumnIndex == dataGridViewTO.Columns["SequenceGroupIndex"].Index)
            {
                var newValue = (string)dataGridViewTO.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var techOperationWork = (TechOperationWork)dataGridViewTO.Rows[e.RowIndex].Cells[0].Value;

                if (techOperationWork != null)
                {
                    if (newValue != techOperationWork.GetSequenceGroupIndex().ToString())
                    {
                        if (newValue == "")
                        {
                            techOperationWork.SetSequenceGroupIndex(0);
                        }
                        else
                        {
                            if (int.TryParse(newValue, out var result))
                            {
                                techOperationWork.SetSequenceGroupIndex(result);
                            }
                        }

                        TechOperationForm.UpdateGrid();
                    }
                }
            }
            else if (e.ColumnIndex == dataGridViewTO.Columns["Order"].Index)
            {
                var techOperationWork = (TechOperationWork)dataGridViewTO.Rows[e.RowIndex].Cells[0].Value;

                int newOrder = 1;
                try
                {
                    newOrder = Convert.ToInt32(dataGridViewTO.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                }
                catch (FormatException ex)
                {
                    MessageBox.Show("Введите числовое значение!", "Ошибка формата!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dataGridViewTO.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = techOperationWork.Order;
                    return;
                }

                if (techOperationWork != null)
                {
                    newOrder = newOrder <= 0 ? 1 : newOrder;
                    newOrder = newOrder > dataGridViewTO.RowCount ? dataGridViewTO.RowCount : newOrder;

                    if (newOrder < techOperationWork.Order)
                    {
                        var povtorOrder = techOperationWork.executionWorks
                            .Where(rep => rep.techTransition.IsRepeatTransition()
                                && !rep.techTransition.IsRepeatAsInTcTransition()
                                && rep.ExecutionWorkRepeats.Any(pov => pov.ChildExecutionWork.techOperationWork.Order >= newOrder
                                    && pov.ChildExecutionWork.techOperationWorkId != techOperationWork.Id))
                            .ToList();

                        if (povtorOrder.Count > 0)
                        {
                            var result = MessageBox.Show("Вы пытаетесь переместить ТО с технологическим переходом \"Повтор\" выше последнего входящего в него элемента. Переместить?", "Внимание!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            switch (result)
                            {
                                case DialogResult.Yes:
                                    foreach (var pov in povtorOrder)
                                    {
                                        RemoveRepeatsWithBiggerOrder(pov, newOrder);
                                    }
                                    break;
                                case DialogResult.No:
                                    dataGridViewTO.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = techOperationWork.Order;
                                    return;
                            }
                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() =>
                    {
                        var currentOrder = techOperationWork.Order;
                        if (currentOrder == newOrder)
                            return;

                        DGVProcessing.ReorderRows(dataGridViewTO.Rows[e.RowIndex], newOrder, dataGridViewTO);

                        foreach (DataGridViewRow dataGridViewRow in dataGridViewTO.Rows)
                        {
                            var techOperationWorkRow = (TechOperationWork)dataGridViewRow.Cells[0].Value;
                            techOperationWorkRow.Order = dataGridViewRow.Index + 1; // Обновляем свойство Order
                        }

                        // Обновляем номера ImageOwner
                        UpdateImageOwnerNumbers(TechOperationForm.TehCarta);

                        TechOperationForm.UpdateGrid();
                    }));
                }
            }
        }

        private void DataGridViewTO_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dataGridViewTO.Columns["ParallelIndex"].Index
                || e.ColumnIndex == dataGridViewTO.Columns["SequenceGroupIndex"].Index
                || e.ColumnIndex == dataGridViewTO.Columns["Order"].Index)
            {
                TechOperationForm.CellChangeReadOnly(dataGridViewTO.Rows[e.RowIndex].Cells[e.ColumnIndex], false);
            }
            else
            {
                TechOperationForm.CellChangeReadOnly(dataGridViewTO.Rows[e.RowIndex].Cells[e.ColumnIndex], true);
            }
        }

        #endregion

        #region TP

        private void dataGridViewTPLocal_RowHeightChanged(object? sender, DataGridViewRowEventArgs e)
        {
            const int MaxRowHeight = 80; // Максимальная высота строки в пикселях

            if (e.Row.Height > MaxRowHeight)
            {
                e.Row.Height = MaxRowHeight; // Ограничиваем высоту строки
            }
        }
        private void DataGridViewTPLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            var work = SelectedTO;

			if (work == null)
				return;

			if (e.ColumnIndex == dataGridViewTPLocal.Columns["Comment"].Index)
            {
                var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var idd = (Guid)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;
                var wor = work.executionWorks.SingleOrDefault(s => s.IdGuid == idd);
                if (wor != null)
                {
                    wor.Comments = gg;
                    TechOperationForm.UpdateGrid(); // todo: заменить на обновление ячейки
                }
            }
            else if (e.ColumnIndex == dataGridViewTPLocal.Columns["Coefficient"].Index)
            {
                var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var idd = (Guid)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

                var wor = work.executionWorks.SingleOrDefault(s => s.IdGuid == idd);
                if (wor != null)
                {
                    var oldValue = wor.Value;

                    try
                    {
                        wor.Coefficient = gg ?? "";

                        var techTransition = wor.techTransition;

                        if (techTransition == null)
                            throw new Exception("Технологический переход не найден.");

                        var time = techTransition.TimeExecution.ToString().Replace(',', '.');
                        var coefDict = _tcViewState.TechnologicalCard.Coefficients.ToDictionary(c => c.Code, c => c.Value);
                        var coefficient = wor.Coefficient;

                        wor.Value = MathScript.EvaluateCoefficientExpression(coefficient, coefDict, time);
                        UpdateCoefficient(wor, oldValue);

                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ex.InnerException?.Message ?? ex.Message;
                        MessageBox.Show($"Ошибка при расчёте времени перехода:\n\n{errorMessage}", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        wor.Value = -1;
                    }
                    finally
                    {

                        // todo: реализовать обновление только ячейки времени выполнения, а не всей таблицы
                        BeginInvoke(new Action(() =>
                        {
                            UpdateLocalTP();
                        }));

                        TechOperationForm.UpdateGrid(); // todo: обновлять только при изменении значений
                    }
                }
            }
            else if (e.ColumnIndex == dataGridViewTPLocal.Columns["Order1"].Index)
            {
                var idd = (Guid)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;
                var wor = work.executionWorks.SingleOrDefault(s => s.IdGuid == idd);

                int newOrder = 1;
                try
                {
                    newOrder = Convert.ToInt32(dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                }
                catch (FormatException ex)
                {
                    MessageBox.Show("Введите числовое значение!", "Ошибка формата!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = wor.Order;
                    return;
                }

                if (wor != null && wor.Order != newOrder)
                {
                    newOrder = newOrder <= 0 ? 1 : newOrder;
                    newOrder = newOrder > dataGridViewTPLocal.RowCount ? dataGridViewTPLocal.RowCount : newOrder;
                    int lastRepeatOrder = 0;

                    if (wor.ExecutionWorkRepeats.Count != 0)
                    {
                        var repeatedEwList = wor.ExecutionWorkRepeats.Where(r => r.ChildExecutionWork.techOperationWorkId == wor.techOperationWorkId).ToList();
                        lastRepeatOrder = repeatedEwList.Count == 0 ? 0 : repeatedEwList.Max(r => r.ChildExecutionWork.Order);
                    }

                    if (wor.Repeat && wor.RepeatsTCId == null && newOrder <= lastRepeatOrder && wor.ExecutionWorkRepeats.Count != 0)
                    {
                        var result = MessageBox.Show("Вы пыатетесь переместить технологический переход \"Повтор\" выше последнего входящего в него элемента. Переместить?", "Внимание!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        switch(result)
                        {
                            case DialogResult.Yes:
                                RemoveRepeatsWithBiggerOrder(wor, newOrder);
                                break;
                            case DialogResult.No:
                                dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = wor.Order;
                                return;
                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() =>//используется для обхода рекурсивного вызова перемещения строк
                    {
                        DGVProcessing.ReorderRows(dataGridViewTPLocal.Rows[e.RowIndex], newOrder, dataGridViewTPLocal);


                        foreach (DataGridViewRow dataGridViewRow in dataGridViewTPLocal.Rows)
                        {
                            // кажется так лаконичнее, но перед релизом не решаюсь внедрять. Предварительно работает
                            var IddGuid = (Guid)dataGridViewRow.Cells[0].Value;

                            var ord = Convert.ToInt32(dataGridViewRow.Cells["Order1"].Value);

                            var bg = work.executionWorks.SingleOrDefault(s => s.IdGuid == IddGuid);
                            bg.Order = ord;
                        }

                        TechOperationForm.UpdateGrid();

                    }));

                }

            }
            dataGridViewTPLocal.AutoResizeRows();

        }

        private void DataGridViewTPLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                _logger.LogUserAction($"Щелчок по ячейке удаления ТП в строке {e.RowIndex}.");

                var work = SelectedTO;

                if (work == null) {
                    _logger.Warning("Не выбрана ТО. Удаление ТП невозможно.");
					return; }

				var IddGuid = (Guid)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

                // запрос на подтверждение удаления
                var result = MessageBox.Show("Вы уверены, что хотите удалить ТП?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    _logger.Information("Удаление ТП c GUID {TechTransitionGuid}", IddGuid);

                    TechOperationForm.DeleteTechTransit(IddGuid, work);
                    UpdateRelatedReplays(FindExecutionWorkById(IddGuid) ?? throw new Exception("ТП не найден по Guid в методе DataGridViewTPLocal_CellClick"));//Вызывается данная функция так как необходимо пересчитать связанные EW, но коэффициент этого EW не менялся
                    UpdateLocalTP();

                    foreach (DataGridViewRow dataGridViewRow in dataGridViewTPLocal.Rows)
                    {
                        IddGuid = (Guid)dataGridViewRow.Cells[0].Value;

                        var bg = work.executionWorks.SingleOrDefault(s => s.IdGuid == IddGuid);
                        bg.Order = dataGridViewRow.Index + 1;
                    }

                    TechOperationForm.UpdateGrid();
                    // todo: реализовать удаление ТП так, чтобы небыло необходимости в обновлении основной таблицы
                }
            }
        }

        private void DataGridViewTPLocal_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            // todo: прерделать на метод по отрисовке строк, а не ячеек

            if (e.ColumnIndex == dataGridViewTPLocal.Columns["Comment"].Index
                || e.ColumnIndex == dataGridViewTPLocal.Columns["Order1"].Index)// Индекс столбца с checkBox
            {
                TechOperationForm.CellChangeReadOnly(dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex], false);

            }
            else if (e.ColumnIndex == dataGridViewTPLocal.Columns["Coefficient"].Index)
            {
                var nameIndex = dataGridViewTPLocal.Columns["dataGridViewTextBoxColumn8"]?.Index ?? 0;
                string rowName = dataGridViewTPLocal.Rows[e.RowIndex].Cells[nameIndex].Value.ToString() ?? "";

				if (rowName == "Повторить" || rowName == "Выполнить в соответствии с ТК")
				{
                    TechOperationForm.CellChangeReadOnly(dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex], true);
                }
                else
                {
                    TechOperationForm.CellChangeReadOnly(dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex], false);
                }
            }
            else
            {
                TechOperationForm.CellChangeReadOnly(dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex], true);
            }
        }

        private void DataGridViewTPLocal_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridViewTPLocal.SelectedCells.Count == 1)
            {
                var rowIndex = dataGridViewTPLocal.SelectedCells[0].RowIndex;
                // Get the ExecutionWork object corresponding to the selected row
                var selectedRow = dataGridViewTPLocal.Rows[rowIndex];
                var id = (Guid)selectedRow.Cells[0].Value;
                var executionWork = FindExecutionWorkById(id);

                // Смена выбора в комбобоксе
                comboBoxTT.SelectedItem = executionWork;
            }
        }

        private void DataGridViewTO_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridViewTO.SelectedCells.Count == 1)
            {
                var rowIndex = dataGridViewTO.SelectedCells[0].RowIndex;
                // Get the ExecutionWork object corresponding to the selected row
                var selectedRow = dataGridViewTO.Rows[rowIndex];
                var selectedObject = (TechOperationWork)selectedRow.Cells[0].Value;

                if (selectedObject != null && selectedObject == SelectedTO)
                {
                    return;
                }

                // найти выбранное ТО из источника данных комбобокса
                var selectedObject2 = comboBoxTO.Items.Cast<TechOperationWork>()
                    .SingleOrDefault(s => s == selectedObject);

                if (selectedObject2 != null)
                {
                    // Смена выбора в комбобоксе
                    comboBoxTO.SelectedItem = selectedObject2;
                }

            }
        }

        // Method to find ExecutionWork by its ID
        private ExecutionWork? FindExecutionWorkById(Guid id)
        {
            foreach (var techOperationWork in TechOperationForm.TechOperationWorksList)
            {
                var executionWork = techOperationWork.executionWorks.SingleOrDefault(ew => ew.IdGuid == id);
                if (executionWork != null)
                {
                    return executionWork;
                }
            }
            return null;
        }
        private void DataGridViewTPAll_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                _logger.LogUserAction($"Щелчок по ячейке добавить ТП в строке {e.RowIndex}.");

                var work = SelectedTO;

                if (work == null) {
					_logger.Warning("Не выбрана ТО. Добавление ТП невозможно.");
					return;
				}

				var techTransition = (TechTransition)dataGridViewTPAll.Rows[e.RowIndex].Cells[0].Value;

                CoefficientForm? coefficient = null;

                if (!techTransition.IsRepeatTypeTransition())
                {
                    coefficient = new CoefficientForm(techTransition);
                    if (coefficient.ShowDialog() != DialogResult.OK)
                    {
                        // Если пользователь закрыл форму без сохранения, то обнуляем коэффициент
                        //coefficient = null;
						_logger.LogUserAction("Отмена добавления ТП.");
						return;
                    }
                }

                _logger.Information("Добавление ТП: {TechOperation} (id: {Id})",
                            techTransition.Name, techTransition.Id);

                //TechOperationForm.AddTechTransition(techTransition, work, null, coefficient);
                var coefficientValue = coefficient?.GetCoefficient;
                TechOperationForm.InsertNewExecutionWork(techTransition, work, coefficient: coefficientValue);
                UpdateLocalTP();
                // выбрать новый ТП в комбобоксе
                //HighlightTOTTRow(true);
                //TechOperationForm.UpdateGrid();// todo: избавиться от обновления путём добавления в в таблицу вручную

            }
        }

        private void AddNewTP(TechTransition TP, TechOperationWork TOW)
        {
            TechOperationForm.AddNewExecutionWork(TP, TOW);
            UpdateLocalTP();
            TechOperationForm.UpdateGrid();
        }

        private void DataGridViewTPAll_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridViewTPAll.SelectedRows.Count > 0)
            {
                var dc = (TechTransition)dataGridViewTPAll.SelectedRows[0].Cells[0].Value;

                labelComName.Text = dc.CommentName;
                labelComTime.Text = dc.CommentTimeExecution;
            }
        }

        private void TextBoxPoiskTP_TextChanged(object? sender, EventArgs e)
        {
            UpdateGridAllTP();
        }
        private IEnumerable<TechTransition> FilterTechTransitions(string searchText)
        {
            return allTP.Where(to =>
            (string.IsNullOrEmpty(searchText) || to.Name.ToLower().Contains(searchText.ToLower()))

                && ((comboBoxTPCategoriya.SelectedIndex == 0 || string.IsNullOrEmpty((string)comboBoxTPCategoriya.SelectedItem))
                    ||
                    to.Category == (string)comboBoxTPCategoriya.SelectedItem)
                && (!to.IsRepeatTransition())//.Name != "Повторить")//заглушка для копий Повторить в технологических переходах
                /*&& (to.IsReleased == true || to.CreatedTCId == TechOperationForm.tcId || to.IsReleased == false )*/  //закомментирована фильтрация тп, выводятся все ТП
                )
            ;
        }
        public void UpdateGridAllTP()
        {
            var offScroll = dataGridViewTPAll.FirstDisplayedScrollingRowIndex;
            dataGridViewTPAll.Rows.Clear();

            var work = SelectedTO;

            var context = TechOperationForm.context;

            allTP = context.TechTransitions.ToList(); // todo: добавить фильтрацию по выпуску и номеру карты
            //var list = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.ToList();

            if (comboBoxTPCategoriya.Items.Count == 0)
            {
                comboBoxTPCategoriya.Items.Add("Все");

                var allCategories = allTP.Select(tp => tp.Category).Distinct();
                foreach (var category in allCategories)
                {
                    if (string.IsNullOrEmpty(category))
                    {
                        continue;
                    }
                    comboBoxTPCategoriya.Items.Add(category);
                }
            }

            var filteredTransitions = FilterTechTransitions(textBoxPoiskTP.Text);

            // находим ТП "Повторить п." и добавляем его первым в список
            var repeatTechTransition = allTP.SingleOrDefault(tp => tp.IsRepeatTransition());//.Name == "Повторить п.");
            if (repeatTechTransition != null)
            {
                List<object> listItem = new()
                {
                    repeatTechTransition,
                    "Добавить",
                    repeatTechTransition.Name,
                    repeatTechTransition.TimeExecution
                };
                dataGridViewTPAll.Rows.Add(listItem.ToArray());
            }

            if (work?.techOperation.Category == "Типовая ТО")
            {
                return;
            }

            foreach (TechTransition techTransition in filteredTransitions)// allTP)
            {
                if (techTransition.IsRepeatTransition()) //.Name == "Повторить п.") // todo: переработать данную проверку на расширение TechTransitionExtension
				{
                    continue;
                }

                List<object> listItem = new()
                {
                    techTransition,
                    "Добавить",
                    techTransition.Name,
                    techTransition.TimeExecution
                };

                dataGridViewTPAll.Rows.Add(listItem.ToArray());
                if (!techTransition.IsReleased)
                    dataGridViewTPAll.Rows[dataGridViewTPAll.RowCount - 1].DefaultCellStyle.BackColor = Color.LightGray;
            }

            RestoreScrollPosition(dataGridViewTPAll, offScroll);
        }



        private void ComboBoxTPCategoriya_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateGridAllTP();
        }

        public void UpdateLocalTP()
        {
            if (dataGridViewTPLocal == null) return;

            var selectedTP = SelectedTP;
            var work = SelectedTO;

            if (work == null) {
                _logger.Warning("Не выбрана ТО. Обновление ТП невозможно.");
				return;
            }

            var offScroll = dataGridViewTPLocal.FirstDisplayedScrollingRowIndex;

            dataGridViewTPLocal.Rows.Clear();

            var LocalTPs = TechOperationForm.TechOperationWorksList.Single(s => s == work)
                .executionWorks.Where(w => w.Delete == false)
                .OrderBy(o => o.Order).ToList();

            foreach (ExecutionWork executionWork in LocalTPs)
            {
                List<object> listItem = new List<object>
                {
                    executionWork.IdGuid
                };

                // добавляем кнопку "Удалить" только для не типовых ТО
                if (work.techOperation.Category == "Типовая ТО" && executionWork.Repeat == false)
                {
                    listItem.Add("");
                }
                else
                {
                    listItem.Add("Удалить");
                }

                if (executionWork.Repeat)
                {
                    listItem.Add(executionWork.techTransition.IsRepeatAsInTcTransition() ? "Выполнить в соответствии с ТК" : "Повторить"); // todko: добавить сюда пункиты и артикул ТК
					listItem.Add(executionWork.Order);
                    listItem.Add("");

                    listItem.Add("");
                    //listItem.Add("");
                }
                else
                {
                    listItem.Add(executionWork.techTransition?.Name);
                    listItem.Add(executionWork.Order);
                    listItem.Add(executionWork.techTransition?.TimeExecution);

                    listItem.Add(executionWork.Coefficient);

                    //if(executionWork.Value==-1)
                    //{
                    //    listItem.Add("Ошибка");
                    //}
                    //else
                    //{
                    //    listItem.Add(executionWork.Value);
                    //}
                }

                //////////////////////////////////////////////26.06.2024
                //listItem.Add(executionWork.techTransition?.TimeExecution);

                //listItem.Add(executionWork.Coefficient);


                if (executionWork.Value == -1)
                {
                    listItem.Add("Ошибка");
                }
                else
                {
                    listItem.Add(executionWork.Value);
                }
                ////////////////////////////////////////////////////////

                listItem.Add(executionWork.Comments);


                dataGridViewTPLocal.Rows.Add(listItem.ToArray());
            }

            //listExecutionWork = new List<ExecutionWork>(LocalTPs);

            if (LocalTPs.Count == 0)
            {
                comboBoxTT.DataSource = null;
            }
            else
            {
                comboBoxTT.DataSource = new List<ExecutionWork>(LocalTPs); ;
            }

            RestoreScrollPosition(dataGridViewTPLocal, offScroll);
            dataGridViewTPLocal.AutoResizeRows();

            // Выделить строку соотвествующую активному ТП из комбобокс
            if (selectedTP != null)
            {
                // сбросить выделенные объекты
                dataGridViewTPLocal.ClearSelection();

                var selectedRowIndex = dataGridViewTPLocal.Rows.Cast<DataGridViewRow>()
                    .Select(r => r.Cells[0].Value)
                    .ToList()
                    .IndexOf(selectedTP.IdGuid);

                if (selectedRowIndex >= 0 && selectedRowIndex < dataGridViewTPLocal.Rows.Count)
                {
                    dataGridViewTPLocal.Rows[selectedRowIndex].Selected = true;
                }
            }
        }
        #endregion

        #region Staff
        private void DataGridViewStaff_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dataGridViewStaff.CommitEdit(DataGridViewDataErrorContexts.Commit);
            ClickDataGridViewStaff();
        }

        private void TextBoxPersonalPoisk_TextChanged(object? sender, EventArgs e)
        {
            UpdateStaffAll();
        }

        public void ClickDataGridViewStaff()
        {
			if (SelectedTP == null) return;

			bool updateTO = false;
            var AllStaff = TechOperationForm.TehCarta.Staff_TCs.ToList();
            var LocalEw = SelectedTP;

			foreach (DataGridViewRow row in dataGridViewStaff.Rows)
            {
                if (row?.Cells[0].Value is not Staff_TC staffInRow || row.Cells[2].Value is not bool isChecked)
                    continue;

                var staffInEw = LocalEw.Staffs.SingleOrDefault(s => s == staffInRow);
                if (isChecked && staffInEw == null)
                {
                    var addingStaff = AllStaff.SingleOrDefault(s => s == staffInRow);
                    if (addingStaff == null)
                    {
                        _logger.Warning("Не удалось найти персонал с id {Id}", staffInRow.IdAuto);
                        continue;
                    }

                    LocalEw.Staffs.Add(addingStaff);
                    updateTO = true;
                }
                else if (!isChecked && staffInEw != null)
                {
                    var sta = LocalEw.Staffs.Remove(staffInEw);
                    updateTO = true;
                }
            }

            if (updateTO)
            {
                //TechOperationForm.UpdateGrid();// todo: заменить на обновление ячейки
                TechOperationForm.UpdateStaffInRow(LocalEw, LocalEw.Staffs);
            }

        }
        private void DataGridViewStaff_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)  // Проверка, что это столбец с чекбоксами
            {
                var staff_TC = (Staff_TC)dataGridViewStaff.Rows[e.RowIndex].Cells[0].Value;
                var symbol = staff_TC.Symbol;
                var EW = SelectedTP;
                if (EW != null)
                {
                    var staffs = EW.Staffs.Where(w => w.Symbol == symbol).ToList();

                    // Проверяем, есть ли уже такой символ среди выбранных
                    if (staffs.Count >= 1 && (bool)dataGridViewStaff.Rows[e.RowIndex].Cells[2].Value == false)
                    {
                        MessageBox.Show("Роль с таким обозначением уже добавлена в переход");
                        e.Cancel = true;
                    }
                }
            }
        }


        private void DataGridViewStaff_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                var vv = TechOperationForm.TehCarta.Staff_TCs;
                var Idd = (Staff_TC)dataGridViewStaff.Rows[e.RowIndex].Cells[0].Value;
                var value = (string)dataGridViewStaff.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                Idd.Symbol = value;

                Task.Run(() =>
                {
                    this.BeginInvoke((Action)(() => UpdateGridStaff()));
                });

                TechOperationForm.UpdateGrid();// todo: заменить на обновление ячейки
                                               //TechOperationForm.UpdateStaffInRow

            }
        }

        private void DataGridViewStaffAll_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                _logger.LogUserAction($"Щелчок по ячейке добавить персонал в строке {e.RowIndex}.");

                var idd = (Staff)dataGridViewStaffAll.Rows[e.RowIndex].Cells[0].Value;
                var vv = TechOperationForm.TehCarta.Staff_TCs;

                if (idd != null)
                {
                    Staff_TC staffTc = new Staff_TC();
                    staffTc.Order = TechOperationForm.TehCarta.Staff_TCs.Count + 1;
                    staffTc.Child = idd;
                    staffTc.Symbol = " ";
                    vv.Add(staffTc);

                    _logger.Information("Добавление персонала: {Staff_TC} (symbol: {Synbol}, id: {Id})",
                        staffTc.Symbol, staffTc.Child?.Name, staffTc.IdAuto);

                    // TechOperationForm.TehCarta.Staff_TCs.Add(staffTc);
                    Task.Run(() =>
                    {
                        this.BeginInvoke((Action)(() => UpdateGridStaff()));
                    });
                }

            }
        }


        private void DataGridViewStaff_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                _logger.LogUserAction($"Щелчок по ячейке удаления персонала в строке {e.RowIndex}.");

                var idd = (Staff_TC)dataGridViewStaff.Rows[e.RowIndex].Cells[0].Value;

                if (idd != null)
                {
                    if (MessageBox.Show("Вы действительно хотите удалить данную роль из техкарты?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                    {
                        _logger.Information("Удаление персонала: {Staff_TC} (symbol: {Synbol}, id: {Id})",
                             idd.Symbol, idd.Child?.Name, idd.IdAuto);

                        var vv = TechOperationForm.TehCarta.Staff_TCs;
                        vv.Remove(idd);

                        foreach (var techOperation in TechOperationForm.TechOperationWorksList)
                        {
                            foreach (var executionWork in techOperation.executionWorks)
                            {
                                var staffToDelete = executionWork.Staffs.Where(s => s.IdAuto == idd.IdAuto && s.ChildId == idd.ChildId).FirstOrDefault();
                                if (staffToDelete != null)
                                    executionWork.Staffs.Remove(staffToDelete);
                            }
                        }

                        UpdateGridStaff();
                        TechOperationForm.UpdateGrid();
                    }
                }
            }
        }




        public void UpdateGridStaff()
        {
            var offScroll = dataGridViewStaff.FirstDisplayedScrollingRowIndex;
            var ExecutionWorkBox = SelectedTP;
            dataGridViewStaff.Rows.Clear();
            if (ExecutionWorkBox == null)
            {
                return;
            }

            var work = SelectedTO;
            var LocalTP = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.FirstOrDefault(s => s.IdGuid == ExecutionWorkBox.IdGuid);

            if (LocalTP == null)
                LocalTP = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.FirstOrDefault();

            var AllStaff = TechOperationForm.TehCarta.Staff_TCs.OrderBy(x => x.Symbol);


            foreach (Staff_TC staffTc in AllStaff)
            {
                List<object> listItem = new List<object>();
                listItem.Add(staffTc);
                listItem.Add("Удалить");

                var vs = LocalTP.Staffs.SingleOrDefault(s => s == staffTc);
                if (vs != null)
                {
                    listItem.Add(true);
                }
                else
                {
                    listItem.Add(false);
                }

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
                dataGridViewStaff.Rows.Add(listItem.ToArray());
            }

            RestoreScrollPosition(dataGridViewStaff, offScroll);
        }

        public void UpdateStaffAll()
        {
            var offScroll = dataGridViewStaffAll.FirstDisplayedScrollingRowIndex;

            var context = TechOperationForm.context;
            AllStaff = context.Staffs.ToList();

            dataGridViewStaffAll.Rows.Clear();

            var filteredPersonal = FilterStaff(PoiskPersonal.Text);
            foreach (Staff staff in filteredPersonal)
            {
                AddStuffToGridAllStaff(staff);
            }

            RestoreScrollPosition(dataGridViewStaffAll, offScroll);
        }

        public void UpdateGridStaffAll()
        {
            var ExecutionWorkBox = SelectedTP;

            var context = TechOperationForm.context;
            AllStaff = context.Staffs.ToList();

            dataGridViewStaffAll.Rows.Clear();
            if (ExecutionWorkBox == null)
            {
                return;
            }

            var filteredPersonal = FilterStaff(PoiskPersonal.Text);
            foreach (Staff staff in filteredPersonal)
            {
                AddStuffToGridAllStaff(staff);
            }
        }

        private void AddStuffToGridAllStaff(Staff staff)
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
            dataGridViewStaffAll.Rows.Add(staffRow.ToArray());
        }

        private IEnumerable<Staff> FilterStaff(string searchText)
        {
            return AllStaff.Where(stf => string.IsNullOrEmpty(searchText) || stf.Name.ToLower().Contains(searchText.ToLower()));
        }


        #endregion

        #region средства защиты

        private void DataGridViewSZ_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dataGridViewLocalSZ.CommitEdit(DataGridViewDataErrorContexts.Commit);
            ClickDataGridViewSZ();
        }
        private void ClickDataGridViewSZ()
        {
            bool updateTO = false;
            var allSZ = TechOperationForm.TehCarta.Protection_TCs.ToList();
            var localEw = SelectedTP;

            foreach (DataGridViewRow row in dataGridViewLocalSZ.Rows)
            {
                if (row?.Cells[0].Value is not Protection_TC protectionInRow || row.Cells[2].Value is not bool isChecked)
                    continue;

                var selectedProtection = localEw.Protections.SingleOrDefault(p => p == protectionInRow);

                if (isChecked && selectedProtection == null)
                {
                    var szFromAll = allSZ.SingleOrDefault(s => s == protectionInRow);

                    if (szFromAll == null)
                    {
                        _logger.Warning("Не удалось найти СЗ с ParentId:{ParentId}, ChildId:{ChildId}", protectionInRow.ParentId, protectionInRow.ChildId);
                        continue;
                    }

                    localEw.Protections.Add(szFromAll);
                    updateTO = true;
                }
                else if (!isChecked && selectedProtection != null)
                {
                    localEw.Protections.Remove(selectedProtection);
                    updateTO = true;
                }
            }

            if (updateTO)
            {
                //TechOperationForm.UpdateGrid();
                TechOperationForm.UpdateProtectionsInRow(localEw, localEw.Protections);
            }
        }
        private void TextBoxPoiskSZ_TextChanged(object? sender, EventArgs e)
        {
            UpdateGridAllSZ();
        }
        public void UpdateGridAllSZ()
        {
            var offScroll = dataGridViewAllSZ.FirstDisplayedScrollingRowIndex;
            dataGridViewAllSZ.Rows.Clear();

            var work = SelectedTP;

            if (work == null)
            {
                return;
            }

            // TechOperationForm.HighlightExecutionWorkRow(work, true);

            var context = TechOperationForm.context;

            var protection = context.Protections.ToList();
            var LocalTP = work.Protections.ToList();

            var Allsz = TechOperationForm.TehCarta.Protection_TCs.ToList();

            //todo: удалить за ненадобностью, изменен формат работы с СЗ
            //foreach (Protection_TC prot in Allsz)
            //{
            //    if (textBoxPoiskSZ.Text != "" &&
            //        prot.Child.Name.ToLower().IndexOf(textBoxPoiskSZ.Text.ToLower()) == -1)
            //    {
            //        continue;
            //    }


            //    if (LocalTP.SingleOrDefault(s => s.Child == prot.Child) != null)
            //    {
            //        continue;
            //    }


            //    List<object> listItem = new List<object>
            //    {
            //        prot.Child,
            //        "Добавить",
            //        prot.Child.Name,
            //        prot.Child.Type,
            //        prot.Child.Unit,
            //        prot.Quantity
            //    };

            //    dataGridViewAllSZ.Rows.Add(listItem.ToArray());
            //}


            foreach (Protection prot in protection)
            {
                if (textBoxPoiskSZ.Text != "" &&
                    prot.Name.ToLower().IndexOf(textBoxPoiskSZ.Text.ToLower()) == -1)
                {
                    continue;
                }

                if (LocalTP.SingleOrDefault(s => s.Child == prot) != null)
                {
                    continue;
                }

                if (Allsz.SingleOrDefault(s => s.Child == prot) != null)
                {
                    continue;
                }

                List<object> listItem = new List<object>
                {
                    prot,
                    "Добавить",
                    prot.Name,
                    prot.Type,
                    prot.Unit,
                    ""
                };
                dataGridViewAllSZ.Rows.Add(listItem.ToArray());
            }

            RestoreScrollPosition(dataGridViewAllSZ, offScroll);
        }

        public void UpdateGridLocalSZ()
        {
            var offScroll = dataGridViewLocalSZ.FirstDisplayedScrollingRowIndex;

            dataGridViewLocalSZ.Rows.Clear();

            var LocalTP = SelectedTP; // TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.Single(s => s.IdGuid == ExecutionWorkBox.IdGuid);

            if (LocalTP == null)
            {
                return;
            }

            var AllSZ = TechOperationForm.TehCarta.Protection_TCs.OrderBy(x => x.Order).ToList();

            foreach (Protection_TC protection_TC in AllSZ)
            {
                List<object> listItem = new List<object>();
                listItem.Add(protection_TC);
                listItem.Add("Удалить");

                var vs = LocalTP.Protections.SingleOrDefault(s => s == protection_TC);
                if (vs != null)
                    listItem.Add(true);
                else
                    listItem.Add(false);

                listItem.Add(protection_TC.Child.Name);
                listItem.Add(protection_TC.Child.Type);
                listItem.Add(protection_TC.Child.Unit);
                listItem.Add(protection_TC.Quantity);

                dataGridViewLocalSZ.Rows.Add(listItem.ToArray());
            }

            RestoreScrollPosition(dataGridViewLocalSZ, offScroll);
            dataGridViewLocalSZ.AutoResizeRows();
        }

        private void DataGridViewAllSZ_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                _logger.LogUserAction($"Щелчок по ячейке добавить СЗ в строке {e.RowIndex}.");

                var work = SelectedTP;
                var Idd = (Protection)dataGridViewAllSZ.Rows[e.RowIndex].Cells[0].Value;

                var tehCardProtections = TechOperationForm.TehCarta.Protection_TCs;
                var orderMax = 0;
                var tc = work.techOperationWork.technologicalCard;

                var list = tc.Protection_TCs.ToList();

                if (list.Count > 0)
                {
                    orderMax = list.Max(m => m.Order);
                }

                Protection_TC protectionTc = new Protection_TC();
                protectionTc.Child = Idd;
                protectionTc.ParentId = TechOperationForm._tcId;
                protectionTc.Parent = tc;
                protectionTc.Quantity = 1;
                protectionTc.Order = orderMax + 1;


                tehCardProtections.Add(protectionTc);

                _logger.Information("Добавление СЗ: {Protection_TC} (id: {Id})",
                    protectionTc.Child.Name, protectionTc.Child);

                UpdateGridAllSZ();
                UpdateGridLocalSZ();
            }
        }

        private void DataGridViewLocalSZ_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                _logger.LogUserAction($"Щелчок по ячейке удаления СЗ в строке {e.RowIndex}.");

                var idd = (Protection_TC)dataGridViewLocalSZ.Rows[e.RowIndex].Cells[0].Value;

                if (idd != null)
                {
                    var result = MessageBox.Show("Вы действительно хотите полностью удалить данное СЗ из техкарты?", "Удалить СЗ", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                    if (result == DialogResult.Yes)
                    {
                        var techCart = TechOperationForm.TehCarta.Protection_TCs;
                        techCart.Remove(idd);

                        _logger.Information("Удаление СЗ: {Protection_TC} (id: {Id})",
                            idd.Child.Name, idd.Child);


                        foreach (var techOperation in TechOperationForm.TechOperationWorksList)
                        {
                            foreach (var executionWork in techOperation.executionWorks)
                            {
                                var protectionToDelete = executionWork.Protections.Where(s => s.ChildId == idd.ChildId).FirstOrDefault();
                                if (protectionToDelete != null)
                                    executionWork.Protections.Remove(protectionToDelete);
                            }

                        }

                        UpdateGridLocalSZ();
                        UpdateGridAllSZ();
                        TechOperationForm.UpdateGrid();
                    }
                }
            }
        }

        #endregion

        #region Component


        private void TextBoxPoiskComponent_TextChanged(object? sender, EventArgs e)
        {
            UpdateComponentAll();
        }



        private void ComboBoxFilterComponent_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateComponentAll();
        }

        private List<string> AllFilterComponent;

        public void UpdateComponentAll()
        {
            var offScroll = dataGridViewComponentAll.FirstDisplayedScrollingRowIndex;
            dataGridViewComponentAll.Rows.Clear();

            var work = SelectedTO;
            if (work == null)
            {
                _logger.Warning("Не выбрана ТО. Обновление компонентов невозможно.");
				return;
            }

            var context = TechOperationForm.context;

            var AllMyComponent = TechOperationForm.TehCarta.Component_TCs.Where(w => w.ParentId == TechOperationForm._tcId).OrderBy(x => x.Order).ToList();

            var AllComponent = context.Components.ToList();

            var LocalComponent = work.ComponentWorks.Where(c => c.IsDeleted == false).ToList();


            bool UpdFilt = false;

            if (AllFilterComponent == null)
            {
                UpdFilt = true;
                AllFilterComponent = new List<string>();
                AllFilterComponent.Add("Все");
            }


            foreach (Component_TC componentTc in AllMyComponent)
            {
                if (textBoxPoiskComponent.Text != "" &&
                    componentTc.Child.Name.ToLower().IndexOf(textBoxPoiskComponent.Text.ToLower()) == -1)
                {
                    continue;
                }

                if (LocalComponent.SingleOrDefault(s => s.component == componentTc.Child) != null)
                {
                    continue;
                }

                if (comboBoxFilterComponent.SelectedIndex > 0)
                {
                    var selCateg = (string)comboBoxFilterComponent.SelectedItem;

                    if (selCateg != componentTc.Child.Categoty)
                    {
                        continue;
                    }

                }


                List<object> listItem = new List<object>
                {
                    componentTc.Child,
                    "Добавить",
                    componentTc.Child.Name,
                    componentTc.Child.Type,
                    componentTc.Child.Unit,
                    componentTc.Quantity
                };
                dataGridViewComponentAll.Rows.Add(listItem.ToArray());
            }

            foreach (Component component in AllComponent)
            {
                if (UpdFilt)
                {
                    if (!AllFilterComponent.Contains(component.Categoty))
                    {
                        AllFilterComponent.Add(component.Categoty);
                    }
                }


                if (textBoxPoiskComponent.Text != "" &&
                    component.Name.ToLower().IndexOf(textBoxPoiskComponent.Text.ToLower()) == -1)
                {
                    continue;
                }

                if (LocalComponent.SingleOrDefault(s => s.component == component) != null)
                {
                    continue;
                }

                if (AllMyComponent.SingleOrDefault(s => s.Child == component) != null)
                {
                    continue;
                }


                if (comboBoxFilterComponent.SelectedIndex > 0)
                {
                    var selCateg = (string)comboBoxFilterComponent.SelectedItem;

                    if (selCateg != component.Categoty)
                    {
                        continue;
                    }

                }


                List<object> listItem = new List<object>
                {
                    component,
                    "Добавить",
                    component.Name,
                    component.Type,
                    component.Unit,
                    ""
                };
                dataGridViewComponentAll.Rows.Add(listItem.ToArray());
            }


            if (UpdFilt)
            {
                comboBoxFilterComponent.DataSource = AllFilterComponent;
                comboBoxFilterComponent.SelectedIndex = 0;
            }

            RestoreScrollPosition(dataGridViewComponentAll, offScroll);
        }

        public void UpdateComponentLocal()
        {
            var offScroll = dataGridViewComponentLocal.FirstDisplayedScrollingRowIndex;
            dataGridViewComponentLocal.Rows.Clear();
            var work = SelectedTO;
            if (work == null)
            {
                _logger.Warning("Не выбрана ТО. Обновление компонентов невозможно.");
				return;
            }


            var LocalComponent = work.ComponentWorks.Where(c => c.IsDeleted == false).ToList();

            foreach (ComponentWork componentWork in LocalComponent)
            {
                List<object> listItem = new List<object>
                {
                    componentWork,
                    "Удалить",
                    componentWork.component.Name,
                    componentWork.component.Type ?? "",
                    componentWork.component.Unit,
                    componentWork.Quantity.ToString(),
                    componentWork.Comments ?? ""
				};
                dataGridViewComponentLocal.Rows.Add(listItem.ToArray());
            }

            RestoreScrollPosition(dataGridViewComponentLocal, offScroll);
            dataGridViewComponentLocal.AutoResizeRows();

        }

        private void DataGridViewComponentAll_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                _logger.LogUserAction($"Щелчок по ячейке добавить компонент в строке {e.RowIndex}.");

                var work = SelectedTO;

                if (work == null)
                {
					_logger.Warning("Не выбрана ТО. Добавление компонента невозможно.");
					return;
				}

                var Idd = (Component)dataGridViewComponentAll.Rows[e.RowIndex].Cells[0].Value;

                var existedIdd = work.ComponentWorks.Where(c => c.componentId == Idd.Id && c.IsDeleted == true).FirstOrDefault();
                var exustInComponent = TechOperationForm.TehCarta.Component_TCs.Where(t => t.ChildId == Idd.Id).FirstOrDefault();

                if (existedIdd != null)
                {
                    existedIdd.IsDeleted = false;
                    work.ComponentWorks.Remove(existedIdd);
                    work.ComponentWorks.Insert(work.ComponentWorks.Count, existedIdd);
                }
                else
                {
                    ComponentWork componentWork = new ComponentWork();
                    componentWork.component = Idd;
                    componentWork.componentId = Idd.Id;
                    componentWork.Quantity = exustInComponent != null ? exustInComponent.Quantity: 1;
                    work.ComponentWorks.Add(componentWork);
                }

                if (exustInComponent == null)
                {
                    Component_TC newComponent = new Component_TC();
                    newComponent.Parent = TechOperationForm.TehCarta;
                    newComponent.ParentId = TechOperationForm.TehCarta.Id;
                    newComponent.ChildId = Idd.Id;
                    newComponent.Child = Idd;
                    newComponent.Order = TechOperationForm.TehCarta.Tool_TCs.Count + 1;
                    TechOperationForm.TehCarta.Component_TCs.Add(newComponent);
                }

                UpdateComponentAll();
                UpdateComponentLocal();
                TechOperationForm.UpdateGrid();
            }
        }

        private void DataGridViewComponentLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            //HighlightTOTTRow();

            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                _logger.LogUserAction($"Щелчок по ячейке удаления компонента в строке {e.RowIndex}.");

                var work = SelectedTO;

                if (work == null)
                {
					_logger.Warning("Не выбрана ТО. Удаление компонента невозможно.");
					return;
				}

				var Idd = (ComponentWork)dataGridViewComponentLocal.Rows[e.RowIndex].Cells[0].Value;

                TechOperationForm.MarkToDeleteComponentWork(work, Idd);

                UpdateComponentAll();
                UpdateComponentLocal();
                TechOperationForm.UpdateGrid();
            }
        }

        private void DataGridViewComponentLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 5)
            {
                var Idd = (ComponentWork)dataGridViewComponentLocal.Rows[e.RowIndex].Cells[0].Value; // todo: переделать на более безапосный способ
				var value = (object)dataGridViewComponentLocal.Rows[e.RowIndex].Cells[5].Value;

                double don = 0;
                if (value is double doubleValue)
                {
                    don = doubleValue;
                }
                else if (!double.TryParse((string)value, out don))
                {
                    don = 0;
                }

                if (Idd.Quantity == don)
                    return;

                Idd.Quantity = don;
                TechOperationForm.UpdateGrid();
            }

            if (e.ColumnIndex == 6)
            {
                var Idd = (ComponentWork)dataGridViewComponentLocal.Rows[e.RowIndex].Cells[0].Value;
                var value = (string)dataGridViewComponentLocal.Rows[e.RowIndex].Cells[6].Value;

                Idd.Comments = value ?? "";
                TechOperationForm.UpdateGrid();
            }
            dataGridViewComponentLocal.AutoResizeRows();
        }

        #endregion

        #region Instument

        public void UpdateInstrumentLocal()
        {
            toolControl.RefreshToolDataLocal();
        }

        #endregion

        #region этапы
        private void TextBoxPoiskMach_TextChanged(object? sender, EventArgs e)
        {
            dataGridViewMehaUpdate();
        }

        public void dataGridViewMehaUpdate()
        {
            var offScroll = dataGridViewMeha.FirstDisplayedScrollingRowIndex;
            dataGridViewMeha.Rows.Clear();

            var Msch = TechOperationForm.TehCarta.Machine_TCs.ToList();
            var context = TechOperationForm.context;
            var all = context.Machines.ToList();

            foreach (var machine in Msch)
            {
                if (textBoxPoiskMach.Text != "" &&
                    machine.Child.Name.ToLower().IndexOf(textBoxPoiskMach.Text.ToLower()) == -1)
                {
                    continue;
                }

                List<object> listItem = new List<object>();
                listItem.Add(machine.Child);
                listItem.Add(true);
                listItem.Add(machine.Child.Name);
                listItem.Add(machine.Child.Type ?? "");
                listItem.Add(machine.Child.Unit);
                listItem.Add(machine.Quantity);
                dataGridViewMeha.Rows.Add(listItem.ToArray());
            }


            foreach (Machine machine in all)
            {
                if (textBoxPoiskMach.Text != "" &&
                    machine.Name.ToLower().IndexOf(textBoxPoiskMach.Text.ToLower()) == -1)
                {
                    continue;
                }

                var sin = Msch.SingleOrDefault(s => s.Child == machine);
                if (sin == null)
                {
                    List<object> listItem = new List<object>();
                    listItem.Add(machine);
                    listItem.Add(false);
                    listItem.Add(machine.Name);
                    listItem.Add(machine.Type ?? "");
                    listItem.Add(machine.Unit);
                    listItem.Add("");
                    dataGridViewMeha.Rows.Add(listItem.ToArray());
                }
            }

            RestoreScrollPosition(dataGridViewMeha, offScroll);
        }

        public void dataGridViewEtapUpdate()
        {
            var offScroll = dataGridViewEtap.FirstDisplayedScrollingRowIndex;
            dataGridViewEtap.Rows.Clear();

            var al = TechOperationForm.TechOperationWorksList.Where(w => w.Delete == false).OrderBy(o => o.Order);

            var allMsch = TechOperationForm.TehCarta.Machine_TCs.ToList();

            while (dataGridViewEtap.Columns.Count > 5)
            {
                dataGridViewEtap.Columns.RemoveAt(5);
            }

            foreach (var machine in allMsch)
            {
                var chach = new DataGridViewCheckBoxColumn();
                chach.HeaderText = machine.Child.Name;

                dataGridViewEtap.Columns.Add(chach);
            }


            foreach (TechOperationWork techOperationWork in al)
            {
                var lli = techOperationWork.executionWorks.Where(w => w.Delete == false).OrderBy(o => o.Order);
                foreach (ExecutionWork Wor in lli)
                {
                    List<object> listItem = new List<object>();
                    listItem.Add(Wor);

                    listItem.Add($"№{techOperationWork.Order} {techOperationWork.techOperation.Name}");
                    if (Wor.Repeat)
                    {
                        listItem.Add(Wor.techTransition.IsRepeatAsInTcTransition() ? $"№{Wor.RowOrder} Выполнить в соответствии с ТК" : $"№{Wor.RowOrder} Повторить");
                    }
                    else
                    {
                        listItem.Add($"№{Wor.RowOrder} {Wor.techTransition?.Name}" ?? "");
                    }

                    if (Wor.Etap == "")
                    {
                        listItem.Add("0");
                    }
                    else
                    {
                        listItem.Add(Wor.Etap);
                    }

                    if (Wor.Posled == "")
                    {
                        listItem.Add("0");
                    }
                    else
                    {
                        listItem.Add(Wor.Posled);
                    }

                    foreach (var machine in allMsch)
                    {
                        var fv = Wor.Machines.SingleOrDefault(w => w.Child == machine.Child);
                        if (fv == null)
                        {
                            listItem.Add(false);
                        }
                        else
                        {
                            listItem.Add(true);
                        }
                    }

                    dataGridViewEtap.Rows.Add(listItem.ToArray());
                }
            }

            RestoreScrollPosition(dataGridViewEtap, offScroll);
            dataGridViewEtap.AutoResizeRows();

        }

        private void DataGridViewEtap_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            HighlightTOTTRow(false, (ExecutionWork)dataGridViewEtap.Rows[e.RowIndex].Cells[0].Value);
        }

        private void DataGridViewEtap_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                ExecutionWork vb = (ExecutionWork)dataGridViewEtap.Rows[e.RowIndex].Cells[0].Value;
                string strEtap = (string)dataGridViewEtap.Rows[e.RowIndex].Cells[3].Value;

                vb.Etap = string.IsNullOrEmpty(strEtap) ? "0" : strEtap;
                dataGridViewEtap.Rows[e.RowIndex].Cells[3].Value = vb.Etap;

                TechOperationForm.UpdateGrid();
            }

            if (e.ColumnIndex == 4)
            {
                ExecutionWork vb = (ExecutionWork)dataGridViewEtap.Rows[e.RowIndex].Cells[0].Value;
                string strEtap = (string)dataGridViewEtap.Rows[e.RowIndex].Cells[4].Value;

                vb.Posled = string.IsNullOrEmpty(strEtap) ? "0" : strEtap;
                dataGridViewEtap.Rows[e.RowIndex].Cells[4].Value = vb.Posled;

                TechOperationForm.UpdateGrid();
            }

            dataGridViewEtap.AutoResizeRows();
        }

        private void DataGridViewEtap_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 4)
            {
                dataGridViewEtap.CommitEdit(DataGridViewDataErrorContexts.Commit);
                var che = (bool)dataGridViewEtap.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var id = (ExecutionWork)dataGridViewEtap.Rows[e.RowIndex].Cells[0].Value;

                var allMsch = _tcViewState.TechnologicalCard.Machine_TCs.ToList();
                var mach = allMsch[e.ColumnIndex - 5];

                if (mach.ExecutionWorks == null)
                    mach.ExecutionWorks = new List<ExecutionWork>();

                var bn = id.Machines.SingleOrDefault(s => s == mach);
                if (che)
                {
                    if (bn == null)
                    {
                        id.Machines.Add(mach);
                        mach.ExecutionWorks.Add(id);
                        TechOperationForm.UpdateGrid();
                    }
                }
                else
                {
                    if (bn != null)
                    {
                        id.Machines.Remove(bn);
                        mach.ExecutionWorks.Remove(id);
                        TechOperationForm.UpdateGrid();
                    }
                }

                HighlightTOTTRow(false, (ExecutionWork)dataGridViewEtap.Rows[e.RowIndex].Cells[0].Value);
            }
        }



        private void DataGridViewMeha_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {

            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                dataGridViewMeha.CommitEdit(DataGridViewDataErrorContexts.Commit);
                var id = (Machine)dataGridViewMeha.Rows[e.RowIndex].Cells[0].Value;
                var che = (bool)dataGridViewMeha.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                var allMsch = TechOperationForm.TehCarta.Machines.ToList();

                if (che)
                {
                    var bn = TechOperationForm.TehCarta.Machine_TCs.SingleOrDefault(s => s.Child == id);
                    if (bn == null)
                    {
                        bn = new Machine_TC();
                        bn.Child = id;
                        bn.Order = TechOperationForm.TehCarta.Machine_TCs.Count + 1;
                        bn.Quantity = 1;
                        bn.Parent = TechOperationForm.TehCarta;
                        TechOperationForm.TehCarta.Machine_TCs.Add(bn);
                        dataGridViewEtapUpdate();
                        dataGridViewMehaUpdate();
                        TechOperationForm.UpdateGrid();
                    }
                }
                else
                {
                    var bn = TechOperationForm.TehCarta.Machine_TCs.SingleOrDefault(s => s.Child == id);
                    if (bn != null)
                    {
                        TechOperationForm.TehCarta.Machine_TCs.Remove(bn);
                        dataGridViewEtapUpdate();
                        dataGridViewMehaUpdate();
                        TechOperationForm.UpdateGrid();
                    }
                }

            }
        }


        #endregion

        #region Повторить

        //private ExecutionWork? executionWorkPovtor;
        //private void DataGridViewPovtor_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        //{
        //    dataGridViewPovtor.CommitEdit(DataGridViewDataErrorContexts.Commit);

        //    if (e.ColumnIndex == 1 && e.RowIndex >= 0)
        //    {
        //        var currentEW = (ExecutionWork)dataGridViewPovtor.Rows[e.RowIndex].Cells[0].Value;
        //        var isSelected = (bool)dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

        //        if (executionWorkPovtor != null)
        //        {
        //            if (isSelected)
        //            {
        //                if (!executionWorkPovtor.ListexecutionWorkRepeat2.Contains(currentEW))
        //                {
        //                    executionWorkPovtor.ListexecutionWorkRepeat2.Add(currentEW);
        //                    TechOperationForm.UpdateGrid();
        //                }
        //            }
        //            else
        //            {
        //                if (executionWorkPovtor.ListexecutionWorkRepeat2.Contains(currentEW))
        //                {
        //                    executionWorkPovtor.ListexecutionWorkRepeat2.Remove(currentEW);
        //                    TechOperationForm.UpdateGrid();
        //                }
        //            }
        //        }
        //    }
        //}
        
		//private void DataGridViewPovtor_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
		//{
		//	// todo: реализовать пресчёт значения
		//	if (e.ColumnIndex == 5 && e.RowIndex >= 0)
		//	{
		//		var currentEW = (ExecutionWork)dataGridViewPovtor.Rows[e.RowIndex].Cells[0].Value;
		//		var isSelected = (bool)dataGridViewPovtor.Rows[e.RowIndex].Cells[1].Value;

		//		if (executionWorkPovtor != null)
		//		{
		//			var existingRepeat = executionWorkPovtor.ExecutionWorkRepeats
		//				.FirstOrDefault(x => x.ChildExecutionWork == currentEW);

		//			var cell = dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex];

		//			if (isSelected)
		//			{
		//				RecalculateExecutionWorkPovtorValue(executionWorkPovtor);
		//				TechOperationForm.UpdateGrid();
		//			}
		//			else
		//			{
		//				// отмена изменений
		//				cell.Value = null;
		//			}

		//			//UpdateLocalTP(); // todo: проверить, нужно ли обновлять локальные ТП
		//		}
		//	}
		//}
		//private void dataGridViewPovtor_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
		//{
		//    if (executionWorkPovtor == null) return;

		//    var executionWork = (ExecutionWork)dataGridViewPovtor.Rows[e.RowIndex].Cells[0].Value;
		//    // позиция для executionWorkPovtor в таблице dataGridViewPovtor
		//    var powtorIndex = dataGridViewPovtor.Rows.Cast<DataGridViewRow>().ToList().FindIndex(x => x.Cells[0].Value == executionWorkPovtor);
		//    bool isReadOnlyRow = powtorIndex < e.RowIndex;

		//    if (e.ColumnIndex == 1) // Индекс столбца с checkBox
		//    {
		//        if (executionWork == executionWorkPovtor)
		//        {
		//            SetReadOnlyAndColor(e.ColumnIndex, Color.DarkSeaGreen);
		//        }
		//        else if (isReadOnlyRow)
		//        {
		//            SetReadOnlyAndColor(e.ColumnIndex, Color.DarkSalmon);
		//        }
		//    }
		//    else if (e.ColumnIndex == 5 || e.ColumnIndex == 6 || e.ColumnIndex == 7) // Индекс столбца с checkBox
		//    {
		//        if (executionWork == executionWorkPovtor || isReadOnlyRow)
		//        {
		//            // Делаем ячейку недоступной
		//            dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
		//        }
		//        else // todo: снимать повтор, если объект перемещают "ниже" повтора
		//        {
		//            var existingRepeat = executionWorkPovtor.ExecutionWorkRepeats
		//                .SingleOrDefault(x => x.ChildExecutionWork == executionWork);

		//            dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = existingRepeat != null ? Color.LightGray : Color.White;
		//            dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = existingRepeat == null;
		//        }
		//    }
		//    void SetReadOnlyAndColor(int columnIndex, Color color, bool readOnly = true)
		//    {
		//        dataGridViewPovtor.Rows[e.RowIndex].Cells[columnIndex].ReadOnly = readOnly;
		//        dataGridViewPovtor.Rows[e.RowIndex].DefaultCellStyle.BackColor = color;
		//    }
		//}

		//private void DataGridViewPovtor_CellContentClick(object? sender, DataGridViewCellEventArgs e)
		//{
		//    dataGridViewPovtor.CommitEdit(DataGridViewDataErrorContexts.Commit);


		//    if (e.ColumnIndex == 1 && e.RowIndex >= 0)
		//    {
		//        var currentEW = (ExecutionWork)dataGridViewPovtor.Rows[e.RowIndex].Cells[0].Value;
		//        var isSelected = (bool)dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

		//        // позиция для executionWorkPovtor в таблице dataGridViewPovtor
		//        var powtorIndex = dataGridViewPovtor.Rows.Cast<DataGridViewRow>().ToList().FindIndex(x => x.Cells[0].Value == executionWorkPovtor);

		//        if (executionWorkPovtor != null && powtorIndex > e.RowIndex)//currentEW.Order < executionWorkPovtor.Order)
		//        {
		//            var existingRepeat = executionWorkPovtor.ExecutionWorkRepeats
		//                .SingleOrDefault(x => x.ChildExecutionWork == currentEW);

		//            if (isSelected)
		//            {
		//                if (existingRepeat == null)
		//                {
		//                    var newRepeat = new ExecutionWorkRepeat
		//                    {
		//                        ParentExecutionWork = executionWorkPovtor,
		//                        ParentExecutionWorkId = executionWorkPovtor.Id,
		//                        ChildExecutionWork = currentEW,
		//                        ChildExecutionWorkId = currentEW.Id,
		//                        NewCoefficient = "*1"
		//                    };

		//                    dataGridViewPovtor.Rows[e.RowIndex].Cells[5].Value = "*1";
		//                    executionWorkPovtor.ExecutionWorkRepeats.Add(newRepeat);
		//                    TechOperationForm.context.ExecutionWorkRepeats.Add(newRepeat);
		//                }
		//            }
		//            else
		//            {
		//                if (existingRepeat != null)
		//                {
		//                    dataGridViewPovtor.Rows[e.RowIndex].Cells[5].Value = "";
		//                    executionWorkPovtor.ExecutionWorkRepeats.Remove(existingRepeat);
		//                    TechOperationForm.context.ExecutionWorkRepeats.Remove(existingRepeat);
		//                }
		//            }


		//            UpdateCoefficient(executionWorkPovtor);

		//            // Перерисовать таблицу
		//            dataGridViewPovtor.Invalidate();
		//            //dataGridViewTPLocal.Invalidate();

		//            TechOperationForm.UpdateGrid(); // todo: реализовать метод по изменению значения только для одной строки в таблице TechOperationForm
		//        }
		//    }
		//}

		//private void DataGridViewPovtor_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
		//{
		//    if ((e.ColumnIndex == 5 || e.ColumnIndex == 6 || e.ColumnIndex == 7) && e.RowIndex >= 0)
		//    {
		//        var currentEW = (ExecutionWork)dataGridViewPovtor.Rows[e.RowIndex].Cells[0].Value;
		//        var isSelected = (bool)dataGridViewPovtor.Rows[e.RowIndex].Cells[1].Value;

		//        if (executionWorkPovtor != null)
		//        {
		//            var existingRepeat = executionWorkPovtor.ExecutionWorkRepeats
		//                .SingleOrDefault(x => x.ChildExecutionWork == currentEW);

		//            var cell = dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex];

		//            if (isSelected)
		//            {
		//                if (existingRepeat != null)
		//                {

		//                    var cellValueStr = (string)cell.Value;

		//                    if (e.ColumnIndex == 5)
		//                    {
		//                        if (string.IsNullOrEmpty(cellValueStr))
		//                        {
		//                            existingRepeat.NewCoefficient = string.Empty;
		//                        }
		//                        else
		//                        {
		//                            existingRepeat.NewCoefficient = cellValueStr;
		//                        }
		//                    }
		//                    else if (e.ColumnIndex == 6)
		//                    {
		//                        existingRepeat.NewEtap = cellValueStr;
		//                    }
		//                    else if (e.ColumnIndex == 7)
		//                    {
		//                        existingRepeat.NewPosled = cellValueStr;
		//                    }

		//                    //RecalculateExecutionWorkPovtorValue(executionWorkPovtor);
		//                    //TechOperationForm.UpdateGrid();
		//                }
		//            }
		//            else
		//            {
		//                // отмена изменений
		//                cell.Value = null;

		//            }

		//            UpdateCoefficient(executionWorkPovtor);
		//            TechOperationForm.UpdateGrid();
		//            //UpdateLocalTP();
		//        }
		//    }
		//}
		//public void UpdatePovtor()
  //      {
  //          dataGridViewPovtor.Rows.Clear();
  //          executionWorkPovtor = null;

  //          var select = dataGridViewTPLocal.SelectedRows;
  //          if (select.Count > 0)
  //          {
  //              var id = (Guid)select[0].Cells[0].Value;

  //              var al = TechOperationForm.TechOperationWorksList.Where(w => w.Delete == false).OrderBy(o => o.Order);

  //              ExecutionWork? exeWork = null;
  //              // Поиск ТП с заданным Guid
  //              foreach (TechOperationWork techOperationWork in al)
  //              {
  //                  if (exeWork != null)
  //                  {
  //                      break;
  //                  }
  //                  foreach (ExecutionWork executionWork in techOperationWork.executionWorks)
  //                  {
  //                      if (executionWork.IdGuid == id)
  //                      {
  //                          exeWork = executionWork;
  //                          break;
  //                      }
  //                  }
  //              }

  //              if (exeWork != null && exeWork.Repeat)
  //              {
  //                  executionWorkPovtor = exeWork;
  //                  //var selectedEW = exeWork.ListexecutionWorkRepeat2.ToList();
  //                  var selectedEW = exeWork.ExecutionWorkRepeats.Select(ewr => ewr.ChildExecutionWork).ToList();
  //                  var selectedEWR = exeWork.ExecutionWorkRepeats.ToList();
  //                  foreach (TechOperationWork techOperationWork in al)
  //                  {
  //                      var allEwInTo = techOperationWork.executionWorks.Where(w => w.Delete == false /*&& w.Repeat == false*/).OrderBy(o => o.Order); ////// 26/06/2024 - добавил повторы в выборку. Т.к. в картах такие объекты тоже входят в повторы

  //                      foreach (ExecutionWork executionWork in allEwInTo)
  //                      {
  //                          var isSelected = selectedEW.SingleOrDefault(s => s == executionWork) != null;

  //                          List<object> listItem = new List<object>
  //                          {
  //                              executionWork
  //                          };
  //                          if (isSelected)
  //                          {
  //                              listItem.Add(true);
  //                          }
  //                          else
  //                          {
  //                              listItem.Add(false);
  //                          }
  //                          listItem.Add($"№{techOperationWork.Order} {techOperationWork.techOperation.Name}");
  //                          listItem.Add(executionWork.techTransition?.Name ?? ""); // todo - проверить, может ли имя быть null у EW

  //                          listItem.Add(executionWork.Coefficient ?? "");

  //                          if (isSelected)
  //                          {
  //                              ExecutionWorkRepeat? techOperationWorkRepeat = selectedEWR.SingleOrDefault(s => s.ChildExecutionWork == executionWork);

  //                              if (techOperationWorkRepeat != null)
  //                              {
  //                                  listItem.Add(techOperationWorkRepeat.NewCoefficient);

  //                                  listItem.Add(techOperationWorkRepeat.NewEtap);
  //                                  listItem.Add(techOperationWorkRepeat.NewPosled);
  //                              }
  //                          }

  //                          dataGridViewPovtor.Rows.Add(listItem.ToArray());
  //                      }
  //                  }
  //              }

  //          }

  //      }

        private void RecalculateExecutionWorkPovtorValue(ExecutionWork executionWorkPovtor)
        {
            if (!executionWorkPovtor.Repeat) return;

            double totalValue = 0;
            try
            {
                var coefDict = _tcViewState.TechnologicalCard.Coefficients.ToDictionary(c => c.Code, c => c.Value);

                foreach (var repeat in executionWorkPovtor.ExecutionWorkRepeats)
                {
                    if (repeat.ChildExecutionWork.Delete)
                        continue;

                    var value = repeat.ChildExecutionWork.Value;

                    totalValue += MathScript.EvaluateCoefficientExpression(repeat.NewCoefficient, coefDict, value.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при пересчете значения повтора");
                string errorMessage = ex.InnerException?.Message ?? ex.Message;

                MessageBox.Show($"Ошибка при пересчете значения повтора:\n\n{errorMessage}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                totalValue = -1;
            }
            finally
            {
                executionWorkPovtor.Value = totalValue;
            }
        }

        private void UpdateRelatedReplays(ExecutionWork updatedExecutionWork)
        {
            var allExecutionWorks = _tcViewState.GetAllExecutionWorks();
            var allRepeats = allExecutionWorks.Where(ew => ew.Repeat && ew.ExecutionWorkRepeats.Any(e => e.ChildExecutionWorkId == updatedExecutionWork.Id));

            foreach (var executionWork in allRepeats)
            {
                UpdateCoefficient(executionWork);
            }
        }

        private void UpdateCoefficient(ExecutionWork editedExecutionWork, double? oldCoefficient = null)
        {
            if (editedExecutionWork == null)
                return;

            if (oldCoefficient != null && oldCoefficient != editedExecutionWork.Value)
            {
                UpdateRelatedReplays(editedExecutionWork);
                return;
            }

            oldCoefficient = editedExecutionWork.Value;
            if (editedExecutionWork.Repeat)
                RecalculateExecutionWorkPovtorValue(editedExecutionWork);// todo: проверить, нужно ли это здесь вообще!

            if (oldCoefficient != editedExecutionWork.Value)
                UpdateRelatedReplays(editedExecutionWork);

        }
        #endregion

        private void RemoveRepeatsWithBiggerOrder(ExecutionWork repeat, int newOrder)
        {
            for (int i = 0; i < repeat.ExecutionWorkRepeats.Count; i++)
            {
                var rep = repeat.ExecutionWorkRepeats[i];
                if (rep.ChildExecutionWork.Order >= newOrder)
                {
                    repeat.ExecutionWorkRepeats.Remove(rep);
                    i--;
                }
            }
        }

        private void HighlightTOTTRow(bool HighlightTT = false, ExecutionWork? exWork = null)
        {
            // todo: слишком часто вызывается местод (при переключениии минимум 2 раза)
            if (SelectedTO == null || SelectedTP == null
                || (highlightRowData == (SelectedTO.techOperationId, SelectedTO.Order, null, null) && !HighlightTT && exWork == null)
                || (exWork == null && highlightRowData == (SelectedTO.techOperationId, SelectedTO.Order, SelectedTP.techTransitionId, SelectedTP.Order))
                || (exWork != null && highlightRowData == (exWork.techOperationWork.techOperationId, exWork.techOperationWork.Order, exWork.techTransitionId, exWork.Order)))
            {
                return;
            }

            if (exWork != null)
            {
                TechOperationForm.SelectCurrentRow(exWork.techOperationWork, exWork);
                highlightRowData = (exWork.techOperationWork.techOperationId, exWork.techOperationWork.Order, exWork.techTransitionId, exWork.Order);
                return;
            }

            if (HighlightTT)
            {
                TechOperationForm.SelectCurrentRow(SelectedTO, SelectedTP);

                highlightRowData = (SelectedTO.techOperationId, SelectedTO.Order, SelectedTP.techTransitionId, SelectedTP.Order);
            }
            else
            {
                TechOperationForm.SelectCurrentRow(SelectedTO);
                highlightRowData = (SelectedTO.techOperationId, SelectedTO.Order, null, null);
            }
        }

        private void tabControl1_SelectedIndexChanged(object? sender, EventArgs? e)
        {
            _logger.Information("Переключение на вкладку: {TabName}", tabControl1.SelectedTab?.Name);

            switch (tabControl1.SelectedTab?.Name)
            {
                case "tabPageTO":
                    UpdateLocalTO();
                    //UpdateGridAllTP();
                    SetComboBoxesVisibility(false, false);
                    break;

                case "tabPageTP":
                    UpdateLocalTP();
                    UpdateGridAllTP();
                    SetComboBoxesVisibility(true, false);
                    break;

                case "tabPageStaff":
					//UpdateComboBoxTT(); // по моему обновление излишне, т.к. данный выбранного ТП не меняются
					UpdateGridStaff();
                    UpdateGridStaffAll();
                    SetComboBoxesVisibility(true, true);
                    break;

                case "tabPageComponent":
                    UpdateComponentLocal();
                    UpdateComponentAll();
                    SetComboBoxesVisibility(true, false);
                    break;

                case "tabPageTool":
                    if (SelectedTO == null)
                    {
                        _logger.Warning("Не выбрано TO. Обновление данных для вкладки невозможно.");
                        return;
                    }
                    toolControl.SetParentTechOpetarionWork(SelectedTO);
                    SetComboBoxesVisibility(true, false);
                    break;

                case "tabPageProtection":
                    //UpdateComboBoxTT();
                    UpdateGridLocalSZ();
                    UpdateGridAllSZ();
                    SetComboBoxesVisibility(true, true);
                    break;

                case "tabPageStage":
                    dataGridViewEtapUpdate();
                    dataGridViewMehaUpdate();
                    SetComboBoxesVisibility(false, false);
                    break;

                case "tabPageRepeat":
					if (SelectedTP == null)
					{
						_logger.Warning("Не выбрано ТП. Обновление данных для вкладки невозможно.");
						return;
					}
					_ = repeatExecutionControl.SetParentExecutionWorkAsync(SelectedTP);
					SetComboBoxesVisibility(false, false);
                    break;
				//case "tabPageRepeatsAsInTc": //tabControl1.SelectedTab == tabPageRepeatsAsInTc
				//	if (SelectedTP == null)
				//	{
				//		_logger.Warning("Не выбрано ТП. Обновление данных для вкладки невозможно.");
				//		return;
				//	}
				//	_ = repeatAsInTcExecutionControl.SetParentExecutionWorkAsync(SelectedTP);
				//	SetComboBoxesVisibility(false, false);
				//	break;

					// todo: добавить обработку по умолчанию, если требуется
			}

			_logger.Information("Обновление данных для вкладки завершено.");
        }
		private void SetComboBoxesVisibility(bool toVisible, bool ttVisible)
        {
            comboBoxTO.Visible = toVisible;
            comboBoxTT.Visible = ttVisible;
        }

        //public void UpdateTable(int table) // метод не используется
        //      {
        //          if (table == 1)
        //          {
        //              var list = dataGridViewTO.Rows;
        //              foreach (DataGridViewRow dataGridViewRow in list)

        //              {
        //                  // кажется так лаконичнее, но перед релизом не решаюсь внедрять. Предварительно работает
        //                  //var techOperationWork = (TechOperationWork)dataGridViewRow.Cells[0].Value; 
        //                  //techOperationWork.Order = dataGridViewRow.Index; // Обновляем свойство Order

        //                  var idd = (TechOperationWork)dataGridViewRow.Cells[0].Value;
        //                  var ord = (int)dataGridViewRow.Cells["Order"].Value;
        //                  TechOperationWork TechOperat = TechOperationForm.TechOperationWorksList.SingleOrDefault(s => s == idd);

        //                  if (TechOperat != null)
        //                  {
        //                      TechOperat.Order = ord;
        //                  }

        //              }

        //              // Обновляем comboBoxTO
        //              UpdateComboBoxTO();

        //              TechOperationForm.UpdateGrid();
        //          }

        //          if (table == 2)
        //          {
        //              var list = dataGridViewTPLocal.Rows;
        //              var work = SelectedTO;//(TechOperationWork)comboBoxTO.SelectedItem;

        //              foreach (DataGridViewRow dataGridViewRow in list)
        //              {
        //                  var IddGuid = (Guid)dataGridViewRow.Cells[0].Value;
        //                  var ord = (int)dataGridViewRow.Cells["Order1"].Value;

        //                  var bg = work.executionWorks.SingleOrDefault(s => s.IdGuid == IddGuid);
        //                  bg.Order = ord;

        //              }
        //              TechOperationForm.UpdateGrid();
        //          }

        //      }
        private void UpdateComboBoxTO()
        {
            UpdateComboBoxTODataSource();

            // Сохраняем текущий выбранный TechOperationWork из dataGridViewTO
            TechOperationWork? selectedTechOperationWork = null;
            if (dataGridViewTO.SelectedRows.Count > 0)
            {
                selectedTechOperationWork = (TechOperationWork)dataGridViewTO.SelectedRows[0].Cells[0].Value;
            }

            SelectComboBoxTOItem(selectedTechOperationWork);
            UpdateComboBoxTT();
        }

        private void UpdateComboBoxTODataSource()
        {
            // todo: не выполнять метод, если выбранное ТО соответствует отправленному в метод
            // Создаем новый список TechOperationWork в текущем порядке
            List<TechOperationWork> towInGrid = new List<TechOperationWork>();
            foreach (DataGridViewRow row in dataGridViewTO.Rows)
            {
                var techOperationWork = (TechOperationWork)row.Cells[0].Value;
                towInGrid.Add(techOperationWork);
            }

            // Временно отключаем обновление интерфейса ComboBox
            comboBoxTO.BeginUpdate();
            try
            {
                // Обновляем DataSource для comboBoxTO
                comboBoxTO.DataSource = null;
                comboBoxTO.DataSource = towInGrid;
            }
            finally
            {
                // Включаем обновление интерфейса ComboBox
                comboBoxTO.EndUpdate();
            }
        }

        private void SelectComboBoxTOItem(TechOperationWork? selectedTechOperationWork)
        {
            if (comboBoxTO.DataSource is List<TechOperationWork> currentComboBoxToList)
                // Устанавливаем выбранный элемент в comboBoxTO
                if (selectedTechOperationWork != null && currentComboBoxToList.Contains(selectedTechOperationWork))
                {
                    comboBoxTO.SelectedItem = selectedTechOperationWork;
                }
        }

        private void UpdateComboBoxTT(ExecutionWork? selectedItem = null)
        {
            UpdateComboBoxTPDataSourse();

			if (selectedItem == null)
			{
                selectedItem = SelectedTP;
			}

			if (selectedItem == null || SelectedTO != selectedItem.techOperationWork)
				return;

			SelectComboBoxItem(selectedItem, comboBoxTT);
		}

		private void UpdateComboBoxTPDataSourse()
		{
			var LocalTPs = TechOperationForm.TechOperationWorksList.Single(s => s == SelectedTO)
				.executionWorks.Where(w => w.Delete == false)
				.OrderBy(o => o.Order).ToList();

			comboBoxTT.BeginUpdate();
			try
			{
				var listExecutionWork = new List<ExecutionWork>(LocalTPs);
				comboBoxTT.DataSource = null;
				comboBoxTT.DataSource = listExecutionWork;
			}
			finally
			{
				// Включаем обновление интерфейса ComboBox
				comboBoxTT.EndUpdate();
			}
		}

        private void SelectComboBoxItem<T>(T selectedItem, ComboBox comboBox)
		{
			if (comboBox.DataSource is List<T> currentComboBoxList)
			{
				if (selectedItem != null)
				{
					comboBoxTT.SelectedItem = selectedItem;
				}
			}
			else
			{
                _logger.Warning($"Не удалось установить выбранный элемент в ComboBox {comboBox.Name}. DataSource не является списком {nameof(T)}.");
			}
		}

		private void btnCreateNewTP_Click(object sender, EventArgs e)
		{
			_logger.LogUserAction("Создание нового ТП");

			if (SelectedTO == null)
			{
				MessageBox.Show("Выберите ТО для добавления нового ТП.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 1. Открытие формы по добавлению нового ТП с передачей номера ТК
			var AddingForm = new Win7_TechTransitionEditor(new TechTransition() { CreatedTCId = TechOperationForm._tcId }, isNewObject: true);
            AddingForm.AfterSave = async (createdObj) => //todo: Проверить, есть ли более надежный способ
            {
                AddNewTP(createdObj, SelectedTO);
                TechOperationForm.context.TechTransitions.Add(createdObj);
                TechOperationForm.context.Attach(createdObj);
            };//AddNewObjectInDataGridView(createdObj);
            
            AddingForm.ShowDialog();
		}

        private void btnAddNewTO_Click(object sender, EventArgs e)
        {
			_logger.LogUserAction("Создание новой ТО");
			var AddingForm = new Win7_TechOperation_Window(isTcEditingForm: true, createdTcId: TechOperationForm._tcId);
            AddingForm.AfterSave = async (createdObj) => AddTOWToGridLocalTO(createdObj, true);
            AddingForm.ShowDialog();

            UpdateAllTO();
        }

        private void btnToChange_Click(object sender, EventArgs e)
        {
			_logger.LogUserAction("Замена ТО");

            if (dataGridViewTO.SelectedCells.Count == 1 && dataGridViewAllTO.SelectedRows.Count == 1)
            {
                var rowIndex = dataGridViewTO.SelectedCells[0].RowIndex;
                // Получаем выделенную строку из dataGridViewTO
                var selectedRowTO = dataGridViewTO.Rows[rowIndex];
                var techOperationWork = (TechOperationWork)selectedRowTO.Cells[0].Value;

                // Получаем выделенную строку из dataGridViewAllTO
                var selectedRowAllTO = dataGridViewAllTO.SelectedRows[0];
                var newTechOperation = (TechOperation)selectedRowAllTO.Cells[0].Value;

                if (techOperationWork.techOperationId == newTechOperation.Id)
                {
                    MessageBox.Show("Вы выбрали одинаковые операции.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show($"Вы уверены, что хотите заменить {techOperationWork.techOperation.Name} на {newTechOperation.Name}?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }

                if (newTechOperation.Category == "Типовая ТО")
                {
                    MessageBox.Show("Замена на типовую ТО недопустима.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Обновляем TechOperation в выделенной строке dataGridViewTO
                techOperationWork.techOperation = newTechOperation;
                techOperationWork.techOperationId = newTechOperation.Id;

                // Обновляем отображение в dataGridViewTO
                UpdateLocalTO();
                TechOperationForm.UpdateGrid();
            }
            else
            {
                MessageBox.Show("Выберите по одной строке в обеих таблицах для замены.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnReplaceTP_Click(object sender, EventArgs e)
        {
            _logger.LogUserAction("Замена ТП");

            var work = SelectedTO;

            if (work == null)
                return;

            if (dataGridViewTPLocal.SelectedCells.Count == 1 && dataGridViewTPAll.SelectedRows.Count == 1)
            {
                var rowIndex = dataGridViewTPLocal.SelectedCells[0].RowIndex;
                //get selected row
                var selectedRowTP = dataGridViewTPLocal.Rows[rowIndex];
                var idd = (Guid)selectedRowTP.Cells[0].Value;
                var executionWork = work.executionWorks.Where(e => e.IdGuid == idd).FirstOrDefault();

                //get replacement tp row
                var selectedRowTpAll = dataGridViewTPAll.SelectedRows[0];
                var newTechTranition = (TechTransition)selectedRowTpAll.Cells[0].Value;

                if (executionWork.techTransitionId == newTechTranition.Id)
                {
                    MessageBox.Show("Вы выбрали одинаковые операции.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if(work.techOperation.Category == "Типовая ТО")
                {
                    MessageBox.Show("Вы не можете заменять ТП в типовых Тех. операциях", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }    

                var result = MessageBox.Show($"Вы уверены, что хотите заменить {executionWork.techTransition.Name} на {newTechTranition.Name}?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }

                executionWork.techTransition = newTechTranition;
                executionWork.techTransitionId = newTechTranition.Id;

                UpdateLocalTP();
                TechOperationForm.UpdateGrid();
            }
            else
            {
                MessageBox.Show("Выберите по одной строке в обеих таблицах для замены.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddEditTechOperationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

		public void OnActivate()
		{
			// пересчитать значения для ТП и Повторов
			RecalculateAllTPValues();
            RecalculateAllRepeatsValues();

			TechOperationForm.UpdateGrid();
		}

        private void UpdateImageOwnerNumbers(TechnologicalCard technologicalCard)
        {
            _logger.Information("Обновление номеров ImageOwner для технологической карты ID: {Id}", technologicalCard.Id);

            try
            {
                // Получаем все TechOperationWork для данной TechnologicalCard, отсортированные по Order
                var techOperations = TechOperationForm.TechOperationWorksList
                    .Where(tow => !tow.Delete)
                    .OrderBy(tow => tow.Order)
                    .ToList();

                int imageNum = 1;
                foreach(var tow in techOperations)
                {
                    var images = tow.executionWorks.SelectMany(ew => ew.ImageList)
                        .Where(io => io.Role == ImageRole.Image) // Только рисунки
                        .Distinct()
                        .OrderBy(i => i.Number)
                        .ToList();

                    images.ForEach(image =>
                    {
                        image.Number = imageNum;
                        imageNum++;
                    });
                }

                _logger.Information("Номера ImageOwner успешно обновлены.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при обновлении номеров ImageOwner.");
            }
        }

        private void RecalculateAllTPValues()
		{
			try
			{

				foreach (var techOperationWork in TechOperationForm.TechOperationWorksList)
			    {
				    foreach (var executionWork in techOperationWork.executionWorks)
				    {
                        try
						{
							var techTransition = executionWork.techTransition;

							if (techTransition == null)
								throw new Exception("Технологический переход не найден.");
                            var time = techTransition.TimeExecution.ToString().Replace(",", ".");
							var coefDict = _tcViewState.TechnologicalCard.Coefficients.ToDictionary(c => c.Code, c => c.Value);
							var coefficient = executionWork.Coefficient;

                            if(string.IsNullOrEmpty(coefficient))
							{
								executionWork.Value = double.TryParse(time, out var value) ? value : 0;
								continue;
							}
							executionWork.Value = MathScript.EvaluateCoefficientExpression(coefficient, coefDict, time);
						}
						catch (Exception ex)
						{
							string errorMessage = ex.InnerException?.Message ?? ex.Message;
							executionWork.Value = -1;
							throw new Exception($"Ошибка при расчёте времени перехода:\n\n{errorMessage}");
						}

					}
			    }
			}
			catch (Exception ex)
			{
				string errorMessage = ex.InnerException?.Message ?? ex.Message;
				MessageBox.Show(errorMessage, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
            finally
            {
                // todo: реализовать обновление только ячейки времени выполнения, а не всей таблицы
                BeginInvoke(new Action(() =>
                {
                    UpdateLocalTP();
                }));
            }
        }
        private void RecalculateAllRepeatsValues()
        {
			try
			{
                var allRepeats = TechOperationForm.TechOperationWorksList.SelectMany(w => w.executionWorks.Where(ew => ew.Repeat)).ToList();
				foreach (var repeat in allRepeats)
				{
					RecalculateExecutionWorkPovtorValue(repeat);
				}
			}
			catch (Exception ex)
			{
				string errorMessage = ex.InnerException?.Message ?? ex.Message;
				MessageBox.Show(errorMessage, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

        public void SelectPageTab(string tabPageName)
		{
			tabControl1.SelectTab(tabPageName);
		}

        public void SelectTO(TechOperationWork techOperationWork)
        {
			if (techOperationWork == null)
				return;
			var index = TechOperationForm.TechOperationWorksList.IndexOf(techOperationWork);
			if (index == -1)
				return;
			dataGridViewTO.ClearSelection();
			dataGridViewTO.Rows[index].Selected = true;
			dataGridViewTO.FirstDisplayedScrollingRowIndex = index;
		}

        public void SelectTP(ExecutionWork executionWork)
        {
			if (executionWork == null)
				return;

			SelectComboBoxTOItem(executionWork.techOperationWork);

			UpdateComboBoxTT(executionWork);
            HighlightDataGridTPLocalItem(executionWork);
		}

        private void HighlightDataGridTPLocalItem(ExecutionWork executionWork)
        {
			if (executionWork == null)
				return;
			var index = GetRowIndexByGuid(executionWork.IdGuid); //Cells[0].Value
			if (index == -1)
				return;

			dataGridViewTPLocal.ClearSelection();
			dataGridViewTPLocal.Rows[index].Cells[2].Selected = true;
			dataGridViewTPLocal.FirstDisplayedScrollingRowIndex = index;
		}

		/// <summary>
		/// Ищет в dataGridViewComponentLocal строку, связанную с переданным ComponentWork,
		/// и выделяет её. Если ничего не найдено, выделение сбрасывается.
		/// </summary>
		/// <param name="compWork">Объект ComponentWork, по которому идёт поиск.</param>
		public void HighlighComponentLocalRow(ComponentWork compWork)
		{
			// Снимаем текущее выделение
			dataGridViewComponentLocal.ClearSelection();

			if (compWork == null)
				return;

			// Проходим по всем строкам dataGridViewInstrumentLocal
			for (int i = 0; i < dataGridViewComponentLocal.Rows.Count; i++)
			{
				var cellValue = dataGridViewComponentLocal.Rows[i].Cells[0].Value;
				if (cellValue is ComponentWork obj)
				{
					// Сравниваем идентификаторы инструмента и компонента.
					if (obj.componentId == compWork.componentId)
					{
						HighlighDataGridViewRowByIndex(i, dataGridViewComponentLocal);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Ищет в dataGridViewInstrumentLocal строку, связанную с переданным ToolWork,
		/// и выделяет её. Если ничего не найдено, выделение сбрасывается.
		/// </summary>
		/// <param name="toolWork">Объект ToolWork, по которому идёт поиск.</param>
		public void HighlightInstrumentLocalRow(ToolWork toolWork)
		{
			// Снимаем текущее выделение
			dataGridViewInstrumentLocal.ClearSelection();

			if (toolWork == null)
				return;

			// Проходим по всем строкам dataGridViewInstrumentLocal
			for (int i = 0; i < dataGridViewInstrumentLocal.Rows.Count; i++)
			{
				// Предположим, что в первой ячейке лежит объект типа ToolWork
				// (или иной класс, где есть toolId).
				var cellValue = dataGridViewInstrumentLocal.Rows[i].Cells[0].Value;
				if (cellValue is ToolWork obj)
				{
					if (obj.toolId == toolWork.toolId)
					{
                        HighlighDataGridViewRowByIndex(i, dataGridViewInstrumentLocal);
						break;
					}
				}
			}
		}

		private void HighlighDataGridViewRowByIndex(int i, DataGridView dataGridView)
		{
			// Если это нужная строка – выделяем её
			dataGridView.Rows[i].Selected = true;
			// Прокрутка грида так, чтобы выделенная строка была видна
			dataGridView.FirstDisplayedScrollingRowIndex = i;
		}



		private int GetRowIndexByGuid(Guid guid)
		{
            DataGridView dataGridView = dataGridViewTPLocal;

			foreach (DataGridViewRow row in dataGridView.Rows)
			{
				if ((Guid)row.Cells[0].Value == guid)
				{
					return row.Index;
				}
			}
			return -1;
		}
	}
}
