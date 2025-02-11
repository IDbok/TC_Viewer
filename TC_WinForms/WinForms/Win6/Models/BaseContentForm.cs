using Serilog;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Extensions;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent.Work;

namespace TC_WinForms.WinForms.Win6.Models;

public abstract class BaseContentForm<T, TIntermediate> : Form, IViewModeable, IOnActivationForm, IDynamicForm
	where T : BaseDisplayedEntity, new() //class, IFormulaItem, IIntermediateDisplayedEntity, INotifyPropertyChanged, new()
	where TIntermediate : IIntermediateTableIds, IUpdatableEntity, new()
{
	protected TcViewState _tcViewState;
	protected ILogger _logger;

	protected abstract IList<TIntermediate> TargetTable { get; } 

	protected BindingList<T> _bindingList;
	protected List<T> _changedObjects = new();
	protected List<T> _newObjects = new();
	protected List<T> _deletedObjects = new();

	protected Dictionary<T, T> _replacedObjects = new(); 

	protected abstract DataGridView DgvMain { get; }
	protected abstract Panel PnlControls { get; }
	public BaseContentForm() {	}

	public void UpdateDynamicCardParametrs()
	{
		// скрывть столбец с коэффициентами
		DgvMain.Columns[nameof(BaseDisplayedEntity.Formula)].Visible = _tcViewState.IsViewMode ? false : _tcViewState.TechnologicalCard.IsDynamic;

		DgvMain.Columns[nameof(BaseDisplayedEntity.Quantity)].DefaultCellStyle.BackColor = 
			_tcViewState.IsViewMode || _tcViewState.TechnologicalCard.IsDynamic ? Color.White : Color.LightGray;

		DgvMain.Columns[nameof(BaseDisplayedEntity.Quantity)].ReadOnly = _tcViewState.IsViewMode || _tcViewState.TechnologicalCard.IsDynamic;

		if (_tcViewState.TechnologicalCard.IsDynamic && !_tcViewState.IsViewMode)
			LoadObjects();
	}
	// Дочерние формы определят конкретную загрузку объектов
	protected abstract void LoadObjects();

	// Дочерние формы настраивают столбцы DataGridView
	protected virtual void InitializeDataGridViewColumns()
	{
		SetDGVColumnsSettings();
	}
	protected virtual void InitializeDataGridViewEvents()
	{
		var dgvEventService = new DGVEvents(DgvMain);
		//dgvEventService.SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);

		DgvMain.CellFormatting += dgvEventService.dgvMain_CellFormatting;
		DgvMain.CellValidating += dgvEventService.dgvMain_CellValidating;
		DgvMain.CellValueChanged += dgvMain_CellValueChanged;
	}

	// Метод для получения словаря коэффициентов
	protected Dictionary<string, double> GetCoefficientDictionary()
	{
		return _tcViewState.TechnologicalCard.Coefficients.ToDictionary(c => c.Code, c => c.Value);
	}

	public virtual async void OnActivate()
	{
		if (_tcViewState.TechnologicalCard.IsDynamic)
			await RecalculateQuantitiesAsync();
	}

	protected async Task RecalculateQuantitiesAsync()
	{
		if (_bindingList == null || _bindingList.Count == 0)
			return;

		try
		{
			var coefDict = GetCoefficientDictionary();

			await Task.Run(() =>
			{
				foreach (var item in _bindingList.OfType<IFormulaItem>())
				{
					ApplyFormulaToItem(item, coefDict);
				}
			});

			RefreshDataGridView();
		}
		catch (Exception ex)
		{
			LogAndShowError($"Ошибка при пересчёте значений Quantity: {ex.Message}",
							"Ошибка пересчёта значений. Проверьте корректность данных.",
							ex.InnerException?.Message);
		}
	}

	protected void RecalculateQuantities()
	{
		if (_bindingList == null || _bindingList.Count == 0)
			return;

		try
		{
			var coefDict = GetCoefficientDictionary();

			foreach (var item in _bindingList.OfType<IFormulaItem>())
			{
				ApplyFormulaToItem(item, coefDict);
			}

			RefreshDataGridView();
		}
		catch (Exception ex)
		{
			LogAndShowError($"Ошибка при пересчёте значений Quantity: {ex.Message}",
							"Ошибка пересчёта значений. Проверьте корректность данных.",
							ex.InnerException?.Message);
		}
	}

	protected virtual void ApplyFormulaToItem(IFormulaItem item, Dictionary<string, double> coefDict)
	{
		if (string.IsNullOrWhiteSpace(item.Formula)) return;

		try
		{
			item.Quantity = MathScript.EvaluateCoefficientExpression(item.Formula, coefDict);
		}
		catch (Exception ex)
		{
			_logger?.Error($"Ошибка вычисления формулы для элемента '{item.Name} - {item.Type}': {ex.Message}");
			throw new InvalidOperationException($"Ошибка вычисления формулы для элемента '{item.Name} - {item.Type}'.", ex);
		}
	}

	protected virtual void RefreshDataGridView()
	{
		DgvMain?.Refresh();
	}

	protected void LogAndShowError(string logMessage, string userMessage, string? innerError = null)
	{
		_logger?.Error(logMessage);

		if (!string.IsNullOrWhiteSpace(innerError))
			userMessage += "\n\n" + innerError;

		MessageBox.Show(userMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	protected void LogUserAction(string message)
	{
		_logger?.LogUserAction(message);
	}

	protected void RecalculateQuantityForObject(T displayedObject)
	{
		if (displayedObject == null 
			|| string.IsNullOrEmpty(displayedObject.Formula))
			return;

		try
		{
			var coefDict = GetCoefficientDictionary();
			displayedObject.Quantity = MathScript.EvaluateCoefficientExpression(displayedObject.Formula, coefDict);
			RefreshDataGridView();
		}
		catch (Exception ex)
		{
			LogAndShowError(
				$"Ошибка вычисления формулы: {ex.Message}",
				$"Ошибка вычисления формулы для элемента " +
				$"'{displayedObject.Name} - {displayedObject.Type}'. " +
				$"Проверьте формулу.");
		}
	}

	protected virtual void SetDGVColumnsSettings()
	{
		// Автоподбор ширины столбцов под ширину таблицы
		DgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		DgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
		DgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		DgvMain.RowHeadersWidth = 25;

		// Автоперенос в ячейках
		DgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

		int pixels = 35;

		// Общие минимальные ширины столбцов
		var fixColumnWidths = GetFixedColumnWidths(pixels);
		foreach (var column in fixColumnWidths)
		{
			if (DgvMain.Columns.Contains(column.Key))
			{
				DgvMain.Columns[column.Key].Width = column.Value;
				DgvMain.Columns[column.Key].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
				DgvMain.Columns[column.Key].Resizable = DataGridViewTriState.False;
			}
		}

		// Настройка возможности изменения размеров определённых столбцов
		DgvMain.Columns[nameof(BaseDisplayedEntity.Type)].Resizable = DataGridViewTriState.True;

		// Установка столбцов только для чтения
		foreach (DataGridViewColumn column in DgvMain.Columns)
		{
			column.ReadOnly = true;
		}

		// Разрешаем редактирование определённых столбцов
		var changeableColumns = GetEditableColumns();
		foreach (var columnName in changeableColumns)
		{
			if (DgvMain.Columns.Contains(columnName))
			{
				DgvMain.Columns[columnName].ReadOnly = false;
				DgvMain.Columns[columnName].DefaultCellStyle.BackColor = Color.LightGray;
			}
		}
	}
	public virtual void SetViewMode(bool? isViewMode = null)// todo: можно перенести в BaseForm
	{
		PnlControls.Visible = !_tcViewState.IsViewMode;

		DgvMain.Columns[nameof(BaseDisplayedEntity.ChildId)].Visible = !_tcViewState.IsViewMode;

		// make columns editable
		DgvMain.Columns[nameof(BaseDisplayedEntity.Order)].ReadOnly = _tcViewState.IsViewMode;
		//DgvMain.Columns[nameof(BaseDisplayedEntity.Formula)].ReadOnly = _tcViewState.IsViewMode;
		DgvMain.Columns[nameof(BaseDisplayedEntity.Quantity)].ReadOnly = _tcViewState.IsViewMode;
		DgvMain.Columns[nameof(BaseDisplayedEntity.Note)].ReadOnly = _tcViewState.IsViewMode;

		DgvMain.Columns[nameof(BaseDisplayedEntity.Order)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
		DgvMain.Columns[nameof(BaseDisplayedEntity.Formula)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
		DgvMain.Columns[nameof(BaseDisplayedEntity.Quantity)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
		DgvMain.Columns[nameof(BaseDisplayedEntity.Note)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;

		UpdateDynamicCardParametrs();

		// update form
		RefreshDataGridView();
	}
	protected abstract void SaveReplacedObjects();

	// Метод для получения фиксированных ширин столбцов
	protected virtual Dictionary<string, int> GetFixedColumnWidths(int pixels)
	{
		return new Dictionary<string, int>
		{
			{ nameof(BaseDisplayedEntity.Order), 1 * pixels },
			{ nameof(BaseDisplayedEntity.Type), 4 * pixels },
			{ nameof(BaseDisplayedEntity.Unit), 2 * pixels },
			{ nameof(BaseDisplayedEntity.Formula), 3 * pixels },
			{ nameof(BaseDisplayedEntity.Quantity), 3 * pixels },
			{ nameof(BaseDisplayedEntity.ChildId), 2 * pixels }
		};
	}
	protected virtual int GetNewObjectOrder()
	{
		return _bindingList.Any() ? (_bindingList.Select(o => o.Order).Max() + 1) : 1;
	}

	// Метод для получения списка редактируемых столбцов
	protected virtual List<string> GetEditableColumns()
	{
		return new List<string>
	{
		nameof(BaseDisplayedEntity.Order),
		nameof(BaseDisplayedEntity.Formula),
		nameof(BaseDisplayedEntity.Note)
	};
	}
	protected virtual void Win_Load(object sender, EventArgs e) // todo: можно перенести в BaseForm
	{
		_logger.Information("Загрузка формы");

		LoadObjects();
		DisplayedEntityHelper.SetupDataGridView<T>(DgvMain);

		DgvMain.AllowUserToDeleteRows = false;

		SetViewMode();

		if(_tcViewState.TechnologicalCard.IsDynamic)
			RecalculateQuantities();
	}
	protected virtual void dgvMain_CellEndEdit(object sender, DataGridViewCellEventArgs e)
	// todo - fix problem with selection replacing row (error while remove it)
	{
		DGVProcessing.ReorderRows(DgvMain, e, _bindingList);
	}
	protected virtual void dgvMain_CellValueChanged(object sender, DataGridViewCellEventArgs e) // todo: можно перенести в BaseForm
	{
		if (e.RowIndex >= 0 && DgvMain.Columns[e.ColumnIndex].Name == nameof(BaseDisplayedEntity.Formula))
		{
			var row = DgvMain.Rows[e.RowIndex];
			if (row.DataBoundItem is T displayedComponent)
			{
				RecalculateQuantityForObject(displayedComponent);
			}
		}
	}
	protected virtual void BindingList_ListChanged(object sender, ListChangedEventArgs e)
	{
		DisplayedEntityHelper.ListChangedEventHandlerIntermediate
			(e, _bindingList, _newObjects, _changedObjects, _deletedObjects);

		if (_changedObjects.Count != 0)
		{
			foreach (var obj in _changedObjects)
			{
				var changedObject = TargetTable //_tcViewState.TechnologicalCard.Component_TCs
					.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
				if (changedObject != null)
					changedObject.ApplyUpdates(CreateNewObject(obj));
			}
			_changedObjects.Clear();

		}
	}
	protected abstract TIntermediate CreateNewObject(BaseDisplayedEntity dObj);

}
