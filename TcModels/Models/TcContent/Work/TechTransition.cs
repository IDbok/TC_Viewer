
using System.Reflection.Metadata;
using TcModels.Models.Helpers;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent
{
    public class TechTransition: IIdentifiable, IUpdatableEntity, IReleasable
        ,IValidatable, IHasUniqueConstraints<TechTransition>, IRequiredProperties
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double TimeExecution { get; set; }

        public string? Category { get; set; }
        public bool? TimeExecutionChecked { get; set; } = false;
        public string? CommentName { get; set; }
        public string? CommentTimeExecution { get; set; }
        
        public List<ExecutionWork> ExecutionWorks { get; set; }
        public List<TechTransitionTypical> techTransitionTypicals { get; set; }

        public bool IsReleased { get; set; } = false;
        public int? CreatedTCId { get; set; } = null;
        public List<string> GetPropertiesRequired { get; } = new List<string>
        {
                { nameof(Name) },
                { nameof(Category) },
                { nameof(TimeExecution)}
        };
        public Dictionary<string, string> GetPropertiesNames { get; } = new Dictionary<string, string>
        {
            { nameof(Id), "ID" },
            { nameof(Name), "Название" },
            { nameof(TimeExecution), "Время выполнения" },
            { nameof(Category), "Категория" },
            { nameof(TimeExecutionChecked), "Время выполнения проверено" },
            { nameof(CommentName), "Комментарий к наименованию" },
            { nameof(CommentTimeExecution), "Комментарий к времени выполнения" }
        };
        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is TechTransition sourceObject)
            {
                Name = sourceObject.Name;
                TimeExecution = sourceObject.TimeExecution;
                Category = sourceObject.Category;
                TimeExecutionChecked = sourceObject.TimeExecutionChecked;
                CommentName = sourceObject.CommentName;
                CommentTimeExecution = sourceObject.CommentTimeExecution;
                ExecutionWorks = sourceObject.ExecutionWorks;
                IsReleased = sourceObject.IsReleased;
            }
        }
        public string[] GetRequiredProperties()
        {
            return new[] {

                nameof(Name),
            };
        }

        public IEnumerable<UniqueConstraint<TechTransition>> GetUniqueConstraints()
        {
            // Возвращаем условия уникальности
            yield return new UniqueConstraint<TechTransition>(
                x => x.Name == this.Name,
                "Поле 'Наименование' должно быть уникальным."
            );
        }
    }
}
