using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;

namespace TcModels.Models.TcContent
{
    public class Protection : IModelStructure//4. Требования к средствам защиты
    {
        static private EModelType modelType = EModelType.Protection;
        public EModelType ModelType { get { return modelType; } }

        public List<TechnologicalCard> TechnologicalCards { get; set; } = new();
        public List<Protection_TC> Protection_TCs { get; set; } = new();
        
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; }
        public string Unit { get; set; }
        public float? Price { get; set; }
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public List<LinkEntety> Links { get; set; } = new();
        public string ClassifierCode { get; set; }
    }
}
