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

		public override string ToString()
        {
            return $"{Order}.{Child.Name} (id: {ChildId}) {Quantity}";
        }
    }
}
