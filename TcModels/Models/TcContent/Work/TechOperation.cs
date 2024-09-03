
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent
{
    public class TechOperation: IIdentifiable, IUpdatableEntity, IReleasable
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
    }
}
