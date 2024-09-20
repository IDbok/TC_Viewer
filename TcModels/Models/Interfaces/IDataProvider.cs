using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.TcContent;

namespace TcModels.Models.Interfaces;

public interface IDataProvider
{
    TechnologicalCard GetDataTechCard(int id, string cardName);
    List <TechOperationWork> GetDataTechOperationList(int id, string cardName);

}


