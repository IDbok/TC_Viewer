namespace TcModels.Models.Interfaces
{
    public interface IIntermediateTable<P, C>
    {
        public int ParentId { get; set; }
        public P? Parent { get; set; }
        public int ChildId { get; set; }
        public C? Child { get; set; }
    }
}
