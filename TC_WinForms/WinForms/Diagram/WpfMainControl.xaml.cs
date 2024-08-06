using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Windows;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram
{
    /// <summary>
    /// Логика взаимодействия для WpfMainControl.xaml
    /// </summary>
    public partial class WpfMainControl : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private readonly TcViewState _tcViewState;

        public MyDbContext context;

        private int tcId;
        public TechnologicalCard technologicalCard;

        public TechnologicalCard TehCarta; // todo: непонятно, зачем это поле
        public List<TechOperationWork> TechOperationWorksList;

        public DiagramForm diagramForm; // используется только для проверки (фиксации) наличия изменений

        private List<DiagamToWork> DeletedDiagrams = new List<DiagamToWork>();
        private List<TechOperationWork> AvailableTechOperationWorks = new List<TechOperationWork>();

        public event PropertyChangedEventHandler? PropertyChanged;
        public bool IsCommentViewMode => _tcViewState.IsCommentViewMode;
        private bool IsViewMode => _tcViewState.IsViewMode;
        public bool IsHiddenInViewMode => !IsViewMode;
        private void OnViewModeChanged()
        {
            OnPropertyChanged(nameof(IsHiddenInViewMode));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public WpfMainControl()
        {
            InitializeComponent();
        }

        public WpfMainControl(int tcId, DiagramForm _diagramForm, TcViewState tcViewState)
        {
            InitializeComponent();
            DataContext = this;

            _tcViewState = tcViewState;

            this.tcId = tcId;
            diagramForm = _diagramForm;
            context = new MyDbContext();

            technologicalCard = context.TechnologicalCards.Single(x => x.Id == tcId);

            TehCarta = context.TechnologicalCards
               .Include(t => t.Machines).Include(t => t.Machine_TCs)
               .Include(t => t.Protection_TCs)
               //.Include(t => t.Protections)
               .Include(t => t.Tool_TCs)
               .Include(t => t.Component_TCs)
               .Include(t => t.Staff_TCs)
                .Single(s => s.Id == tcId);

            TechOperationWorksList =
               context.TechOperationWorks.Where(w => w.TechnologicalCardId == tcId)
                   //.Include(i=>i.technologicalCard).ThenInclude(t=>t.Machine_TCs)
                   //.Include(i => i.technologicalCard).ThenInclude(t => t.Machines)
                   .Include(i => i.techOperation)
                   //.Include(i => i.technologicalCard).ThenInclude(t => t.Protection_TCs)
                   //.Include(i => i.technologicalCard).ThenInclude(t => t.Protections)
                   //.Include(i => i.technologicalCard).ThenInclude(t => t.Tool_TCs)
                   //.Include(i => i.technologicalCard).ThenInclude(t => t.Component_TCs)
                   .Include(i => i.ComponentWorks).ThenInclude(t => t.component)

                   .Include(r => r.executionWorks).ThenInclude(t => t.techTransition)
                   .Include(r => r.executionWorks).ThenInclude(t => t.Protections)
                   .Include(r => r.executionWorks).ThenInclude(t => t.Machines)
                   .Include(r => r.executionWorks).ThenInclude(t => t.Staffs)
                   //.Include(r => r.executionWorks).ThenInclude(t => t.ListexecutionWorkRepeat2) // todo - проверить, всё ли работает без этого 
                   .Include(r => r.ToolWorks).ThenInclude(r => r.tool).ToList();

            foreach(var tow in TechOperationWorksList)
            {
                AvailableTechOperationWorks.Add(tow);
            }

           var ListShag = context.DiagamToWork.Where(w=>w.technologicalCard == technologicalCard)

                 .Include(i => i.ListDiagramParalelno)
                .ThenInclude(ie => ie.techOperationWork)

                .Include(i=>i.ListDiagramParalelno)
                .ThenInclude(i=>i.ListDiagramPosledov)
                .ThenInclude(i => i.ListDiagramShag)
                .ThenInclude(i=>i.ListDiagramShagToolsComponent)

                .OrderBy(o => o.Order)
                             
                .ToList();

            // Сгруппировать по ParallelIndex, если ParallelIndex = null, то записать в отдельную группу
            var ListShagGroup = ListShag.GroupBy(g => g.ParallelIndex).ToList();

            foreach (var ListShagItem in ListShagGroup)
            {
                bool isNull = ListShagItem.Key == null;

                if (!isNull)
                {
                    var wpfTo = new WpfTo(this, _tcViewState, addDiagram: false);

                    ListWpfControlTO.Children.Add(wpfTo);

                    foreach (DiagamToWork item in ListShagItem.OrderBy(x=> x.Order).ToList())
                    {
                        wpfTo.AddParallelTO(item);
                    }
                }
                else
                {
                    foreach (DiagamToWork item in ListShagItem.ToList())
                    {
                        var wpfTo = new WpfTo(this, _tcViewState, item);

                        ListWpfControlTO.Children.Add(wpfTo);
                    }
                }
            }

            Nomeraciya();

            //foreach (DiagamToWork item in ListShag)
            //{
            //    ListWpfControlTO.Children.Add(new WpfControlTO(this, item)); // ListWpfControlTO - это StackPanel в WpfMainControl.xaml
            //    Nomeraciya();
            //}
            _tcViewState.ViewModeChanged += OnViewModeChanged;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfTOIsAvailable())
            {
                ListWpfControlTO.Children.Add(new WpfTo(this, _tcViewState));
                diagramForm.HasChanges = true;
                Nomeraciya();
            }
        }

        public void DeleteControlTO(WpfControlTO controlTO)
        {
            // найти и удалить controlTO из ListWpfControlTO (WpfTo)
            foreach(WpfTo wpfToItem in ListWpfControlTO.Children)
            {
                if (wpfToItem.Children.Contains(controlTO))
                {
                    DeletedDiagrams.Add(controlTO.diagamToWork);

                    wpfToItem.DeleteWpfControlTO(controlTO);
                    if (wpfToItem.Children.Count == 0)
                    {
                        ListWpfControlTO.Children.Remove(wpfToItem);
                    }
                    else if(wpfToItem.Children.Count == 1)
                    {
                        var lastWpfTo = wpfToItem.Children[0];
                        lastWpfTo.ParallelButtonsVisibility(false);
                        lastWpfTo.diagamToWork.ParallelIndex = null;
                    }

                    Nomeraciya();
                    break;
                }
            }

            //ListWpfControlTO.Children.Remove(controlTO);

            
            //diagramForm.HasChanges = true;

            //if (ListWpfControlTO.Children.Count==0)
            //{
            //    if (context.DiagamToWork.SingleOrDefault(s => s == controlTO.diagamToWork) != null)
            //        context.DiagamToWork.Remove(controlTO.diagamToWork);

            //    ListWpfControlTO.Children.Add(new WpfTo(this, addDiagram: true));
            //    Nomeraciya();
            //}
        }

        internal void Nomeraciya() // todo : раскомментировать
        {
            int nomer = 1;

            int Order1 = 1; // порядковый номер отображения TO
            int Order2 = 1; // порядковый номер отображения параллельных операций
            int Order3 = 1;
            int Order4 = 1;

            foreach (WpfTo wpfToItem in ListWpfControlTO.Children)
            {
                foreach (var wpfControlToItem in wpfToItem.Children)
                {
                    if (wpfControlToItem.diagamToWork != null) wpfControlToItem.diagamToWork.Order = Order1;

                    Order1++;
                    Order2 = 1;
                    Order3 = 1;
                    Order4 = 1;

                    foreach (WpfParalelno wpfParallelItem in wpfControlToItem.ListWpfParalelno.Children)
                    {
                        if (wpfParallelItem.diagramParalelno != null) wpfParallelItem.diagramParalelno.Order = Order2++;
                        Order3 = 1;
                        Order4 = 1;

                        foreach (WpfPosledovatelnost item3 in wpfParallelItem.ListWpfPosledovatelnost.Children)
                        {
                            if (item3.diagramPosledov != null) item3.diagramPosledov.Order = Order3++;
                            Order4 = 1;

                            foreach (WpfShag item4 in item3.ListWpfShag.Children)
                            {
                                if (item4.diagramShag != null) item4.diagramShag.Order = Order4++;
                                item4.SetNomer(nomer);
                                nomer++;
                            }
                        }
                    }
                }
                
            }

            diagramForm.HasChanges = true;
        }

        public void Save()
        {
            Nomeraciya();
            try
            {
                foreach (WpfTo wpfToItem in ListWpfControlTO.Children)
                {
                    foreach (var wpfControlToItem in wpfToItem.Children)
                    {
                        foreach (WpfParalelno item2 in wpfControlToItem.ListWpfParalelno.Children)
                        {
                            foreach (WpfPosledovatelnost item3 in item2.ListWpfPosledovatelnost.Children)
                            {
                                foreach (WpfShag item4 in item3.ListWpfShag.Children)
                                {
                                    item4.SaveCollection();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            
            try
            {
                foreach (DiagamToWork item in DeletedDiagrams)
                {
                    item.techOperationWork = null;
                }

                var bbn = context.DiagamToWork.Where(w => w.techOperationWork == null).ToList();
                foreach (DiagamToWork item in bbn)
                {
                    context.DiagamToWork.Remove(item);
                }

                context.SaveChanges();
            }
            catch (Exception ee)
            {
                System.Windows.Forms.MessageBox.Show(ee.Message);
            }
           
        }

        
        internal void Order(int v, WpfControlTO wpfControlTO)
        {
            // v = 1 - вниз, v = 2 - вверх

            // Поиск к какому WpfTo принадлежит wpfControlTO
            WpfTo? wpfToContainer = FindContainer(wpfControlTO);

            if (wpfToContainer == null) return;

            if (v == 1)
            {
                int ib = ListWpfControlTO.Children.IndexOf(wpfToContainer);
                if (ib < ListWpfControlTO.Children.Count - 1)
                {
                    var cv = ListWpfControlTO.Children[ib + 1];
                    ListWpfControlTO.Children.Remove(cv);
                    ListWpfControlTO.Children.Insert(ib, cv);
                }
            }

            if (v == 2)
            {
                int ib = ListWpfControlTO.Children.IndexOf(wpfToContainer);
                if (ib != 0)
                {
                    var cv = ListWpfControlTO.Children[ib];
                    ListWpfControlTO.Children.Remove(cv);
                    ListWpfControlTO.Children.Insert(ib - 1, cv);
                }
            }

            Nomeraciya();


            WpfTo? FindContainer(WpfControlTO wpfControlTO)
            {
                foreach (WpfTo wpfToItem in ListWpfControlTO.Children)
                {
                    if (wpfToItem.Children.Contains(wpfControlTO))
                    {
                        return wpfToItem;
                    }
                }
                return null;
            }
        }
        public DiagamToWork? CheckInDeletedDiagrams(TechOperationWork techOperationWork)
        {
            return DeletedDiagrams.FirstOrDefault(d => d.techOperationWork == techOperationWork);
        }
        public void DeleteFromDeletedDiagrams(DiagamToWork diagramToWork)
        {
            DeletedDiagrams.Remove(diagramToWork);
        }

        public List<TechOperationWork> GetAvailableTechOperationWorks()
        {
            var allTOWs = TechOperationWorksList;
            var notAvailableTOWs = new List<TechOperationWork>();
            var availableTOWs = new List<TechOperationWork>();
            // получаем все tow из уже добавленных в WpfTo
            foreach(DiagamToWork dtw in GetAllDiagramToWorks())
            {
                notAvailableTOWs.Add(dtw.techOperationWork);
            }
            // получаем доступные tow
            foreach (var tow in allTOWs)
            {
                if (!notAvailableTOWs.Contains(tow))
                {
                    availableTOWs.Add(tow);
                }
            }

            return availableTOWs;
        }

        public List<DiagamToWork> GetAllDiagramToWorks()
        {
            var diagramToWorks = new List<DiagamToWork>();
            // получаем все tow из уже добавленных в WpfTo
            foreach (WpfTo wpfToItem in ListWpfControlTO.Children)
            {
                foreach (var wpfControlToItem in wpfToItem.Children)
                {
                    if (wpfControlToItem.diagamToWork != null)
                    {
                        diagramToWorks.Add(wpfControlToItem.diagamToWork);
                    }
                }
            }
            return diagramToWorks;
        }
        /// <summary>
        /// Проверяет, что кол-во DiagramToWork в ТК меньше чем кол-во TechOperationWorks
        /// </summary>
        /// <returns>true - если, TechOperationWorks больше чем  DiagramToWork. Если кол-во равно выдаёт сообщение о том, что все ТО представлены на блок-схеме</returns>
        public bool CheckIfTOIsAvailable()
        {
            var allDiagramToWorks = GetAllDiagramToWorks();
            if (allDiagramToWorks.Count >= TechOperationWorksList.Count)
            {
                System.Windows.Forms.MessageBox.Show("Все существующие в ТК операции уже представлены на блок-схеме");
                return false;
            }
            return true;
        }
    }

    

}

