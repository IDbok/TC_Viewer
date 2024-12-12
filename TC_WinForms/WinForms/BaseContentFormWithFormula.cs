using Serilog;
using System.ComponentModel;
using TC_WinForms.Services;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms;

public abstract class BaseContentFormWithFormula<T> : BaseContentForm
	where T : class, IFormulaItem, IIntermediateDisplayedEntity, INotifyPropertyChanged, new()
{
	protected TcViewState _tcViewState;
	protected ILogger _logger;

	protected BindingList<T> _bindingList;
	protected List<T> _changedObjects = new();
	protected List<T> _newObjects = new();
	protected List<T> _deletedObjects = new();

	// DataGridView должен быть доступен в потомках
	protected abstract DataGridView DgvMain { get; }

	// Дочерние формы определят конкретную загрузку объектов
	protected abstract void LoadObjects();

	// Дочерние формы настраивают столбцы DataGridView
	protected abstract void InitializeDataGridViewColumns();

	// Метод для получения словаря коэффициентов
	protected Dictionary<string, double> GetCoefficientDictionary()
	{
		return _tcViewState.TechnologicalCard.Coefficients.ToDictionary(c => c.Code, c => c.Value);
	}

	public override async void OnActivate()
	{
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
}
// Интерфейс для элементов, у которых есть формулы
public interface IFormulaItem
{
	string? Formula { get; }
	double? Quantity { get; set; }
	string Name { get; }
	string? Type { get; }
}