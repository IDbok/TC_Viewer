
using TcModels.Models.Interfaces;

namespace TcModels.Models
{
    public class Author: INameable
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int AccessLevel { get; set; }
        public List<TechnologicalProcess> TechnologicalProcesses { get; set; } = new();
        public List<TechnologicalCard> TechnologicalCards { get; set; } = new();

        public override string ToString()
        {
            return $"{Name} {Surname}";
        }

    }
}
