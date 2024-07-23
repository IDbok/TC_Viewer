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
        //WpfMainControl _wpfMainControl;
        public DiagamToWork diagamToWork;
        public  WpfMainControl _wpfMainControl; // через этот объект осуществляется добавление DiagamToWork в TechnologicalCard
        public bool New=false;

        public WpfControlTO()
        {
            InitializeComponent();
        }

        public WpfControlTO(WpfMainControl wpfMainControl, DiagamToWork diagamToWork)
        {
            InitializeComponent();

            this._wpfMainControl = wpfMainControl;
            TechOperationWorksList = wpfMainControl.TechOperationWorksList; //techOperationWorksList;

            //if (_diagamToWork == null)
            //{
            //    New = true;
            //    diagamToWork = new DiagamToWork();
            //}
            this.diagamToWork = diagamToWork;
            if (diagamToWork.Id == 0)
            {
                New = true;
            }

            //foreach (TechOperationWork? item in TechOperationWorksList.OrderBy(o => o.Order).ToList())
            //{
            //    ComboBoxTO.Items.Add(item); // todo: чтобы убрать возможность выбора уже отображаемых ТО из ComboBoxTO, нужно добавить проверку наличия item в diagamToWork
            //}

            UpdateDiagramToWork();
        }

        private void UpdateDiagramToWork()
        {
            if (this.diagamToWork.techOperationWork != null)
            {
                ComboBoxTO.Items.Clear();
                ComboBoxTO.Items.Add(this.diagamToWork.techOperationWork);
                ComboBoxTO.SelectedItem = this.diagamToWork.techOperationWork;
                ComboBoxTO.IsReadOnly = true;
                ComboBoxTO.IsEnabled = false;

                ListWpfParalelno.Visibility = Visibility.Visible;
                ButtonAddShag.Visibility = Visibility.Visible;


                _wpfMainControl.technologicalCard.DiagamToWork.Add(this.diagamToWork);

                ListWpfParalelno.Children.Clear();

                if (this.diagamToWork.ListDiagramParalelno.Count == 0)
                {
                    ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, this));
                }

                foreach (DiagramParalelno diagramParalelno in this.diagamToWork.ListDiagramParalelno)
                {
                    ListWpfParalelno.Children.Add(new WpfParalelno(diagramParalelno.techOperationWork, this, diagramParalelno));
                }

            }

            ParallelButtonsVisibility(this.diagamToWork.ParallelIndex != null);
            this._wpfMainControl.Nomeraciya();
        }


        public void ParallelButtonsVisibility(bool isVisible)
        {
            pnlParallelButtons.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, this));
            _wpfMainControl.diagramForm.HasChanges = true;
            _wpfMainControl.Nomeraciya();
        }

        private void ComboBoxTO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxTO.SelectedItem != null)
            {
                if (diagamToWork.techOperationWork != null)
                {
                    return;
                }

                ListWpfParalelno.Visibility = Visibility.Visible;
                ButtonAddShag.Visibility = Visibility.Visible;

                var techOperationWork = (TechOperationWork)ComboBoxTO.SelectedItem;
                var deletedDiagramToWork = _wpfMainControl.CheckInDeletedDiagrams(techOperationWork);

                if (deletedDiagramToWork != null)
                {
                    _wpfMainControl.DeleteFromDeletedDiagrams(deletedDiagramToWork);

                    if(this.diagamToWork.ParallelIndex != null)
                        deletedDiagramToWork.ParallelIndex = this.diagamToWork.ParallelIndex;

                    this.diagamToWork = deletedDiagramToWork;

                    UpdateDiagramToWork();
                    //_wpfMainControl.technologicalCard.DiagamToWork.Add(deletedDiagramToWork);
                    //_wpfMainControl.DeleteFromDeletedDiagrams(deletedDiagramToWork);
                }
                else
                {
                    diagamToWork.techOperationWork = techOperationWork;
                    _wpfMainControl.technologicalCard.DiagamToWork.Add(diagamToWork);

                    ListWpfParalelno.Children.Clear();
                    ListWpfParalelno.Children.Add(new WpfParalelno(techOperationWork, this));
                    _wpfMainControl.diagramForm.HasChanges = true;

                    ComboBoxTO.IsReadOnly = true;
                    ComboBoxTO.IsEnabled = false;
                    Nomeraciya();
                }
            }

            //if (ComboBoxTO.SelectedItem != null)
            //{
            //    if (diagamToWork.techOperationWork != null)
            //    {
            //        return;
            //    }

            //    ListWpfParalelno.Visibility = Visibility.Visible;
            //    ButtonAddShag.Visibility = Visibility.Visible;

            //    diagamToWork.techOperationWork = (TechOperationWork)ComboBoxTO.SelectedItem;
            //    _wpfMainControl.technologicalCard.DiagamToWork.Add(diagamToWork);

            //    ListWpfParalelno.Children.Clear();
            //    ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, this));
            //    _wpfMainControl.diagramForm.HasChanges = true;

            //    ComboBoxTO.IsReadOnly = true;
            //    ComboBoxTO.IsEnabled = false;
            //    Nomeraciya();
            //}
        }

        public void DeteteParalelno(WpfParalelno paralelno)
        {
            ListWpfParalelno.Children.Remove(paralelno);           

            _wpfMainControl.diagramForm.HasChanges = true;

            if (ListWpfParalelno.Children.Count == 0)
            {
                _wpfMainControl.technologicalCard.DiagamToWork.Remove(diagamToWork);
                _wpfMainControl.DeleteControlTO(this);
            }
        }

        internal void Nomeraciya()
        {
            if (_wpfMainControl != null)
            {
                _wpfMainControl.Nomeraciya();
            }
        }

        internal void Order(int v, WpfParalelno wpfParalelno)
        {
            if (v == 1)
            {
                int ib = ListWpfParalelno.Children.IndexOf(wpfParalelno);
                if (ib < ListWpfParalelno.Children.Count - 1)
                {
                    var cv = ListWpfParalelno.Children[ib + 1];
                    ListWpfParalelno.Children.Remove(cv);
                    ListWpfParalelno.Children.Insert(ib, cv);

                    _wpfMainControl.Nomeraciya();
                }
            }

            if (v == 2)
            {
                int ib = ListWpfParalelno.Children.IndexOf(wpfParalelno);
                if (ib != 0)
                {
                    var cv = ListWpfParalelno.Children[ib];
                    ListWpfParalelno.Children.Remove(cv);
                    ListWpfParalelno.Children.Insert(ib - 1, cv);
                    _wpfMainControl.Nomeraciya();
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _wpfMainControl.Order(1, this);
            _wpfMainControl.diagramForm.HasChanges = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _wpfMainControl.Order(2, this);
            _wpfMainControl.diagramForm.HasChanges = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            _wpfMainControl.diagramForm.HasChanges = true;

            diagamToWork.ParallelIndex = null;
            _wpfMainControl.technologicalCard.DiagamToWork.Remove(diagamToWork);

            _wpfMainControl.DeleteControlTO(this);
        }

        private void btnMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void btnMoveRight_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void ComboBoxTO_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var availableTechOperationWorks = _wpfMainControl.GetAvailableTechOperationWorks();
               
            foreach (TechOperationWork? item in availableTechOperationWorks.OrderBy(o => o.Order).ToList())
            {
                ComboBoxTO.Items.Add(item); // todo: чтобы убрать возможность выбора уже отображаемых ТО из ComboBoxTO, нужно добавить проверку наличия item в diagamToWork
            }
        }

    }
}
