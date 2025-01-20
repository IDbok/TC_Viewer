using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    /// <inheritdoc />
    public partial class OutlayCountMachineColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OutlayCount",
                table: "Machine_TC",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutlayCount",
                table: "Machine_TC");
        }
    }
}
