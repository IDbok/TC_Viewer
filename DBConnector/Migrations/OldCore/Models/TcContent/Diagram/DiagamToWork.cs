using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcDbConnector.Migrations.OldCore.Models.TcContent
{
    public class DiagamToWork
    {
        public int Id { get; set; }
        public TechOperationWork techOperationWork { get; set; }

        public TechnologicalCard technologicalCard { get; set; }


        public List<DiagramParalelno> ListDiagramParalelno { get; set; } = new List<DiagramParalelno>();
        public int Order { get; set; }
    }
}
