namespace TcModels.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using TcModels.Models.TcContent;

public class ImageStorage
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public ImageStorageType StorageType { get; set; } = ImageStorageType.Base64;
    public string Category { get; set; } = "TechnologicalCard";

    // Свойство для хранения изображения в формате Base64
    public string? ImageBase64 { get; set; }

    // Свойство для хранения пути к файлу изображения
    public string? MimeType { get; set; } = ""; // MIME тип изображения "image/png"
    public string? FilePath { get; set; }

    // Метод для сохранения изображения в файл
    [NotMapped]
    public bool IsChanged = false;
    public void SaveImageToFile(string filePath)
    {
        if (string.IsNullOrEmpty(ImageBase64))
        {
            throw new InvalidOperationException("Base64Image is null or empty.");
        }

        byte[] imageBytes = Convert.FromBase64String(ImageBase64);
        File.WriteAllBytes(filePath, imageBytes);
        FilePath = filePath;
    }

    // Метод для загрузки изображения из файла и конвертации его в Base64
    public void LoadImageFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found.", filePath);
        }

        byte[] imageBytes = File.ReadAllBytes(filePath);
        ImageBase64 = Convert.ToBase64String(imageBytes);
        FilePath = filePath;
    }

    // Метод для удаления файла изображения
    public void DeleteImageFile()
    {
        if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
        {
            throw new InvalidOperationException("File path is invalid or file does not exist.");
        }

        File.Delete(FilePath);
        FilePath = null;
    }

    // Метод для очистки Base64 данных
    public void ClearBase64Image()
    {
        ImageBase64 = "";
    }
}

public enum ImageStorageType
{
    Base64,
    FilePath
}

[Obsolete($"Enum усторел")]
public enum ImageCategory
{
    ExecutionScheme,
    Shag,
    Other = 100
}

