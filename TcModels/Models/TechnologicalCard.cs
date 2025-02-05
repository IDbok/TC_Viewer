using AutoMapper;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using TcModels.Models.Helpers;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models
{
	public class TechnologicalCard: INameable, IUpdatableEntity, IValidatable, IHasUniqueConstraints<TechnologicalCard>
    {
        public enum TechnologicalCardStatus
        {
            [Description("Не заполнена")] // ТК создана, но не заполнена
            Created,

            [Description("Проект")] //ТК заполнена и готова к проверке руководителя
            Draft,

            [Description("Работа над замечаниями")] // ТК просмотрена руководителем, но требует доработки
            Remarked,

            [Description("Выпущена")] //ТК проверена и утверждена руководителем
            Approved,

            [Description("Недействительная")] //ТК помечена как недействительная
            Rejected,

            [Description("Заполнена")] //ТК заполнена (таблицы 1-6) и готова к проверке руководителя
            Completed
        }

        public static Dictionary<string, string> GetPropertiesNames()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "ID" },
                { nameof(Article), "Артикул" },
                { nameof(Version), "Версия" },
                { nameof(Name), "Название" },
                { nameof(Type), "Тип карты" },
                { nameof(NetworkVoltage), "Сеть, кВ" },
                { nameof(TechnologicalProcessType), "Тип тех. процесса" },
                { nameof(TechnologicalProcessName), "Тех. процесс" },
                { nameof(Parameter), "Параметр" },
                { nameof(FinalProduct), "Конечный продукт" },
                { nameof(Applicability), "Применимость техкарты" },
                { nameof(Note), "Примечания" },
                { nameof(IsCompleted), "Наличие" }
            };
        }
        public static Dictionary<string, int> GetPropertiesOrder()
        {
            int i = 0;
            return new Dictionary<string, int>
            {
                { nameof(Id), 11 },
                { nameof(Name), -1 },

                { nameof(Article), 0 },
                { nameof(Type), 1 },
                { nameof(NetworkVoltage), 2 },
                { nameof(TechnologicalProcessType), 3 },
                { nameof(TechnologicalProcessName), 4 },
                { nameof(Parameter), 5 },
                { nameof(FinalProduct), 6 },
                { nameof(Applicability), 7 },
                { nameof(Note), 8 },
                { nameof(IsCompleted), 9 },
                { nameof(Version), 10 },

                { nameof(TechnologicalProcessNumber), -1 },
                { nameof(Description), -1 },
                { nameof(DamageType), -1 },
                { nameof(RepairType), -1},
                { nameof(Data), -1 },

            };
        }


        public int Id { get; set; }
        public string Article { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string Version { get; set; } = "0.0.0.0";

        public string Type { get; set; } // Тип карты
        public float NetworkVoltage { get; set; } // Сеть, кВ
        public string? TechnologicalProcessType { get; set; } // Тип тех. процесса
        public string? TechnologicalProcessName { get; set; } // Технологический процесс
        public string? TechnologicalProcessNumber { get; set; } // Номер тех. процесса
        public string? Parameter { get; set; } // Параметр
        public string? FinalProduct { get; set; } // Конечный продукт (КП)
        public string? Applicability { get; set; } // Применимость техкарты
        public string? Note { get; set; } // Примечания
        public string? DamageType { get; set; } // Тип повреждения
        public string?  RepairType { get; set; } // Тип ремонта
        public bool IsCompleted { get; set; } // Наличие ТК
        public bool IsDynamic { get; set; } // Динамическая ТК
        [NotMapped]
        [Obsolete]
		public string? ExecutionSchemeBase64 { get; set; }

        public long? ExecutionSchemeImageId { get; set; }
        public ImageStorage? ExecutionSchemeImage { get; set; }

        public TechnologicalCardStatus Status { get; set; } = TechnologicalCardStatus.Created;


        public List<Author> Authors { get; set; } = new();
        public List<TechnologicalProcess> TechnologicalProcess { get; set; } = new();
        
        [NotMapped]
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        
        public List<Staff> Staffs { get; set; } = new ();
        public List<Staff_TC> Staff_TCs { get; set; } = new();
        public List<TcContent.Component> Components { get; set; } = new();
        public List<Component_TC> Component_TCs { get; set; } = new();
        public List<Machine> Machines { get; set; } = new();
        public List<Machine_TC> Machine_TCs { get; set; } = new();
        public List<Protection> Protections { get; set; } = new();
        public List<Protection_TC> Protection_TCs { get; set; } = new();
        public List<Tool> Tools { get; set; } = new();
        public List<Tool_TC> Tool_TCs { get; set; } = new();

        public List<TechOperationWork> TechOperationWorks { get; set; } = new();

        public List<DiagamToWork> DiagamToWork { get; set; } = new();

		public List<Coefficient> Coefficients { get; set; } = new();



		//public int? WorkStepsId { get; set; }
		//public List<WorkStep> WorkSteps { get; set; } = new();

		public Staff_TC ConnectObject(Staff staff, int order, string symbol)
        {
            Staff_TC staff_tc = new Staff_TC { Parent = this, Child = staff, Order = order, Symbol = symbol };
            Staff_TCs.Add(new Staff_TC { Parent = this, Child = staff, Order = order, Symbol = symbol });
            Staffs.Add(staff);
            return staff_tc;
        }
        /// <summary>
        /// Add object to TC and connect it with TC
        /// </summary>
        /// <typeparam name="T">Intermediate class (Tool_TC, Machine_TC etc)</typeparam>
        /// <typeparam name="C">Child class (Toll, Machine etc)</typeparam>
        /// <param name="obj">Child class object</param>
        /// <param name="order"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public T ConnectObject<T, C>(C obj, int order, int quantity)
            where T : class, IStructIntermediateTable<TechnologicalCard, C>, new()
            where C : class, IModelStructure
        {
            T obj_tc= new T { Parent = this, Child = obj, Order = order, Quantity = quantity };

            if (typeof(T) == typeof(Component_TC))
            {
                Component_TCs.Add(obj_tc as Component_TC);
                Components.Add(obj as TcContent.Component);
            }
            else if (typeof(T) == typeof(Tool_TC))
            {
                Tool_TCs.Add(obj_tc as Tool_TC);
                Tools.Add(obj as Tool);
            }
            else if (typeof(T) == typeof(Machine_TC))
            {
                Machine_TCs.Add(obj_tc as Machine_TC);
                Machines.Add(obj as Machine);
            }
            else if (typeof(T) == typeof(Protection_TC))
            {
                Protection_TCs.Add(obj_tc as Protection_TC);
                Protections.Add(obj as Protection);
            }

            //else if (typeof(T) == typeof(WorkStep_TC)) // todo - add WorkStep_TC
            //{
            //    WorkStep_TCs.Add(new WorkStep_TC { Parent = this, Child = obj, Order = order, Quantity = quantity });
            //    WorkSteps.Add(obj as WorkStep);
            //}
            return obj_tc;
        }
        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is TechnologicalCard sourceCard)
            {
                Article = sourceCard.Article;
                Version = sourceCard.Version;
                Description = sourceCard.Description;
                Name = sourceCard.Name;
                Type = sourceCard.Type;
                NetworkVoltage = sourceCard.NetworkVoltage;
                TechnologicalProcessType = sourceCard.TechnologicalProcessType;
                TechnologicalProcessName = sourceCard.TechnologicalProcessName;
                Parameter = sourceCard.Parameter;
                FinalProduct = sourceCard.FinalProduct;
                Applicability = sourceCard.Applicability;
                Note = sourceCard.Note;
                IsCompleted = sourceCard.IsCompleted;
                TechnologicalProcessNumber = sourceCard.TechnologicalProcessNumber;
                DamageType = sourceCard.DamageType;
                RepairType = sourceCard.RepairType;
                Data = sourceCard.Data;
                TechnologicalProcess = sourceCard.TechnologicalProcess;
                Status = sourceCard.Status;

                ExecutionSchemeImageId = sourceCard.ExecutionSchemeImageId;
            }
        }
        public override string ToString()
        {
            return $"{Id}.{Article} {Name}";
        }

        private TechnologicalCard CopyFrom(TechnologicalCard sourceCard)
        {
            // Создаем новый объект TechnologicalCard и копируем простые свойства
            TechnologicalCard newCard = new TechnologicalCard
            {
                Article = sourceCard.Article,
                Name = sourceCard.Name,
                Description = sourceCard.Description,
                Version = sourceCard.Version,
                Type = sourceCard.Type,
                NetworkVoltage = sourceCard.NetworkVoltage,
                TechnologicalProcessType = sourceCard.TechnologicalProcessType,
                TechnologicalProcessName = sourceCard.TechnologicalProcessName,
                TechnologicalProcessNumber = sourceCard.TechnologicalProcessNumber,
                Parameter = sourceCard.Parameter,
                FinalProduct = sourceCard.FinalProduct,
                Applicability = sourceCard.Applicability,
                Note = sourceCard.Note,
                DamageType = sourceCard.DamageType,
                RepairType = sourceCard.RepairType,
                IsCompleted = sourceCard.IsCompleted,
                ExecutionSchemeBase64 = sourceCard.ExecutionSchemeBase64,
                ExecutionSchemeImageId = sourceCard.ExecutionSchemeImageId,
            };

            // Копируем промежуточные таблицы
            newCard.Staff_TCs = sourceCard.Staff_TCs.Select(stc =>
                new Staff_TC
                { Parent = newCard, Child = stc.Child, Order = stc.Order, Symbol = stc.Symbol }).ToList();
            newCard.Component_TCs = sourceCard.Component_TCs.Select(ctc =>
                new Component_TC
                { Parent = newCard, Child = ctc.Child, Order = ctc.Order, Quantity = ctc.Quantity, Note = ctc.Note }).ToList();
            newCard.Machine_TCs = sourceCard.Machine_TCs.Select(mtc =>
                new Machine_TC
                { Parent = newCard, Child = mtc.Child, Order = mtc.Order, Quantity = mtc.Quantity, Note = mtc.Note }).ToList();
            newCard.Protection_TCs = sourceCard.Protection_TCs.Select(ptc =>
                new Protection_TC
                { Parent = newCard, Child = ptc.Child, Order = ptc.Order, Quantity = ptc.Quantity, Note = ptc.Note }).ToList();
            newCard.Tool_TCs = sourceCard.Tool_TCs.Select(ttc =>
                new Tool_TC
                { Parent = newCard, Child = ttc.Child, Order = ttc.Order, Quantity = ttc.Quantity, Note = ttc.Note }).ToList();

            // Копируем списки связанных объектов
            newCard.TechOperationWorks = sourceCard.TechOperationWorks.Select(tow => 
                new TechOperationWork (newCard, tow.techOperation, tow.Order, tow.GetParallelIndexString())
                //{ 
                //    technologicalCard = newCard,
                //    techOperation = tow.techOperation,

                    
                //    ParallelIndex = tow.GetParallelIndexString(),
                //    Order = tow.Order,

                //}
                ).ToList();

            foreach(var tow in newCard.TechOperationWorks)
            {
                tow.ToolWorks = sourceCard.TechOperationWorks.SelectMany(tow => tow.ToolWorks).Select(tw => 
                    new ToolWork 
                    { 
                        techOperationWork = tow,
                        tool = tw.tool,
                        Quantity = tw.Quantity,
                        Comments = tw.Comments
                    }).ToList();

                tow.ComponentWorks = sourceCard.TechOperationWorks.SelectMany(tow => tow.ComponentWorks).Select(cw => 
                    new ComponentWork 
                    { 
                        techOperationWork = tow,
                        component = cw.component,
                        Quantity = cw.Quantity,
                        Comments = cw.Comments
                    }).ToList();

                tow.executionWorks = sourceCard.TechOperationWorks.SelectMany(tow => tow.executionWorks).Select(ew => 
                    new ExecutionWork
                    {
                        Order = ew.Order,

                        techOperationWork = tow,
                        techTransition = ew.techTransition,

                        Repeat = ew.Repeat,

                        Coefficient = ew.Coefficient,
                        Value = ew.Value,

                        Comments = ew.Comments,

                        Etap = ew.Etap,
                        Posled = ew.Posled,

                        Vopros = ew.Vopros,
                        Otvet = ew.Otvet,

                        PictureName = ew.PictureName

                    }).ToList();
            }

            newCard.DiagamToWork = sourceCard.DiagamToWork.Select(dtow => new DiagamToWork { /* Копирование свойств диаграмм */ }).ToList();

            

            // Возвращаем новый экземпляр TechnologicalCard с копиями всех связанных объектов
            return newCard;
        }

        public string[] GetRequiredProperties()
        {
            return new[] { 

                nameof(Article), 
                nameof(Type), 
                nameof(NetworkVoltage) 

            };
        }

        public IEnumerable<UniqueConstraint<TechnologicalCard>> GetUniqueConstraints()
        {
            // Возвращаем условия уникальности
            yield return new UniqueConstraint<TechnologicalCard>(
                x => x.Article == this.Article,
                "Поле 'Артикул' должно быть уникальным."
            );

            //yield return new UniqueConstraint<TechnologicalCard>(
            //    x => x.Type == this.Type,
            //    "Поле 'Тип' должно быть уникальным."
            //);
        }

        public TechnologicalCard DeepCopyTC(TechnologicalCard sourceCard)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OpenProfile(0));
            });
            var newCard = new TechnologicalCard();

            var mapper = config.CreateMapper();

            newCard = mapper.Map<TechnologicalCard>(sourceCard);

            return newCard;
        }
    }
}
