namespace TcModels.Models.TcContent
{
	public class DiagramShagToolsComponent
    {
        public int Id { get; set; }

        public ToolWork? toolWork { get; set; }
        public int? toolWorkId { get; set; }

        public ComponentWork? componentWork { get; set; }
        public int? componentWorkId { get; set; }

        public double Quantity { get; set; }

        public DiagramShag DiagramShag { get; set; }
        public int DiagramShagId { get; set; }
        public string? Comment { get; set; } // todo: добавить в БД

    }
}
