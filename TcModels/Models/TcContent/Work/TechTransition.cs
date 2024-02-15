using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.TcContent
{
    public class TechTransition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double TimeExecution { get; set; }
        
        public List<ExecutionWork> ExecutionWorks { get; set; }
    }
}
