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
        public bool PrintWorkSteps { get; set; }
        public bool PrintDiagram { get; set; }
        public bool PrintOutlay { get; set; }
        public bool PrintRoadMap { get; set; }
    }
}
