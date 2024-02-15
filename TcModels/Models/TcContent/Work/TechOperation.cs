using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.TcContent
{
    public class TechOperation
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<TechOperationWork> techOperationWorks { get; set; }

    }
}
