using System.ComponentModel.DataAnnotations;

namespace TcModels.Models.TcContent.Work
{

	public class SumEW
    {
        [Key]
        public Guid guid { get; set; }
        public ExecutionWork executionWork { get; set; }
    }
}
