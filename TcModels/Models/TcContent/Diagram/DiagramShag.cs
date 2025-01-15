using System.ComponentModel.DataAnnotations.Schema;

namespace TcModels.Models.TcContent
{
	public class DiagramShag
    {
        public int Id { get; set; }

        public string Deystavie { get; set; } = "";

        // todo: перенести на работу через ImageStorage
        public string ImageBase64 { get; set; } = "";

        public string NameImage { get; set; } = "";

        public int Nomer { get; set; }

        public List<DiagramShagToolsComponent> ListDiagramShagToolsComponent { get; set; } = new List<DiagramShagToolsComponent>();

        public DiagramPosledov DiagramPosledov { get; set; }
        public int DiagramPosledovId { get; set; }

  //      [NotMapped] public string? ParallelIndex { get; set; }
		//[NotMapped] public string? SequenceIndex { get; set; }

		public int Order { get; set; }

        public string? LeadComment { get; set; } // todo: добавить в БД
        public string? ImplementerComment { get; set; } // todo: добавить в БД

    }
}
