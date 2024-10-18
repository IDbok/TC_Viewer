using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.TcContent
{
    public class DiagramShagToolsComponent
    {
        public int Id { get; set; }

        public ToolWork? toolWork { get; set; }
        public int? toolWorkId { get; set; }

        public ComponentWork? componentWork { get; set; }
        public int? componentWorkId { get; set; }

        public double Quantity { get; set; }

        public DiagramShag DiagramShag { get; set; }
        public int DiagramShagId { get; set; }
        public string? ToolComponentComment { get; set; } // todo: добавить в БД

    }
}
