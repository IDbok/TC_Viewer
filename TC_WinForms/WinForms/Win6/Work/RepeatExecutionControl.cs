using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;
using System.Windows.Input;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Controls;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcDbConnector.Repositories;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Work;

/// <summary>
/// Управляет функциональностью повторных переходов (ExecutionWork с признаком Repeat) 
/// в рамках технологической карты. Предоставляет возможность выбрать, какие шаги (ChildExecutionWork)
/// "подтягивать" для повтора, а также позволяет при необходимости изменять коэффициент, этап и 
/// последовательность выполнения.
/// </summary>
public partial class RepeatExecutionControl : UserControl
{
	// todo: Как быть с динамическими коэффициентами?
	// todo: вставить как элемент в addEdit... чтобы отображался в конструкторе

	#region Поля и константы

	private ILogger _logger;

	private ExecutionWork? _parentExecutionWork;
	private List<ExecutionWork> _executionWorks;
	private readonly TcViewState _tcViewState;
	private readonly MyDbContext _context;
	private readonly TechnologicalCardRepository _tcRepos = new TechnologicalCardRepository();

	private SearchBox<TechnologicalCard>? searchBox;

	#endregion


	/// <summary>
	/// Создаёт новый экземпляр <see cref="RepeatExecutionControl"/>.
	/// </summary>
	/// <param name="myDbContext">Экземпляр <see cref="MyDbContext"/> для доступа к БД.</param>
	/// <param name="tcViewState">Объект, хранящий текущее состояние ТК и другие настройки.</param>
	public RepeatExecutionControl(MyDbContext myDbContext, TcViewState tcViewState)
	{
		InitializeComponent();
		_logger = Log.Logger.ForContext<RepeatExecutionControl>();
		_context = myDbContext;
		_tcViewState = tcViewState;
		_executionWorks = _tcViewState.GetAllExecutionWorks();

		dataGridViewRepeats.AutoGenerateColumns = false;
		dataGridViewRepeats.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

		// Подписка на события
		dataGridViewRepeats.CellContentClick += dataGridViewRepeats_CellContentClick;
		dataGridViewRepeats.CellEndEdit += dataGridViewRepeats_CellEndEdit;
		dataGridViewRepeats.CellFormatting += dataGridViewRepeats_CellFormatting;
		dataGridViewRepeats.CellValidating += dataGridView_CellValidating;
		dataGridViewRepeats.CellValueChanged += dataGridViewRepeats_CellValueChanged;

		_logger.Information("RepeatExecutionControl создан");
	}


	#region Свойства и события

	// todo: Исправить. обновляется при изменения выбора в повторе (трижды при снятии, дважды при установки)
	/// <summary>
	/// Событие, вызываемое при любом изменении данных в контроле, 
	/// например при выборе или снятии выбора повторяемого шага.
	/// </summary>
	public event EventHandler? DataChanged;

	#endregion


	#region Инициализация и настройка

	private void SetSearchBox()
	{
		searchBox = new SearchBox<TechnologicalCard>();

		searchBox.SelectedItemChanged += async (sender, e) =>
		{

			if (_parentExecutionWork == null) return;

			var selectedTc = e.SelectedItem;

			if (selectedTc == null) return;

			_logger.Information("Выбран элемент SearchBox с ID {SelectedItemId}", selectedTc.Id);

			var itemsToRemove = _parentExecutionWork.ExecutionWorkRepeats.ToList();
			foreach (var ewr in itemsToRemove)
			{
				_context.ExecutionWorkRepeats.Remove(ewr);
			}
			_parentExecutionWork.ExecutionWorkRepeats.Clear();

			_parentExecutionWork.RepeatsTCId = selectedTc.Id;
			await LoadExecutionWorksByTcIdAsync(selectedTc.Id);
		};

		// добавить в панель pnlControls
		pnlControls.Controls.Add(searchBox);
	}

