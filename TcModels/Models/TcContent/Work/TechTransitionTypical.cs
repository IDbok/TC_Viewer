using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.TcContent.Work
{
    public class TechTransitionTypical
    {
        public int Id { get; set; }

        public int TechOperationId { get; set; }
        public TechOperation TechOperation { get; set; } = null!;
        public int TechTransitionId { get; set; }
        public TechTransition TechTransition { get; set; } = null!;

        public string Etap { get; set; } = "";
        public string Posled { get; set; } = "";

    }
}
