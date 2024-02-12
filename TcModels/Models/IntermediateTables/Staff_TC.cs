
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TcModels.Models.IntermediateTables
{
    public class Staff_TC : IIntermediateTable<TechnologicalCard,Staff>
    {
        public static List<string> GetChangeablePropertiesNames()
        {
            return new List<string>
            {
                nameof(Order),
                nameof(Symbol),
            };
        }
        public static Dictionary<string, string> GetPropertiesNames()
        {
            var propNames = new Dictionary<string, string> { 
                { nameof(ChildId), "Символ" },
                { nameof(Child), "Символ" },
                { nameof(ParentId), "ID тех. карты" },
                { nameof(Parent), "Символ" },
                { nameof(Order), "№" },
                { nameof(Symbol), "Символ" },
            };


            return propNames;
        }
        public static Dictionary<string, int> GetPropertiesOrder()
        {
            return new Dictionary<string, int>
            {
                { nameof(ChildId), -1 },
                { nameof(Child), -1 },
                { nameof(ParentId), -1},
                { nameof(Parent), -1 },
                { nameof(Order), 0 },

                { nameof(Symbol), 1 },
            };
        }

        public int ChildId { get; set; }
        public Staff? Child { get; set; }

        public int ParentId { get; set; }
        public TechnologicalCard? Parent { get; set; }

        public int Order { get; set; }
        public string Symbol { get; set; }

        public override string ToString()
        {
            return $"{Order}.{Child.Name} (id: {ChildId}) {Symbol}";
        }
    }
}