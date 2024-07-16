using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcDbConnector.Migrations.OldCore.Models.Interfaces
{
    public interface IDGViewable : IRequiredProperties
    {
        
        static Dictionary<string, int> GetPropertiesOrder { get; } = null!;
        static List<string> GetChangeablePropertiesNames { get; } = null!;
    }
}
