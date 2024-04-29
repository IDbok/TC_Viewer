using TcModels.Models.IntermediateTables;

namespace TcModels.Models.Interfaces
{
    public interface IModelStructure : INameable, IClassifaerable
    {
        public static EModelType ModelType { get; }
        static string ModelTypeName { get; }

        string Type { get; set; }
        string Unit { get; set; }
        float? Price { get; set; }
        string? Manufacturer { get; set; }
        List<LinkEntety> Links { get; set; }
        string? Description { get; set; }

        public bool IsReleased { get; set; }
        public int? CreatedTCId { get; set; }

    }
}
