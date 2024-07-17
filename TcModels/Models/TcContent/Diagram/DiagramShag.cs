using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.TcContent
{
    public class DiagramShag
    {
        public int Id { get; set; }

        public string Deystavie { get; set; } = "";

        public string ImageBase64 { get; set; } = "";

        public string NameImage { get; set; } = "";

        public int Nomer { get; set; }

        public List<DiagramShagToolsComponent> ListDiagramShagToolsComponent { get; set; } = new List<DiagramShagToolsComponent>();

        public DiagramPosledov DiagramPosledov { get; set; }
        public int DiagramPosledovId { get; set; }
        
        public int Order { get; set; }

    }
}
