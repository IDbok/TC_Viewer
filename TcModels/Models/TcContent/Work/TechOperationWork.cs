using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        public ICollection<ExecutionWork> executionWorks { get; set; } = new List<ExecutionWork>();

       public TechnologicalCard technologicalCard { get; set; }
       public int TechnologicalCardId { get; set; }

       [NotMapped] public bool Delete { get; set; } = false;
       [NotMapped] public bool NewItem { get; set; } = false;


       public override string ToString()
       {
           return techOperation.Name;
       }
    }
}
