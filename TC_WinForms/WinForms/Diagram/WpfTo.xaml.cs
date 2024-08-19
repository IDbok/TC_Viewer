﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

        // поиск объекта wpfToSequence в котором находится wpfControlTO
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
                //_wpfMainControl.Order(2, wpfControlTO);
                break;

            case MoveDirection.Down:
                wpfToSequence.ChangeOrder(wpfControlTO, MoveDirection.Down);
                //_wpfMainControl.Order(1, wpfControlTO);
                break;

                //case MoveDirection.Up:
        }


        //ListTOParalelno.Children.Clear();
        //Children.Clear();
        //foreach (var child in Children)
        //{
        //    //ListTOParalelno.Children.Add(child);
        //    Children.Add(child);
        //}

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

}
