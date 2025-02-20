using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    /// <inheritdoc />
    public partial class MachineStaffOutlayColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInOutlay",
                table: "Staff_TC",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInOutlay",
                table: "Machine_TC",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInOutlay",
                table: "Staff_TC");

            migrationBuilder.DropColumn(
                name: "IsInOutlay",
                table: "Machine_TC");
        }
    }
}
