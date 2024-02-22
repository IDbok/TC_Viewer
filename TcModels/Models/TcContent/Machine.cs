using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent
{
    public class Machine : IModelStructure, IClassifaerable, IDGViewable, IUpdatableEntity  //3. Требования к механизмам
    {
        static EModelType modelType = EModelType.Machine;
        public EModelType ModelType { get => modelType; }

        public static Dictionary<string, string> GetPropertiesNames()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "ID" },
                { nameof(Name), "Наименование" },
                { nameof(Type), "Тип" },
                { nameof(Unit), "Ед.изм." },
                { nameof(Price), "Стоимость, руб. без НДС" },
                { nameof(Description), "Описание" },
                { nameof(Manufacturer), "Производители (поставщики)" },
                //{ nameof(Links), "Ссылки" }, // todo - fix problem with Links (load it from DB to DGV)
                { nameof(ClassifierCode), "Код в classifier" },
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
                { nameof(Unit), 3 },
                { nameof(Price), 4 },
                { nameof(Description), 5 },
                { nameof(Manufacturer), 6 },
                //{ nameof(Links), 7 },
                { nameof(ClassifierCode), 7 },

            };
        }
        public static List<string> GetPropertiesRequired()
        {
            return new List<string>
            {
                { nameof(Name)},
                { nameof(Unit) },
                { nameof(ClassifierCode) },
            };
        }

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

        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is Machine sourceObject)
            {
                Name = sourceObject.Name;
                Type = sourceObject.Type;
                Unit = sourceObject.Unit;
                Price = sourceObject.Price;
                Description = sourceObject.Description;
                Manufacturer = sourceObject.Manufacturer;
                Links = sourceObject.Links;
                ClassifierCode = sourceObject.ClassifierCode;
            }
        }
    }
}
