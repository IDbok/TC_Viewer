using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.Extensions
{
    public static class DataGridViewExtensions
    {
        public static void RestoreScrollPosition(this DataGridView dataGridView, int position, Serilog.ILogger logger)
        {
            try
            {
                if (position > 0 && position < dataGridView.Rows.Count)
                {
                    dataGridView.FirstDisplayedScrollingRowIndex = position;
                }
            }
            catch (Exception e)
            {
                logger.Warning($"В процессе установки позиции скролла произошла ошибка {e.Message}");
            }
        }
    }
}
