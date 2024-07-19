using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcDbConnector.Migrations.OldCore.Models.TcContent
{
    public class DiagramParalelno
    {
        public int Id { get; set; }

        public int techOperationWorkId { get; set; }
        public TechOperationWork techOperationWork { get; set; }
        public int DiagamToWorkId { get; set; }
        public DiagamToWork DiagamToWork { get; set; }

        public List<DiagramPosledov> ListDiagramPosledov { get; set; } = new List<DiagramPosledov>();

        public int Order { get; set; }

    }
}
