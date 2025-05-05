using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.TcContent;

namespace TcModels.Models
{
    public class ImageOwner
    {
        public long Id { get; set; }
        public long ImageStorageId { get; set; }
        public ImageStorage ImageStorage { get; set; }
        public int TechnologicalCardId { get; set; }
        public TechnologicalCard TechnologicalCard { get; set; }
        public string? Name { get; set; } = "Без имени";
        public int? Number { get; set; } = 1;
        public ImageType ImageRoleType { get; set; }
        public List<ExecutionWork> ExecutionWorks { get; set; } = new List<ExecutionWork>();
        public List<DiagramShag> DiagramShags { get; set; } = new List<DiagramShag>();

        public override string ToString()
        {
            return $"Рисунок {Number}";
        }
    }
    public enum ImageType
    {
        ExecutionScheme,
        Image
    }
}
