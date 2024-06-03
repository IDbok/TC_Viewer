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
    /// Логика взаимодействия для WpfPosledovatelnost.xaml
    /// </summary>
    public partial class WpfPosledovatelnost : System.Windows.Controls.UserControl
    {
        private TechOperationWork selectedItem;
        WpfParalelno wpfParalelno;

        public WpfPosledovatelnost()
        {
            InitializeComponent();
        }

        public WpfPosledovatelnost(TechOperationWork selectedItem, WpfParalelno _wpfParalelno)
        {
            InitializeComponent();
            this.selectedItem = selectedItem;
            wpfParalelno = _wpfParalelno;

            ListWpfShag.Children.Clear();
            ListWpfShag.Children.Add(new WpfShag(selectedItem, this));
            wpfParalelno.Nomeraciya();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfShag.Children.Add(new WpfShag(selectedItem, this));

            wpfParalelno.Nomeraciya();
        }

        public void DeleteItem(WpfShag Item)
        {
            ListWpfShag.Children.Remove(Item);

            if(ListWpfShag.Children.Count==0)
            {
                wpfParalelno.DeletePosledovatelnost(this);
            }

            wpfParalelno.Nomeraciya();
        }
    }
}
