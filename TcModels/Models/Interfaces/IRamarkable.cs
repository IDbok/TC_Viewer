using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.Interfaces
{
    public interface IRamarkable // todo: написаль описание интерфейса + исправить опечатку
    {
        public bool IsRemarkClosed { get; set; }
        public string Remark {  get; set; }
    }
}
