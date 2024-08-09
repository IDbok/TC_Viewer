using System.ComponentModel;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms.Services;
/// <summary>
/// Класс предназначен для реализации логики выбора объектов в DataGridView для AddingFormMode.
/// 1. Создать поле класса с SelectionService _selectionService
/// 2. В конструкторе создать экземпляр и передать в него DataGridView и список объектов (new SelectionService<T>(dgvMain, _displayedObjects);)
/// 3. Подписаться на события DataGridView: CellValueChanged и CurrentCellDirtyStateChanged. (в методе SetAddingFormEvents)
/// 4. В конце метода FilteringObjects вызвать метод RestoreSelectedIds()
/// 5. В методе BtnAddSelected_Click вызвать метод GetSelectedObjects() для получения выбранных объектов
/// </summary>
/// <typeparam name="T"></typeparam>
public class SelectionService<T> where T : class, IIdentifiable
{
    private readonly DataGridView _dataGridView;
    private readonly List<int> _selectedIds;
    private readonly List<T> _displayedObjects;
    
    public SelectionService(DataGridView dataGridView, List<T> displayedObjects, List<int>? selectedIds = null)
    {
        _dataGridView = dataGridView;
        _displayedObjects = displayedObjects;

        if(selectedIds == null)
            _selectedIds = new();
        else
            _selectedIds = selectedIds;
    }
    public List<T> GetSelectedObjects()
    {
        return _displayedObjects.Where(x => _selectedIds.Contains(x.Id)).ToList();
    }

    public void CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == _dataGridView.Columns["Selected"].Index && e.RowIndex >= 0)
        {
            var row = _dataGridView.Rows[e.RowIndex];
            if (row.DataBoundItem is T item)
            {
                bool isSelected = Convert.ToBoolean(row.Cells["Selected"].Value);
                if (isSelected)
                {
                    _selectedIds.Add(item.Id);
                }
                else
                {
                    _selectedIds.Remove(item.Id);
                }
            }
        }
    }

    public void CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
        if (_dataGridView.IsCurrentCellDirty)
        {
            _dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    public void RestoreSelectedIds()
    {
        foreach (DataGridViewRow row in _dataGridView.Rows)
        {
            if (row.DataBoundItem is T item)
            {
                if (_selectedIds.Contains(item.Id))
                {
                    row.Cells["Selected"].Value = true;
                }
            }
        }
    }
}

