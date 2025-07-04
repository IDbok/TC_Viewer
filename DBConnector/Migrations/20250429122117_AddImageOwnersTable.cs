using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    public partial class AddImageOwnersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "ImageStorage",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "ImageStorage",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImageOwners",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ImageStorageId = table.Column<long>(type: "bigint", nullable: false),
                    TechnologicalCardId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false, defaultValue: "Без имени")
                        .Annotation("MySql:CharSet", "utf8mb4")
                        .Annotation("MySql:DefaultValue", "Без имени"),
                    Number = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageOwners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageOwners_ImageStorage_ImageStorageId",
                        column: x => x.ImageStorageId,
                        principalTable: "ImageStorage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageOwners_TechnologicalCards_TechnologicalCardId",
                        column: x => x.TechnologicalCardId,
                        principalTable: "TechnologicalCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("ALTER TABLE ImageOwners ALTER COLUMN Name SET DEFAULT 'Без имени';");

            migrationBuilder.CreateTable(
                name: "DiagramShagImageOwner",
                columns: table => new
                {
                    DiagramShagsId = table.Column<int>(type: "int", nullable: false),
                    ImageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagramShagImageOwner", x => new { x.DiagramShagsId, x.ImageListId });
                    table.ForeignKey(
                        name: "FK_DiagramShagImageOwner_DiagramShag_DiagramShagsId",
                        column: x => x.DiagramShagsId,
                        principalTable: "DiagramShag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiagramShagImageOwner_ImageOwners_ImageListId",
                        column: x => x.ImageListId,
                        principalTable: "ImageOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExecutionWorkImageOwner",
                columns: table => new
                {
                    ExecutionWorksId = table.Column<int>(type: "int", nullable: false),
                    ImageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionWorkImageOwner", x => new { x.ExecutionWorksId, x.ImageListId });
                    table.ForeignKey(
                        name: "FK_ExecutionWorkImageOwner_ExecutionWorks_ExecutionWorksId",
                        column: x => x.ExecutionWorksId,
                        principalTable: "ExecutionWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExecutionWorkImageOwner_ImageOwners_ImageListId",
                        column: x => x.ImageListId,
                        principalTable: "ImageOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramShagImageOwner_ImageListId",
                table: "DiagramShagImageOwner",
                column: "ImageListId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionWorkImageOwner_ImageListId",
                table: "ExecutionWorkImageOwner",
                column: "ImageListId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageOwners_ImageStorageId",
                table: "ImageOwners",
                column: "ImageStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageOwners_TechnologicalCardId",
                table: "ImageOwners",
                column: "TechnologicalCardId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiagramShagImageOwner");

            migrationBuilder.DropTable(
                name: "ExecutionWorkImageOwner");

            migrationBuilder.DropTable(
                name: "ImageOwners");

            migrationBuilder.AlterColumn<int>(
               name: "Category",
               table: "ImageStorage",
               type: "int",
               nullable: false,
               oldClrType: typeof(string),
               oldType: "longtext")
               .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "ImageStorage");
        }
    }
}
