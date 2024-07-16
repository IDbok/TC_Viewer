using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.IntermediateTables;

namespace TcModels.Models.Interfaces
{
    public interface ILinkable
    {
        List<LinkEntety> Links { get; set; }
    }
}
