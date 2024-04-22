
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent
{
    public class TechOperation: IIdentifiable, IUpdatableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string? Category { get; set; }

        public List<TechOperationWork> techOperationWorks { get; set; } = new List<TechOperationWork> { };

        public List<TechTransitionTypical> techTransitionTypicals { get; set; } = new List<TechTransitionTypical> { };

        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is TechOperation sourceCard)
            {
                Name = sourceCard.Name;
                Category = sourceCard.Category;
            }
        }
    }
}
