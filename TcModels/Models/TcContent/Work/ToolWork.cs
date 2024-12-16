using System.ComponentModel.DataAnnotations.Schema;

namespace TcModels.Models.TcContent
{
	public class ToolWork
    {
        public int Id { get; set; }
        public int techOperationWorkId { get; set; }
        public TechOperationWork techOperationWork { get; set; }

        public Tool tool { get; set; }
        public int toolId { get; set; }
        public double Quantity { get; set; }

        public string? Comments { get; set; } = "";

        [NotMapped] public bool IsDeleted { get; set; } = false;

        public override string ToString()
        {
            return tool?.Name;
        }

    }
}
