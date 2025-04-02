using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.TcContent.RoadMap
{
    public class SequenceCell
    {
        public int Id { get; set; }             // This is the ID of the sequence cell
        public int RoadmapItemId { get; set; }  // This is the ID of the roadmap item
        public int Column { get; set; }         // This is the column of the sequence cell
        public int ColumnSpan { get; set; } = 1;// Объедененные столбцы слева направо
        //public int RowSpan { get; set; } = 1;   // Объедененные строки сверху вниз
        public double Value { get; set; }   // Значение
        public int Order { get; set; }      // Порядок TewchOpertionWork
    }

}
