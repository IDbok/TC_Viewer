using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows.Forms;
using TcDbConnector;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms;

// todo: проверка соотвествия ограничениям класса Coefficient
public partial class CoefficientEditorForm : Form
{
	private BindingSource _bindingSource = null!;
	private List<Coefficient> _coefficients = null!;

	private readonly MyDbContext _context;
	private readonly int _tcId;
	private readonly TcViewState _tcViewState;

	private const int maxCoefficientsCount = 5;


	public CoefficientEditorForm(int tcId, TcViewState tcViewState, MyDbContext context)
	{
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
		var allNumbers = _coefficients.Select(c => c.GetNumber());
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

		var newCode = "Q" + (newNum + 1);
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

	}
}
