using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram
{
    /// <summary>
    /// Логика взаимодействия для WpfTo.xaml
    /// </summary>
    public partial class WpfTo : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private DiagramState _diagramState;

        private readonly TcViewState _tcViewState;

        private WpfMainControl _wpfMainControl;
        private string? parallelIndex;

        public event PropertyChangedEventHandler? PropertyChanged;
        public bool IsCommentViewMode => _tcViewState.IsCommentViewMode;
        public bool IsViewMode => _tcViewState.IsViewMode;
        public bool IsHiddenInViewMode => !IsViewMode;
        private void OnViewModeChanged()
        {
            OnPropertyChanged(nameof(IsHiddenInViewMode));
            OnPropertyChanged(nameof(IsViewMode));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public List<WpfControlTO> Children { get; set; } = new List<WpfControlTO>();
        public WpfTo()
        {
            InitializeComponent();
        }

        public WpfTo(WpfMainControl wpfMainControl,
            TcViewState tcViewState,
            DiagamToWork? _diagramToWork = null, 
            bool addDiagram = true)
        {
            _diagramState = new DiagramState(wpfMainControl, tcViewState, _diagramToWork);
            _diagramState.WpfTo = this;

            InitializeComponent();
            DataContext = this;

            _tcViewState = tcViewState;

            _wpfMainControl = wpfMainControl;

            ListTOParalelno.Children.Clear();
            Children.Clear();

            if(addDiagram)
                AddWpfControlTO(wpfMainControl, _diagramToWork);

            _tcViewState.ViewModeChanged += OnViewModeChanged;

            // Обновление привязки
            OnPropertyChanged(nameof(IsViewMode));
            OnPropertyChanged(nameof(IsHiddenInViewMode));
        }

        public void AddParallelTO(DiagamToWork? diagamToWork)
        {
            AddWpfControlTO(_wpfMainControl, diagamToWork);

            if (diagamToWork != null)
            {
                if (diagamToWork.ParallelIndex != null)
                    parallelIndex = diagamToWork.ParallelIndex;
                else SetParallelIndex(diagamToWork);
            }
        }

        private void SetParallelIndex(DiagamToWork diagamToWork)
        {
            if (diagamToWork.ParallelIndex == null)
            {
                if (Children.Count > 0)
                {
                    if (parallelIndex == null)
                        parallelIndex = new Random().Next(10000).ToString();

                    diagamToWork.ParallelIndex = parallelIndex;

                    _wpfMainControl.diagramForm.HasChanges = true;

                    //установить индекс параллельности для всех дочерних элементов
                    foreach (var child in Children)
                    {
                        child.diagamToWork.ParallelIndex = parallelIndex;
                        child.ParallelButtonsVisibility(true);
                    }
                }
            }
        }

        private void btnAddTOParallel_Click(object sender, RoutedEventArgs e)
        {
            if (_wpfMainControl.CheckIfTOIsAvailable())
            {
                AddWpfControlTO(_wpfMainControl);

                _wpfMainControl.diagramForm.HasChanges = true;
                _wpfMainControl.Nomeraciya();
            }
        }
        private void AddWpfControlTO(WpfMainControl wpfMainControl,
            DiagamToWork? diagamToWork = null)
        {
            if (diagamToWork == null)
                diagamToWork = new DiagamToWork(); SetParallelIndex(diagamToWork);

            var wpfControlTO = new WpfControlTO(_diagramState, diagamToWork); //(wpfMainControl, diagamToWork, _tcViewState);
            wpfControlTO.VerticalAlignment = VerticalAlignment.Top;
            wpfControlTO.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            ListTOParalelno.Children.Add(wpfControlTO);
            Children.Add(wpfControlTO);
        }

        public void DeleteWpfControlTO(WpfControlTO wpfControlTO)
        {
            ListTOParalelno.Children.Remove(wpfControlTO);
            Children.Remove(wpfControlTO);
        }

        public void ChangeOrder(WpfControlTO wpfControlTO, MoveDirection direction)
        {
            switch (direction)
            {
               case MoveDirection.Left:
                    if (Children.IndexOf(wpfControlTO) > 0)
                    {
                        var index = Children.IndexOf(wpfControlTO);
                        Children.Remove(wpfControlTO);
                        Children.Insert(index - 1, wpfControlTO);
                    }
                    break;

                case MoveDirection.Right:
                    if (Children.IndexOf(wpfControlTO) < Children.Count - 1)
                    {
                        var index = Children.IndexOf(wpfControlTO);
                        Children.Remove(wpfControlTO);
                        Children.Insert(index + 1, wpfControlTO);
                    }
                    break;
            }

            ListTOParalelno.Children.Clear();
            foreach (var child in Children)
            {
                ListTOParalelno.Children.Add(child);
            }

            _wpfMainControl.Nomeraciya();
        }

    }
}
