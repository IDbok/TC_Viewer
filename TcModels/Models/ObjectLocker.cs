using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.Interfaces;
using Timer = System.Timers.Timer;

namespace TcModels.Models
{
    public class ObjectLocker: IIdentifiable
    {
        public int Id { get; set; }
        public int ObjectId { get; set; }
        public string ObjectType { get; set; }
        [Timestamp]
        public DateTime TimeStamp { get; set; }
        
        [NotMapped]
        public string? ObjectName {  get; set; }

    }
}
