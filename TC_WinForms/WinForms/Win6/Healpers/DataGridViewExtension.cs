using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.WinForms.Win6.Healpers
{
    public static class DataGridViewExtension
    {
        public static DataGridViewColumn AddColumn(this DataGridView dgv,
            string name, string header,
            ColumnRole role = ColumnRole.None,
            DataGridViewTriState resizable = DataGridViewTriState.NotSet,
            bool visible = true,
            int? width = null,
            int? fillWeigth = null, int? minWidth = null)
        {
            var idx = dgv.Columns.Add(name, header);
            var col = dgv.Columns[idx].WithRole(role);
            col.SortMode = DataGridViewColumnSortMode.NotSortable;
            col.ReadOnly = true;
            col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            col.Visible = visible;
            col.Resizable = resizable;
            if (width.HasValue) {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                col.Width = width.Value;
            }
            else col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            if (fillWeigth.HasValue) {
                col.FillWeight = fillWeigth.Value;
            }
            if (minWidth.HasValue) {
                col.MinimumWidth = minWidth.Value;
            }
            return col;
        }
    }
}
