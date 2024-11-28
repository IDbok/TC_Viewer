using Antlr4.Runtime.Tree.Xpath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC_WinForms.Interfaces;

namespace TC_WinForms.Services
{
    public static class CheckOpenFormService
    {
        public static Form? AreFormOpen(int objectId, string FormType)
        {
            FormCollection fc = Application.OpenForms;

            foreach (Form frm in fc)
            {
                //iterate through
                if (frm.GetType().Name == FormType && frm is IOpenFormCheck openFormCheck && openFormCheck.GetObjectId() == objectId)
                    return frm;
            }
            return null;
        }
    }
}
