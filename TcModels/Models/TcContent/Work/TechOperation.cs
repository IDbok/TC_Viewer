
using System.Text.Json;
using TcModels.Models.Helpers;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent
{
    public class TechOperation: IIdentifiable, IUpdatableEntity, IReleasable
        , IValidatable, IHasUniqueConstraints<TechOperation>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public string? Category { get; set; }

        public List<TechOperationWork> techOperationWorks { get; set; } = new List<TechOperationWork> { };

        public List<TechTransitionTypical> techTransitionTypicals { get; set; } = new List<TechTransitionTypical> { };

        public bool IsReleased { get; set; } = false;
        public int? CreatedTCId { get; set; } = null;

        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is TechOperation sourceObject)
            {
                Name = sourceObject.Name;
                Category = sourceObject.Category;

                IsReleased = sourceObject.IsReleased;
                CreatedTCId = sourceObject.CreatedTCId;
            }
        }
        public string[] GetRequiredProperties()
        {
            return new[] {

                nameof(Name),
            };
        }

        public IEnumerable<UniqueConstraint<TechOperation>> GetUniqueConstraints()
        {
            // Возвращаем условия уникальности
            yield return new UniqueConstraint<TechOperation>(
                x => x.Name == this.Name,
                "Поле 'Наименование' должно быть уникальным."
            );

            // todo: уточнить, на сколько важена проверка уникальных технологических преходов в типовой ТО

            //if(!string.IsNullOrEmpty(Category) && Category == "Типовая ТО")
            //{
            //    var techTransitionKey = GetTechTransitionTypicalsKey();


            //    yield return new UniqueConstraint<TechOperation>(
            //    x => JsonSerializer.Serialize(x.techTransitionTypicals) == techTransitionKey,
            //    "Набор технологических преходов типовой ТО должен быть уникальным."
            //    );
            //}
        }
        //// Метод для создания строкового представления списка techTransitionTypicals
        //private string GetTechTransitionTypicalsKey()
        //{
        //    // Используем JSON-сериализацию для создания представления
        //    return JsonSerializer.Serialize(techTransitionTypicals);
        //}
    }
}
