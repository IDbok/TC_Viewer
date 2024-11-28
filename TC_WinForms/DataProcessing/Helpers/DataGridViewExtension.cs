using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.DataProcessing.Helpers
{
    public static class DataGridViewExtension
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }

        public static void ResizeRows(this DataGridView dgv, int minimumHeight)
        {
            foreach (DataGridViewRow row in dgv.Rows)

            {
                DataGridViewRow dataGridViewRow = dgv.Rows.SharedRow(row.Index);

                int preferredThickness = row.GetPreferredHeight(row.Index, DataGridViewAutoSizeRowMode.AllCells, true);

                if (preferredThickness < minimumHeight)
                {
                    preferredThickness = minimumHeight;
                }

                row.Height = preferredThickness;
            }
        }
    }
}
