
namespace TcModels.Models.IntermediateTables
{
    public class Instrument_kit <T> : IStructIntermediateTable <T,T>
        where T : IModelStructure
    {
        public int ParentId { get; set; }
        public T Parent { get; set; }

        public int ChildId { get; set; }
        public T Child { get; set; }

        // public EModelType ModelType { get; set; } = T.ModelType;

        public int Order { get; set; }
        public int Quantity { get; set; } // todo - make it double

        public override string ToString()
        {
            return $"-  {Child.Name} (id: {ChildId})";
        }
    }
}
