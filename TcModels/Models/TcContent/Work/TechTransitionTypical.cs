namespace TcModels.Models.TcContent.Work
{
	public class TechTransitionTypical
    {
        public int Id { get; set; }

        public int TechOperationId { get; set; }
        public TechOperation TechOperation { get; set; } = null!;
        public int TechTransitionId { get; set; }
        public TechTransition TechTransition { get; set; } = null!;

        public int Order { get; set; } = 0;

        public string Etap { get; set; } = "";
        public string Posled { get; set; } = "";

        public string? Coefficient { get; set; }
        public string? Comments { get; set; }

    }
}
