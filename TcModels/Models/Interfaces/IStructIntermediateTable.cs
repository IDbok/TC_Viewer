namespace TcModels.Models.Interfaces
{
	public interface IStructIntermediateTable<P, C> : IIntermediateTable<P, C>, IOrderable
    {
        public double Quantity { get; set; }
        public string? Note { get; set; }

    }
}
