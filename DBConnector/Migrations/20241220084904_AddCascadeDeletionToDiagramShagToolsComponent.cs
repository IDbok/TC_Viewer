using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeletionToDiagramShagToolsComponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagramShagToolsComponent_ComponentWorks_componentWorkId",
                table: "DiagramShagToolsComponent");

            migrationBuilder.DropForeignKey(
                name: "FK_DiagramShagToolsComponent_ToolWorks_toolWorkId",
                table: "DiagramShagToolsComponent");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagramShagToolsComponent_ComponentWorks_componentWorkId",
                table: "DiagramShagToolsComponent",
                column: "componentWorkId",
                principalTable: "ComponentWorks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiagramShagToolsComponent_ToolWorks_toolWorkId",
                table: "DiagramShagToolsComponent",
                column: "toolWorkId",
                principalTable: "ToolWorks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagramShagToolsComponent_ComponentWorks_componentWorkId",
                table: "DiagramShagToolsComponent");

            migrationBuilder.DropForeignKey(
                name: "FK_DiagramShagToolsComponent_ToolWorks_toolWorkId",
                table: "DiagramShagToolsComponent");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagramShagToolsComponent_ComponentWorks_componentWorkId",
                table: "DiagramShagToolsComponent",
                column: "componentWorkId",
                principalTable: "ComponentWorks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagramShagToolsComponent_ToolWorks_toolWorkId",
                table: "DiagramShagToolsComponent",
                column: "toolWorkId",
                principalTable: "ToolWorks",
                principalColumn: "Id");
        }
    }
}
