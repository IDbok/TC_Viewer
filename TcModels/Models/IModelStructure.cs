
using TcModels.Models.TcContent;

namespace TcModels.Models
{
    public interface IModelStructure : IIdentifiable
    {
        public static EModelType ModelType { get; }
        static string ModelTypeName { get; }

        string Name { get; set; }
        string Type { get; set; }
        string Unit { get; set; }
        float? Price { get; set; }
    }
}
