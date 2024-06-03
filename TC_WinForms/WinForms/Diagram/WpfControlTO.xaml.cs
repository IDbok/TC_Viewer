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
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class WpfControlTO : System.Windows.Controls.UserControl
    {
        public List<TechOperationWork> TechOperationWorksList;
        WpfMainControl wpfMainControl;

        public WpfControlTO()
        {
            InitializeComponent();
        }

        public WpfControlTO(List<TechOperationWork> techOperationWorksList, WpfMainControl _wpfMainControl)
        {
            InitializeComponent();

            TechOperationWorksList = techOperationWorksList;
            wpfMainControl = _wpfMainControl;

            foreach (TechOperationWork? item in TechOperationWorksList.OrderBy(o=>o.Order).ToList())
            {
                ComboBoxTO.Items.Add(item);
            }
            wpfMainControl.Nomeraciya();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, this));
            wpfMainControl.Nomeraciya();
        }

        private void ComboBoxTO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ComboBoxTO.SelectedItem != null)
            {
                ListWpfParalelno.Visibility = Visibility.Visible;
                ButtonAddShag.Visibility = Visibility.Visible;

                ListWpfParalelno.Children.Clear();
                ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, this));

                ComboBoxTO.IsReadOnly = true;
                ComboBoxTO.IsEnabled = false;
                Nomeraciya();
            }
        }

        public void DeteteParalelno(WpfParalelno paralelno)
        {
            ListWpfParalelno.Children.Remove(paralelno);

            if (ListWpfParalelno.Children.Count == 0)
            {
                wpfMainControl.DeleteControlTO(this);
            }
        }

        internal void Nomeraciya()
        {
            wpfMainControl.Nomeraciya();
        }
    }
}
