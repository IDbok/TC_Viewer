using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.Interfaces;

namespace TcModels.Models.TcContent.Work
{
    public interface IUpdatableEntity
    {
        void ApplyUpdates(IUpdatableEntity source);
    }
}
