namespace TC_WinForms.DataProcessing.Helpers;

using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TC_WinForms.WinForms;

public static class TempFileCleaner
{
	private static readonly ILogger _logger = Log.Logger;

	private static readonly List<string> TempFiles = new List<string>();
    private static readonly string TempDirectory = Path.Combine(Path.GetTempPath(), "TC_Viewer");// Path.GetTempPath();

    static TempFileCleaner()
    {
        // Подписываемся на событие завершения приложения
        Application.ApplicationExit += OnApplicationExit;
        AppDomain.CurrentDomain.ProcessExit += (s, e) => CleanUpTempFiles();

		CleanUpAllFilesInexecutionSchemesFolder();
	}

    public static string CreateTempFile(long imageId, byte[] fileData)
    {
        string tempFilePath = GetTempFilePath(imageId);

        // Сохраняем данные во временный файл
        File.WriteAllBytes(tempFilePath, fileData);

		// Добавляем файл в список для последующей очистки
		AddTempFile(tempFilePath);

        return tempFilePath;
    }

    private static void AddTempFile(string tempFilePath)
	{
		// проверяем наличие в списке временных файлов
		if (!TempFiles.Contains(tempFilePath))
		{
			// Добавляем файл в список для последующей очистки
			TempFiles.Add(tempFilePath);
		}
	}

	public static string GetTempFilePath(long imageId)
    {
        // Определяем путь до директории ExecutionSchemes
        string executionSchemesPath = Path.Combine(TempDirectory, "ExecutionSchemes");

        // Проверяем и создаем папку, если её нет
        if (!Directory.Exists(executionSchemesPath))
        {
            Directory.CreateDirectory(executionSchemesPath);
        }

        // Создаем уникальное имя файла на основе идентификатора изображения
        return Path.Combine(executionSchemesPath, $"{imageId}.tmp");
    }

    private static void OnApplicationExit(object sender, EventArgs e)
    {
        CleanUpTempFiles();
    }

    private static void CleanUpAllFilesInexecutionSchemesFolder()
	{
        try{
            _logger.Information("Очистка временных файлов в папке ExecutionSchemes");

			string executionSchemesPath = Path.Combine(TempDirectory, "ExecutionSchemes");

			if (Directory.Exists(executionSchemesPath))
			{
				Directory.Delete(executionSchemesPath, true);
			}

		}
		catch (Exception ex)
		{
			_logger.Error($"Не удалось удалить временные файлы в папке ExecutionSchemes: {ex.Message}");
		}

		
	}

	public static void CleanUpTempFiles()
    {
        foreach (var tempFilePath in TempFiles)
        {
            try
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, если файл не удалось удалить
                Console.WriteLine($"Не удалось удалить временный файл {tempFilePath}: {ex.Message}");
            }
        }

        // Очищаем список временных файлов
        TempFiles.Clear();
    }

    public static void CleanUpTempFiles( string filePath )
    {
        try
        {
            // проверяем налиик в во временных файлах
            if (TempFiles.Contains(filePath))
            {
                TempFiles.Remove(filePath);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

        }
        catch (Exception ex)
        {
            // Логируем ошибку, если файл не удалось удалить
            Console.WriteLine($"Не удалось удалить временный файл {filePath}: {ex.Message}");
        }
    }
}

