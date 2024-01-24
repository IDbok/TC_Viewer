
using TcModels.Models.Interfaces;

namespace TcModels.Models
{
    public class Author: INameable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public int AccessLevel { get; set; }
        public List<TechnologicalProcess> TechnologicalProcesses { get; set; } = new();
        public List<TechnologicalCard> TechnologicalCards { get; set; } = new();

        public override string ToString()
        {
            return $"{Name} {Surname}";
        }

    }
}
