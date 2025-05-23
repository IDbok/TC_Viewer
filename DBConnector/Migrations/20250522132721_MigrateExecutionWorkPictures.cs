using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TcDbConnector.Migrations
{
    /// <inheritdoc />
    public partial class MigrateExecutionWorkPictures : Migration
    {
        /// <inheritdoc />
        
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                // Удаление процедуры, если она уже существует
                migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS Link_ExecutionWork_To_ImageOwner;");

                // Создание процедуры
                migrationBuilder.Sql(@"
CREATE PROCEDURE Link_ExecutionWork_To_ImageOwner()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE ew_id INT;
    DECLARE tech_op_id INT;
    DECLARE tech_card_id INT;
    DECLARE picture_names TEXT;

    DECLARE cur CURSOR FOR 
        SELECT Id, techOperationWorkId, PictureName FROM executionworks WHERE PictureName IS NOT NULL AND PictureName <> '';
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

    CREATE TEMPORARY TABLE IF NOT EXISTS temp_numbers (num INT);

    OPEN cur;

    read_loop: LOOP
        FETCH cur INTO ew_id, tech_op_id, picture_names;
        IF done THEN 
            LEAVE read_loop;
        END IF;

        DELETE FROM temp_numbers;

        SELECT TechnologicalCardId INTO tech_card_id
        FROM techoperationworks
        WHERE Id = tech_op_id;

        SET picture_names = REPLACE(picture_names, '\r', '');
        SET picture_names = REPLACE(picture_names, '\n', ',');
        SET picture_names = REPLACE(picture_names, 'Рис.', '');
        SET picture_names = REPLACE(picture_names, 'рис.', '');
        SET picture_names = REPLACE(picture_names, ' ', '');
        SET picture_names = REPLACE(picture_names, ';', ',');

        SET @source = picture_names;

        WHILE LENGTH(@source) > 0 DO
            SET @entry = SUBSTRING_INDEX(@source, ',', 1);
            SET @source = MID(@source, LENGTH(@entry) + 2);

            IF INSTR(@entry, '-') > 0 THEN
                SET @from = CAST(SUBSTRING_INDEX(@entry, '-', 1) AS UNSIGNED);
                SET @to = CAST(SUBSTRING_INDEX(@entry, '-', -1) AS UNSIGNED);
                WHILE @from <= @to DO
                    INSERT IGNORE INTO temp_numbers (num) VALUES (@from);
                    SET @from = @from + 1;
                END WHILE;
            ELSE
                INSERT IGNORE INTO temp_numbers (num) VALUES (CAST(@entry AS UNSIGNED));
            END IF;
        END WHILE;

        INSERT IGNORE INTO executionworkimageowner (ExecutionWorksId, ImageListId)
        SELECT ew_id, io.Id
        FROM imageowners io
        JOIN temp_numbers tn ON io.Number = tn.num
        WHERE io.TechnologicalCardId = tech_card_id;

    END LOOP;

    CLOSE cur;
    DROP TEMPORARY TABLE IF EXISTS temp_numbers;
END;
");

                // Вызов процедуры
                migrationBuilder.Sql("CALL Link_ExecutionWork_To_ImageOwner();");
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
                // Удаление связей
                migrationBuilder.Sql(@"
DELETE ewi
FROM executionworkimageowner ewi
JOIN executionworks ew ON ew.Id = ewi.ExecutionWorksId
WHERE ew.PictureName IS NOT NULL AND ew.PictureName <> '';
");

                // Удаление процедуры
                migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Link_ExecutionWork_To_ImageOwner;");
            }
        
    }
}

