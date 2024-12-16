using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TcModels.Models.Interfaces;

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
