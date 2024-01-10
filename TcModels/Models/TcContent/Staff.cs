using TcModels.Models.IntermediateTables;

namespace TcModels.Models.TcContent
{
    public class Staff : IIdentifiable//IModelStructure //1. Требования к составу бригады и квалификации
    {
        static private EModelType modelType = EModelType.Staff;
        public EModelType ModelType { get { return modelType; } }

        public List<TechnologicalCard> TechnologicalCards { get; set; } = new();
        public List<Staff_TC> Staff_TCs { get; set; } = new();
        public Staff()
        {

        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Functions { get; set; }
        public string? CombineResponsibility { get; set; }

        //public string? ElSaftyGroup { get; set; }
        //public string? Grade { get; set; }
        public string Qualification { get; set; }
        public string? Comment { get; set; }

        public string ToSring()
        {
            return $"{Id} {Name} {Type} {CombineResponsibility} {Qualification}" +
                $"\n{Comment}";
        }
    }
}
