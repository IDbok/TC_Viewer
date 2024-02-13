using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;

namespace TcModels.Models.TcContent
{
    public class Component : IModelStructure, IClassifaerable, IDGViewable //2. Требования к материалам и комплектующим
    {
        static EModelType modelType = EModelType.Component;
        public static EModelType ModelType { get => modelType; }

        public static Dictionary<string, string> GetPropertiesNames { get; } = new Dictionary<string, string>
            {
                { nameof(Id), "ID" },
                { nameof(Name), "Наименование" },
                { nameof(Type), "Тип" },
                { nameof(Unit), "Ед.изм." },
                { nameof(Price), "Стоимость, руб. без НДС" },
                { nameof(Description), "Описание" },
                { nameof(Manufacturer), "Производители (поставщики)" },
                //{ nameof(Links), "Ссылки" }, // todo - fix problem with Links (load it from DB to DGV)
                { nameof(Categoty), "Категория" },
                { nameof(ClassifierCode), "Код в classifier" },
            };
        
        public static Dictionary<string, int> GetPropertiesOrder { get; } = new Dictionary<string, int>
            {
                { nameof(Id), 0 },
                { nameof(Name), 1 },
                { nameof(Type), 2 },
                { nameof(Unit), 3 },
                { nameof(Price), 4 },
                { nameof(Description), 5 },
                { nameof(Manufacturer), 6 },
                //{ nameof(Links), 7 },
                { nameof(Categoty), 7 },
                { nameof(ClassifierCode), 8 },

            };
        
        public static List<string> GetPropertiesRequired { get; } = new List<string>
            {
                { nameof(Name)},
                { nameof(Unit) },
                { nameof(Categoty) },
                { nameof(ClassifierCode) },
            };
        

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
        public string Categoty { get; set; } = "StandComp";
        public string ClassifierCode { get; set; }

        public override string ToString()
        {
            return $"{Id}. {Name} {Type} {Price}/{Unit}";
        }

    }
}
