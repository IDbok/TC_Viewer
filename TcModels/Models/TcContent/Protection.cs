using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent
{
    public class Protection : IModelStructure, IClassifaerable, IDGViewable, IUpdatableEntity//4. Требования к средствам защиты
    {

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
                { nameof(ClassifierCode), "Код в classifier" },
            };
        
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
        public List<string> GetPropertiesRequired { get; } = new List<string>
            {
                { nameof(Name)},
                { nameof(Unit) },
                { nameof(ClassifierCode) },
            };
        

        static private EModelType modelType = EModelType.Protection;
        public EModelType ModelType { get { return modelType; } }

        public List<TechnologicalCard> TechnologicalCards { get; set; } = new();
        public List<Protection_TC> Protection_TCs { get; set; } = new();
        
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
            if (source is Component sourceObject)
            {
                Name = sourceObject.Name;
                Type = sourceObject.Type;
                Unit = sourceObject.Unit;
                Price = sourceObject.Price;
                Description = sourceObject.Description;
                Manufacturer = sourceObject.Manufacturer;
                CompareLinks(sourceObject.Links);
                ClassifierCode = sourceObject.ClassifierCode;
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
    }
}
