using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Work;
using TcModels.Models.Interfaces;
using static TC_WinForms.WinForms.Win6_Staff;

namespace TC_WinForms.DataProcessing
{
	public class DGVEvents
    {

        private int rowIndexFromMouseDown;
        private DataGridViewRow draggingRow;
        private int columnIndexFromMouseDown;
        private bool dragging;

        private DataGridView _dgv;


        public object EventsObj;
        public int Table = 0;

        public DGVEvents(DataGridView dgv)
        {
            _dgv = dgv;
        }
        public DGVEvents()
        {
            
        }

        public void AddGragDropEvents(DataGridView dgv)
        {
            // Включаем поддержку перетаскивания строк
            dgv.AllowDrop = true;
            dgv.AllowUserToOrderColumns = true;

            // Обработчики событий для поддержки перетаскивания
            dgv.MouseDown += DataGridView_MouseDown;
            dgv.MouseMove += DataGridView_MouseMove;
            dgv.DragOver += DataGridView_DragOver;
            dgv.DragDrop += DataGridView_DragDrop;

            // Событие для отрисовки индекса строки
            //dgv.RowPostPaint += DataGridView_RowPostPaint;
        }

        public void SetRowsUpAndDownEvents(Button btnMoveUp, Button btnMoveDown, DataGridView dgv)
        {
            this._dgv = dgv;
            btnMoveUp.Click += btnMoveUp_Click;
            btnMoveDown.Click += btnMoveDown_Click;
        }

        private void DataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            var dataGridView = (DataGridView)sender;

            rowIndexFromMouseDown = dataGridView.HitTest(e.X, e.Y).RowIndex;
            if (rowIndexFromMouseDown != -1)
            {
                draggingRow = dataGridView.Rows[rowIndexFromMouseDown];
                columnIndexFromMouseDown = dataGridView.HitTest(e.X, e.Y).ColumnIndex;

            }

        }

        private void DataGridView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && draggingRow != null)
            {
                dragging = true;
                ((DataGridView)sender).DoDragDrop(draggingRow, DragDropEffects.Move);
            }
        }
        private void DataGridView_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        private void DataGridView_DragDrop(object sender, DragEventArgs e)
        {
            var dataGridView = (DataGridView)sender;

            Point clientPoint = dataGridView.PointToClient(new Point(e.X, e.Y));

            int rowIndexTo = dataGridView.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            if (rowIndexTo == -1)
            {
                rowIndexTo = dataGridView.Rows.Count - 2;
            }

            if (rowIndexFromMouseDown < rowIndexTo)
            {
                rowIndexTo += 1;
            }

            //var newRow = CloneDataGridViewRow(draggingRow);

            //dataGridView.Rows.Insert(rowIndexTo, newRow);
            //dataGridView.Rows.Remove(draggingRow);

            if(draggingRow.Index < rowIndexTo)//Если строка перемещается вниз, то не прибавляем дополнительный шаг
            {
                DGVProcessing.ReorderRows(draggingRow, rowIndexTo, dataGridView);
            }
            else
                DGVProcessing.ReorderRows(draggingRow, rowIndexTo + 1, dataGridView);

            dragging = false;


            if (EventsObj != null)
            {
                if (EventsObj is AddEditTechOperationForm)
                {
                    //((AddEditTechOperationForm)EventsObj).UpdateTable(Table);
                }
                else
                {
                    draggingRow = null;
                }

            }
        }

        private void DataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var dataGridView = (DataGridView)sender;
            using (var b = new SolidBrush(dataGridView.RowHeadersDefaultCellStyle.ForeColor))
            {
                e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 10, e.RowBounds.Location.Y + 4);
            }
        }

        private void btnMoveUp_Click(object? sender, EventArgs e)
        {
            var selectedRows = DGVProcessing.GetSelectedRows(_dgv);
            DGVProcessing.MoveRowsUp(selectedRows, _dgv);
            DGVProcessing.SetOrderColumnsValuesAsRowIndex(_dgv);
            DGVProcessing.SelectRows(selectedRows, _dgv);
        }
        private void btnMoveDown_Click(object? sender, EventArgs e)
        {
            var selectedRows = DGVProcessing.GetSelectedRows(_dgv);
            DGVProcessing.MoveRowsDown(selectedRows, _dgv);
            DGVProcessing.SetOrderColumnsValuesAsRowIndex(_dgv);
            DGVProcessing.SelectRows(selectedRows, _dgv);
        }

        public void dgvMain_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
		{
			// Отрисовка индекса строки
			if (e.RowIndex >= 0)
			{
				var row = _dgv.Rows[e.RowIndex];
				//var displayedObject = row.DataBoundItem as IReleasable;
				if (row.DataBoundItem is IReleasable displayedObject && !displayedObject.IsReleased)
				{
					// Меняем цвет строки в зависимости от значения свойства IsReleased
					row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#d1c6c2"); // Цвет для строк, где IsReleased = false
				}
			}
		}

        [Obsolete("Метод вызывается слишком часто. Вместо него лучше использовать dgvMain_RowPrePaint")]
		public void dgvMain_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            // Проверяем, что это не заголовок столбца и не новая строка
            if (e.RowIndex >= 0 && e.RowIndex < _dgv.Rows.Count)
            {
                var row = _dgv.Rows[e.RowIndex];
                var displayedObject = row.DataBoundItem as IReleasable;
                if (displayedObject != null)
                {
					// Меняем цвет строки в зависимости от значения свойства IsReleased
					if (!displayedObject.IsReleased)
                    {
                        row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#d1c6c2"); // Цвет для строк, где IsReleased = false
                    }
                }
            }
        }
        public void dgvMain_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            // Проверяем, редактируется ли столбец "Order"
            if (_dgv.Columns[e.ColumnIndex].Name == nameof(DisplayedStaff_TC.Order))
            {
                // Проверяем, что значение не пустое и является допустимым целым числом
                if (string.IsNullOrWhiteSpace(e.FormattedValue.ToString()) || !int.TryParse(e.FormattedValue.ToString(), out _))
                {
                    e.Cancel = true; // Отменяем редактирование

                    // Получаем объект, связанный с редактируемой строкой
                    var row = _dgv.Rows[e.RowIndex];
                    var displayedStaff = row.DataBoundItem as IPreviousOrderable;

                    if (displayedStaff != null)
                    {
                        // Восстанавливаем предыдущее значение
                        _dgv.CancelEdit(); // Отменяем текущее редактирование
                        row.Cells[e.ColumnIndex].Value = displayedStaff.PreviousOrder; // Восстанавливаем предыдущее значение
                    }

                    MessageBox.Show("Значение в столбце 'Order' должно быть целым числом и не может быть пустым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
