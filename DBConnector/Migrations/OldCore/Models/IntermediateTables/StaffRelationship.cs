using TcDbConnector.Migrations.OldCore.Models.TcContent;

namespace TcDbConnector.Migrations.OldCore.Models.IntermediateTables;

public class StaffRelationship
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public Staff Staff { get; set; }

    public int RelatedStaffId { get; set; }
    public Staff RelatedStaff { get; set; }
}
