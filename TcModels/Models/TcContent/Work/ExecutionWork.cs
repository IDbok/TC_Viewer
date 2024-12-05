using AutoMapper;
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

        //public List<ExecutionWork> WorkRepeat { get; set; } = new List<ExecutionWork>();
        public bool Repeat { get; set; } = false;

        // public ExecutionWorkRepeat? executionWorkRepeat { get; set; }

        [NotMapped] public List<ExecutionWork> ListexecutionWorkRepeat { get; set; } = new List<ExecutionWork>();
        [NotMapped] public List<ExecutionWork> ListexecutionWorkRepeat2 { get; set; } = new List<ExecutionWork>(); // adding repeat items
        public List<ExecutionWorkRepeat> ExecutionWorkRepeats { get; set; } = new List<ExecutionWorkRepeat>();


        public Guid? sumEw { get; set; } // не актуально
        public Guid? maxEw { get; set; } // не акутально

        public string? Coefficient { get; set; } = "";
        public double Value { get; set; } // время выполнения
        public string? Comments { get; set; } = "";

        [NotMapped] public bool NewItem { get; set; }
        [NotMapped] public bool Delete { get; set; }
        [NotMapped] public Guid IdGuid { get; set; }
        
        public int Order { get; set; }
        public string Etap { get; set; } = "";
        public string Posled { get; set; } = "";
        [NotMapped] public double TempTimeExecution { get; set; }


        public string Vopros { get; set; } = "";
        public string Otvet { get; set; } = "";

        public string? PictureName { get; set; } = "";

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

        public ExecutionWork DeepCopyEW(ExecutionWork sourceExecutionWork, TechnologicalCard newtechnologicalCard)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OpenProfile(2));
            });
            var newExecutionWork = new ExecutionWork();

            var mapper = config.CreateMapper();

            newExecutionWork = mapper.Map<ExecutionWork>(sourceExecutionWork);

            var staffs = sourceExecutionWork.Staffs;
            var protections = sourceExecutionWork.Protections;
            var machines = sourceExecutionWork.Machines;

            var currentStaffs = newtechnologicalCard.Staff_TCs.Where(s => staffs.Exists(staffs => staffs.ChildId == s.ChildId && staffs.Order == s.Order && staffs.Symbol == s.Symbol)).ToList();
            var currentProtections = newtechnologicalCard.Protection_TCs.Where(s => protections.Exists(staffs => staffs.ChildId == s.ChildId && staffs.Order == s.Order)).ToList();
            var currentMachines = newtechnologicalCard.Machine_TCs.Where(s => machines.Exists(staffs => staffs.ChildId == s.ChildId && staffs.Order == s.Order)).ToList();

            newExecutionWork.Staffs.AddRange(currentStaffs);
            newExecutionWork.Protections.AddRange(currentProtections);
            newExecutionWork.Machines.AddRange(currentMachines);

            return newExecutionWork;
        }
    }
}
