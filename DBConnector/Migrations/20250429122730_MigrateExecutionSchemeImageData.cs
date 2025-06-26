using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    /// <inheritdoc />
    public partial class MigrateExecutionSchemeImageData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Вставка записей в ImageOwner
            migrationBuilder.Sql(@"
            INSERT INTO ImageOwners (TechnologicalCardId, ImageStorageId, Role)
            SELECT tc.Id, tc.ExecutionSchemeImageId, 0 -- 0 = ExecutionScheme
            FROM TechnologicalCards tc
            JOIN ImageStorage img ON tc.ExecutionSchemeImageId = img.Id
            WHERE img.Category = '0' OR img.Category = 'ExecutionScheme'
        ");

            // Очистка поля ExecutionSchemeImageId
            migrationBuilder.Sql(@"
            UPDATE TechnologicalCards
            SET ExecutionSchemeImageId = NULL
            WHERE ExecutionSchemeImageId IS NOT NULL
        ");

            // Обновление всех категорий на "TechnologicalCard"
            migrationBuilder.Sql(@"
            UPDATE ImageStorage
            SET Category = 'TechnologicalCard'
            WHERE Category = '0'
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Восстановление ExecutionSchemeImageId
            migrationBuilder.Sql(@"
            UPDATE tc
            SET ExecutionSchemeImageId = io.ImageStorageId
            FROM TechnologicalCards tc
            JOIN ImageOwners io ON tc.Id = io.TechnologicalCardId
            WHERE io.Role = 0 -- ExecutionScheme
        ");

            // Удаление этих ImageOwner
            migrationBuilder.Sql(@"
            DELETE FROM ImageOwners
            WHERE Role = 0
        ");

            // Восстановление старых значений Category
            migrationBuilder.Sql(@"
            UPDATE ImageStorage
            SET Category = 'ExecutionScheme'
            WHERE Category = 'TechnologicalCard'
        ");
        }
    }
}
