using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TcModels.Models.IntermediateTables
{
    public class Component_TC : IStructIntermediateTable<TechnologicalCard, Component>, IDGViewable
    {
        public static Dictionary<string, string> GetPropertiesNames { get; } = new Dictionary<string, string>
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
        public static List<string> GetPropertiesRequired { get; } = new List<string>
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
        public string? Note { get; set; }
        public override string ToString()
        {
            return $"{Order}.{Child.Name} (id: {ChildId}) {Quantity}";
        }
    }
}
