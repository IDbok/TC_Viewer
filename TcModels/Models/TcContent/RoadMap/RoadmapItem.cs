using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent.RoadMap
{
    public class RoadMapItem: IUpdatableEntity
    {
        public int Id { get; set; }         // ID
        public int TowId { get; set; }      // Id TewchOpertionWork
        public string TOName { get; set; }  // Название TewchOpertion
        public string Staffs { get; set; }  // Строка символов Staff
        public string Note { get; set; }    // Примечание
        public int Order { get; set; }      // Порядок
        public string? Remark { get; set; } // Замечание
        public string? Reply { get; set; }  // Ответ
        [NotMapped]
        public SequenceCell SequenceCells { get; set; } = new SequenceCell(); // Список SequenceCell
        [NotMapped]
        public TechOperationWork techOperationWork { get; set; } = new TechOperationWork(); // Список SequenceCell
        public double[] SequenceData { get; set; } // Массив значений 

        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is RoadMapItem sourceCard)
            {
                TOName = sourceCard.TOName;
                Staffs = sourceCard.Staffs;
                Note = sourceCard.Note;
                Order = sourceCard.Order;
                SequenceData = sourceCard.SequenceData;
                Remark = sourceCard.Remark;
                Reply = sourceCard.Reply;
            }
        }
    }
}
