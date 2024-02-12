using TcModels.Models.IntermediateTables;

namespace TcModels.Models.Interfaces
{
    public interface IModelStructure : INameable
    {
        public static EModelType ModelType { get; }
        static string ModelTypeName { get; }

        string Name { get; set; }
        string Type { get; set; }
        string Unit { get; set; }
        float? Price { get; set; }
        List<LinkEntety> Links { get; set; }

    }
}
