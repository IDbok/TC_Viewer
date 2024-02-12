using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.DataProcessing
{
    public static class MessageSender
    {
        public static DialogResult SendQuestionDeliteObjects(string message
            , List<DataGridViewRow> rowsToDelete, string colName)
        {
            message += "\n";
            foreach (var row in rowsToDelete)
            {
                message += colName +": "+ row.Cells[colName].Value.ToString() + ";\n";
            }

            string caption = "Удаление Технологических карт";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result;
            result = MessageBox.Show(message, caption, buttons);
            return result;
        }
    }
}
