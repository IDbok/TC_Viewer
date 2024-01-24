using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using TcModels.Models.IntermediateTables;

namespace TcModels.Models.TcContent
{
    public class Component : IModelStructure //2. Требования к материалам и комплектующим
    {
        static EModelType modelType = EModelType.Component;
        public static EModelType ModelType { get => modelType; }


        public List<Component> Parents { get; set; } = new();
        public List<Component> Children { get; set; } = new();
        public List<Instrument_kit<Component>> Kit { get; set; } = new();

        public List<TechnologicalCard> TechnologicalCards { get; set; } = new();
        public List<Component_TC> Component_TCs { get; set; } = new();

        public int Id  { get; set; }

        public string Name { get; set; }
        public string? Type { get; set; }
        public string Unit { get; set; }
        public float? Price { get; set; }
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public List<LinkEntety> Links { get; set; } = new();
        public string Categoty { get; set; }
        public string ClassifierCode { get; set; }

        public override string ToString()
        {
            return $"{Id}. {Name} {Type} {Price}/{Unit}";
        }

    }
}
