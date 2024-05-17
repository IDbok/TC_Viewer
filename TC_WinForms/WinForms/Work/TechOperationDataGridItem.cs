using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Work
{
    public class TechOperationDataGridItem
    {
        public int Nomer { get; set; }
        public string TechOperation { get; set; }

        public string Staff { get; set; }

        public string TechTransition { get; set; }

        public string TechTransitionValue { get; set; }


        public string Protections { get; set; }

        public bool ItsTool = false;

        public bool ItsComponent = false;


        public string Etap = "";
        public string Posled = "";

        public string TimeEtap = "";

        public bool Work = false;
        public ExecutionWork techWork;
        public List<bool> listMach = new List<bool>();
        public List<string> listMachStr = new List<string>();

        public string Comments = "";

        public string Vopros = "";
        public string Otvet = "";

        public ExecutionWork executionWorkItem=null;

    }
}
