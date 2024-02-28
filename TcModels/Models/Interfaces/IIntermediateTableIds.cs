
namespace TcModels.Models.Interfaces
{
    public interface IIntermediateTableIds
    {
        public int ParentId { get; set; }
        public int ChildId { get; set; }
    }
}
