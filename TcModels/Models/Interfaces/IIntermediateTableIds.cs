
namespace TcModels.Models.Interfaces
{
    public interface IIntermediateTableIds : IOrderable
	{
        public int ParentId { get; set; }
        public int ChildId { get; set; }
    }
}
