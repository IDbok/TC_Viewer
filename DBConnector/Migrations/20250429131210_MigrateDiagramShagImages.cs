using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    /// <inheritdoc />
    public partial class MigrateDiagramShagImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 0. Добавление временных колонок
            migrationBuilder.Sql(@"
                ALTER TABLE ImageStorage 
                ADD COLUMN TempDiagramShagId BIGINT NULL,
                ADD COLUMN TempDiagramToWorkId BIGINT NULL;
            ");

            // 1. Вставка уникальных изображений в ImageStorage с временными колонками
            migrationBuilder.Sql(@"
                INSERT INTO ImageStorage (ImageBase64, Name, Category, StorageType, ImageType, TempDiagramShagId, TempDiagramToWorkId)
                SELECT 
                    ds.ImageBase64,
                    MIN(ds.NameImage),
                    'TechnologicalCard',
                    0,
                    'image/png',
                    MIN(ds.Id),
                    MIN(dpr.DiagamToWorkId)
                FROM DiagramShag ds
                JOIN DiagramPosledov dp ON ds.DiagramPosledovId = dp.Id
                JOIN DiagramParalelno dpr ON dp.DiagramParalelnoId = dpr.Id
                WHERE ds.ImageBase64 IS NOT NULL
                  AND ds.ImageBase64 != ''
                  AND dpr.DiagamToWorkId IS NOT NULL
                GROUP BY ds.ImageBase64
                HAVING MIN(ds.ImageBase64) IS NOT NULL;
            ");

            // 2. Вставка записей в ImageOwners
            migrationBuilder.Sql(@"
                INSERT INTO ImageOwners (TechnologicalCardId, ImageStorageId, Name, Number, Role)
                SELECT 
                    dtw.TechnologicalCardId,
                    img.Id,
                    img.Name,
                    ds.`Nomer`,
                    1
                FROM ImageStorage img
                JOIN DiagramShag ds ON img.TempDiagramShagId = ds.Id
                JOIN DiagramPosledov dp ON ds.DiagramPosledovId = dp.Id
                JOIN DiagramParalelno dpr ON dp.DiagramParalelnoId = dpr.Id
                JOIN DiagamToWork dtw ON dpr.DiagamToWorkId = dtw.Id
                WHERE img.TempDiagramShagId IS NOT NULL;
            ");

            // 3. Вставка связей в DiagramShagImageOwner
            migrationBuilder.Sql(@"
                INSERT INTO DiagramShagImageOwner (DiagramShagsId, ImageListId)
                SELECT 
                    img.TempDiagramShagId,
                    io.Id
                FROM ImageStorage img
                JOIN ImageOwners io ON io.ImageStorageId = img.Id
                WHERE img.TempDiagramShagId IS NOT NULL;
            ");

            // 4. Очистка полей ImageBase64 и NameImage в DiagramShag
            migrationBuilder.Sql(@"
                UPDATE DiagramShag
                SET ImageBase64 = '',
                    NameImage = ''
                WHERE ImageBase64 IS NOT NULL AND ImageBase64 != '';
            ");

            // 5. Удаление временных колонок
            migrationBuilder.Sql(@"
                ALTER TABLE ImageStorage 
                DROP COLUMN TempDiagramShagId,
                DROP COLUMN TempDiagramToWorkId;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Восстановление ImageBase64 и NameImage в DiagramShag из ImageStorage
            migrationBuilder.Sql(@"
                UPDATE DiagramShag ds
                JOIN DiagramShagImageOwner dsio ON ds.Id = dsio.DiagramShagsId
                JOIN ImageOwners io ON io.Id = dsio.ImageListId AND io.Role = 1
                JOIN ImageStorage img ON img.Id = io.ImageStorageId
                SET ds.ImageBase64 = img.ImageBase64,
                    ds.NameImage = img.Name;
            ");

            // 2. Удаление связей в DiagramShagImageOwner
            migrationBuilder.Sql(@"
                DELETE dsio
                FROM DiagramShagImageOwner dsio
                JOIN ImageOwners io ON dsio.ImageListId = io.Id
                WHERE io.Role = 1;
            ");

            // 3. Удаление записей из ImageOwners
            migrationBuilder.Sql(@"
                DELETE FROM ImageOwners
                WHERE Role = 1;
            ");

            // 4. Удаление изображений из ImageStorage без связей
            migrationBuilder.Sql(@"
                DELETE img
                FROM ImageStorage img
                LEFT JOIN ImageOwners io ON img.Id = io.ImageStorageId
                WHERE img.Category = 'TechnologicalCard'
                  AND io.Id IS NULL;
            ");
        }
    }
}
