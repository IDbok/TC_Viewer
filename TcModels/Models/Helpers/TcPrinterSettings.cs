using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.Helpers
{
    public class TcPrinterSettings
    {
        public long? TcId { get; set; }
        public string? TcName { get; set; }
        public bool PrintWorkSteps { get; set; } = true;
        public bool PrintDiagram { get; set; } = true;
        public bool PrintOutlay { get; set; } = true;
        public bool PrintRoadMap { get; set; } = true;
    }
}
