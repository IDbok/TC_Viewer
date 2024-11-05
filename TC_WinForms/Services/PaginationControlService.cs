using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TC_WinForms.Interfaces;

namespace TC_WinForms.Services
{
    public class PaginationControlService: IPagination
    {
        public int _pageSize
        {
            get => 50;
        }

        private int _pageIndex;

        public int pageIndex 
        { 
            get => _pageIndex; 
            set
            {
                if (_pageIndex != value && value != null)
                    _pageIndex = value;
            }
        }

        private List<int>? _displayedData;

        public List<int>? displayedData 
        {
            get => _displayedData;
            set
            {
                if (_displayedData != value && value != null)
                    _displayedData = value;
            }
        }

        private PageInfoEventArgs _pageInfoChanged;

        public PageInfoEventArgs PageInfoChanged 
        {
            get => _pageInfoChanged;
            set
            {
                if (_pageInfoChanged != value && value != null)
                    _pageInfoChanged = value;
            }
        }

        public List<int> UpdateDisplayedData()
        {
            // Расчет отображаемых записей
            int totalRecords = displayedData.Count;
            int startRecord = pageIndex * _pageSize + 1;
            // Обеспечиваем, что endRecord не превышает общее количество записей
            int endRecord = Math.Min(startRecord + _pageSize - 1, totalRecords);

            int skipedItems = pageIndex * _pageSize;

            // Обновляем данные
            var pageData = displayedData.Skip(skipedItems).Take(_pageSize).ToList();

            PageInfoChanged = new PageInfoEventArgs
            {
                StartRecord = startRecord,
                EndRecord = endRecord,
                TotalRecords = totalRecords
            };

            return pageData;
        }
    }
}
