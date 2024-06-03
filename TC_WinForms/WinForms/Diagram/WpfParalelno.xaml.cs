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
        WpfControlTO wpfControlTO;

        public WpfParalelno()
        {
            InitializeComponent();
        }

        public WpfParalelno(TechOperationWork selectedItem, WpfControlTO _wpfControlTO)
        {
            InitializeComponent();

            this.selectedItem = selectedItem;
            wpfControlTO = _wpfControlTO;

            ListWpfPosledovatelnost.Children.Clear();
            ListWpfPosledovatelnost.Children.Add(new WpfPosledovatelnost(selectedItem,this));
            wpfControlTO.Nomeraciya();
        }           

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfPosledovatelnost.Children.Add(new WpfPosledovatelnost(selectedItem, this));
            wpfControlTO.Nomeraciya();
        }

        public void DeletePosledovatelnost(WpfPosledovatelnost wpfPosledovatelnost)
        {
            ListWpfPosledovatelnost.Children.Remove(wpfPosledovatelnost);

            if(ListWpfPosledovatelnost.Children.Count==0)
            {
                wpfControlTO.DeteteParalelno(this);
            }
        }

        internal void Nomeraciya()
        {
            wpfControlTO.Nomeraciya();
        }
    }
}
