﻿using TcModels.Models.Helpers;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent
{
    //5. Требования к инструментам и приспособлениям
    public class Tool : IModelStructure, IClassifaerable, IDGViewable, IUpdatableEntity, ICategoryable, ILinkable, IReleasable,
        IValidatable, IHasUniqueConstraints<Tool>
    {
        static private EModelType modelType = EModelType.Tool;
        public EModelType ModelType { get { return modelType; } }

        public Dictionary<string, string> GetPropertiesNames { get; } =  new Dictionary<string, string>
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
        

        public List<TechnologicalCard> TechnologicalCards { get; set; } = new();
        public List<Tool_TC> Tool_TCs { get; set; } = new();
        
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; }
        public string Unit { get; set; }
        public float? Price { get; set; }
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public List<LinkEntety> Links { get; set; } = new ();
        public string Categoty { get; set; } = "Tool";  // todo: исправить название на Category
        public string ClassifierCode { get; set; }

        public bool IsReleased { get; set; } = false;
        public int? CreatedTCId { get; set; } = null;

        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is Tool sourceObject)
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

        public string[] GetRequiredProperties()
        {
            return new[] {

                nameof(Name),
                nameof(Type),
            };
        }

        public IEnumerable<UniqueConstraint<Tool>> GetUniqueConstraints()
        {
            // Возвращаем условия уникальности
            yield return new UniqueConstraint<Tool>(
                x => x.Name == this.Name && x.Type == this.Type,
                "Комбинация полей 'Наименование' и 'Тип' должна быть уникальной."
            );

            if (!string.IsNullOrEmpty(this.ClassifierCode))
            {
                yield return new UniqueConstraint<Tool>(
                    x => x.ClassifierCode == this.ClassifierCode,
                    "Поле 'Код в classifier' должно быть уникальным."
                );
            }

            if (!string.IsNullOrEmpty(this.Description))
            {
                yield return new UniqueConstraint<Tool>(
                    x => x.Description == this.Description,
                    "Поле 'Описание' должно быть уникальным."
                );
            }
        }
    }
}
