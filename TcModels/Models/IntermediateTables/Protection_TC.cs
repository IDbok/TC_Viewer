using TcModels.Models.TcContent;

namespace TcModels.Models.IntermediateTables
{
    public class Protection_TC : IStructIntermediateTable<TechnologicalCard,Protection>
    {
        public int ChildId { get; set; }
        public Protection Child { get; set; }

        public int ParentId { get; set; }
        public TechnologicalCard Parent { get; set; }

        public int Order { get; set; }
        public int Quantity { get; set; }

        public override string ToString()
        {
            return $"{Order}.{Child.Name} (id: {ChildId}) {Quantity}";
        }
    }
}
