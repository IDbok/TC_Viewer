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
        public DiagamToWork diagamToWork;
        public  WpfMainControl _wpfMainControl;
        public bool New=false;

        public WpfControlTO()
        {
            InitializeComponent();
        }

        public WpfControlTO(List<TechOperationWork> techOperationWorksList, WpfMainControl _wpfMainControl, DiagamToWork _diagamToWork = null)
        {
            InitializeComponent();

            this._wpfMainControl = _wpfMainControl;

            if (_diagamToWork == null)
            {
                New = true;
                diagamToWork = new DiagamToWork();
            }
            else
            {
                diagamToWork = _diagamToWork;

                if(diagamToWork.techOperationWork!=null)
                {
                    ListWpfParalelno.Visibility = Visibility.Visible;
                    ButtonAddShag.Visibility = Visibility.Visible;
                    _wpfMainControl.technologicalCard.DiagamToWork.Add(diagamToWork);

                    ListWpfParalelno.Children.Clear();

                    if (diagamToWork.ListDiagramParalelno.Count == 0)
                    {
                        ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, this));
                    }

                    ComboBoxTO.IsReadOnly = true;
                    ComboBoxTO.IsEnabled = false;
                    Nomeraciya();
                }
            }

            TechOperationWorksList = techOperationWorksList;
            wpfMainControl = _wpfMainControl;

            foreach (TechOperationWork? item in TechOperationWorksList.OrderBy(o=>o.Order).ToList())
            {
                ComboBoxTO.Items.Add(item);
            }
            wpfMainControl.Nomeraciya();

            if (diagamToWork.techOperationWork != null)
            {
                ComboBoxTO.SelectedItem = diagamToWork.techOperationWork;
            }


                foreach (DiagramParalelno diagramParalelno in diagamToWork.ListDiagramParalelno)
            {
                ListWpfParalelno.Children.Add(new WpfParalelno(diagramParalelno.techOperationWork, this, diagramParalelno));
                wpfMainControl.Nomeraciya();
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, this));
            _wpfMainControl.diagramForm.HasChanges = true;
            wpfMainControl.Nomeraciya();
        }

        private void ComboBoxTO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ComboBoxTO.SelectedItem != null)
            {
                if (diagamToWork.techOperationWork != null)
                {
                    return;
                }

                ListWpfParalelno.Visibility = Visibility.Visible;
                ButtonAddShag.Visibility = Visibility.Visible;

                diagamToWork.techOperationWork = (TechOperationWork)ComboBoxTO.SelectedItem;
                _wpfMainControl.technologicalCard.DiagamToWork.Add(diagamToWork);

                ListWpfParalelno.Children.Clear();
                ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, this));
                _wpfMainControl.diagramForm.HasChanges = true;

                ComboBoxTO.IsReadOnly = true;
                ComboBoxTO.IsEnabled = false;
                Nomeraciya();
            }
        }

        public void DeteteParalelno(WpfParalelno paralelno)
        {
            ListWpfParalelno.Children.Remove(paralelno);           

            _wpfMainControl.diagramForm.HasChanges = true;

            if (ListWpfParalelno.Children.Count == 0)
            {
                _wpfMainControl.technologicalCard.DiagamToWork.Remove(diagamToWork);
                wpfMainControl.DeleteControlTO(this);
            }
        }

        internal void Nomeraciya()
        {
            if (wpfMainControl != null)
            {
                wpfMainControl.Nomeraciya();
            }
        }
    }
}
