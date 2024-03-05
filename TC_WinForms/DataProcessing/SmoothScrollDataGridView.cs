using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.DataProcessing
{
    using System;
    using System.Windows.Forms;

    public class SmoothScrollDataGridView : DataGridView
    {
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                int currentIndex = this.FirstDisplayedScrollingRowIndex;
                int scrollLines = SystemInformation.MouseWheelScrollLines;

                // Проверяем направление прокрутки
                if (e.Delta > 0) // Прокрутка вверх
                {
                    if (currentIndex > 0) currentIndex -= scrollLines;
                    if (currentIndex < 0) currentIndex = 0;
                }
                else if (e.Delta < 0) // Прокрутка вниз
                {
                    if (currentIndex < this.Rows.Count - 1) currentIndex += scrollLines;
                    if (currentIndex > this.Rows.Count - 1) currentIndex = this.Rows.Count - 1;
                }

                // Применяем изменения
                if (this.Rows.Count > 0 && currentIndex >= 0 && currentIndex < this.Rows.Count)
                {
                    this.FirstDisplayedScrollingRowIndex = currentIndex;
                }
            }

            // Предотвращаем вызов базового обработчика событий, чтобы избежать "прыгающей" прокрутки
            // base.OnMouseWheel(e); // Закомментировано, чтобы отключить стандартное поведение
        }
    }

}
