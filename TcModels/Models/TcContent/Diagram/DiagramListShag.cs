using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.TcContent
{
    public class DiagramListShag
    {
        public int Id { get; set; }

        public DiagamToWork diagamToWork { get; set; }

        public List<DiagramParalelno> ListDiagramParalelno { get; set;}
    }
}
