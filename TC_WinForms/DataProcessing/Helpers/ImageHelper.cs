using System.IO;

namespace TC_WinForms.DataProcessing.Helpers;

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class ImageHelper
{
    public static void SaveImageToTempFile(string base64String, long imageId)
    {
        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);

            // Создаем временный файл и сохраняем в него изображение
            string tempFilePath = TempFileCleaner.CreateTempFile(imageId, imageBytes);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении изображения во временный файл: {ex.Message}");
        }
    }

    public static string? LoadImageFromTempFileAsBase64(long imageId)
    {
        try
        {
            string tempFilePath = TempFileCleaner.GetTempFilePath(imageId);

            // Проверяем, существует ли временный файл
            if (File.Exists(tempFilePath))
            {
                // Читаем данные изображения из временного файла
                byte[] imageBytes = File.ReadAllBytes(tempFilePath);

                // Конвертируем изображение в строку Base64
                return Convert.ToBase64String(imageBytes);
            }
            else
            {
                // Если файл не существует, возвращаем null
                return null;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке изображения из временного файла: {ex.Message}");
            return null;
        }
    }



}

