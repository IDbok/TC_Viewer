using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.IntermediateTables;

namespace TcModels.Models.TcContent
{
    public class TechOperationWork
    {
        public int Id { get; set; }
        public TechOperation techOperation { get; set; }

        public  List<ToolWork> ToolWorks { get; set; }

        public List<ComponentWork> ComponentWorks { get; set; }

        public List<ExecutionWork> executionWorks { get; set; }

       
    }
}
