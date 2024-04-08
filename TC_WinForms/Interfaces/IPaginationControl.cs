using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.Interfaces
{
    public interface IPaginationControl
    {
        void GoToNextPage();
        void GoToPreviousPage();
        // Определение события для уведомления об изменении информации страницы.
        event EventHandler<PageInfoEventArgs> PageInfoChanged;

        // Метод для вызова события, передающего информацию о странице.
        void RaisePageInfoChanged(PageInfoEventArgs e);
    }

    public class PageInfoEventArgs : EventArgs
    {
        public int StartRecord { get; set; }
        public int EndRecord { get; set; }
        public int TotalRecords { get; set; }
    }
}
