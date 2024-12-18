using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TC_WinForms.WinForms.Win6.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram;

/// <summary>
/// Логика взаимодействия для WpfTo.xaml
/// </summary>
public partial class WpfTo : System.Windows.Controls.UserControl, INotifyPropertyChanged
{
    private DiagramState _diagramState;

    private readonly TcViewState _tcViewState;

    private WpfMainControl _wpfMainControl;
    private string? parallelIndex;
    public ObservableCollection<WpfToSequence> Children { get; set; } = new();
    public int? MaxSequenceIndex => Children.Max(x => x.GetSequenceIndexAsInt());

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
    public WpfTo()
    {
        InitializeComponent();
    }

    // todo: избавитться от двух конструкторов, использовать со списком диаграмм
    public WpfTo(WpfMainControl wpfMainControl,
        TcViewState tcViewState,
        List<DiagamToWork> diagramToWorks)
    {
        InitializeComponent();
        DataContext = this;

        _diagramState = new DiagramState(wpfMainControl, tcViewState);
        _diagramState.WpfTo = this;

        _tcViewState = tcViewState; // todo: избавиться от этого поля
        _wpfMainControl = wpfMainControl; // todo: избавиться от этого поля

        //diagramToWorks = diagramToWorks.OrderBy(x => x.Order).ToList();
        foreach (var diagramToWork in diagramToWorks)
            AddDiagramToWork(diagramToWork);

        _tcViewState.ViewModeChanged += OnViewModeChanged;
        // Обновление привязки
        OnPropertyChanged(nameof(IsViewMode));
        OnPropertyChanged(nameof(IsHiddenInViewMode));
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

        if(addDiagram)
            AddDiagramToWork(_diagramToWork);

        _tcViewState.ViewModeChanged += OnViewModeChanged;

        // Обновление привязки
        OnPropertyChanged(nameof(IsViewMode));
        OnPropertyChanged(nameof(IsHiddenInViewMode));
    }

    public void AddParallelTO(DiagamToWork? diagamToWork)
    {
        AddDiagramToWork(diagamToWork);

        if (diagamToWork != null)
        {
            if (diagamToWork.ParallelIndex != null)
                parallelIndex = diagamToWork.GetParallelIndex();
            //else UpdateParallelIndex(diagamToWork);
        }
    }

    private void UpdateParallelIndex()
    {
        if (Children.Count > 0)
        {
            parallelIndex ??= SetParallelIndex();
            _wpfMainControl.diagramForm.HasChanges = true;

            //установить индекс параллельности для всех дочерних элементов
            foreach (var wpfSq in Children)
            {
                wpfSq.UpdateParallelIndex();
                //wpfSq.ParallelButtonsVisibility(true);
            }
        }
    }

    private void btnAddTOParallel_Click(object sender, RoutedEventArgs e)
    {
        if (_wpfMainControl.CheckIfTOIsAvailable())
        {
            AddDiagramToWork();

            _wpfMainControl.diagramForm.HasChanges = true;
            _wpfMainControl.Nomeraciya();
        }
    }
    private void AddDiagramToWork(DiagamToWork? diagamToWork = null)
    {
        if (diagamToWork == null)
        {
            diagamToWork = new DiagamToWork();// UpdateParallelIndex(diagamToWork);
        }


        var sequenceIndex = diagamToWork.GetSequenceIndex();
        if(sequenceIndex == null)
        {
            CreateWpfToSequence(diagamToWork);
        }
        else 
        {
            var wpfToSequence = Children.FirstOrDefault(x => x.GetSequenceIndex() == sequenceIndex);
            if (wpfToSequence == null)
            {
                CreateWpfToSequence(diagamToWork);
            }
            else
            {
                wpfToSequence.AddSequenceTO(diagamToWork);
            }
        }

        UpdateParallelIndex();
    }

    private WpfToSequence CreateWpfToSequence(DiagamToWork diagramToWork)
    {
        WpfToSequence? wpfToSequence = new WpfToSequence(_diagramState, diagramToWork);
        wpfToSequence.VerticalAlignment = VerticalAlignment.Top;
        wpfToSequence.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

        Children.Add(wpfToSequence);
        return wpfToSequence;
    }

