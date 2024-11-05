using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.Interfaces
{
    public interface IPagination
    {
        public int _pageSize { get; }
        public List<int>? displayedData { get;  set; }
        public int pageIndex { get; set; }
        public List<int> UpdateDisplayedData();

        public PageInfoEventArgs PageInfoChanged { get; set; }
    }
}
