using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TC_WinForms.WinForms.Win6.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram;

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

    private bool isDropDownOpen = false;
    private TechOperationWork? _currentTechOperationWork;
    public ObservableCollection<WpfParalelno> Children { get; set; } = new();

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

        this.diagamToWork = _diagramState.DiagramToWork!;
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
    private void SubscribeToSelectionChanged()
    {
        ComboBoxTO.SelectionChanged += ComboBoxTO_SelectionChanged;
    }
    private void UnsubscribeFromSelectionChanged()
    {
        ComboBoxTO.SelectionChanged -= ComboBoxTO_SelectionChanged;
    }
    private void SetTechOperationWorkToComboBox(TechOperationWork techOperationWork)
    {
        // Временно отписываемся от события
        UnsubscribeFromSelectionChanged();

        try
        {
            ComboBoxTO.SelectedItem = this.diagamToWork.techOperationWork;
        }
        finally
        {
            // Возвращаем подписку на событие
            SubscribeToSelectionChanged();
        }
    }
    private void UpdateDiagramToWork()
    {
        if (this.diagamToWork.techOperationWork != null)
        {
            ComboBoxTO.Items.Clear();
            ComboBoxTO.Items.Add(this.diagamToWork.techOperationWork);
            //ComboBoxTO.SelectedItem = this.diagamToWork.techOperationWork;
            SetTechOperationWorkToComboBox(this.diagamToWork.techOperationWork);

            ListWpfParalelno.Visibility = Visibility.Visible;

            // проверка на соотвествие diagamToWork с _diagramState.DiagamToWork
            if (diagamToWork.Id != _diagramState.DiagramToWork.Id)
            {
                _diagramState.DiagramToWork = diagamToWork;
            }

            Children.Clear();

            if (this.diagamToWork.ListDiagramParalelno.Any())
            {
                foreach (DiagramParalelno diagramParalelno in this.diagamToWork.ListDiagramParalelno.OrderBy(x => x.Order))
                {
                    Children.Add(
                        new WpfParalelno(diagramParalelno.techOperationWork, _diagramState, diagramParalelno));
                }
            }
            else
            {
                Children.Add(
                    new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, _diagramState));
            }
        }

        ParallelButtonsVisibility(this.diagamToWork.ParallelIndex != null);
        this._wpfMainControl.Nomeraciya();
        
    }


    public void ParallelButtonsVisibility(bool isVisible) // todo: перенести для WpfToSequence
    {
        // pnlParallelButtons.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (this.diagamToWork.techOperationWork == null)
            return;

        //ListWpfParalelno.Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, _diagramState));// this, _tcViewState));
        Children.Add(new WpfParalelno((TechOperationWork)ComboBoxTO.SelectedItem, _diagramState));
        _wpfMainControl.diagramForm.HasChanges = true;
        _wpfMainControl.Nomeraciya();

        _diagramState.HasChanges();
    }

    private void ComboBoxTO_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxTO.SelectedItem != null)
        {
            var techOperationWork = (TechOperationWork)ComboBoxTO.SelectedItem;

            // Если данный блок пустой
            if (diagamToWork.techOperationWork != null)
            {
                if(diagamToWork.techOperationWork.Id == techOperationWork.Id)
                {
                    return;
                }

                diagamToWork.techOperationWork = techOperationWork;
                diagamToWork.techOperationWorkId = techOperationWork.Id;

                if (diagamToWork.ListDiagramParalelno.Count > 0)
                {
                    foreach(var item in diagamToWork.ListDiagramParalelno)
                    {
                        item.techOperationWork = techOperationWork;
                        item.techOperationWorkId = techOperationWork.Id;

                        foreach (var item2 in item.ListDiagramPosledov)
                        {
                            foreach (var item3 in item2.ListDiagramShag)
                            {
                                foreach (var item4 in item3.ListDiagramShagToolsComponent)
                                {
                                    if (item4.componentWork != null)
                                    {
                                        // проверка существует ли в новой ТО ComponentWork с таким же ComponentId
                                        var componentWork = techOperationWork.ComponentWorks
                                            .FirstOrDefault(x => x.componentId == item4.componentWork.componentId);
                                        if (componentWork != null)
                                            item4.componentWork = componentWork;
                                        
                                    }
                                    else if (item4.toolWork != null)
                                    {
                                        // проверка существует ли в новой ТО ToolWork с таким же ToolId
                                        var toolWork = techOperationWork.ToolWorks
                                            .FirstOrDefault(x => x.toolId == item4.toolWork.toolId);
                                        if (toolWork != null)
                                            item4.toolWork = toolWork;
                                    }
                                }
                                // тут нужно бы обновить таблицу с инструментами и компонентами в Шаге
                                
                            }
                        }
                    }

                    UpdateAllShag();
                    _diagramState.HasChanges();
                }
                return;
            }

            ListWpfParalelno.Visibility = Visibility.Visible;
            ButtonAddShag.Visibility = Visibility.Visible;

            
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

                //ListWpfParalelno.Children.Clear();
                //ListWpfParalelno.Children.Add(new WpfParalelno(techOperationWork, _diagramState));
                Children.Clear();
                Children.Add(new WpfParalelno(techOperationWork, _diagramState));

                _wpfMainControl.diagramForm.HasChanges = true;

                // todo : вопрос, как быть с объектами Component and Tool, которые привязаны к конкретному TechOperationWork
                //ComboBoxTO.IsReadOnly = true;
                //ComboBoxTO.IsEnabled = false;

                Nomeraciya();
            }

            ButtonAddShag.IsEnabled = true;
        }
    }

    private void UpdateAllShag()
    {
        foreach (var item in Children)
        {
            item.UpdateAllShag();
        }
    }
    public void DeteteParalelno(WpfParalelno paralelno)
    {
        //ListWpfParalelno.Children.Remove(paralelno);
        Children.Remove(paralelno);

        _wpfMainControl.diagramForm.HasChanges = true;

        if (Children.Count == 0) //ListWpfParalelno.Children.Count == 0)
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
            //int ib = ListWpfParalelno.Children.IndexOf(wpfParalelno);
            //if (ib < ListWpfParalelno.Children.Count - 1)
            //{
            //    var cv = ListWpfParalelno.Children[ib + 1];
            //    ListWpfParalelno.Children.Remove(cv);
            //    ListWpfParalelno.Children.Insert(ib, cv);

            //    _wpfMainControl.Nomeraciya();
            //}
            int ib = Children.IndexOf(wpfParalelno);
            if (ib < Children.Count - 1)
            {
                var cv = Children[ib + 1];
                Children.Remove(cv);
                Children.Insert(ib, cv);

                _wpfMainControl.Nomeraciya();
            }
        }

        if (v == 2)
        {
            //int ib = ListWpfParalelno.Children.IndexOf(wpfParalelno);
            //if (ib != 0)
            //{
            //    var cv = ListWpfParalelno.Children[ib];
            //    ListWpfParalelno.Children.Remove(cv);
            //    ListWpfParalelno.Children.Insert(ib - 1, cv);
            //    _wpfMainControl.Nomeraciya();

            int ib = Children.IndexOf(wpfParalelno);
            if (ib != 0)
            {
                var cv = Children[ib];
                Children.Remove(cv);
                Children.Insert(ib - 1, cv);
                _wpfMainControl.Nomeraciya();
            }
        }
    }

    private void ButtonDown_Click(object sender, RoutedEventArgs e)
    {
        _diagramState.WpfTo?.ChangeOrder(this, MoveDirection.Down);
    }

    private void ButtonUp_Click(object sender, RoutedEventArgs e)
    {
        _diagramState.WpfTo?.ChangeOrder(this, MoveDirection.Up);
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.Forms.MessageBox.Show("Вы действительно хотите удалить операцию?",
            "Удаление операции", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.No)
            return;

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

        if (diagamToWork.techOperationWork != null)
        {
            ComboBoxTO.Items.Add(diagamToWork.techOperationWork);
        }
        foreach (TechOperationWork? item in availableTechOperationWorks.OrderBy(o => o.Order).ToList())
        {
            ComboBoxTO.Items.Add(item); // todo: чтобы убрать возможность выбора уже отображаемых ТО из ComboBoxTO, нужно добавить проверку наличия item в diagamToWork
        }
    }

    private void ComboBoxTO_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!isDropDownOpen)
        {
            // Останавливаем прокрутку самого ComboBox
            e.Handled = true;

            // Поднимаем событие вверх по визуальному дереву для прокрутки формы
            var parent = ((System.Windows.Controls.ComboBox)sender).Parent as UIElement;
            if (parent != null)
            {
                parent.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent
                });
            }
        }
    }
    private void ComboBoxTO_DropDownOpened(object sender, EventArgs e)
    {
        isDropDownOpen = true;
    }

    private void ComboBoxTO_DropDownClosed(object sender, EventArgs e)
    {
        isDropDownOpen = false;

        // Сохраняем предыдущее значение при закрытии списка
        if (ComboBoxTO.SelectedItem == null )
        {
            SetTechOperationWorkToComboBox(diagamToWork.techOperationWork);
        }
    }


}
