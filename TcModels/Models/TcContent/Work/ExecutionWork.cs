using System.ComponentModel.DataAnnotations.Schema;
using TcModels.Models.IntermediateTables;

namespace TcModels.Models.TcContent
{
    public class ExecutionWork
    {
        public int Id { get; set; }
        public TechOperationWork techOperationWork { get; set; }
        public int techOperationWorkId { get; set; }

        public TechTransition? techTransition { get; set; }
        public int? techTransitionId { get; set; }

        public List<Staff_TC> Staffs { get; set; } = new List<Staff_TC>();

        public List<Protection_TC> Protections { get; set; } = new List<Protection_TC>();

        public List<Machine_TC> Machines { get; set; } = new List<Machine_TC>(); // если присутствует machine_tc значит участвует в этапе

        public List<ExecutionWork> WorkRepeat { get; set; } = new List<ExecutionWork>();
        public bool Repeat { get; set; } = false;

        // public ExecutionWorkRepeat? executionWorkRepeat { get; set; }

        public List<ExecutionWork> ListexecutionWorkRepeat { get; set; } = new List<ExecutionWork>();  
        public List<ExecutionWork> ListexecutionWorkRepeat2 { get; set; } = new List<ExecutionWork>(); // adding repeat items

        public Guid? sumEw { get; set; } // не актуально
        public Guid? maxEw { get; set; } // не акутально

        public string? Coefficient { get; set; } = "";
        public double Value { get; set; }
        public string Comments { get; set; } = "";

        [NotMapped] public bool NewItem { get; set; }
       [NotMapped] public bool Delete { get; set; }
       [NotMapped] public Guid IdGuid { get; set; }
        
       public int Order { get; set; }
       public string Etap { get; set; } = "";
       public string Posled { get; set; } = "";
       [NotMapped] public double TempTimeExecution { get; set; }

       public override string ToString()
       {
           if (techTransition != null)
           {
               return techTransition.Name;
           }
           else
           {
               if (Repeat)
               {
                   return "Повторить";
                }
               else
               {
                   return base.ToString();
                }

           }
       }
    }
}
