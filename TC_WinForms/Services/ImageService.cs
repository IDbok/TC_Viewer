using System;
using System.IO;
using TcModels.Models;
using TcModels.Models.TcContent;
using static TcModels.Models.ImageOwner;

namespace TC_WinForms.Services
{
    /// <summary>
    /// Сервис для работы с изображениями и их хранилищем.
    /// </summary>
    public static class ImageService
    {
        #region Создание изображений

        /// <summary>
        /// Создаёт новый экземпляр <see cref="ImageStorage"/>, копируя свойства из переданного.
        /// </summary>
        /// <param name="imageStorage">Источник данных изображения.</param>
        /// <returns>Новый экземпляр <see cref="ImageStorage"/>.</returns>
        public static ImageStorage CreateNewImage(ImageStorage imageStorage)
        {
            return new ImageStorage
            {
                Name = imageStorage.Name,
                Category = imageStorage.Category,
                ImageBase64 = imageStorage.ImageBase64,
                ImageType = imageStorage.ImageType,
                StorageType = imageStorage.StorageType
            };
        }

        /// <summary>
        /// Создаёт новый экземпляр <see cref="ImageStorage"/>, на основе выбранного файла.
        /// </summary>
        /// <param name="filename">Путь к файлу изображения.</param>
        /// <returns>Новый объект <see cref="ImageStorage"/> с данными изображения в формате base64.</returns>
        public static ImageStorage CreateNewImageFromBase64(string filename)
        {
            try
            {
                var image = System.Drawing.Image.FromFile(filename);
                byte[] bytes = File.ReadAllBytes(filename);
                string base64 = Convert.ToBase64String(bytes);

                return new ImageStorage
                {
                    Name = filename,
                    Category = "TechnologicalCard",
                    ImageBase64 = base64,
                    ImageType = "image/png",
                    StorageType = ImageStorageType.Base64
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading image: " + ex.Message);
            }
        }

        #endregion

        #region Обновление изображений

        /// <summary>
        /// Обновляет содержимое изображения <see cref="ImageStorage"/> на основе выбранного файла.
        /// </summary>
        /// <param name="image">Существующий объект <see cref="ImageStorage"/>.</param>
        /// <param name="filename">Путь к новому файлу изображения.</param>
        /// <returns>Обновлённый объект <see cref="ImageStorage"/>.</returns>
        public static ImageStorage UpdateImageFromBase64(ImageStorage image, string filename)
        {
            try
            {
                var loadedImage = System.Drawing.Image.FromFile(filename);
                byte[] bytes = File.ReadAllBytes(filename);
                string base64 = Convert.ToBase64String(bytes);

                image.Name = filename;
                image.ImageBase64 = base64;
                image.IsChanged = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading image: " + ex.Message);
            }

            return image;
        }

        /// <summary>
        /// Применяет изменения одного объекта <see cref="ImageStorage"/> к другому, сохраняя ID оригинала.
        /// </summary>
        /// <param name="sourse">Оригинальный объект с ID.</param>
        /// <param name="newImage">Новый объект с актуальными данными.</param>
        /// <returns>Объединённый объект <see cref="ImageStorage"/>.</returns>
        public static ImageStorage ApplyStorageChanges(ImageStorage sourse, ImageStorage newImage)
        {
            return new ImageStorage
            {
                Id = sourse.Id,
                Name = newImage.Name,
                Category = newImage.Category,
                ImageBase64 = newImage.ImageBase64,
                ImageType = newImage.ImageType,
                StorageType = newImage.StorageType
            };
        }

        #endregion

        #region ImageOwner

        /// <summary>
        /// Создаёт новый объект <see cref="ImageOwner"/> на основе изображения, ТК , типа изображения и номера.
        /// </summary>
        /// <param name="image">Объект <see cref="ImageStorage"/>.</param>
        /// <param name="tc">Технологическая карта.</param>
        /// <param name="imageType">Тип изображения.</param>
        /// <param name="number">Порядковый номер изображения.</param>
        /// <returns>Новый объект <see cref="ImageOwner"/>.</returns>
        public static ImageOwner CreateNewImageOwner(ImageStorage image, TechnologicalCard tc, ImageType imageType, int number)
        {
            return new ImageOwner
            {
                ImageStorage = image,
                ImageStorageId = image.Id,
                TechnologicalCard = tc,
                TechnologicalCardId = tc.Id,
                Name = image.Name,
                Number = number,
                ImageRoleType = imageType
            };
        }

        #endregion
    }
}
