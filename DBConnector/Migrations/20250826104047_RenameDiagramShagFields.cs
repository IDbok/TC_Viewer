using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    /// <inheritdoc />
    public partial class RenameDiagramShagFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nomer",
                table: "DiagramShag",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "Deystavie",
                table: "DiagramShag",
                newName: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Number",
                table: "DiagramShag",
                newName: "Nomer");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "DiagramShag",
                newName: "Deystavie");
        }
    }
}
