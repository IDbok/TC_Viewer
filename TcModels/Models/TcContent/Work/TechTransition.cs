using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.Interfaces;

namespace TcModels.Models.TcContent
{
    public class TechTransition: IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double TimeExecution { get; set; }

        public string? Category { get; set; }
        public bool? TimeExecutionChecked { get; set; } = false;
        public string? CommentName { get; set; }
        public string? CommentTimeExecution { get; set; }
        
        public List<ExecutionWork> ExecutionWorks { get; set; }
    }
}
