using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TcModels.Models;
using TcModels.Models.TcContent;
using static TcModels.Models.ImageOwner;

namespace TC_WinForms.Services
{
    public static class ImageService
    {
        public static ImageStorage UpdateImageFromBase64(ImageStorage image, string filename)
        {
            System.Drawing.Image imag;
            try
            {
                imag = System.Drawing.Image.FromFile(filename);
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
        public static ImageStorage CreateNewImageFromBase64(string filename)
        {
            System.Drawing.Image imag;

            try
            {
                imag = System.Drawing.Image.FromFile(filename);

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
    }
}
