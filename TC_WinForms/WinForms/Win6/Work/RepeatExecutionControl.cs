using Serilog;
using System.Data;
using System.Windows.Input;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Controls;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Work;
public partial class RepeatExecutionControl : UserControl
{
	// todo: для повтора в соответствии с ТК добавить выбот ТК в отдельной панели, которая будет отображаться только для ТК с повтором другой ТК
	// todo: реализовать выбор по средствам "поиска" с выпадением списка совпадений по вводимому тексту (+ поиск по id)

	// todo: добавить логгирование
	private ILogger _logger;

	private ExecutionWork? _parentExecutionWork;
	private List<ExecutionWork> _executionWorks;
	private readonly TcViewState _tcViewState;
	private readonly MyDbContext _context;

	// todo: обновляется при изменения выбора в повторе (трижды при снятии, дважды при установки)
	public event EventHandler? DataChanged;	// Событие, если надо сообщать «внешнему» коду, что данные изменились

	// Конструктор
	public RepeatExecutionControl(MyDbContext myDbContext, TcViewState tcViewState)// List<ExecutionWork> executionWorks)
	{
		InitializeComponent();
		_logger = Log.Logger.ForContext<RepeatExecutionControl>();
		_context = myDbContext;
		_tcViewState = tcViewState;
		_executionWorks = _tcViewState.GetAllExecutionWorks();

		if (true)
		{
			pnlControls.Visible = true;
			// добавление SearchBox в панель
			var searchBox = new SearchBox<TechnologicalCard>();
			// добавить в панель pnlControls
			pnlControls.Controls.Add(searchBox);
		}


		// Пример: настройка dataGridViewRepeats (элемента внутри этого UserControl)
		dataGridViewRepeats.AutoGenerateColumns = false;
		dataGridViewRepeats.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

		// Подписка на события
		dataGridViewRepeats.CellContentClick += dataGridViewRepeats_CellContentClick;
		dataGridViewRepeats.CellEndEdit += dataGridViewRepeats_CellEndEdit;
		dataGridViewRepeats.CellFormatting += dataGridViewRepeats_CellFormatting;
		dataGridViewRepeats.CellValidating += dataGridView_CellValidating;
		dataGridViewRepeats.CellValueChanged += dataGridViewRepeats_CellValueChanged;
	}

	// Свойство/метод, чтобы «снаружи» задать, какую ExecutionWork (или список) показываем
	public void SetParentExecutionWork(ExecutionWork? parentEW)
	{
		_parentExecutionWork = parentEW;
		RefreshData();
	}

	// Основное обновление данных грида
	public void RefreshData()
	{
		_executionWorks = _tcViewState.GetAllExecutionWorks();
		UpdatePovtor();
	}
	public void UpdatePovtor()
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

		Console.WriteLine();
	}

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

			if (_parentExecutionWork != null && powtorIndex > e.RowIndex)//currentEW.Order < executionWorkPovtor.Order)
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

				//UpdateLocalTP();
			}
		}
	}

	private void dataGridViewRepeats_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
	{
		if (_parentExecutionWork == null) return;

		var executionWork = (ExecutionWork)dataGridViewRepeats.Rows[e.RowIndex].Cells[0].Value;
		// позиция для _parentExecutionWork в таблице dataGridViewRepeats
		var powtorIndex = dataGridViewRepeats.Rows.Cast<DataGridViewRow>().ToList().FindIndex(x => x.Cells[0].Value == _parentExecutionWork);
		bool isReadOnlyRow = powtorIndex < e.RowIndex;

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
			else // todo: снимать повтор, если объект перемещают "ниже" повтора
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
			Log.Error(ex, "Ошибка при пересчете значения повтора");
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
}
