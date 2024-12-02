using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // закомментировано на время первого применения миграции
            //migrationBuilder.AlterDatabase()
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Authors",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Name = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Surname = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Email = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        AccessLevel = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Authors", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Components",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Name = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Type = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Unit = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Price = table.Column<float>(type: "float", nullable: true),
            //        Description = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Manufacturer = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Categoty = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ClassifierCode = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsReleased = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedTCId = table.Column<int>(type: "int", nullable: true),
            //        Image = table.Column<byte[]>(type: "longblob", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Components", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "ImageStorage",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Name = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        StorageType = table.Column<int>(type: "int", nullable: false),
            //        Category = table.Column<int>(type: "int", nullable: false),
            //        ImageBase64 = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        FilePath = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ImageStorage", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Machines",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Name = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Type = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Unit = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Price = table.Column<float>(type: "float", nullable: true),
            //        Description = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Manufacturer = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ClassifierCode = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsReleased = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedTCId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Machines", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Protections",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Name = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Type = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Unit = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Price = table.Column<float>(type: "float", nullable: true),
            //        Description = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Manufacturer = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ClassifierCode = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsReleased = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedTCId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Protections", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Staffs",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Name = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Type = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Functions = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CombineResponsibility = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Qualification = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Comment = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsReleased = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedTCId = table.Column<int>(type: "int", nullable: true),
            //        OriginalId = table.Column<int>(type: "int", nullable: false),
            //        Version = table.Column<int>(type: "int", nullable: false),
            //        UpdateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        ClassifierCode = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Staffs", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "TechnologicalProcesses",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Name = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Type = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Description = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Version = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TechnologicalProcesses", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "TechOperations",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Name = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Category = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsReleased = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedTCId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TechOperations", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "TechTransitions",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Name = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        TimeExecution = table.Column<double>(type: "double", nullable: false),
            //        Category = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        TimeExecutionChecked = table.Column<bool>(type: "tinyint(1)", nullable: true),
            //        CommentName = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CommentTimeExecution = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsReleased = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedTCId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TechTransitions", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Tools",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Name = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Type = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Unit = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Price = table.Column<float>(type: "float", nullable: true),
            //        Description = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Manufacturer = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Categoty = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ClassifierCode = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsReleased = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedTCId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Tools", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Instrument_kit<Component>",
            //    columns: table => new
            //    {
            //        ParentId = table.Column<int>(type: "int", nullable: false),
            //        ChildId = table.Column<int>(type: "int", nullable: false),
            //        Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
            //        Quantity = table.Column<double>(type: "double", nullable: false, defaultValue: 0.0),
            //        Note = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Instrument_kit<Component>", x => new { x.ParentId, x.ChildId });
            //        table.ForeignKey(
            //            name: "FK_Instrument_kit<Component>_Components_ChildId",
            //            column: x => x.ChildId,
            //            principalTable: "Components",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Instrument_kit<Component>_Components_ParentId",
            //            column: x => x.ParentId,
            //            principalTable: "Components",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "TechnologicalCards",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Article = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Name = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Description = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Version = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Type = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        NetworkVoltage = table.Column<float>(type: "float", nullable: false),
            //        TechnologicalProcessType = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        TechnologicalProcessName = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        TechnologicalProcessNumber = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Parameter = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        FinalProduct = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Applicability = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Note = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        DamageType = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        RepairType = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        ExecutionSchemeImageId = table.Column<long>(type: "bigint", nullable: true),
            //        Status = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TechnologicalCards", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_TechnologicalCards_ImageStorage_ExecutionSchemeImageId",
            //            column: x => x.ExecutionSchemeImageId,
            //            principalTable: "ImageStorage",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.SetNull);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "StaffRelationship",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        StaffId = table.Column<int>(type: "int", nullable: false),
            //        RelatedStaffId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_StaffRelationship", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_StaffRelationship_Staffs_RelatedStaffId",
            //            column: x => x.RelatedStaffId,
            //            principalTable: "Staffs",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_StaffRelationship_Staffs_StaffId",
            //            column: x => x.StaffId,
            //            principalTable: "Staffs",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "AuthorTechnologicalProcess",
            //    columns: table => new
            //    {
            //        AuthorsId = table.Column<int>(type: "int", nullable: false),
            //        TechnologicalProcessesId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AuthorTechnologicalProcess", x => new { x.AuthorsId, x.TechnologicalProcessesId });
            //        table.ForeignKey(
            //            name: "FK_AuthorTechnologicalProcess_Authors_AuthorsId",
            //            column: x => x.AuthorsId,
            //            principalTable: "Authors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_AuthorTechnologicalProcess_TechnologicalProcesses_Technologi~",
            //            column: x => x.TechnologicalProcessesId,
            //            principalTable: "TechnologicalProcesses",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "TechTransitionTypicals",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        TechOperationId = table.Column<int>(type: "int", nullable: false),
            //        TechTransitionId = table.Column<int>(type: "int", nullable: false),
            //        Etap = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Posled = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Coefficient = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Comments = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TechTransitionTypicals", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_TechTransitionTypicals_TechOperations_TechOperationId",
            //            column: x => x.TechOperationId,
            //            principalTable: "TechOperations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_TechTransitionTypicals_TechTransitions_TechTransitionId",
            //            column: x => x.TechTransitionId,
            //            principalTable: "TechTransitions",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "LinkEntety",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Link = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Name = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        ComponentId = table.Column<int>(type: "int", nullable: true),
            //        MachineId = table.Column<int>(type: "int", nullable: true),
            //        ProtectionId = table.Column<int>(type: "int", nullable: true),
            //        ToolId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_LinkEntety", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_LinkEntety_Components_ComponentId",
            //            column: x => x.ComponentId,
            //            principalTable: "Components",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_LinkEntety_Machines_MachineId",
            //            column: x => x.MachineId,
            //            principalTable: "Machines",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_LinkEntety_Protections_ProtectionId",
            //            column: x => x.ProtectionId,
            //            principalTable: "Protections",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_LinkEntety_Tools_ToolId",
            //            column: x => x.ToolId,
            //            principalTable: "Tools",
            //            principalColumn: "Id");
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "AuthorTechnologicalCard",
            //    columns: table => new
            //    {
            //        AuthorsId = table.Column<int>(type: "int", nullable: false),
            //        TechnologicalCardsId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AuthorTechnologicalCard", x => new { x.AuthorsId, x.TechnologicalCardsId });
            //        table.ForeignKey(
            //            name: "FK_AuthorTechnologicalCard_Authors_AuthorsId",
            //            column: x => x.AuthorsId,
            //            principalTable: "Authors",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_AuthorTechnologicalCard_TechnologicalCards_TechnologicalCard~",
            //            column: x => x.TechnologicalCardsId,
            //            principalTable: "TechnologicalCards",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Component_TC",
            //    columns: table => new
            //    {
            //        ChildId = table.Column<int>(type: "int", nullable: false),
            //        ParentId = table.Column<int>(type: "int", nullable: false),
            //        Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
            //        Quantity = table.Column<double>(type: "double", nullable: false, defaultValue: 0.0),
            //        Note = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Component_TC", x => new { x.ParentId, x.ChildId });
            //        table.ForeignKey(
            //            name: "FK_Component_TC_Components_ChildId",
            //            column: x => x.ChildId,
            //            principalTable: "Components",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Component_TC_TechnologicalCards_ParentId",
            //            column: x => x.ParentId,
            //            principalTable: "TechnologicalCards",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Machine_TC",
            //    columns: table => new
            //    {
            //        ChildId = table.Column<int>(type: "int", nullable: false),
            //        ParentId = table.Column<int>(type: "int", nullable: false),
            //        Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
            //        Quantity = table.Column<double>(type: "double", nullable: false, defaultValue: 0.0),
            //        Note = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Machine_TC", x => new { x.ParentId, x.ChildId });
            //        table.ForeignKey(
            //            name: "FK_Machine_TC_Machines_ChildId",
            //            column: x => x.ChildId,
            //            principalTable: "Machines",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Machine_TC_TechnologicalCards_ParentId",
            //            column: x => x.ParentId,
            //            principalTable: "TechnologicalCards",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Protection_TC",
            //    columns: table => new
            //    {
            //        ChildId = table.Column<int>(type: "int", nullable: false),
            //        ParentId = table.Column<int>(type: "int", nullable: false),
            //        Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
            //        Quantity = table.Column<double>(type: "double", nullable: false, defaultValue: 0.0),
            //        Note = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Protection_TC", x => new { x.ParentId, x.ChildId });
            //        table.ForeignKey(
            //            name: "FK_Protection_TC_Protections_ChildId",
            //            column: x => x.ChildId,
            //            principalTable: "Protections",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Protection_TC_TechnologicalCards_ParentId",
            //            column: x => x.ParentId,
            //            principalTable: "TechnologicalCards",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Staff_TC",
            //    columns: table => new
            //    {
            //        IdAuto = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        ChildId = table.Column<int>(type: "int", nullable: false),
            //        ParentId = table.Column<int>(type: "int", nullable: false),
            //        Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
            //        Symbol = table.Column<string>(type: "longtext", nullable: false, defaultValue: "-")
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Staff_TC", x => x.IdAuto);
            //        table.ForeignKey(
            //            name: "FK_Staff_TC_Staffs_ChildId",
            //            column: x => x.ChildId,
            //            principalTable: "Staffs",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Staff_TC_TechnologicalCards_ParentId",
            //            column: x => x.ParentId,
            //            principalTable: "TechnologicalCards",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "TechnologicalCardTechnologicalProcess",
            //    columns: table => new
            //    {
            //        TechnologicalCardsId = table.Column<int>(type: "int", nullable: false),
            //        TechnologicalProcessId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TechnologicalCardTechnologicalProcess", x => new { x.TechnologicalCardsId, x.TechnologicalProcessId });
            //        table.ForeignKey(
            //            name: "FK_TechnologicalCardTechnologicalProcess_TechnologicalCards_Tec~",
            //            column: x => x.TechnologicalCardsId,
            //            principalTable: "TechnologicalCards",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_TechnologicalCardTechnologicalProcess_TechnologicalProcesses~",
            //            column: x => x.TechnologicalProcessId,
            //            principalTable: "TechnologicalProcesses",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Tool_TC",
            //    columns: table => new
            //    {
            //        ChildId = table.Column<int>(type: "int", nullable: false),
            //        ParentId = table.Column<int>(type: "int", nullable: false),
            //        Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
            //        Quantity = table.Column<double>(type: "double", nullable: false, defaultValue: 0.0),
            //        Note = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Tool_TC", x => new { x.ParentId, x.ChildId });
            //        table.ForeignKey(
            //            name: "FK_Tool_TC_TechnologicalCards_ParentId",
            //            column: x => x.ParentId,
            //            principalTable: "TechnologicalCards",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Tool_TC_Tools_ChildId",
            //            column: x => x.ChildId,
            //            principalTable: "Tools",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "TechOperationWorks",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        techOperationId = table.Column<int>(type: "int", nullable: false),
            //        TechnologicalCardId = table.Column<int>(type: "int", nullable: false),
            //        ParallelIndex = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Order = table.Column<int>(type: "int", nullable: false),
            //        ComponentTCChildId = table.Column<int>(name: "Component_TCChildId", type: "int", nullable: true),
            //        ComponentTCParentId = table.Column<int>(name: "Component_TCParentId", type: "int", nullable: true),
            //        ToolTCChildId = table.Column<int>(name: "Tool_TCChildId", type: "int", nullable: true),
            //        ToolTCParentId = table.Column<int>(name: "Tool_TCParentId", type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TechOperationWorks", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_TechOperationWorks_Component_TC_Component_TCParentId_Compone~",
            //            columns: x => new { x.ComponentTCParentId, x.ComponentTCChildId },
            //            principalTable: "Component_TC",
            //            principalColumns: new[] { "ParentId", "ChildId" });
            //        table.ForeignKey(
            //            name: "FK_TechOperationWorks_TechOperations_techOperationId",
            //            column: x => x.techOperationId,
            //            principalTable: "TechOperations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_TechOperationWorks_TechnologicalCards_TechnologicalCardId",
            //            column: x => x.TechnologicalCardId,
            //            principalTable: "TechnologicalCards",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_TechOperationWorks_Tool_TC_Tool_TCParentId_Tool_TCChildId",
            //            columns: x => new { x.ToolTCParentId, x.ToolTCChildId },
            //            principalTable: "Tool_TC",
            //            principalColumns: new[] { "ParentId", "ChildId" });
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "ComponentWorks",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        techOperationWorkId = table.Column<int>(type: "int", nullable: false),
            //        componentId = table.Column<int>(type: "int", nullable: false),
            //        Quantity = table.Column<double>(type: "double", nullable: false),
            //        Comments = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ComponentWorks", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_ComponentWorks_Components_componentId",
            //            column: x => x.componentId,
            //            principalTable: "Components",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_ComponentWorks_TechOperationWorks_techOperationWorkId",
            //            column: x => x.techOperationWorkId,
            //            principalTable: "TechOperationWorks",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "DiagamToWork",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        techOperationWorkId = table.Column<int>(type: "int", nullable: false),
            //        technologicalCardId = table.Column<int>(type: "int", nullable: false),
            //        ParallelIndex = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Order = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DiagamToWork", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_DiagamToWork_TechOperationWorks_techOperationWorkId",
            //            column: x => x.techOperationWorkId,
            //            principalTable: "TechOperationWorks",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_DiagamToWork_TechnologicalCards_technologicalCardId",
            //            column: x => x.technologicalCardId,
            //            principalTable: "TechnologicalCards",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "ExecutionWorks",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        techOperationWorkId = table.Column<int>(type: "int", nullable: false),
            //        techTransitionId = table.Column<int>(type: "int", nullable: true),
            //        Repeat = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        sumEw = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
            //        maxEw = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
            //        Coefficient = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Value = table.Column<double>(type: "double", nullable: false),
            //        Comments = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Order = table.Column<int>(type: "int", nullable: false),
            //        Etap = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Posled = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Vopros = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Otvet = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        PictureName = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ExecutionWorks", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_ExecutionWorks_TechOperationWorks_techOperationWorkId",
            //            column: x => x.techOperationWorkId,
            //            principalTable: "TechOperationWorks",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_ExecutionWorks_TechTransitions_techTransitionId",
            //            column: x => x.techTransitionId,
            //            principalTable: "TechTransitions",
            //            principalColumn: "Id");
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "ToolWorks",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        techOperationWorkId = table.Column<int>(type: "int", nullable: false),
            //        toolId = table.Column<int>(type: "int", nullable: false),
            //        Quantity = table.Column<double>(type: "double", nullable: false),
            //        Comments = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ToolWorks", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_ToolWorks_TechOperationWorks_techOperationWorkId",
            //            column: x => x.techOperationWorkId,
            //            principalTable: "TechOperationWorks",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_ToolWorks_Tools_toolId",
            //            column: x => x.toolId,
            //            principalTable: "Tools",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "DiagramParalelno",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        techOperationWorkId = table.Column<int>(type: "int", nullable: false),
            //        DiagamToWorkId = table.Column<int>(type: "int", nullable: false),
            //        Order = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DiagramParalelno", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_DiagramParalelno_DiagamToWork_DiagamToWorkId",
            //            column: x => x.DiagamToWorkId,
            //            principalTable: "DiagamToWork",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_DiagramParalelno_TechOperationWorks_techOperationWorkId",
            //            column: x => x.techOperationWorkId,
            //            principalTable: "TechOperationWorks",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "ExecutionWorkMachine_TC",
            //    columns: table => new
            //    {
            //        ExecutionWorksId = table.Column<int>(type: "int", nullable: false),
            //        MachinesParentId = table.Column<int>(type: "int", nullable: false),
            //        MachinesChildId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ExecutionWorkMachine_TC", x => new { x.ExecutionWorksId, x.MachinesParentId, x.MachinesChildId });
            //        table.ForeignKey(
            //            name: "FK_ExecutionWorkMachine_TC_ExecutionWorks_ExecutionWorksId",
            //            column: x => x.ExecutionWorksId,
            //            principalTable: "ExecutionWorks",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_ExecutionWorkMachine_TC_Machine_TC_MachinesParentId_Machines~",
            //            columns: x => new { x.MachinesParentId, x.MachinesChildId },
            //            principalTable: "Machine_TC",
            //            principalColumns: new[] { "ParentId", "ChildId" },
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "ExecutionWorkProtection_TC",
            //    columns: table => new
            //    {
            //        ExecutionWorksId = table.Column<int>(type: "int", nullable: false),
            //        ProtectionsParentId = table.Column<int>(type: "int", nullable: false),
            //        ProtectionsChildId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ExecutionWorkProtection_TC", x => new { x.ExecutionWorksId, x.ProtectionsParentId, x.ProtectionsChildId });
            //        table.ForeignKey(
            //            name: "FK_ExecutionWorkProtection_TC_ExecutionWorks_ExecutionWorksId",
            //            column: x => x.ExecutionWorksId,
            //            principalTable: "ExecutionWorks",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_ExecutionWorkProtection_TC_Protection_TC_ProtectionsParentId~",
            //            columns: x => new { x.ProtectionsParentId, x.ProtectionsChildId },
            //            principalTable: "Protection_TC",
            //            principalColumns: new[] { "ParentId", "ChildId" },
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "ExecutionWorkRepeats",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        ParentExecutionWorkId = table.Column<int>(type: "int", nullable: false),
            //        ChildExecutionWorkId = table.Column<int>(type: "int", nullable: false),
            //        NewCoefficient = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        NewEtap = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        NewPosled = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ExecutionWorkRepeats", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_ExecutionWorkRepeats_ExecutionWorks_ChildExecutionWorkId",
            //            column: x => x.ChildExecutionWorkId,
            //            principalTable: "ExecutionWorks",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_ExecutionWorkRepeats_ExecutionWorks_ParentExecutionWorkId",
            //            column: x => x.ParentExecutionWorkId,
            //            principalTable: "ExecutionWorks",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "ExecutionWorkStaff_TC",
            //    columns: table => new
            //    {
            //        ExecutionWorksId = table.Column<int>(type: "int", nullable: false),
            //        StaffsIdAuto = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ExecutionWorkStaff_TC", x => new { x.ExecutionWorksId, x.StaffsIdAuto });
            //        table.ForeignKey(
            //            name: "FK_ExecutionWorkStaff_TC_ExecutionWorks_ExecutionWorksId",
            //            column: x => x.ExecutionWorksId,
            //            principalTable: "ExecutionWorks",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_ExecutionWorkStaff_TC_Staff_TC_StaffsIdAuto",
            //            column: x => x.StaffsIdAuto,
            //            principalTable: "Staff_TC",
            //            principalColumn: "IdAuto",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "DiagramPosledov",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        DiagramParalelnoId = table.Column<int>(type: "int", nullable: false),
            //        Order = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DiagramPosledov", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_DiagramPosledov_DiagramParalelno_DiagramParalelnoId",
            //            column: x => x.DiagramParalelnoId,
            //            principalTable: "DiagramParalelno",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "DiagramShag",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Deystavie = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ImageBase64 = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        NameImage = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Nomer = table.Column<int>(type: "int", nullable: false),
            //        DiagramPosledovId = table.Column<int>(type: "int", nullable: false),
            //        Order = table.Column<int>(type: "int", nullable: false),
            //        LeadComment = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ImplementerComment = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DiagramShag", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_DiagramShag_DiagramPosledov_DiagramPosledovId",
            //            column: x => x.DiagramPosledovId,
            //            principalTable: "DiagramPosledov",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "DiagramShagToolsComponent",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        toolWorkId = table.Column<int>(type: "int", nullable: true),
            //        componentWorkId = table.Column<int>(type: "int", nullable: true),
            //        Quantity = table.Column<double>(type: "double", nullable: false),
            //        DiagramShagId = table.Column<int>(type: "int", nullable: false),
            //        Comment = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DiagramShagToolsComponent", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_DiagramShagToolsComponent_ComponentWorks_componentWorkId",
            //            column: x => x.componentWorkId,
            //            principalTable: "ComponentWorks",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_DiagramShagToolsComponent_DiagramShag_DiagramShagId",
            //            column: x => x.DiagramShagId,
            //            principalTable: "DiagramShag",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_DiagramShagToolsComponent_ToolWorks_toolWorkId",
            //            column: x => x.toolWorkId,
            //            principalTable: "ToolWorks",
            //            principalColumn: "Id");
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AuthorTechnologicalCard_TechnologicalCardsId",
            //    table: "AuthorTechnologicalCard",
            //    column: "TechnologicalCardsId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AuthorTechnologicalProcess_TechnologicalProcessesId",
            //    table: "AuthorTechnologicalProcess",
            //    column: "TechnologicalProcessesId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Component_TC_ChildId",
            //    table: "Component_TC",
            //    column: "ChildId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ComponentWorks_componentId",
            //    table: "ComponentWorks",
            //    column: "componentId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ComponentWorks_techOperationWorkId",
            //    table: "ComponentWorks",
            //    column: "techOperationWorkId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DiagamToWork_technologicalCardId",
            //    table: "DiagamToWork",
            //    column: "technologicalCardId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DiagamToWork_techOperationWorkId",
            //    table: "DiagamToWork",
            //    column: "techOperationWorkId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DiagramParalelno_DiagamToWorkId",
            //    table: "DiagramParalelno",
            //    column: "DiagamToWorkId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DiagramParalelno_techOperationWorkId",
            //    table: "DiagramParalelno",
            //    column: "techOperationWorkId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DiagramPosledov_DiagramParalelnoId",
            //    table: "DiagramPosledov",
            //    column: "DiagramParalelnoId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DiagramShag_DiagramPosledovId",
            //    table: "DiagramShag",
            //    column: "DiagramPosledovId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DiagramShagToolsComponent_componentWorkId",
            //    table: "DiagramShagToolsComponent",
            //    column: "componentWorkId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DiagramShagToolsComponent_DiagramShagId",
            //    table: "DiagramShagToolsComponent",
            //    column: "DiagramShagId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DiagramShagToolsComponent_toolWorkId",
            //    table: "DiagramShagToolsComponent",
            //    column: "toolWorkId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ExecutionWorkMachine_TC_MachinesParentId_MachinesChildId",
            //    table: "ExecutionWorkMachine_TC",
            //    columns: new[] { "MachinesParentId", "MachinesChildId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_ExecutionWorkProtection_TC_ProtectionsParentId_ProtectionsCh~",
            //    table: "ExecutionWorkProtection_TC",
            //    columns: new[] { "ProtectionsParentId", "ProtectionsChildId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_ExecutionWorkRepeats_ChildExecutionWorkId",
            //    table: "ExecutionWorkRepeats",
            //    column: "ChildExecutionWorkId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ExecutionWorkRepeats_ParentExecutionWorkId",
            //    table: "ExecutionWorkRepeats",
            //    column: "ParentExecutionWorkId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ExecutionWorks_techOperationWorkId",
            //    table: "ExecutionWorks",
            //    column: "techOperationWorkId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ExecutionWorks_techTransitionId",
            //    table: "ExecutionWorks",
            //    column: "techTransitionId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ExecutionWorkStaff_TC_StaffsIdAuto",
            //    table: "ExecutionWorkStaff_TC",
            //    column: "StaffsIdAuto");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Instrument_kit<Component>_ChildId",
            //    table: "Instrument_kit<Component>",
            //    column: "ChildId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_LinkEntety_ComponentId",
            //    table: "LinkEntety",
            //    column: "ComponentId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_LinkEntety_MachineId",
            //    table: "LinkEntety",
            //    column: "MachineId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_LinkEntety_ProtectionId",
            //    table: "LinkEntety",
            //    column: "ProtectionId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_LinkEntety_ToolId",
            //    table: "LinkEntety",
            //    column: "ToolId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Machine_TC_ChildId",
            //    table: "Machine_TC",
            //    column: "ChildId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Protection_TC_ChildId",
            //    table: "Protection_TC",
            //    column: "ChildId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Staff_TC_ChildId",
            //    table: "Staff_TC",
            //    column: "ChildId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Staff_TC_ParentId",
            //    table: "Staff_TC",
            //    column: "ParentId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_StaffRelationship_RelatedStaffId",
            //    table: "StaffRelationship",
            //    column: "RelatedStaffId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_StaffRelationship_StaffId",
            //    table: "StaffRelationship",
            //    column: "StaffId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_TechnologicalCards_ExecutionSchemeImageId",
            //    table: "TechnologicalCards",
            //    column: "ExecutionSchemeImageId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_TechnologicalCardTechnologicalProcess_TechnologicalProcessId",
            //    table: "TechnologicalCardTechnologicalProcess",
            //    column: "TechnologicalProcessId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_TechOperationWorks_Component_TCParentId_Component_TCChildId",
            //    table: "TechOperationWorks",
            //    columns: new[] { "Component_TCParentId", "Component_TCChildId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_TechOperationWorks_TechnologicalCardId",
            //    table: "TechOperationWorks",
            //    column: "TechnologicalCardId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_TechOperationWorks_techOperationId",
            //    table: "TechOperationWorks",
            //    column: "techOperationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_TechOperationWorks_Tool_TCParentId_Tool_TCChildId",
            //    table: "TechOperationWorks",
            //    columns: new[] { "Tool_TCParentId", "Tool_TCChildId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_TechTransitionTypicals_TechOperationId",
            //    table: "TechTransitionTypicals",
            //    column: "TechOperationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_TechTransitionTypicals_TechTransitionId",
            //    table: "TechTransitionTypicals",
            //    column: "TechTransitionId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Tool_TC_ChildId",
            //    table: "Tool_TC",
            //    column: "ChildId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ToolWorks_techOperationWorkId",
            //    table: "ToolWorks",
            //    column: "techOperationWorkId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ToolWorks_toolId",
            //    table: "ToolWorks",
            //    column: "toolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorTechnologicalCard");

            migrationBuilder.DropTable(
                name: "AuthorTechnologicalProcess");

            migrationBuilder.DropTable(
                name: "DiagramShagToolsComponent");

            migrationBuilder.DropTable(
                name: "ExecutionWorkMachine_TC");

            migrationBuilder.DropTable(
                name: "ExecutionWorkProtection_TC");

            migrationBuilder.DropTable(
                name: "ExecutionWorkRepeats");

            migrationBuilder.DropTable(
                name: "ExecutionWorkStaff_TC");

            migrationBuilder.DropTable(
                name: "Instrument_kit<Component>");

            migrationBuilder.DropTable(
                name: "LinkEntety");

            migrationBuilder.DropTable(
                name: "StaffRelationship");

            migrationBuilder.DropTable(
                name: "TechnologicalCardTechnologicalProcess");

            migrationBuilder.DropTable(
                name: "TechTransitionTypicals");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "ComponentWorks");

            migrationBuilder.DropTable(
                name: "DiagramShag");

            migrationBuilder.DropTable(
                name: "ToolWorks");

            migrationBuilder.DropTable(
                name: "Machine_TC");

            migrationBuilder.DropTable(
                name: "Protection_TC");

            migrationBuilder.DropTable(
                name: "ExecutionWorks");

            migrationBuilder.DropTable(
                name: "Staff_TC");

            migrationBuilder.DropTable(
                name: "TechnologicalProcesses");

            migrationBuilder.DropTable(
                name: "DiagramPosledov");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Protections");

            migrationBuilder.DropTable(
                name: "TechTransitions");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "DiagramParalelno");

            migrationBuilder.DropTable(
                name: "DiagamToWork");

            migrationBuilder.DropTable(
                name: "TechOperationWorks");

            migrationBuilder.DropTable(
                name: "Component_TC");

            migrationBuilder.DropTable(
                name: "TechOperations");

            migrationBuilder.DropTable(
                name: "Tool_TC");

            migrationBuilder.DropTable(
                name: "Components");

            migrationBuilder.DropTable(
                name: "TechnologicalCards");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropTable(
                name: "ImageStorage");
        }
    }
}