	/// <summary>
	/// Инициирует логику для отображения поля поиска (SearchBox) при необходимости. 
	/// Если у <see cref="_parentExecutionWork"/> есть <c>RepeatsTCId</c>, 
	/// то управление даёт пользователю выбрать нужную технологическую карту.
	/// </summary>
	/// <param name="searchVisible">Отвечает за видимость панели с поиском ТК (по умолчанию true).</param>
	public async Task SetSearchBoxParamsAsync(bool searchVisible = true)
	{
		if (searchVisible)
		{
			pnlControls.Visible = true;

			if (searchBox == null)
			{
				SetSearchBox();
			}

			var data = await _tcRepos.GetAllAsync();//await _context.TechnologicalCards.ToListAsync();

			// Указываем источник данных
			// Обратите внимание: тип <string>, а DisplayMemberFunc = x => x, 
			// поскольку это просто строка.
			searchBox!.DataSource = data.Where(tc => _parentExecutionWork != null 
				&& tc.Id != _parentExecutionWork.RepeatsTCId
				&& tc.Id != _tcViewState.TechnologicalCard.Id 
				).ToList();
			searchBox.DisplayMemberFunc = x => $"{x.Name} {x.Article}";

			searchBox.SearchCriteriaFunc = x => x.ToString();

			// Устанавливаем выбранную ТК, если есть _parentExecutionWork с RepeatsTCId
			if (_parentExecutionWork?.RepeatsTCId != null)
			{
				var selectedTc = data.FirstOrDefault(tc => tc.Id == _parentExecutionWork.RepeatsTCId);
				if (selectedTc != null)
				{
					searchBox.SetSelectedItem(selectedTc, false);
				}
				else
				{
					searchBox.SetSelectedItem(null, false);
				}
			}
		}
		else
		{
			pnlControls.Visible = false;
		}
	}

	/// <summary>
	/// Устанавливает для данного контрола родительский переход 
	/// (тот, у которого <c>Repeat = true</c>), чтобы впоследствии 
	/// пользователь мог выбрать, какие ChildExecutionWork добавлять/убирать из повторения.
	/// </summary>
	/// <param name="parentEW">Повторяющий переход (ExecutionWork.Repeat = true).</param>
	public async Task SetParentExecutionWorkAsync(ExecutionWork parentEW)
	{
		_logger.Information("Установка родительского ТП {ParentEWId}", parentEW?.Id);

		if (parentEW == null || !parentEW.Repeat)
		{
			_parentExecutionWork = null;
			_executionWorks = new List<ExecutionWork>();
			await SetSearchBoxParamsAsync(searchVisible: false);
			RefreshData();
			return;
		}

		_parentExecutionWork = parentEW;

		var check = parentEW.RepeatsTCId != null;

		_executionWorks.Clear();

		await SetSearchBoxParamsAsync(parentEW.RepeatsTCId != null);

		if (parentEW.RepeatsTCId == null)
		{
			_executionWorks = _tcViewState.GetAllExecutionWorks();
		}
		else
		{
			if (parentEW.RepeatsTCId != 0) 
				_executionWorks = await GetAllExecutionWorksByTcIdAsync(parentEW.RepeatsTCId.Value);
		}

		RefreshData();
	}

	/// <summary>
	/// Выполняет повторное считывание данных из текущего <see cref="_parentExecutionWork"/> 
	/// и перезаполняет <c>dataGridViewRepeats</c> записями для возможных повторяемых шагов.
	/// </summary>
	public void RefreshData()
	{
		dataGridViewRepeats.Rows.Clear();
		if (_parentExecutionWork == null) return;

		ExecutionWork exeWork = _parentExecutionWork;

		var repeats = _parentExecutionWork.ExecutionWorkRepeats;
		if (repeats == null) return;

		var selectedEW = repeats.Select(ewr => ewr.ChildExecutionWork).ToList();
		var selectedEWR = repeats.ToList();

		if (exeWork != null && exeWork.Repeat)
		{
			foreach (ExecutionWork executionWork in _executionWorks)
			{
				var techOperationWork = executionWork.techOperationWork;
				var isSelected = selectedEW.SingleOrDefault(s => s == executionWork) != null;

				List<object> listItem =
							[
								executionWork,
								isSelected ? true : false,
								$"№{techOperationWork.Order} {techOperationWork.techOperation.Name}",
								executionWork.techTransition?.Name ?? "",
								executionWork.Coefficient ?? "",
							];

				if (isSelected)
				{
					ExecutionWorkRepeat? techOperationWorkRepeat = selectedEWR.SingleOrDefault(s => s.ChildExecutionWork == executionWork);

					if (techOperationWorkRepeat != null)
					{
						listItem.Add(techOperationWorkRepeat.NewCoefficient);
						listItem.Add(techOperationWorkRepeat.NewEtap);
						listItem.Add(techOperationWorkRepeat.NewPosled);
					}
				}

				dataGridViewRepeats.Rows.Add(listItem.ToArray());
			}
		}
	}

