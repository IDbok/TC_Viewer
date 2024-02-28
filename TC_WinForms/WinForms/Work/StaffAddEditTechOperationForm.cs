using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.IntermediateTables;

namespace TC_WinForms.WinForms.Work
{
    public class StaffAddEditTechOperationForm 
    {
        public Staff_TC staffTc;

        public string Value
        {
            get
            {
                return staffTc.Symbol;
            }
        }

        public Staff_TC Key
        {
            get
            {
                return staffTc;
            }
        }


        public override string ToString()
        {
            return staffTc.Symbol;
        }
    }
}
