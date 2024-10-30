using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram;

/// <summary>
/// Логика взаимодействия для WpfToSequence.xaml
/// </summary>
public partial class WpfToSequence : System.Windows.Controls.UserControl, INotifyPropertyChanged
{
    private DiagramState _diagramState;

    private string? sequenceIndex;

    public event PropertyChangedEventHandler? PropertyChanged;
    public bool IsCommentViewMode => _diagramState.TcViewState.IsCommentViewMode;
    public bool IsViewMode => _diagramState.TcViewState.IsViewMode;
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
    public ObservableCollection<WpfControlTO> Children { get; set; } = new ();

    public WpfToSequence()
    {
        InitializeComponent();
    }

    public WpfToSequence (DiagramState diagramState, DiagamToWork? diagramToWork = null)
    {
        InitializeComponent();
        DataContext = this;

        _diagramState = new DiagramState (diagramState);

        if (diagramToWork != null)
        {
            AddSequenceTO(diagramToWork);
            sequenceIndex = diagramToWork.GetSequenceIndex();
        }

        _diagramState.TcViewState.ViewModeChanged += OnViewModeChanged;
        // Обновление привязки
        OnPropertyChanged(nameof(IsViewMode));
        OnPropertyChanged(nameof(IsHiddenInViewMode));
    }

    public void AddSequenceTO(DiagamToWork? diagramToWork)
    {

        AddDiagramToWork(diagramToWork);

        if( Children.Count > 1 && sequenceIndex == null)
        {
            var sequenceIndex = _diagramState.WpfTo?.MaxSequenceIndex ?? 0 + 1;
            SetSequenceIndex(sequenceIndex.ToString());
        }

        //if (diagamToWork != null)
        //{
        //    if (diagamToWork.ParallelIndex != null)
        //        parallelIndex = diagamToWork.ParallelIndex;
        //    else SetParallelIndex(diagamToWork);
        //}
    }

    //private void UpdateParallelIndex(DiagamToWork diagamToWork)
    //{
    //    if (diagamToWork.ParallelIndex == null)
    //    {
    //        if (Children.Count > 0)
    //        {
    //            var parallelIndex = _diagramState.WpfTo?.GetParallelIndex();
    //            if (parallelIndex == null)
    //            {
    //                return;
    //                //parallelIndex = _diagramState.WpfTo?.SetParallelIndex();
    //            }

    //            if (sequenceIndex == null)
    //            {
    //                return;
    //                //sequenceIndex = "1";
    //            }

    //            parallelIndex = parallelIndex + "/" + sequenceIndex;

    //            diagamToWork.ParallelIndex = parallelIndex;
    //            _diagramState.HasChanges();

    //            //установить индекс параллельности для всех дочерних элементов
    //            foreach (var child in Children)
    //            {
    //                child.diagamToWork.ParallelIndex = parallelIndex;
    //                child.ParallelButtonsVisibility(true);
    //            }
    //        }
    //    }
    //}
    public void UpdateParallelIndex()
    {
        var parallelIndex = _diagramState.WpfTo?.GetParallelIndex();
        if (parallelIndex != null)
        {
            if (Children.Count > 1)
            {
                if (sequenceIndex == null)
                {
                    var maxIndex = _diagramState.WpfTo?.MaxSequenceIndex ?? 0;
                    sequenceIndex = (maxIndex + 1).ToString();
                }
                parallelIndex = parallelIndex + "/" + sequenceIndex;
            }

            //установить индекс параллельности для всех дочерних элементов
            foreach (var child in Children)
            {
                child.diagamToWork.ParallelIndex = parallelIndex;

                //child.ParallelButtonsVisibility(true);
            }

            _diagramState.HasChanges();
        }
    }

    private void btnAddTOSequence_Click(object sender, RoutedEventArgs e)
    {
        if (_diagramState.WpfMainControl.CheckIfTOIsAvailable())
        {
            AddDiagramToWork();

            _diagramState.HasChanges();
            _diagramState.WpfMainControl.Nomeraciya();

            UpdateParallelIndex();
        }
    }
    private void AddDiagramToWork(DiagamToWork? diagramToWork = null)
    {
        diagramToWork ??= new DiagamToWork(); 
        
        // UpdateParallelIndex(diagramToWork); 

        var wpfControlTO = new WpfControlTO(_diagramState, diagramToWork); //(wpfMainControl, diagamToWork, _tcViewState);
        wpfControlTO.VerticalAlignment = VerticalAlignment.Top;
        wpfControlTO.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

        //ListTOParalelno.Children.Add(wpfControlTO);
        Children.Add(wpfControlTO);
    }

    public void DeleteWpfControlTO(WpfControlTO wpfControlTO)
    {
        //ListTOParalelno.Children.Remove(wpfControlTO);
        Children.Remove(wpfControlTO);
        if (Children.Count == 0)
        {
            _diagramState.WpfTo?.DeleteWpfToSequence(this);
        }
        else
        if (Children.Count == 1)
        {
            var lastWpfTo = Children[0];
            lastWpfTo.ParallelButtonsVisibility(false);
            lastWpfTo.diagamToWork.ParallelIndex = _diagramState.WpfTo?.GetParallelIndex();
        }
    }

    public void ChangeOrder(WpfControlTO wpfControlTO, MoveDirection direction)
    {
        var hasChanges = false;

        // Получаем индекс wpfControlTO в текущей последовательности
        int controlIndex = Children.IndexOf(wpfControlTO);

        switch (direction)
        {
            case MoveDirection.Up:
                if (controlIndex > 0)
                {
                    Children.Move(controlIndex, controlIndex - 1);

                    hasChanges = true;
                }
                break;

            case MoveDirection.Down:
                if (controlIndex < Children.Count - 1)
                {

                    Children.Move(controlIndex, controlIndex + 1);

                    hasChanges = true;
                }
                break;
        }

        if(hasChanges)
            _diagramState.WpfMainControl.Nomeraciya();
    }

    public void SetSequenceIndex(string index)
    {
        sequenceIndex = index;

        foreach (var child in Children)
        {
            child.diagamToWork.ParallelIndex = child.diagamToWork.ParallelIndex?.Split('/')[0] + "/" + sequenceIndex;
        }
    }
    public string? GetSequenceIndex()
    {
        return sequenceIndex;
    }
    public int? GetSequenceIndexAsInt() 
    {         
        return int.TryParse(sequenceIndex, out var result) ? result : null;
    }

    private void BtnMoveLeft_Click(object sender, RoutedEventArgs e)
    {
        var item = Children[0];
        if (item != null)
            _diagramState.WpfTo?.ChangeOrder(item, MoveDirection.Left);
    }
    private void BtnMoveRight_Click(object sender, RoutedEventArgs e)
    {
        var item = Children[0];
        if (item != null)
            _diagramState.WpfTo?.ChangeOrder(item, MoveDirection.Right);
        
    }
}
