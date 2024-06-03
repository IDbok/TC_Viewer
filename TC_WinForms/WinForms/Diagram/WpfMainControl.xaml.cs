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


        public TechnologicalCard TehCarta;
        public List<TechOperationWork> TechOperationWorksList;

        public WpfMainControl()
        {
            InitializeComponent();
        }

        public WpfMainControl(int tcId)
        {
            InitializeComponent();
            this.tcId = tcId;

            context = new MyDbContext();


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

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfControlTO.Children.Add(new WpfControlTO(TechOperationWorksList,this));
            Nomeraciya();
        }

        public void DeleteControlTO(WpfControlTO controlTO)
        {
            ListWpfControlTO.Children.Remove(controlTO);

            if(ListWpfControlTO.Children.Count==0)
            {
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
    }
}
