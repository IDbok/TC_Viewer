using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.Interfaces
{
    public interface INameable: IIdentifiable
    {
        int Id { get; set; }
        string Name { get; set; }
    }
}
