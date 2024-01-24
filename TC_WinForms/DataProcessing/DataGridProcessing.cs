using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.DataProcessing
{
    public static class DataGridProcessing
    {
        // method to colorize cell in datagridview
        public static void ColorizeCell(DataGridView dgv, int columnNumber, int rowNumber, string color)
        {
            dgv.Rows[rowNumber].Cells[columnNumber].Style.BackColor = System.Drawing.Color.Red;
        }

        //public static void ColorizeCells(DataGridView dgv, int columnNumber, int rowNumber, string color)
        //{
        //    foreach (System.Windows.Forms.DataGridViewRow row in dgv.Rows)
        //    {
        //        if (row.Cells[columnNumber].Value.ToString() == color)
        //        {
        //            row.Cells[columnNumber].Style.BackColor = System.Drawing.Color.Red;
        //        }
        //    }
        //}
    }
}
