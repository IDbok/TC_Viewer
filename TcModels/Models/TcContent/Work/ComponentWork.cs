using System.ComponentModel.DataAnnotations.Schema;

namespace TcModels.Models.TcContent
{
	public class ComponentWork
    {
        public int Id { get; set; }
        public int techOperationWorkId { get; set; }
        public TechOperationWork techOperationWork { get; set; }
        public Component component { get; set; }
        public int componentId { get; set; }
        public double Quantity { get; set; }
        public string? Comments { get; set; } = "";

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
        public override string ToString()
        {
            return component?.Name;
        }

    }
}
