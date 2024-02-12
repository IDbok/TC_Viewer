
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
        public static Dictionary<string, string> GetPropertiesNames_old(out Dictionary<string, string> ownNames)
        {
            var childNames = Staff.GetPropertiesNames();
            var names = new Dictionary<string, string> { };

            ownNames = new Dictionary<string, string>
            {
                { nameof(ChildId), "Символ" },
                { nameof(Child), "Символ" },
                { nameof(ParentId), "ID тех. карты" },
                { nameof(Parent), "Символ" },
                { nameof(Order), "№" },
                { nameof(Symbol), "Символ" },
            };

            
            // add own names
            foreach (var ownName in ownNames)
            {
                names.Add(ownName.Key, ownName.Value);
            }

            foreach ( var childName in childNames)
            {
                names.Add(childName.Key, childName.Value);
            }

            return names;
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

        public static Dictionary<string, int> GetPropertiesOrder_old(out Dictionary<string, int> ownOrder)
        {
            var childOrder = Staff.GetPropertiesOrder();
            var order = new Dictionary<string, int>();
            ownOrder = new Dictionary<string, int>
            {
                { nameof(ChildId), -1 },
                { nameof(Child), -1 },
                { nameof(ParentId), -1},
                { nameof(Parent), -1 },
                { nameof(Order), 0 },

                { nameof(Symbol), 1 },
            };

            foreach (var owns in ownOrder)
            {
                order.Add(owns.Key, owns.Value);
            }

            int indexAdd = order.Where(x => x.Value != -1).ToList().Count();

            foreach (var child in childOrder)
            {
                order.Add(child.Key, child.Value + indexAdd);
            }

            return order;
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
