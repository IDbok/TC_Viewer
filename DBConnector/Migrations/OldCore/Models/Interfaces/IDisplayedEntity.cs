using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcDbConnector.Migrations.OldCore.Models.Interfaces
{
    public interface IDisplayedEntity
    {
        Dictionary<string, string> GetPropertiesNames();
        List<string> GetPropertiesOrder();
        List<string> GetRequiredFields();
    }
}
