using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.TcContent
{
    public class ComponentWork
    {
        public int Id { get; set; }
        public TechOperationWork techOperationWork { get; set; }
        public Component component { get; set; }
        public int componentId { get; set; }
        public double Quantity { get; set; }
    }
}
