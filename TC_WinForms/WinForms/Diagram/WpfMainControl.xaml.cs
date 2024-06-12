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

        public TechnologicalCard TehCarta;
        public List<TechOperationWork> TechOperationWorksList;
        public DiagramForm diagramForm;

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
                    .Include(r => r.executionWorks).ThenInclude(t => t.ListexecutionWorkRepeat2)
                   .Include(r => r.ToolWorks).ThenInclude(r => r.tool).ToList();


           var ListShag = context.DiagamToWork.Where(w=>w.technologicalCard == technologicalCard)

                 .Include(i => i.ListDiagramParalelno)
                .ThenInclude(ie => ie.techOperationWork)

                .Include(i=>i.ListDiagramParalelno)
                .ThenInclude(i=>i.ListDiagramPosledov)
                .ThenInclude(i => i.ListDiagramShag)
                .ThenInclude(i=>i.ListDiagramShagToolsComponent)
                             
                .ToList();

            foreach (DiagamToWork item in ListShag)
            {
                ListWpfControlTO.Children.Add(new WpfControlTO(TechOperationWorksList, this, item));
                Nomeraciya();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfControlTO.Children.Add(new WpfControlTO(TechOperationWorksList,this));
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

                ListWpfControlTO.Children.Add(new WpfControlTO(TechOperationWorksList, this));
                Nomeraciya();
            }                                
        }

        internal void Nomeraciya()
        {
            int nomer = 1;

            foreach (WpfControlTO item in ListWpfControlTO.Children)
            {
                foreach (WpfParalelno item2 in item.ListWpfParalelno.Children)
                {
                    foreach (WpfPosledovatelnost item3 in item2.ListWpfPosledovatelnost.Children)
                    {
                        foreach (WpfShag item4 in item3.ListWpfShag.Children)
                        {
                            item4.SetNomer(nomer);
                            nomer++;
                        }
                    }
                }
            }
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
    }
}