    public void DeleteWpfControlTO(WpfControlTO wpfControlTO)
    {
        //Children.Remove(wpfControlTO);
    }

    public void ChangeOrder(WpfControlTO wpfControlTO, MoveDirection direction)
    {

        // Поиск WpfToSequence, содержащего wpfControlTO
        var wpfToSequence = Children.FirstOrDefault(x => x.Children.Contains(wpfControlTO));

        if (wpfToSequence == null)
            return;

        switch (direction)
        {
            case MoveDirection.Left:
                if (Children.IndexOf(wpfToSequence) > 0)
                {
                    var index = Children.IndexOf(wpfToSequence);
                    Children.Remove(wpfToSequence);
                    Children.Insert(index - 1, wpfToSequence);
                }
                break;

            case MoveDirection.Right:
                if (Children.IndexOf(wpfToSequence) < Children.Count - 1)
                {
                    var index = Children.IndexOf(wpfToSequence);
                    Children.Remove(wpfToSequence);
                    Children.Insert(index + 1, wpfToSequence);
                }
                break;

            case MoveDirection.Up:
                wpfToSequence.ChangeOrder(wpfControlTO, MoveDirection.Up);
                ////todo - перемещение вверх вне последовательности и данной WpfTo

                //// Получаем индекс wpfControlTO в текущей последовательности
                //int controlIndex = wpfToSequence.Children.IndexOf(wpfControlTO);
                //// Если индекс больше 0, то перемещаем вверх внутри последовательности
                //if (controlIndex > 0)
                //{
                //    wpfToSequence.ChangeOrder(wpfControlTO, direction);
                //}
                //// если этот индекс 0, то 
                //else
                //{
                //    if (wpfToSequence.Children.Count == 1)
                //    {
                //        var indexWpfTo = _wpfMainControl.Children.IndexOf(this);
                //        if (indexWpfTo > 0)
                //            _wpfMainControl.Children.Move(indexWpfTo, indexWpfTo - 1);
                //    }
                //    else
                //    {
                //        // удаляем ТО из текущей последовательности
                //        wpfToSequence.Children.Remove(wpfControlTO);

                //        // добавление в WpfMainControl новой объект WpfTo с этим ТО выше текущего
                //        // определить положение текущего объекта в списке WpfTo
                //        var indexWpfTo = _wpfMainControl.Children.IndexOf(this);
                //        if(indexWpfTo > 0)
                //            _wpfMainControl.AddDiagramsToChildren(wpfControlTO.diagamToWork, indexWpfTo);
                //    }

                //}
                //// если этот индекс 0, то проверяем индекс родителя в списке последовательностей в данной WpfTo

                //// если этот индекс 0, то вставляем перед текущей WpfTo в WpfMainControl
                //// если нет, то содаём новую WpfToSequence и вставляем перед текущей в WpfTo
                //// если индекс не 0, просто переместить вверх
                ////wpfToSequence.ChangeOrder(wpfControlTO, MoveDirection.Up); 
                ////_wpfMainControl.Order(2, wpfControlTO);
                break;

            case MoveDirection.Down:
                wpfToSequence.ChangeOrder(wpfControlTO, MoveDirection.Down);
                break;
        }

        _wpfMainControl.Nomeraciya();
    }

    public string? GetParallelIndex()
    {
        return parallelIndex;
    }
    public string SetParallelIndex()
    {
        this.parallelIndex = new Random().Next(1000000).ToString();
        return parallelIndex;
    }

    public void DeleteWpfToSequence(WpfToSequence wpfToSequence)
    {
        Children.Remove(wpfToSequence);
        if (Children.Count == 0)
        {
            _diagramState.WpfMainControl.DeleteWpfTO(this);
        }
    }

    private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
    {
        _wpfMainControl.ChangeOrder(this,MoveDirection.Down);
    }

    private void BtnMoveUp_Click(object sender, RoutedEventArgs e)
    {
        _wpfMainControl.ChangeOrder(this, MoveDirection.Up);
    }
}
