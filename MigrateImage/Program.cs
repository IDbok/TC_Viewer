using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using TC_WinForms;
using TcDbConnector;
using TcModels;
using TcModels.Models;

namespace MigrateImageData
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting image data migration...");
                MigrateImageData();
                Console.WriteLine("Migration completed successfully!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during migration: \n{e.Message}\n\nStack Trace:\n{e.StackTrace}");
            }
            finally
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        static void MigrateImageData()
        {
            //var connectionString = "server=10.1.100.142;database=tcvdb_debugtest;user=tavrida;password=tavrida$555";
            var connectionString = "server=localhost; database = tavrida_db_main; user = root; password = root";
            TcDbConnector.StaticClass.ConnectString = connectionString;

            using (var context = new MyDbContext())
            {
                var cards = context.TechnologicalCards.Select(x => x.Id).ToList();

                Console.WriteLine($"Found {cards.Count} technological cards to process");

                int totalProcessed = 0;
                int batchSize = 50; // Размер пакета для сохранения

                foreach (var card in cards)
                {
                    var tc = context.TechnologicalCards.Where(x => x.Id == card)
                        .Include(c => c.DiagamToWork)
                    .ThenInclude(d => d.ListDiagramParalelno)
                    .ThenInclude(p => p.ListDiagramPosledov)
                    .ThenInclude(s => s.ListDiagramShag)
                    .FirstOrDefault();

                    try
                    {
                        var steps = tc.DiagamToWork?
                            .SelectMany(d => d.ListDiagramParalelno)
                            .SelectMany(p => p.ListDiagramPosledov)
                            .SelectMany(s => s.ListDiagramShag)
                            .Where(s => !string.IsNullOrEmpty(s.ImageBase64))
                            .ToList();

                        if (steps == null || steps.Count == 0)
                            continue;

                        int processedInCard = 0;

                        foreach (var shag in steps)
                        {
                            var newImage = new ImageStorage
                            {
                                Name = shag.NameImage,
                                StorageType = ImageStorageType.Base64,
                                ImageBase64 = shag.ImageBase64,
                                MimeType = "image/png",
                            };

                            context.ImageStorage.Add(newImage);
                            context.SaveChanges(); // Сохраняем, чтобы получить ID

                            var newOwner = new ImageOwner
                            {
                                ImageStorageId = newImage.Id,
                                TechnologicalCardId = card,
                                Name = shag.NameImage,
                                Number = shag.Number,
                                Role = ImageRole.Image,
                            };

                            context.ImageOwners.Add(newOwner);
                            shag.ImageList.Add(newOwner);
                            context.Entry(shag).State = EntityState.Modified;
                            processedInCard++;
                            totalProcessed++;

                            // Пакетное сохранение
                            if (totalProcessed % batchSize == 0)
                            {
                                context.SaveChanges();
                                Console.WriteLine($"Processed {totalProcessed} images...");
                            }
                        }

                        // Сохраняем оставшиеся изменения для этой карты
                        if (processedInCard % batchSize != 0)
                        {
                            context.SaveChanges();
                        }

                        Console.WriteLine($"ТК: {tc.Article}; Шагов {steps.Count} - Рисунки сохранены");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing card ID {tc.Id}: {ex.Message}");
                        // Создаем новый контекст после ошибки
                        context.Dispose();
                    }
                }
            }
        }
    }
}
