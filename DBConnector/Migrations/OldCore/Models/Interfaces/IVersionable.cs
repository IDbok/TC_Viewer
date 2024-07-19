
namespace TcDbConnector.Migrations.OldCore.Models.Interfaces;

public interface IVersionable
{
    int OriginalId { get; set; }
    int Version { get; set; }
    DateTime UpdateDate { get; set; }
}
