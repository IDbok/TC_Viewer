using System.Reflection.Metadata;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;

namespace TcModels.Models.TcContent
{
    public class Staff : INameable //1. Требования к составу бригады и квалификации
    {
        public static Dictionary<string, string> GetPropertiesNames()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "ID" },
                { nameof(Name), "Название" },
                { nameof(Type), "Тип" },
                { nameof(Functions), "Функции" },
                { nameof(CombineResponsibility), "Возможность совмещения обязанностей" },
                { nameof(Qualification), "Квалификация" },
                { nameof(Comment), "Комментарии" },
            };
        }
        public static Dictionary<string, int> GetPropertiesOrder()
        {
            int i = 0;
            return new Dictionary<string, int>
            {
                { nameof(Id), 0 },
                { nameof(Name), 1 },
                { nameof(Type), 2 },
                { nameof(Functions), 3 },
                { nameof(CombineResponsibility), 4 },
                { nameof(Qualification), 5 },
                { nameof(Comment), 6 },

            };
        }
        public static List<string> GetPropertiesRequired()
        {
            return new List<string>
            {
                { nameof(Name) },
                { nameof(Type) },
                { nameof(Functions) },
                { nameof(Qualification)},
            };
        }

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
