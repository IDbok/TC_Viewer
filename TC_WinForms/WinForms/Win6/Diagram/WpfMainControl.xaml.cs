using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram
{
	/// <summary>F
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
        public List<DiagamToWork> DiagramToWorkList;


        public DiagramForm diagramForm; // используется только для проверки (фиксации) наличия изменений

        private List<DiagamToWork> DeletedDiagrams = new List<DiagamToWork>();
        private List<TechOperationWork> AvailableTechOperationWorks = new List<TechOperationWork>();

        public ObservableCollection<WpfTo> Children { get; set; } = new();

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

        public WpfMainControl(int tcId, DiagramForm _diagramForm, TcViewState tcViewState, MyDbContext context)
        {
            InitializeComponent();
            DataContext = this;

            _tcViewState = tcViewState;

            this.tcId = tcId;
            diagramForm = _diagramForm;
            this.context = context;

            _tcViewState.ViewModeChanged += OnViewModeChanged;
        }
        //private async Task LoadData()
        //{
        //    double tcLoad = 0;
        //    double towLoad = 0;
        //    double dtwLoad = 0;

        //    var sw = new System.Diagnostics.Stopwatch();

        //    try
        //    {
        //        sw.Start();

        //        technologicalCard = await context.TechnologicalCards.FirstAsync(x => x.Id == tcId);

        //        _tcViewState.TechnologicalCard = technologicalCard;

        //        tcLoad = sw.Elapsed.TotalMilliseconds;
        //        sw.Restart();

        //        TechOperationWorksList = await context.TechOperationWorks.Where(w => w.TechnologicalCardId == tcId)
        //               .Include(i => i.techOperation)
        //               .ToListAsync();

        //        //список ID Технологических операций
        //        var towIds = TechOperationWorksList.Select(t => t.Id).ToList();

        //        //Получаем список всех компонентов которые принадлежат карте
        //        var componentWorks = await context.ComponentWorks.Where(c => towIds.Any(o => o == c.techOperationWorkId))
        //           .Include(t => t.component)
        //           .ToListAsync();

        //        //Получаем список всех инструментов, которые принадлежат карте
        //        var toolWorks = await context.ToolWorks.Where(c => towIds.Any(o => o == c.techOperationWorkId))
        //            .Include(t => t.tool)
        //            .ToListAsync();

        //        //Получаем список всех ExecutionWorks для технологической карты
        //        var executionWorks = await
        //           context.ExecutionWorks.Where(e => towIds.Any(o => o == e.techOperationWorkId))
        //                                 .Include(e => e.techTransition)
        //                                 .Include(e => e.Protections)
        //                                 .Include(e => e.Machines)
        //                                 .Include(e => e.Staffs)
        //                                 .ToListAsync();

        //        towLoad = sw.Elapsed.TotalMilliseconds;
                
        //        sw.Restart();

        //        DiagramToWorkList = await context.DiagamToWork.Where(w => w.technologicalCard == technologicalCard).ToListAsync();



        //        var listDiagramParalelno = await context.DiagramParalelno.Where(p => DiagramToWorkList.Select(i => i.Id).Contains(p.DiagamToWorkId))
        //                                                                 .Include(ie => ie.techOperationWork)
        //                                                                 .ToListAsync();

        //        var listDiagramPosledov = await context.DiagramPosledov.Where(p => listDiagramParalelno.Select(i => i.Id).Contains(p.DiagramParalelnoId))
        //            .ToListAsync();

        //        var listDiagramShag = await context.DiagramShag.Where(d => listDiagramPosledov.Select(i => i.Id).Contains(d.DiagramPosledovId))
        //            .Include(q => q.ListDiagramShagToolsComponent)
        //            .ToListAsync();

        //        dtwLoad = sw.Elapsed.TotalMilliseconds;
        //        sw.Stop();

        //        if (Program.IsTestMode)
        //            System.Windows.Forms.MessageBox.Show($"TC: {tcLoad} ms, TOW: {towLoad} ms, DTW: {dtwLoad} ms");
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
        //    }
        //}
        private void AddDiagramsToChildren()
        {
            AvailableTechOperationWorks.Clear();

            foreach (var tow in TechOperationWorksList)
            {
                AvailableTechOperationWorks.Add(tow);
            }

            // Сгруппировать по ParallelIndex, если ParallelIndex = null, то записать в отдельную группу
            var dTOWGroups = DiagramToWorkList
                .GroupBy(g => g.ParallelIndex != null ? g.GetParallelIndex() : g.Order.ToString())
                .ToList();

            // сгруппировать ListShagGroup по Order DiagramToWork внутри группы
            dTOWGroups = dTOWGroups.OrderBy(o => o.FirstOrDefault()!.Order).ToList();

            foreach (var dTOWGroup in dTOWGroups)
            {
                bool isNull = dTOWGroup.Key == null;

                if (!isNull)
                {
                    AddDiagramsToChildren(dTOWGroup.OrderBy(x => x.Order).ToList());
                    //var wpfTo = new WpfTo(this, _tcViewState, dTOWGroup.OrderBy(x => x.Order).ToList());

                    //Children.Add(wpfTo);
                }
                else
                {
                    foreach (DiagamToWork item in dTOWGroup.ToList())
                    {
                        AddDiagramsToChildren(item);
                        //var wpfTo = new WpfTo(this, _tcViewState, item);

                        //Children.Add(wpfTo);
                    }
                }
            }
        }
        public void AddDiagramsToChildren(List<DiagamToWork> diagamToWorks, int? indexPosition = null)
        {
            var wpfTo = new WpfTo(this, _tcViewState, diagamToWorks);

            if (indexPosition != null)
            {
                Children.Insert(indexPosition.Value, wpfTo);
            }
            else
            {
                Children.Add(wpfTo);
            }
        }
        public void AddDiagramsToChildren(DiagamToWork diagamToWork, int? indexPosition = null)
        {
            AddDiagramsToChildren(new List<DiagamToWork> { diagamToWork }, indexPosition);
        }




        private async void OnLoad(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            
            //await LoadData();
            technologicalCard = _tcViewState.TechnologicalCard;
            TechOperationWorksList = _tcViewState.TechOperationWorksList;


            DiagramToWorkList = _tcViewState.DiagramToWorkList;



            AddDiagramsToChildren();
            Nomeraciya();

            this.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfTOIsAvailable())
            {
                Children.Add(new WpfTo(this, _tcViewState));
                diagramForm.HasChanges = true;
                Nomeraciya();
            }
        }

        public void DeleteControlTO(WpfControlTO controlTO)
        {
            // найти и удалить controlTO из ListWpfControlTO (WpfTo)
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                WpfTo wpfTo = Children[i];
                for (int j = wpfTo.Children.Count - 1; j >= 0; j--)
                {
                    var wpfToSq = wpfTo.Children[j];
                    if (wpfToSq.Children.Contains(controlTO))
                    {
                        DeletedDiagrams.Add(controlTO.diagamToWork);
                        technologicalCard.DiagamToWork.Remove(controlTO.diagamToWork);

                        wpfToSq.DeleteWpfControlTO(controlTO);

                        Nomeraciya();
                        break;
                    }
                }
            }

        }

        internal void Nomeraciya()
        {
            int nomer = 1;

            int Order1 = 1; // порядковый номер отображения TO
            int Order2 = 1; // порядковый номер отображения параллельных операций
            int Order3 = 1;
            int Order4 = 1;

            foreach (WpfTo wpfToItem in Children)
            {
                foreach (var wpfToSq in wpfToItem.Children)
                    foreach(var wpfControlToItem in wpfToSq.Children)
                    {
                        if (wpfControlToItem.diagamToWork != null) wpfControlToItem.diagamToWork.Order = Order1;

                        Order1++;
                        Order2 = 1;
                        Order3 = 1;
                        Order4 = 1;

                        foreach (WpfParalelno wpfParallelItem in wpfControlToItem.Children)
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
                                    if (item4.diagramShag != null)
                                    {
                                        item4.diagramShag.Order = Order4++;
                                    }
                                    // todo: проверка на то, что номер не изменился
                                    item4.SetNomer(nomer);
                                    nomer++;
                                }
                            }
                        }
                    }
                
            }

            if(!_tcViewState.IsViewMode)
            {
                diagramForm.HasChanges = true;
            }   
        }

        public void SaveOnDispose()//здесь не применяется операция сохранения в контексте, используется для сохранения изменений в диаграмме(добавления, перемещения и т.п.)
        {
            Nomeraciya();
            try
            {
                foreach (WpfTo wpfToItem in Children)
                    foreach (var wpfToSq in wpfToItem.Children)
                        foreach (var wpfControlToItem in wpfToSq.Children)
                            foreach (WpfParalelno item2 in wpfControlToItem.Children)
                                foreach (WpfPosledovatelnost item3 in item2.ListWpfPosledovatelnost.Children)
                                    foreach (WpfShag item4 in item3.ListWpfShag.Children)
                                    {
                                        item4.SaveCollection();
                                    }

                diagramForm.HasChanges = false;

                foreach (DiagamToWork item in DeletedDiagrams)
                {
                    item.techOperationWork = null;
                }

                var bbn = context.DiagamToWork.Where(w => w.techOperationWork == null).ToList();
                foreach (DiagamToWork item in bbn)
                {
                    context.DiagamToWork.Remove(item);
                }

                _tcViewState.DiagramToWorkList = technologicalCard.DiagamToWork;
                _tcViewState.TechnologicalCard.DiagamToWork = technologicalCard.DiagamToWork;
            }
            catch (Exception)
            {

            }
        }

        public void Save()
        {
            Nomeraciya();
            try
            {
                foreach (WpfTo wpfToItem in Children)
                    foreach (var wpfToSq in wpfToItem.Children)
                        foreach (var wpfControlToItem in wpfToSq.Children)
                            foreach (WpfParalelno item2 in wpfControlToItem.Children)
                                foreach (WpfPosledovatelnost item3 in item2.ListWpfPosledovatelnost.Children)
                                    foreach (WpfShag item4 in item3.ListWpfShag.Children)
                                    {
                                        item4.SaveCollection();
                                    }

                diagramForm.HasChanges = false;
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


                _tcViewState.DiagramToWorkList = technologicalCard.DiagamToWork;
                _tcViewState.TechnologicalCard.DiagamToWork = technologicalCard.DiagamToWork;


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
                int ib = Children.IndexOf(wpfToContainer);
                if (ib < Children.Count - 1)
                {
                    var cv = Children[ib + 1];
                    Children.Remove(cv);
                    Children.Insert(ib, cv);
                }
            }

            if (v == 2)
            {
                int ib = Children.IndexOf(wpfToContainer);
                if (ib != 0)
                {
                    var cv = Children[ib];
                    Children.Remove(cv);
                    Children.Insert(ib - 1, cv);
                }
            }

            Nomeraciya();


            WpfTo? FindContainer(WpfControlTO wpfControlTO)
            {
                foreach (WpfTo wpfToItem in Children)
                {
                    foreach(var wpfTOSq in wpfToItem.Children)
                    {
                        if (wpfTOSq.Children.Contains(wpfControlTO))
                        {
                            return wpfToItem;
                        }
                    }
                }
                return null;
            }
        }
        public void ChangeOrder(WpfTo wpfTo, MoveDirection direction)
        {

            var index = Children.IndexOf(wpfTo);

            switch (direction)
            {
                case MoveDirection.Up:
                    if (index > 0)
                    {
                        Children.Move(index, index - 1);
                    }
                    break;

                case MoveDirection.Down:
                    if (index < Children.Count - 1)
                    {
                        Children.Move(index, index + 1);
                    }
                    break;
            }

            Nomeraciya();
        }
        public DiagamToWork? CheckInDeletedDiagrams(TechOperationWork techOperationWork)
        {
            return DeletedDiagrams.FirstOrDefault(d => d.techOperationWork == techOperationWork);
        }
        public void DeleteFromDeletedDiagrams(DiagamToWork diagramToWork)
        {
            DeletedDiagrams.Remove(diagramToWork);
            technologicalCard.DiagamToWork.Add(diagramToWork);
        }

        public List<TechOperationWork> GetAvailableTechOperationWorks()
        {
            var allTOWs = TechOperationWorksList;
            var notAvailableTOWs = new List<TechOperationWork>();
            var availableTOWs = new List<TechOperationWork>();
            // получаем все tow из уже добавленных в WpfTo
            var allDiagramToWorks = GetAllDiagramToWorks();
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
            foreach (WpfTo wpfToItem in Children)
            {
                foreach (var wpfToSq in wpfToItem.Children)
                {
                    foreach (var wpfControlToItem in wpfToSq.Children)
                    {
                        if (wpfControlToItem.diagamToWork != null)
                        {
                            diagramToWorks.Add(wpfControlToItem.diagamToWork);
                        }
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
        public void DeleteWpfTO(WpfTo wpfTo)
        {
            Children.Remove(wpfTo);
        }
        public void ReinitializeForm()
        {
            try
            {
				// удалить текущие данные
				Children.Clear();

				diagramForm.ReloadElementHost(new WpfMainControl(tcId, diagramForm, _tcViewState, context));

            }
            catch (Exception ex)
            {
                // Обработка ошибок
                System.Windows.Forms.MessageBox.Show($"Ошибка переинициализации формы: {ex.Message}");
            }
        }
		//private void UserControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		//{
		//	if (Keyboard.Modifiers == ModifierKeys.Control)
		//	{
		//		// Проверяем, является ли Transform ScaleTransform
		//		if (ContentScaleTransform is ScaleTransform scaleTransform)
		//		{
		//			if (e.Key == Key.OemPlus || e.Key == Key.Add) // Ctrl + '+'
		//			{
		//				scaleTransform.ScaleX += 0.1;
		//				scaleTransform.ScaleY += 0.1;
		//				e.Handled = true;
		//			}
		//			else if (e.Key == Key.OemMinus || e.Key == Key.Subtract) // Ctrl + '-'
		//			{
		//				scaleTransform.ScaleX = Math.Max(0.1, scaleTransform.ScaleX - 0.1);
		//				scaleTransform.ScaleY = Math.Max(0.1, scaleTransform.ScaleY - 0.1);
		//				e.Handled = true;
		//			}
		//		}
		//	}
		//}





	}



}

