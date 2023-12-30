using TcModels.Models.IntermediateTables;

namespace TcModels.Models.TcContent
{
    public class Tool : IModelStructure //5. Требования к инструментам и приспособлениям
    {
        static private EModelType modelType = EModelType.Tool;
        public EModelType ModelType { get { return modelType; } }

        //public List<Tool> Parents = new();
        //public List<Tool> Childrens = new();

        //public List<Instrument_kit<Tool>> Kit = new();

        public List<TechnologicalCard> TechnologicalCards { get; set; } = new();
        public List<Tool_TC> Tool_TCs { get; set; } = new();
        
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; }
        public string Unit { get; set; }
        public float? Price { get; set; }
    }
}
