using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Windows.Forms;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms;

// todo: проверка соотвествия ограничениям класса Coefficient
// todo: скрывать данную форму в режиме просмотра
public partial class CoefficientEditorForm : Form
{
	private readonly ILogger _logger;
	private BindingSource _bindingSource = null!;
	private List<Coefficient> _coefficients = null!;

	private readonly MyDbContext _context;
	private readonly int _tcId;
	private readonly TcViewState _tcViewState;

	private const int maxCoefficientsCount = 5;


	public CoefficientEditorForm(int tcId, TcViewState tcViewState, MyDbContext context)
	{
		_logger = Log.Logger.ForContext<CoefficientEditorForm>();
		_logger.Information("Инициализация CoefficientEditorForm: TcId={TcId}", tcId);

		_context = context;
		_tcId = tcId;
		_tcViewState = tcViewState;

		InitializeComponent();

		InitializeData();

	}

	private void InitializeData()
	{
		_coefficients = _tcViewState.TechnologicalCard.Coefficients;
		_bindingSource = new BindingSource { DataSource = _coefficients };

		dgvCoefficients.DataSource = _bindingSource;
	}

	private void btnAddCoefficient_Click(object sender, EventArgs e)
	{
		var count = _coefficients.Count;
		if (count == maxCoefficientsCount)
		{
			var message = $"Количество коэффициентов не может превышать {maxCoefficientsCount}";
			MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		// найти максимум из всех номеров коэффициентов
		int newNum = GetNextCoefficientNumber();

		var newCode = "Q" + (newNum);
		var newC = new Coefficient { TechnologicalCardId = _tcId, Code = newCode };
		_coefficients.Add(newC);
		_bindingSource.ResetBindings(false);

		Debug.WriteLine($"Номер коэффициента: {newC.GetNumber()}");
	}

	private int GetNextCoefficientNumber()
	{
		var allNumbers = _coefficients.Select(c => c.GetNumber()).ToList();
		if (allNumbers.Count == 0) return 1;

		var maxCfNumber = allNumbers.Max();
		var newNum = 0;

		// найти первый отсутствующий в последовательностьи номер
		if (maxCfNumber > maxCoefficientsCount)
		{
			for (int i = 1; i <= maxCoefficientsCount; i++)
			{
				if (!allNumbers.Contains(i))
				{
					newNum = i;
					break;
				}
			}
		}
		else
		{
			newNum = maxCfNumber + 1;
		}

		return newNum;
	}

	private object _previousCellValue;

	private void dgvCoefficients_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
	{
		var cell = dgvCoefficients.Rows[e.RowIndex].Cells[e.ColumnIndex];
		_previousCellValue = cell.Value; // Сохраняем предыдущее значение
	}

	/// <summary>
	/// Выполняет проверку свойства объекта на соответствие атрибутам валидации.
	/// </summary>
	/// <param name="obj">Объект, для которого выполняется проверка.</param>
	/// <param name="propertyName">Имя свойства, которое проверяется.</param>
	/// <param name="propertyValue">Значение свойства.</param>
	/// <param name="errorMessage">Сообщение об ошибке валидации, если она не пройдена.</param>
	/// <returns>True, если валидация прошла успешно; False, если обнаружены ошибки.</returns>
	public static bool TryValidateWithRollback(object obj, string propertyName, object propertyValue, out string errorMessage)
	{
		var context = new ValidationContext(obj) { MemberName = propertyName };
		var results = new List<ValidationResult>();

		// Попытка валидации свойства
		try
		{
			if (!Validator.TryValidateProperty(propertyValue, context, results))
			{
				errorMessage = string.Join("\n", results.Select(r => r.ErrorMessage));
				return false;
			}
		}
		catch (Exception ex)
		{
			errorMessage = $"Ошибка валидации: {ex.Message}";
			return false;
		}

		errorMessage = string.Empty;
		return true;
	}

	private void dgvCoefficients_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
	{
		var cell = dgvCoefficients.Rows[e.RowIndex].Cells[e.ColumnIndex];
		var coefficient = dgvCoefficients.Rows[e.RowIndex].DataBoundItem as Coefficient;

		if (coefficient == null) return;

		// Получаем имя свойства, связанного с редактируемой колонкой
		var propertyName = dgvCoefficients.Columns[e.ColumnIndex].DataPropertyName;
		var propertyValue = e.FormattedValue;

		// Проверяем числовые поля
		if (propertyName == nameof(Coefficient.Value))
		{
			if (propertyValue == null || !double.TryParse(propertyValue.ToString(), out double parsedValue))
			{
				MessageBox.Show("Значение коэффициента должно быть числом.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

				// установка нулегового значения
				e.Cancel = true;
				cell.Value = _previousCellValue;

				return;
			}
			propertyValue = parsedValue;
		}

		// Выполняем валидацию
		if (!TryValidateWithRollback(coefficient, propertyName, propertyValue, out var errorMessage))
		{
			// Показываем сообщение об ошибке
			MessageBox.Show(errorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
			// Отменяем изменение
			e.Cancel = true;
		}
	}

	private void dgvCoefficients_CellEndEdit(object sender, DataGridViewCellEventArgs e)
	{
		// Выполнить пересчёт значений в открытых формах
		_tcViewState.RecalculateValuesWithCoefficients();
	}
}
