using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    /// <inheritdoc />
    public partial class IdToIntermadiateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			//migrationBuilder.AddColumn<Guid>(
			//    name: "UniqueField",
			//    table: "Machine_TC",
			//    type: "char(36)",
			//    nullable: false,
			//    defaultValueSql: "UUID()",
			//    collation: "ascii_general_ci");

			//migrationBuilder.CreateIndex(
			//    name: "IX_Machine_TC_UniqueField",
			//    table: "Machine_TC",
			//    column: "UniqueField",
			//    unique: true);

			// Добавить столбец UniqueField
			migrationBuilder.AddColumn<string>(
				name: "UniqueField",
				table: "Machine_TC",
				type: "char(36)",
				nullable: true); // Сначала делаем поле nullable

			migrationBuilder.AddColumn<string>(
				name: "UniqueField",
				table: "Staff_TC",
				type: "char(36)",
				nullable: true); // Сначала делаем поле nullable

			// Добавить триггер для заполнения UUID
			migrationBuilder.Sql(@"
					CREATE TRIGGER SetMachineTCUuid BEFORE INSERT ON Machine_TC
					FOR EACH ROW
					BEGIN
						IF NEW.UniqueField IS NULL THEN
							SET NEW.UniqueField = UUID();
						END IF;
					END;
				");

			migrationBuilder.Sql(@"
					CREATE TRIGGER SetStaffTCUuid BEFORE INSERT ON Staff_TC
					FOR EACH ROW
					BEGIN
						IF NEW.UniqueField IS NULL THEN
							SET NEW.UniqueField = UUID();
						END IF;
					END;
				");

			// Установить существующие значения UUID для старых данных
			migrationBuilder.Sql("UPDATE Machine_TC SET UniqueField = UUID() WHERE UniqueField IS NULL;");
			migrationBuilder.Sql("UPDATE Staff_TC SET UniqueField = UUID() WHERE UniqueField IS NULL;");

			// Сделать столбец NOT NULL
			migrationBuilder.AlterColumn<string>(
				name: "UniqueField",
				table: "Machine_TC",
				type: "char(36)",
				nullable: false); 
			
			migrationBuilder.AlterColumn<string>(
				name: "UniqueField",
				table: "Staff_TC",
				type: "char(36)",
				nullable: false);

			// Создать уникальный индекс
			migrationBuilder.CreateIndex(
				name: "IX_Machine_TC_UniqueField",
				table: "Machine_TC",
				column: "UniqueField",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Staff_TC_UniqueField",
				table: "Staff_TC",
				column: "UniqueField",
				unique: true);
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_Machine_TC_UniqueField",
            //    table: "Machine_TC");

            //migrationBuilder.DropColumn(
            //    name: "UniqueField",
            //    table: "Machine_TC");

			// Удалить индекс
			migrationBuilder.DropIndex(
				name: "IX_Machine_TC_UniqueField",
				table: "Machine_TC");

			migrationBuilder.DropIndex(
				name: "IX_Staff_TC_UniqueField",
				table: "Staff_TC");

			// Удалить столбец
			migrationBuilder.DropColumn(
				name: "UniqueField",
				table: "Machine_TC");

			migrationBuilder.DropColumn(
				name: "UniqueField",
				table: "Staff_TC");

			// Удалить триггер
			migrationBuilder.Sql("DROP TRIGGER IF EXISTS SetMachineTCUuid;");

			migrationBuilder.Sql("DROP TRIGGER IF EXISTS SetStaffTCUuid;");
		}
    }
}
