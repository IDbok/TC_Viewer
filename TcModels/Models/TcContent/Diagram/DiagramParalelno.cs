namespace TcModels.Models.TcContent
{
	public class DiagramParalelno
    {
        public int Id { get; set; }
        public int techOperationWorkId { get; set; }
        public TechOperationWork techOperationWork { get; set; } // по сути, данная ссылка лишняя, т.к. привязка идёт в DiagamToWork
        public int DiagamToWorkId { get; set; }
        public DiagamToWork DiagamToWork { get; set; }

        public List<DiagramPosledov> ListDiagramPosledov { get; set; } = new List<DiagramPosledov>();

        public int Order { get; set; }

    }
}
