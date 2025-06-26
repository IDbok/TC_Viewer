using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.TcContent;
using static System.Net.Mime.MediaTypeNames;

namespace TcModels.Models
{
    public class ImageOwner
    {
        public long Id { get; set; }
        public long ImageStorageId { get; set; }
        public ImageStorage ImageStorage { get; set; }
        public int TechnologicalCardId { get; set; }
        public TechnologicalCard TechnologicalCard { get; set; }
        public string Name { get; set; } = "Без имени";
        public int Number { get; set; } = 1;
        public ImageRole Role { get; set; }
        public List<ExecutionWork> ExecutionWorks { get; set; } = new List<ExecutionWork>();
        public List<DiagramShag> DiagramShags { get; set; } = new List<DiagramShag>();

        public override string ToString()
        {
            return Role == ImageRole.Image ? $"Рисунок {Number}" : $"Схема исполнения {Number}";
        }
    }
    public enum ImageRole
    {
        ExecutionScheme,
        Image
    }
}
