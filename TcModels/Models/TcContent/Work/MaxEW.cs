using System.ComponentModel.DataAnnotations;

namespace TcModels.Models.TcContent.Work
{
	public class MaxEW
    {
        [Key]
        public Guid guid { get; set; }
        public ExecutionWork? executionWork { get; set; }

        public SumEW? sumEW { get; set; }
    }
}
