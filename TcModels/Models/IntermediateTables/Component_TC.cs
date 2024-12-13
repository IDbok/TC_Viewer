using System.Data;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.IntermediateTables
{
    public class Component_TC : IStructIntermediateTable<TechnologicalCard, Component>, IDGViewable, IUpdatableEntity//, IIntermediateTableIds
	{
        public Dictionary<string, string> GetPropertiesNames { get; } = new Dictionary<string, string>
        {
            { nameof(ChildId), "ID Комплектующие" },
            { nameof(Child), "" },
            { nameof(ParentId), "ID тех. карты" },
            { nameof(Parent), "" },
            { nameof(Order), "№" },
            { nameof(Quantity), "Количество" },
            { nameof(Note), "Примечание" },
        };
        public static Dictionary<string, int> GetPropertiesOrder { get; } = new Dictionary<string, int>
        {
            { nameof(ChildId), -1 },
            { nameof(Child), -1 },
            { nameof(ParentId), -1},
            { nameof(Parent), -1 },

            { nameof(Order), 0 },
            { nameof(Quantity), 1 },
            { nameof(Note), 2 },

        };
        public List<string> GetPropertiesRequired { get; } = new List<string>
        {
            nameof(ChildId),
            nameof(ParentId),
            nameof(Order),
            nameof(Quantity),
        };
        public static List<string> GetChangeablePropertiesNames { get; } = new List<string>
        {
            nameof(Order),
            nameof(Quantity),
            nameof(Note),
        };

        public int ChildId { get; set; }
        public Component Child { get; set; }

        public int ParentId { get; set; }
        public TechnologicalCard Parent { get; set; }

        public int Order { get; set; }
        public double Quantity { get; set; }

		/// <summary>
		/// Формула для расчёта количества (Quantity)
		/// </summary>
		public string? Formula { get; set; }
		public string? Note { get; set; }

        public List<TechOperationWork> TechOperationWorks { get; set; }

        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is Component_TC sourceCard)
            {
                Order = sourceCard.Order;
                Quantity = sourceCard.Quantity;
                Note = sourceCard.Note;
                Formula = sourceCard.Formula;
            }
        }

		/// <summary>
		/// Вычисляет значение количества на основе формулы
		/// </summary>
		/// <returns>Вычисленное значение</returns>
		public double CalculateQuantity()
		{
			if (string.IsNullOrWhiteSpace(Formula))
				return Quantity; // Если формула не задана, возвращаем текущее значение

			try
			{
				var coefficients = Parent?.Coefficients?.ToDictionary(c => c.Code, c => c.Value)
								  ?? new Dictionary<string, double>();

				string formula = Formula;

				// Заменяем шифры коэффициентов в формуле на их значения
				foreach (var coefficient in coefficients)
				{
					formula = formula.Replace(coefficient.Key, coefficient.Value.ToString());
				}

				// Вычисляем значение формулы
				return EvaluateFormula(formula);
			}
			catch
			{
				// Если возникла ошибка при вычислении, возвращаем 0 (или выбрасываем исключение)
				return 0;
			}
		}

		/// <summary>
		/// Вычисляет математическое выражение в строковом формате
		/// </summary>
		/// <param name="expression">Математическое выражение</param>
		/// <returns>Результат вычисления</returns>
		private double EvaluateFormula(string expression)
		{
			DataTable table = new DataTable();
			table.Columns.Add("expression", typeof(string), expression);
			DataRow row = table.NewRow();
			table.Rows.Add(row);
			return double.Parse((string)row["expression"]);
		}

		public override string ToString()
        {
            return $"{Order}.{Child.Name} (id: {ChildId}) {Quantity}";
        }
    }
}
