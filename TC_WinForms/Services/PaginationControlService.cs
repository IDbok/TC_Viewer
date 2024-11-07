using TC_WinForms.Interfaces;

namespace TC_WinForms.Services
{
    public class PaginationControlService<T> where T : class
    {
        /// <summary>
        /// Максимальное количество объектов на одной странице
        /// </summary>
        private readonly int _rowsPerPage;

        /// <summary>
        /// Список всех объектов, которые будут выведены на страницах
        /// </summary>
        private List<T> _allObjects;
        
        
        /// <summary>
        /// Максимальное количество страниц
        /// </summary>
        private int _pageCount;

        /// <summary>
        /// Номер текущей активной страницы
        /// </summary>
        private int _currentPageIndex;

        public PaginationControlService() { }
        public PaginationControlService(int countOfObjectsOnPage)
        {
            this._rowsPerPage = countOfObjectsOnPage;
        }
        /// <summary>
        /// Расчитывает количество всех страниц для текущего количества всех объектов
        /// </summary>
        private void CountTotalPages()
        {
            _pageCount = (int)Math.Ceiling(_allObjects.Count / (double)_rowsPerPage);
        }
        /// <summary>
        /// Увеличивает значение текущей активной страницы на 1
        /// </summary>
        public void GoToNextPage()
        {
            if (_currentPageIndex < _pageCount - 1)
                _currentPageIndex++;
        }
        /// <summary>
        /// Уменьшает значение текущей активной страницы на 1
        /// </summary>
        public void GoToPreviousPage()
        {
            if(_currentPageIndex > 0)
                _currentPageIndex--;
        }

        public void SetAllObjectList(List<T> newAllObjectList)
        {
            _allObjects = newAllObjectList;
            CountTotalPages();
            _currentPageIndex = 0;
        }

        /// <summary>
        /// Обновляет информацию о выводимых на страннице объектах
        /// </summary>
        private void UpdatePageInformation()
        {
            //Получаем данные об общем количестве записей и с какой записи выводится информация
            int totalRecords = _allObjects.Count;
            int startRecord = _currentPageIndex * _rowsPerPage + 1;
            // Обеспечиваем, что endRecord не превышает общее количество записей
            int endRecord = Math.Min(startRecord + _rowsPerPage - 1, totalRecords);

            pageInfo = new PageInfoEventArgs
            {
                StartRecord = startRecord,
                EndRecord = endRecord,
                TotalRecords = totalRecords
            };

        }
        /// <summary>
        /// Формирует и возвращается список объектов, которые будут выводиться на текущей активной странице
        /// </summary>
        /// <returns>Список объектов, которые будут выводиться на активной странице</returns>
        public List<T>? GetPageData()
        {
            int skipedItems = _currentPageIndex * _rowsPerPage;
            var displayedPageData = _allObjects.Skip(skipedItems).Take(_rowsPerPage).ToList();

            return displayedPageData;
        }
        /// <summary>
        /// Информация о странице: 
        ///     место начала вывода записей,
        ///     место конца вывода записей,
        ///     oбщее количество записей
        /// </summary>
        private PageInfoEventArgs pageInfo { get; set; }

        /// <summary>
        /// Обновляет и возвращает информацию о текущей странице
        /// </summary>
        /// <returns>PageInfoEventArgs, содержащее в себе актуальную информацию о текущей старнице</returns>
        public PageInfoEventArgs GetPageInfo()
        {
            UpdatePageInformation();
            return pageInfo;
        } 

    }
}
