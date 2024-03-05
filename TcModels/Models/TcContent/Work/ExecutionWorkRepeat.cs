using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.TcContent
{
    public class ExecutionWorkRepeat
    {
        public int Id { get; set; }
        public ExecutionWork OneexecutionWork { get; set; }

        public List<ExecutionWork> executionWork { get; set; }
    }
}
