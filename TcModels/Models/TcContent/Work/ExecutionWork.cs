using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent
{
    public class ExecutionWork
    {
        public int Id { get; set; }
        public TechOperationWork techOperationWork { get; set; }

        public TechTransition? techTransition { get; set; }

        public List<Staff_TC> Staffs { get; set; }

        public List<Protection_TC> Protections { get; set; }

        public List<Machine_TC> Machines { get; set; }

        public ExecutionWorkRepeat? executionWorkRepeat { get; set; }

        public Guid? sumEw { get; set; }
        public Guid? maxEw { get; set; }

    }
}
