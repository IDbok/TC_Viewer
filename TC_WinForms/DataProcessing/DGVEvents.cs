using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC_WinForms.WinForms.Work;

namespace TC_WinForms.DataProcessing
{
    public class DGVEvents
    {

        private int rowIndexFromMouseDown;
        private DataGridViewRow draggingRow;
        private int columnIndexFromMouseDown;
        private bool dragging;

        private DataGridView dgv;


        public object EventsObj;
        public int Table = 0;

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
            this.dgv = dgv;
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

            DGVProcessing.ReorderRows(draggingRow, rowIndexTo+1, dataGridView);

            dragging = false;


            if (EventsObj != null)
            {
                if (EventsObj is AddEditTechOperationForm)
                {
                    ((AddEditTechOperationForm)EventsObj).UpdateTable(Table);

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
            var selectedRows = DGVProcessing.GetSelectedRows(dgv);
            DGVProcessing.MoveRowsUp(selectedRows, dgv);
            DGVProcessing.SetOrderColumnsValuesAsRowIndex(dgv);
            DGVProcessing.SelectRows(selectedRows, dgv);
        }
        private void btnMoveDown_Click(object? sender, EventArgs e)
        {
            var selectedRows = DGVProcessing.GetSelectedRows(dgv);
            DGVProcessing.MoveRowsDown(selectedRows, dgv);
            DGVProcessing.SetOrderColumnsValuesAsRowIndex(dgv);
            DGVProcessing.SelectRows(selectedRows, dgv);
        }
    }
}
