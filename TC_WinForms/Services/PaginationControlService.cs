using TC_WinForms.Interfaces;

namespace TC_WinForms.Services
{
    public class PaginationControlService<T> where T : class
    {
        /// <summary>
        /// Максимальное количество объектов на одной странице
        /// </summary>
        private readonly int CountOfObjectsOnPage;

        private List<T> _allObjects;
        /// <summary>
        /// Список всех объектов, которые будут выведены на страницах
        /// </summary>
        public List<T> AllObjects
        { 
            private get => _allObjects;
            set
            {
                _allObjects = value;
                CountTotalPages();
                _currentPageIndex = 0;
            }
        }
        /// <summary>
        /// Максимальное количество страниц
        /// </summary>
        private int _totalCountPages;
        
        private int _currentPageIndex;
        /// <summary>
        /// Номер текущей активной страницы
        /// </summary>
        public int CurrentPageIndex 
        { 
            get => _currentPageIndex; 
            private set
                    {
                        if(value != null)
                        {
                            if((_currentPageIndex < _totalCountPages - 1 && value > _currentPageIndex) 
                                || (_currentPageIndex > 0 && value < _currentPageIndex))
                                _currentPageIndex = value;

                        }
                    }
        } 

        public PaginationControlService() { }
        public PaginationControlService(int countOfObjectsOnPage)
        {
            this.CountOfObjectsOnPage = countOfObjectsOnPage;
        }
        /// <summary>
        /// Расчитывает количество всех страниц для текущего количества всех объектов
        /// </summary>
        private void CountTotalPages()
        {
            _totalCountPages = (int)Math.Ceiling(_allObjects.Count / (double)CountOfObjectsOnPage);
        }
        /// <summary>
        /// Увеличивает значение текущей активной страницы на 1
        /// </summary>
        public void GoToNextPage()
        {
            CurrentPageIndex++;
        }
        /// <summary>
        /// Уменьшает значение текущей активной страницы на 1
        /// </summary>
        public void GoToPreviousPage()
        {
            CurrentPageIndex--;
        }
        /// <summary>
        /// Обновляет информацию о выводимых на страннице объектах
        /// </summary>
        private void UpdatePageInformation()
        {
            //Получаем данные об общем количестве записей и с какой записи выводится информация
            int totalRecords = AllObjects.Count;
            int startRecord = CurrentPageIndex * CountOfObjectsOnPage + 1;
            // Обеспечиваем, что endRecord не превышает общее количество записей
            int endRecord = Math.Min(startRecord + CountOfObjectsOnPage - 1, totalRecords);

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
            int skipedItems = CurrentPageIndex * CountOfObjectsOnPage;
            var displayedPageData = AllObjects.Skip(skipedItems).Take(CountOfObjectsOnPage).ToList();
            UpdatePageInformation();

            return displayedPageData;
        }
        /// <summary>
        /// Информация о странице: 
        ///     место начала вывода записей,
        ///     место конца вывода записей,
        ///     oбщее количество записей
        /// </summary>
        public PageInfoEventArgs pageInfo { get; private set; }
    }
}
