using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace TcModels.Models
{
    public class ObjectLocker
    {
        public int Id { get; set; }
        public int ObjectId { get; set; }
        public string ObjectType { get; set; }
        [Timestamp]
        public DateTime TimeStamp { get; set; }
    }
}
