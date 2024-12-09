using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Windows.Forms;
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
		var allNumbers = _coefficients.Select(c => c.GetNumber()).ToList();
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

		var newCode = "Q" + (newNum);
		var newC = new Coefficient { TechnologicalCardId = _tcId, Code = newCode };
		_coefficients.Add(newC);
		_bindingSource.ResetBindings(false);

		Debug.WriteLine($"Номер коэффициента: {newC.GetNumber()}");
	}

	private async void CoefficientEditorForm_Load(object sender, EventArgs e)
	{
		//_coefficients = await _context.Coefficients.Where(c => c.TechnologicalCardId == _tcId).ToListAsync();

		//InitializeData();
	}

	private void dgvCoefficients_CellEndEdit(object sender, DataGridViewCellEventArgs e)
	{
		var row = dgvCoefficients.Rows[e.RowIndex];
		var cell = row.Cells[e.ColumnIndex];
		var coefficient = row.DataBoundItem as Coefficient;

		if (coefficient == null) return;

		// Получаем имя свойства, связанного с редактируемой колонкой
		var propertyName = dgvCoefficients.Columns[e.ColumnIndex].DataPropertyName;

		// Список для хранения ошибок
		var validationResults = new List<ValidationResult>();
		var validationContext = new ValidationContext(coefficient) { MemberName = propertyName };

		// Проверка объекта на соответствие атрибутам
		if (!TryValidateWithRollback(coefficient, propertyName, cell.Value, out var errorMessage))
		{
			MessageBox.Show(errorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
			cell.Value = _previousCellValue;
		}

		// Обновляем привязку данных при успешной валидации
		_bindingSource.ResetBindings(false);
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

		if (!Validator.TryValidateProperty(propertyValue, context, results))
		{
			errorMessage = string.Join("\n", results.Select(r => r.ErrorMessage));
			return false;
		}

		errorMessage = string.Empty;
		return true;
	}

	private void dgvCoefficients_DataError(object sender, DataGridViewDataErrorEventArgs e)
	{
		//// Обработка конкретного типа ошибок
		//if (e.Context == DataGridViewDataErrorContexts.Commit ||
		//	e.Context == DataGridViewDataErrorContexts.CurrentCellChange)
		//{
		//	e.ThrowException = false;
		//}
		//else
		//{
		//	// Логирование других ошибок
		//	_logger.Error(e.Exception, "Ошибка в dgvCoefficients");
		//	MessageBox.Show("Произошла ошибка: " + e.Exception.Message);
		//}
	}

	private void dgvCoefficients_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
	{
		//if (dgvCoefficients.Columns[e.ColumnIndex].Name == "Значение")
		//{
		//	if (string.IsNullOrWhiteSpace(e.FormattedValue.ToString()))
		//	{
		//		// установить значение в ноль

		//		var row = dgvCoefficients.Rows[e.RowIndex];
		//		var cell = row.Cells[e.ColumnIndex];
		//		cell.Value = 0;

		//		MessageBox.Show("Значение не может быть пустым!");
		//	}
		//	else if (!double.TryParse(e.FormattedValue.ToString(), out _))
		//	{
		//		e.Cancel = true;
		//		MessageBox.Show("Введите корректное числовое значение!");

		//	}
		//}
	}
}
