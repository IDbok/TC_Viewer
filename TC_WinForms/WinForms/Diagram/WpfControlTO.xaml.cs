using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class WpfControlTO : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private readonly DiagramState _diagramState;

        private readonly TcViewState _tcViewState;

        public List<TechOperationWork> TechOperationWorksList;
        //WpfMainControl _wpfMainControl;
        public DiagamToWork diagamToWork;
        public  WpfMainControl _wpfMainControl; // через этот объект осуществляется добавление DiagamToWork в TechnologicalCard
        public bool New=false;

        public event PropertyChangedEventHandler? PropertyChanged;
        public bool IsCommentViewMode => _tcViewState.IsCommentViewMode;
        private bool IsViewMode => _tcViewState.IsViewMode;
        public bool IsHiddenInViewMode => !IsViewMode;

        public WpfControlTO()
        {
            InitializeComponent();
        }

        public WpfControlTO(DiagramState diagramState, DiagamToWork diagramToWork) : this(diagramState.WpfMainControl, diagramToWork, diagramState.TcViewState)
        {
            _diagramState = new DiagramState(diagramState);
            _diagramState.DiagramToWork = diagramToWork;
            _diagramState.WpfControlTO = this;
        }
        public WpfControlTO(WpfMainControl wpfMainControl, DiagamToWork diagamToWork, TcViewState tcViewState)
        {
            if(_diagramState == null)
            {
                _diagramState = new DiagramState(wpfMainControl, tcViewState, diagamToWork);
                _diagramState.WpfControlTO = this;
            }

            InitializeComponent();
            DataContext = this;

            _tcViewState = tcViewState;

            this._wpfMainControl = wpfMainControl;
            TechOperationWorksList = wpfMainControl.TechOperationWorksList; //techOperationWorksList;

            this.diagamToWork = diagamToWork;
            if (diagamToWork.Id == 0)
            {
                New = true;
            }

            UpdateDiagramToWork();

            _tcViewState.ViewModeChanged += OnViewModeChanged;

            ButtonAddShag.IsEnabled = true; 
        }

        private void OnViewModeChanged()
        {
            OnPropertyChanged(nameof(IsHiddenInViewMode));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

                //if(!IsViewMode)
                //    ButtonAddShag.Visibility = Visibility.Visible;

                _wpfMainControl.technologicalCard.DiagamToWork.Add(this.diagamToWork);

                ListWpfParalelno.Children.Clear();

                if (this.diagamToWork.ListDiagramParalelno.Count == 0)
                {
                    ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, _diagramState)); // this, _tcViewState));
                }

                foreach (DiagramParalelno diagramParalelno in this.diagamToWork.ListDiagramParalelno.OrderBy(x => x.Order))
                {
                    ListWpfParalelno.Children.Add(new WpfParalelno(diagramParalelno.techOperationWork, _diagramState, diagramParalelno)); //this, _tcViewState, diagramParalelno));
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
            if (this.diagamToWork.techOperationWork == null)
                return;

            ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, _diagramState));// this, _tcViewState));
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
                    ListWpfParalelno.Children.Add(new WpfParalelno(techOperationWork, _diagramState));
                    _wpfMainControl.diagramForm.HasChanges = true;

                    // todo : вопрос, как быть с объектами Component and Tool, которые привязаны к конкретному TechOperationWork
                    ComboBoxTO.IsReadOnly = true;
                    ComboBoxTO.IsEnabled = false;

                    Nomeraciya();
                }

                ButtonAddShag.IsEnabled = true;
            }
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

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            _diagramState.WpfTo?.ChangeOrder(this, MoveDirection.Down);
            //_wpfMainControl.Order(1, this);
            //_wpfMainControl.diagramForm.HasChanges = true;
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            _diagramState.WpfTo?.ChangeOrder(this, MoveDirection.Up);

            //_wpfMainControl.Order(2, this);
            //_wpfMainControl.diagramForm.HasChanges = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            _wpfMainControl.diagramForm.HasChanges = true;

            diagamToWork.ParallelIndex = null;
            //_wpfMainControl.technologicalCard.DiagamToWork.Remove(diagamToWork);

            _wpfMainControl.DeleteControlTO(this);
        }

        private void btnMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            _diagramState.WpfTo?.ChangeOrder(this, MoveDirection.Left);
        }
        private void btnMoveRight_Click(object sender, RoutedEventArgs e)
        {
            _diagramState.WpfTo?.ChangeOrder(this, MoveDirection.Right);
        }
        private void ComboBoxTO_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ComboBoxTO.Items.Clear();

            var availableTechOperationWorks = _wpfMainControl.GetAvailableTechOperationWorks();
               
            foreach (TechOperationWork? item in availableTechOperationWorks.OrderBy(o => o.Order).ToList())
            {
                ComboBoxTO.Items.Add(item); // todo: чтобы убрать возможность выбора уже отображаемых ТО из ComboBoxTO, нужно добавить проверку наличия item в diagamToWork
            }
        }

    }
}
