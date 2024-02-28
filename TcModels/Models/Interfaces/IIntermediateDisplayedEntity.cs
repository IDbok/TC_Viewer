using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.Interfaces
{
    public interface IIntermediateDisplayedEntity : IDisplayedEntity
    {
        List<string> GetKeyFields();
    }
}
