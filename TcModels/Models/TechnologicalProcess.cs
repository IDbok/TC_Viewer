
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection.Metadata;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models
{
    public class TechnologicalProcess: INameable, IUpdatableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public List<Author> Authors { get; set; } = new();
        public string? Description { get; set; }
        public string Version { get; set; } = "0.0.0.1";
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public List<TechnologicalCard> TechnologicalCards { get; set; } = new List<TechnologicalCard> { };

        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is TechnologicalProcess sourceObject)
            {
                Name = sourceObject.Name;
                Type = sourceObject.Type;
                Description = sourceObject.Description;
            }
        }

        public override string ToString()
        {
            return $"{Id}.{Name} {Type}";
        }
    }
}
