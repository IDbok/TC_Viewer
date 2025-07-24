using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    /// <inheritdoc />
    public partial class AddRemarkStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Vopros",
                table: "ExecutionWorks",
                newName: "Remark");

            migrationBuilder.RenameColumn(
                name: "Otvet",
                table: "ExecutionWorks",
                newName: "Reply");

            migrationBuilder.RenameColumn(
                name: "LeadComment",
                table: "DiagramShag",
                newName: "Remark");

            migrationBuilder.RenameColumn(
                name: "ImplementerComment",
                table: "DiagramShag",
                newName: "Reply");

            migrationBuilder.AddColumn<bool>(
                name: "IsRemarkClosed",
                table: "ToolWorks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemarkClosed",
                table: "ExecutionWorks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemarkClosed",
                table: "DiagramShag",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemarkClosed",
                table: "ComponentWorks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemarkClosed",
                table: "ToolWorks");

            migrationBuilder.DropColumn(
                name: "IsRemarkClosed",
                table: "ExecutionWorks");

            migrationBuilder.DropColumn(
                name: "IsRemarkClosed",
                table: "DiagramShag");

            migrationBuilder.DropColumn(
                name: "IsRemarkClosed",
                table: "ComponentWorks");

            migrationBuilder.RenameColumn(
                name: "Remark",
                table: "ExecutionWorks",
                newName: "Vopros");

            migrationBuilder.RenameColumn(
                name: "Reply",
                table: "ExecutionWorks",
                newName: "Otvet");

            migrationBuilder.RenameColumn(
                name: "Remark",
                table: "DiagramShag",
                newName: "LeadComment");

            migrationBuilder.RenameColumn(
                name: "Reply",
                table: "DiagramShag",
                newName: "ImplementerComment");
        }
    }
}
