using TC_WinForms.WinForms.Win6.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram;

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right
}
/// <summary>
/// Класс предназначен для того, чтобы подчинённые объекты могли обращаться к своим родителям
/// </summary>
public class DiagramState 
{
    public readonly TcViewState TcViewState;

    public readonly WpfMainControl WpfMainControl;

    public DiagamToWork? DiagramToWork;

    public WpfTo? WpfTo;
    public WpfControlTO? WpfControlTO;
    public WpfParalelno? WpfParalelno;
    public WpfPosledovatelnost? WpfPosledovatelnost;

    public DiagramState(WpfMainControl wpfMainControl, TcViewState tcViewState, DiagamToWork? diagramToWork = null)
    {
        WpfMainControl = wpfMainControl;
        TcViewState = tcViewState;
        DiagramToWork = diagramToWork;
    }

    public DiagramState(DiagramState diagramState)
    {
        TcViewState = diagramState.TcViewState;
        WpfMainControl = diagramState.WpfMainControl;
        DiagramToWork = diagramState.DiagramToWork;
        WpfTo = diagramState.WpfTo;
        WpfControlTO = diagramState.WpfControlTO;
        WpfParalelno = diagramState.WpfParalelno;
        WpfPosledovatelnost = diagramState.WpfPosledovatelnost;
    }

    public void HasChanges()
    {
        WpfMainControl._diagramForm.HasChanges = true;
    }
}


