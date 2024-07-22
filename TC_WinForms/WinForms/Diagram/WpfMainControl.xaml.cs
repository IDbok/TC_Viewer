using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram
{
    /// <summary>
    /// Логика взаимодействия для WpfMainControl.xaml
    /// </summary>
    public partial class WpfMainControl : System.Windows.Controls.UserControl
    {

        public MyDbContext context;

        private int tcId;
        public TechnologicalCard technologicalCard;

        public TechnologicalCard TehCarta; // todo: непонятно, зачем это поле
        public List<TechOperationWork> TechOperationWorksList;
        public DiagramForm diagramForm; // используется только для проверки (фиксации) наличия изменений

        public WpfMainControl()
        {
            InitializeComponent();
        }

        public WpfMainControl(int tcId, DiagramForm _diagramForm)
        {
            InitializeComponent();
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


           var ListShag = context.DiagamToWork.Where(w=>w.technologicalCard == technologicalCard)

                 .Include(i => i.ListDiagramParalelno)
                .ThenInclude(ie => ie.techOperationWork)

                .Include(i=>i.ListDiagramParalelno)
                .ThenInclude(i=>i.ListDiagramPosledov)
                .ThenInclude(i => i.ListDiagramShag)
                .ThenInclude(i=>i.ListDiagramShagToolsComponent)
                             
                .ToList();

            // Сгруппировать по ParallelIndex, если ParallelIndex = null, то записать в отдельную группу
            var ListShagGroup = ListShag.GroupBy(g => g.ParallelIndex).ToList();

            foreach (var ListShagItem in ListShagGroup)
            {
                bool isNull = ListShagItem.Key == null;

                if (!isNull)
                {
                    var wpfTo = new WpfTo(this);

                    ListWpfControlTO.Children.Add(wpfTo);

                    foreach (DiagamToWork item in ListShagItem.ToList())
                    {
                        wpfTo.AddParallelTO(item);
                    }
                }
                else
                {
                    foreach (DiagamToWork item in ListShagItem.ToList())
                    {
                        var wpfTo = new WpfTo(this, item);

                        ListWpfControlTO.Children.Add(wpfTo);
                    }
                }


            }

            //foreach (DiagamToWork item in ListShag)
            //{
            //    ListWpfControlTO.Children.Add(new WpfControlTO(this, item)); // ListWpfControlTO - это StackPanel в WpfMainControl.xaml
            //    Nomeraciya();
            //}

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfControlTO.Children.Add(new WpfControlTO(this));
            diagramForm.HasChanges = true;
            Nomeraciya();
        }

        public void DeleteControlTO(WpfControlTO controlTO)
        {
            ListWpfControlTO.Children.Remove(controlTO);
            try
            {
                
            }
            catch (Exception)
            {

            }
            
            diagramForm.HasChanges = true;

            if (ListWpfControlTO.Children.Count==0)
            {
                if (context.DiagamToWork.SingleOrDefault(s => s == controlTO.diagamToWork) != null)
                    context.DiagamToWork.Remove(controlTO.diagamToWork);

                ListWpfControlTO.Children.Add(new WpfControlTO( this));
                Nomeraciya();
            }                                
        }

        internal void Nomeraciya() // todo : раскомментировать
        {
            //int nomer = 1;

            //int Order1 = 1; // порядковый номер отображения TO
            //int Order2 = 1; // порядковый номер отображения параллельных операций
            //int Order3 = 1; 
            //int Order4 = 1;

            //foreach (WpfControlTO item in ListWpfControlTO.Children)
            //{
            //   if(item.diagamToWork!=null) item.diagamToWork.Order = Order1;
            //    Order1++;
            //    Order2 = 1;
            //    Order3 = 1;
            //    Order4 = 1;

            //    foreach (WpfParalelno item2 in item.ListWpfParalelno.Children)
            //    {
            //        if (item2.diagramParalelno != null) item2.diagramParalelno.Order = Order2;
            //        Order3 = 1;
            //        Order4 = 1;

            //        foreach (WpfPosledovatelnost item3 in item2.ListWpfPosledovatelnost.Children)
            //        {
            //            if (item3.diagramPosledov != null) item3.diagramPosledov.Order = Order3;
            //            Order4 = 1;

            //            foreach (WpfShag item4 in item3.ListWpfShag.Children)
            //            {
            //                if (item4.diagramShag != null) item4.diagramShag.Order = Order4;
            //                item4.SetNomer(nomer);
            //                nomer++;
            //            }
            //        }
            //    }
            //}
        }

        public void Save()
        {
            try
            {
                foreach (WpfControlTO item in ListWpfControlTO.Children)
                {
                    foreach (WpfParalelno item2 in item.ListWpfParalelno.Children)
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
            catch (Exception)
            {

            }       




            try
            {
                //foreach (WpfControlTO item in ListWpfControlTO.Children)
                //{
                //    if(item.New && item.diagamToWork.techOperationWork!=null)
                //    {
                //        context.DiagamToWork.Add(item.diagamToWork);
                //    }
                //}

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
            if (v == 1)
            {
                int ib = ListWpfControlTO.Children.IndexOf(wpfControlTO);
                if (ib < ListWpfControlTO.Children.Count - 1)
                {
                    var cv = ListWpfControlTO.Children[ib + 1];
                    ListWpfControlTO.Children.Remove(cv);
                    ListWpfControlTO.Children.Insert(ib, cv);

                    Nomeraciya();
                }
            }

            if (v == 2)
            {
                int ib = ListWpfControlTO.Children.IndexOf(wpfControlTO);
                if (ib != 0)
                {
                    var cv = ListWpfControlTO.Children[ib];
                    ListWpfControlTO.Children.Remove(cv);
                    ListWpfControlTO.Children.Insert(ib - 1, cv);
                    Nomeraciya();
                }
            }
        }
    }
}