	#endregion


	#region Вспомогательные методы (приватные)

	/// <summary>
	/// Асинхронно загружает все переходы (ExecutionWork) по переданному <paramref name="id"/> технологической карты.
	/// </summary>
	/// <param name="id">Идентификатор ТК.</param>
	private async Task<List<ExecutionWork>> GetAllExecutionWorksByTcIdAsync(long id)
	{
		var executionWorkList = await _context.TechOperationWorks
			.Where(tow => tow.TechnologicalCardId == id)
			.SelectMany(x => x.executionWorks)
				.Include(ew => ew.techTransition)
				.Include(ew => ew.techOperationWork)
					.ThenInclude(tow => tow.techOperation)
			.ToListAsync();
		if(!executionWorkList.All(e => e.RowOrder == 0))
        {
			executionWorkList.OrderBy(e => e.RowOrder);
        }
		return executionWorkList;
	}

	private async Task LoadExecutionWorksByTcIdAsync(long id)
	{
		_executionWorks.Clear();
		_executionWorks = await GetAllExecutionWorksByTcIdAsync(id);
		RefreshData();
	}

	#endregion


	#region Обработчики событий DataGridView

	// Стобцы в dataGridViewRepeats:
	// 0 dgvRepeatsEwObject
	// 1 dgvRepeatsSelected
	// 2 dgvRepeatsToName
	// 3 dgvRepeatsTpName
	// 4 dgvRepeatsOldCoefficient
	// 5 dgvRepeatsCoefficient
	// 6 dgvRepeatsEtap
	// 7 dgvRepeatsPosled

