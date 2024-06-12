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
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram
{
    /// <summary>
    /// Логика взаимодействия для WpfParalelno.xaml
    /// </summary>
    public partial class WpfParalelno : System.Windows.Controls.UserControl
    {
        private TechOperationWork selectedItem;
       public WpfControlTO wpfControlTO;

        public DiagramParalelno diagramParalelno;

        public WpfParalelno()
        {
            InitializeComponent();
        }

        public WpfParalelno(TechOperationWork selectedItem, WpfControlTO _wpfControlTO, DiagramParalelno _diagramParalelno = null)
        {
            InitializeComponent();

            if (_diagramParalelno == null)
            {
                diagramParalelno = new DiagramParalelno();
                diagramParalelno.techOperationWork = selectedItem;
                _wpfControlTO.diagamToWork.ListDiagramParalelno.Add(diagramParalelno);
            }
            else
            {
                diagramParalelno = _diagramParalelno;
            }

            this.selectedItem = selectedItem;
            wpfControlTO = _wpfControlTO;

            ListWpfPosledovatelnost.Children.Clear();

            if (diagramParalelno.ListDiagramPosledov.Count == 0)
            {
                ListWpfPosledovatelnost.Children.Add(new WpfPosledovatelnost(selectedItem, this));
            }
            else
            {
                foreach (DiagramPosledov diagramPosledov in diagramParalelno.ListDiagramPosledov)
                {
                    ListWpfPosledovatelnost.Children.Add(new WpfPosledovatelnost(selectedItem, this, diagramPosledov));
                }
            }

            wpfControlTO.Nomeraciya();




        }           

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfPosledovatelnost.Children.Add(new WpfPosledovatelnost(selectedItem, this));
            wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
            wpfControlTO.Nomeraciya();
        }

        public void DeletePosledovatelnost(WpfPosledovatelnost wpfPosledovatelnost)
        {
            ListWpfPosledovatelnost.Children.Remove(wpfPosledovatelnost);
            
            wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;

            if (ListWpfPosledovatelnost.Children.Count==0)
            {
                wpfControlTO.diagamToWork.ListDiagramParalelno.Remove(diagramParalelno);
                wpfControlTO.DeteteParalelno(this);
            }
        }

        internal void Nomeraciya()
        {
            wpfControlTO.Nomeraciya();
        }
    }
}
