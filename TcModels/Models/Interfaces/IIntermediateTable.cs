namespace TcModels.Models.Interfaces
{
    public interface IIntermediateTable<P, C>: IIntermediateTableIds
    {
        public P? Parent { get; set; }
        public C? Child { get; set; }
    }
}
