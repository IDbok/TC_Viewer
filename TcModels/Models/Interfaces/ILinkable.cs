using TcModels.Models.IntermediateTables;

namespace TcModels.Models.Interfaces
{
	public interface ILinkable
    {
        List<LinkEntety> Links { get; set; }
    }
}
