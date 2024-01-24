using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.TcContent;

namespace TcModels.Models.IntermediateTables
{
    public class LinkEntety
    {
        public int Id { get; set; }
        public string Link { get; set; }
    }
}
