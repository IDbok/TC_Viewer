using System.ComponentModel.DataAnnotations.Schema;
using TcModels.Models.Interfaces;

namespace TcModels.Models.TcContent
{
	public class ComponentWork: IRamarkable
    {
        public int Id { get; set; }
        public int techOperationWorkId { get; set; }
        public TechOperationWork techOperationWork { get; set; }
        public Component component { get; set; }
        public int componentId { get; set; }
        public double Quantity { get; set; }
        public string? Comments { get; set; } = "";
        public string? Remark { get; set; } = "";
        public string? Reply { get; set; } = "";

        public bool IsRemarkClosed { get; set; } = false;

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
        public override string ToString()
        {
            return component?.Name;
        }

    }
}
