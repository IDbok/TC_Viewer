using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcDbConnector.Migrations.OldCore.Models.TcContent
{
    public class ToolWork
    {
        public int Id { get; set; }
        public int techOperationWorkId { get; set; }
        public TechOperationWork techOperationWork { get; set; }

        public Tool tool { get; set; }
        public int toolId { get; set; }
        public double Quantity { get; set; }

        public string Comments { get; set; } = "";

        public override string ToString()
        {
            return tool?.Name;
        }

    }
}
