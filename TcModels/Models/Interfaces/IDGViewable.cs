using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.Interfaces
{
    public interface IDGViewable
    {
        public static Dictionary<string, string>  GetPropertiesNames { get; } = null!;
        public static Dictionary<string, int> GetPropertiesOrder { get; } = null!;
        public static List<string> GetPropertiesRequired { get; } = null!;
    }
}
