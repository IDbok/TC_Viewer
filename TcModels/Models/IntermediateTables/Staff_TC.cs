
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TcModels.Models.IntermediateTables
{
    public class Staff_TC : IIntermediateTable<TechnologicalCard,Staff>
    {
        public int ChildId { get; set; }
        public Staff Child { get; set; }

        public int ParentId { get; set; }
        public TechnologicalCard Parent { get; set; }

        public int Order { get; set; }
        public string Symbol { get; set; }

        public override string ToString()
        {
            return $"{Order}.{Child.Name} (id: {ChildId}) {Symbol}";
        }
    }
}
