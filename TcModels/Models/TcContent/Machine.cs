using TcModels.Models.IntermediateTables;

namespace TcModels.Models.TcContent
{
    public class Machine : IModelStructure  //3. Требования к механизмам
    {
        static EModelType modelType = EModelType.Machine;
        public EModelType ModelType { get => modelType; }
        public List<TechnologicalCard> TechnologicalCards { get; set; } = new();
        public List<Machine_TC> Machine_TCs { get; set; } = new();

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
