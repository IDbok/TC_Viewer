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

        public TechOperation techOperation { get; set; }
        public TechTransition techTransition { get; set; }

        public string Etap { get; set; } = "";
        public string Posled { get; set; } = "";

    }
}