	private void dataGridViewRepeats_CellContentClick(object? sender, DataGridViewCellEventArgs e)
	{
		dataGridViewRepeats.CommitEdit(DataGridViewDataErrorContexts.Commit);

		var columnName = dataGridViewRepeats.Columns[e.ColumnIndex].Name;
		if (e.RowIndex >= 0 && columnName == "dgvRepeatsSelected")
		{
			var currentEW = (ExecutionWork)dataGridViewRepeats.Rows[e.RowIndex].Cells[0].Value;
			var isSelected = (bool)dataGridViewRepeats.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

			// позиция для executionWorkPovtor в таблице dataGridViewRepeats
			var powtorIndex = dataGridViewRepeats.Rows.Cast<DataGridViewRow>().ToList().FindIndex(x => x.Cells[0].Value == _parentExecutionWork);

			if (_parentExecutionWork != null && (_parentExecutionWork.RepeatsTCId != null || powtorIndex > e.RowIndex))//currentEW.Order < executionWorkPovtor.Order)
			{
				var existingRepeat = _parentExecutionWork.ExecutionWorkRepeats
					.SingleOrDefault(x => x.ChildExecutionWork == currentEW);

				if (isSelected)
				{
					if (existingRepeat == null)
					{
						var newRepeat = new ExecutionWorkRepeat
						{
							ParentExecutionWork = _parentExecutionWork,
							ParentExecutionWorkId = _parentExecutionWork.Id,
							ChildExecutionWork = currentEW,
							ChildExecutionWorkId = currentEW.Id,
							NewCoefficient = "*1"
						};

						dataGridViewRepeats.Rows[e.RowIndex].Cells[5].Value = "*1";
						_parentExecutionWork.ExecutionWorkRepeats.Add(newRepeat);
						_context.ExecutionWorkRepeats.Add(newRepeat);
					}
				}
				else
				{
					if (existingRepeat != null)
					{
						dataGridViewRepeats.Rows[e.RowIndex].Cells[5].Value = "";
						_parentExecutionWork.ExecutionWorkRepeats.Remove(existingRepeat);
						_context.ExecutionWorkRepeats.Remove(existingRepeat);
					}
				}

				UpdateCoefficient(_parentExecutionWork);

				// Перерисовать таблицу
				dataGridViewRepeats.Invalidate();

				// Делегат по обновлению данных в таблице хода работ
				DataChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	private void dataGridViewRepeats_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
	{
		var columnName = dataGridViewRepeats.Columns[e.ColumnIndex].Name;
		// todo: реализовать пресчёт значения
		if (e.RowIndex >= 0 && columnName == "dgvRepeatsCoefficient")
		{
			var currentEW = (ExecutionWork)dataGridViewRepeats.Rows[e.RowIndex].Cells[0].Value;
			var isSelected = (bool)dataGridViewRepeats.Rows[e.RowIndex].Cells[1].Value;

			if (_parentExecutionWork != null)
			{
				var existingRepeat = _parentExecutionWork.ExecutionWorkRepeats
					.FirstOrDefault(x => x.ChildExecutionWork == currentEW);

				var cell = dataGridViewRepeats.Rows[e.RowIndex].Cells[e.ColumnIndex];

				if (isSelected)
				{
					RecalculateExecutionWorkPovtorValue(_parentExecutionWork);
					DataChanged?.Invoke(this, EventArgs.Empty);
				}
				else
				{
					// отмена изменений
					cell.Value = null;
				}
			}
		}
	}

	private void dataGridViewRepeats_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
	{
		if (_parentExecutionWork == null) return;

		var executionWork = (ExecutionWork)dataGridViewRepeats.Rows[e.RowIndex].Cells[0].Value;
		// позиция для _parentExecutionWork в таблице dataGridViewRepeats
		var powtorIndex = dataGridViewRepeats.Rows.Cast<DataGridViewRow>().ToList().FindIndex(x => x.Cells[0].Value == _parentExecutionWork);

		bool isReadOnlyRow = _parentExecutionWork.RepeatsTCId != null ? false : powtorIndex < e.RowIndex;

		var columnName = dataGridViewRepeats.Columns[e.ColumnIndex].Name;

		if (columnName == "dgvRepeatsSelected") // Индекс столбца с checkBox
		{
			if (executionWork == _parentExecutionWork)
			{
				SetReadOnlyAndColor(e.ColumnIndex, Color.DarkSeaGreen);
			}
			else if (isReadOnlyRow)
			{
				SetReadOnlyAndColor(e.ColumnIndex, Color.DarkSalmon);
			}
		}
		else if (columnName == "dgvRepeatsCoefficient" ||
				columnName == "dgvRepeatsEtap" ||
				columnName == "dgvRepeatsPosled")
		{
			if (executionWork == _parentExecutionWork || isReadOnlyRow)
			{
				// Делаем ячейку недоступной
				dataGridViewRepeats.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
			}
			else
			{
				var existingRepeat = _parentExecutionWork.ExecutionWorkRepeats
					.SingleOrDefault(x => x.ChildExecutionWork == executionWork);

				dataGridViewRepeats.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = existingRepeat != null ? Color.LightGray : Color.White;
				dataGridViewRepeats.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = existingRepeat == null;
			}
		}
		void SetReadOnlyAndColor(int columnIndex, Color color, bool readOnly = true)
		{
			dataGridViewRepeats.Rows[e.RowIndex].Cells[columnIndex].ReadOnly = readOnly;
			dataGridViewRepeats.Rows[e.RowIndex].DefaultCellStyle.BackColor = color;
		}
	}

	private void dataGridView_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
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

	private void dataGridViewRepeats_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
	{
		var columnName = dataGridViewRepeats.Columns[e.ColumnIndex].Name;
		if (e.RowIndex >= 0 &&
				(columnName == "dgvRepeatsCoefficient" ||
				columnName == "dgvRepeatsEtap" ||
				columnName == "dgvRepeatsPosled"))
		{
			var currentEW = (ExecutionWork)dataGridViewRepeats.Rows[e.RowIndex].Cells[0].Value;
			var isSelected = (bool)dataGridViewRepeats.Rows[e.RowIndex].Cells[1].Value;

			if (_parentExecutionWork != null)
			{
				var existingRepeat = _parentExecutionWork.ExecutionWorkRepeats
					.SingleOrDefault(x => x.ChildExecutionWork == currentEW);

				var cell = dataGridViewRepeats.Rows[e.RowIndex].Cells[e.ColumnIndex];

				if (isSelected)
				{
					if (existingRepeat != null)
					{

						var cellValueStr = (string)cell.Value;

						if (columnName == "dgvRepeatsCoefficient")
						{
							if (string.IsNullOrEmpty(cellValueStr))
							{
								existingRepeat.NewCoefficient = string.Empty;
							}
							else
							{
								existingRepeat.NewCoefficient = cellValueStr;
							}
						}
						else if (columnName == "dgvRepeatsEtap")
						{
							existingRepeat.NewEtap = cellValueStr;
						}
						else if (columnName == "dgvRepeatsPosled")
						{
							existingRepeat.NewPosled = cellValueStr;
						}
					}
				}
				else
				{
					// отмена изменений
					cell.Value = null;
				}

				UpdateCoefficient(_parentExecutionWork);
				DataChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	#endregion


	#region Методы пересчёта «Повторов»

	/// <summary>
	/// Пересчёт суммарного времени в родительском переходе. 
	/// Берёт все выбранные ChildExecutionWork и суммирует их время, учитывая <see cref="ExecutionWorkRepeat.NewCoefficient"/>.
	/// </summary>
	/// <param name="executionWorkRepeats">Родительский переход (ExecutionWork), для которого идёт пересчёт.</param>
	private void RecalculateExecutionWorkPovtorValue(ExecutionWork executionWorkRepeats)
	{
		if (!executionWorkRepeats.Repeat) return;

		double totalValue = 0;
		try
		{
			var coefDict = _tcViewState.TechnologicalCard.Coefficients.ToDictionary(c => c.Code, c => c.Value);

			foreach (var repeat in executionWorkRepeats.ExecutionWorkRepeats)
			{
				if (repeat.ChildExecutionWork.Delete)
					continue;

				var value = repeat.ChildExecutionWork.Value;

				totalValue += MathScript.EvaluateCoefficientExpression(repeat.NewCoefficient, coefDict, value.ToString());
			}
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "Ошибка при пересчете значения повтора");
			string errorMessage = ex.InnerException?.Message ?? ex.Message;

			MessageBox.Show($"Ошибка при пересчете значения повтора:\n\n{errorMessage}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

			totalValue = -1;
		}
		finally
		{
			executionWorkRepeats.Value = totalValue;
		}
	}

	private void UpdateRelatedReplays(ExecutionWork updatedExecutionWork)
	{
		var allExecutionWorks = _executionWorks;
		var allRepeats = allExecutionWorks.Where(ew => ew.Repeat && ew.ExecutionWorkRepeats.Any(e => e.ChildExecutionWorkId == updatedExecutionWork.Id));

		foreach (var executionWork in allRepeats)
		{
			UpdateCoefficient(executionWork);
		}
	}

	/// <summary>
	/// Обновляет <paramref name="editedExecutionWork"/> и связанные повторы, 
	/// если исходное время или коэффициент изменились.
	/// </summary>
	/// <param name="editedExecutionWork">Редактируемый переход (ExecutionWork).</param>
	/// <param name="oldCoefficient">
	/// Предыдущее значение времени или коэффициента (для отслеживания изменений). 
	/// Если оно <c>null</c> — пересчёт выполняется всегда.
	/// </param>
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
			RecalculateExecutionWorkPovtorValue(editedExecutionWork);

		if (oldCoefficient != editedExecutionWork.Value)
			UpdateRelatedReplays(editedExecutionWork);

	}

	#endregion
}
