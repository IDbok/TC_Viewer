
namespace TcModels.Models.Interfaces;

public interface IVersionable
{
    int OriginalId { get; set; }
    int Version { get; set; }
    DateTime UpdateDate { get; set; }
}
