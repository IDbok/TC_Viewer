
using System.ComponentModel.DataAnnotations.Schema;
using TcModels.Models.Interfaces;

namespace TcModels.Models
{
    public class TechnologicalProcess: INameable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public List<Author> Authors { get; set; } = new();
        public string? Description { get; set; }
        public string Version { get; set; } //= "0.0.0.1";
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public List<TechnologicalCard> TechnologicalCards { get; set; } = new ();

        public override string ToString()
        {
            return $"{Id}.{Name} {Type}";
        }
    }
}
