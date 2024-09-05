using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent
{
    public class Component : IModelStructure, IClassifaerable, IDGViewable, IUpdatableEntity, ICategoryable, ILinkable, IReleasable //2. Требования к материалам и комплектующим
    {
        static EModelType modelType = EModelType.Component;
        public static EModelType ModelType { get => modelType; }

        public Dictionary<string, string> GetPropertiesNames { get; } = new Dictionary<string, string>
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
        
        public List<string> GetPropertiesRequired { get; } = new List<string>
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
        public bool IsReleased { get; set ; } = false;
        public int? CreatedTCId {  get; set; } = null;

        public byte[]? Image { get; set; }

        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is Component sourceObject)
            {
                Name = sourceObject.Name;
                Type = sourceObject.Type;
                Unit = sourceObject.Unit;
                Price = sourceObject.Price;
                Description = sourceObject.Description;
                Manufacturer = sourceObject.Manufacturer;
                Categoty = sourceObject.Categoty;
                CompareLinks(sourceObject.Links);
                ClassifierCode = sourceObject.ClassifierCode;

                IsReleased = sourceObject.IsReleased;
                CreatedTCId = sourceObject.CreatedTCId;
            }
        }

        private void CompareLinks(List<LinkEntety> sourceLinks)
        {
            var linksToRemove = new List<LinkEntety>();
            foreach (var link in Links)
            {
                if (!sourceLinks.Contains(link))
                {
                    linksToRemove.Add(link);
                }
            }

            foreach (var link in linksToRemove)
            {
                Links.Remove(link);
            }

            foreach (var link in sourceLinks)
            {
                if (!Links.Contains(link))
                {
                    Links.Add(link);
                }
                else
                {
                    // обновить поля ссылки
                    Links.Find(l => l.Id == link.Id)!.ApplyUpdates(link);
                }
            }
        }

        public override string ToString()
        {
            return $"{Id}. {Name} {Type} {Price}/{Unit}";
        }

    }
}
