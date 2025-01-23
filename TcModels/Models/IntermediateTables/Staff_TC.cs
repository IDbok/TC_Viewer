using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.IntermediateTables
{
	public class Staff_TC : IIntermediateTable<TechnologicalCard,Staff>, IDGViewable, IUpdatableEntity
    {
        public static List<string> GetChangeablePropertiesNames { get; } = new List<string>
        {
            nameof(Order),
            nameof(Symbol),
        };
        public Dictionary<string, string> GetPropertiesNames { get; } = new Dictionary<string, string>
        {
                { nameof(ChildId), "ID персонал" },
                { nameof(Child), "" },
                { nameof(ParentId), "ID тех. карты" },
                { nameof(Parent), "" },
                { nameof(Order), "№" },
                { nameof(Symbol), "Символ" },
        };
        
        public static Dictionary<string, int> GetPropertiesOrder { get; } = new Dictionary<string, int>
        {
                { nameof(ChildId), -1 },
                { nameof(Child), -1 },
                { nameof(ParentId), -1},
                { nameof(Parent), -1 },

                { nameof(Order), 0 },
                { nameof(Symbol), 1 },
        };
        public List<string> GetPropertiesRequired { get; } = new List<string>
        {
            { nameof(ChildId) },
            { nameof(ParentId) },
            { nameof(Order) },
            { nameof(Symbol)},
        };

        
        public int IdAuto { get; set; }  // TODO: change to long

        public int ChildId { get; set; }
        public Staff? Child { get; set; }

        public int ParentId { get; set; }
        public TechnologicalCard? Parent { get; set; }

        public int Order { get; set; }
        public bool IsInOutlayCount { get; set; }

        public string Symbol { get; set; }

        public List<ExecutionWork> ExecutionWorks { get; set; } = new List<ExecutionWork>(); 
        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is Staff_TC sourceCard)
            {
                Symbol = sourceCard.Symbol;
                Order = sourceCard.Order;
                IsInOutlayCount = sourceCard.IsInOutlayCount;
            }
        }
        public override string ToString()
        {
            return $"{Order}.{Child?.Name} (id: {ChildId}) {Symbol}";
        }
    }
}