using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;

namespace TC_WinForms.DataProcessing
{
    public static class DGVProcessing
    {
        public static void AddColumnsToDGV(Dictionary<string, string> columnNames, DataGridView dgv)
        {
            columnNames.Keys.ToList().ForEach(x => dgv.Columns.Add(x, columnNames[x]));
        }
        public static void SetColumnsOrder(Dictionary<string, int> columnOrder, DataGridView dgv)
        {
            columnOrder.Keys.ToList().ForEach(x =>
            {
                if (columnOrder[x] == -1)
                    dgv.Columns[x].Visible = false;

                else
                    dgv.Columns[x].DisplayIndex = columnOrder[x];
            });
        }
        public static void CreateColumnsCopies(List<string> columnNamesCopy, DataGridView dgv)
        {
            columnNamesCopy.ForEach(x => dgv.Columns.Add(x+"_copy",x));
            columnNamesCopy.ForEach(x => dgv.Columns[x + "_copy"].Visible = false);
        }
        public static void DGVStructure(Dictionary<string, string> columnNames, DataGridView dgv)
        {
            dgv.Columns.Clear();
            foreach (var item in columnNames)
            {
                dgv.Columns.Add(item.Key, item.Value);
            }
        }

        // method to colorize cell in datagridview
        public static void ColorizeCell(DataGridView dgv, int columnNumber, int rowNumber, string color)
        {
            dgv.Rows[rowNumber].Cells[columnNumber].Style.BackColor = System.Drawing.Color.Red;
        }

        //public static void ColorizeCells(DataGridView dgv, int columnNumber, int rowNumber, string color)
        //{
        //    foreach (System.Windows.Forms.DataGridViewRow row in dgv.Rows)
        //    {
        //        if (row.Cells[columnNumber].Value.ToString() == color)
        //        {
        //            row.Cells[columnNumber].Style.BackColor = System.Drawing.Color.Red;
        //        }
        //    }
        //}
        /// <summary>
        /// return list of selected DataGridViewRow if no row is selected is selected rows which cells are selected
        /// </summary>
        /// <param name="dgv"></param>
        /// <returns></returns>
        public static List<DataGridViewRow> GetSelectedRows(DataGridView dgv)
        {
            List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dgv.SelectedRows)
            {
                selectedRows.Add(row);
            }
            if (selectedRows.Count == 0)
            {
                foreach (DataGridViewCell cell in dgv.SelectedCells)
                {
                    if (!selectedRows.Contains(cell.OwningRow))
                    {
                        selectedRows.Add(cell.OwningRow);
                        cell.OwningRow.Selected = true;
                    }
                }
            }
            return selectedRows;
        }
        public static void AddValueToCell(DataGridView dgv, string columnName, int rowIndex, object value)
        {
            if (dgv.Columns.Contains(columnName) && rowIndex >= 0 && rowIndex < dgv.Rows.Count)
            {
                dgv.Rows[rowIndex].Cells[columnName].Value = value;
            }
            else
            {
                // Handle error: column not found or row index out of range
            }
        }
        public static void AddValueToCopyColumn(DataGridView dgv, string columnName, int rowIndex)
        {
            if (dgv.Columns.Contains(columnName) 
                && dgv.Columns.Contains(columnName+"_copy") 
                && rowIndex >= 0 
                && rowIndex < dgv.Rows.Count)
            {
                dgv.Rows[rowIndex].Cells[columnName + "_copy"].Value = dgv.Rows[rowIndex].Cells[columnName].Value;
            }
        }
        public static void SetCopyColumnsValues(DataGridView dgv, List<string> columnNames)
        {
            // check if all columns and their copies are exist 
            if (columnNames.All(x => dgv.Columns.Contains(x) && dgv.Columns.Contains(x + "_copy")))
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    columnNames.ForEach(x => row.Cells[x + "_copy"].Value = row.Cells[x].Value);
                }
            }
            
        }
        public static void DeleteRowsById<T>(List<DataGridViewRow> rowsToDelete, DataGridView dgv, DbConnector dbCon) 
            where T : class, IIdentifiable
        {
            foreach (var row in rowsToDelete)
            {
                dbCon.Delete<T>((int)row.Cells["Id"].Value);
            }

            DeleteRows(rowsToDelete, dgv);
        }
        public static void DeleteRowsStaff_TC(List<DataGridViewRow> rowsToDelete, DataGridView dgv, DbConnector dbCon)
        {
            foreach (var row in rowsToDelete)
            {
                var sttc = new Staff_TC
                {
                    ParentId = (int)row.Cells["ParentId"].Value,
                    ChildId = (int)row.Cells["ChildId"].Value,
                    Order = (int)row.Cells["Order"].Value,
                    Symbol = (string)row.Cells["Symbol"].Value
                };

                dbCon.Delete(sttc);
            }

            DeleteRows(rowsToDelete, dgv);
        }
        public static void DeleteRows(List<DataGridViewRow> rowsToDelete, DataGridView dgv)
        {
            foreach (DataGridViewRow row in rowsToDelete)
            {
                dgv.Rows.Remove(row);
            }
        }

        public static bool CheckUniqueCombination(DataGridView dgv, string colName, string colName2)
        {
            var list = new List<string>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (list.Contains(row.Cells[colName].Value.ToString() + row.Cells[colName2].Value.ToString()))
                {
                    return true;
                }
                else
                {
                    list.Add(row.Cells[colName].Value.ToString() + row.Cells[colName2].Value.ToString());
                }
            }

            return false;
        }
        public static bool CheckUniqueCombination(DataGridView dgv, string colName)
        {
            var list = new List<string>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (list.Contains(row.Cells[colName].Value.ToString()))
                {
                    return false;
                }
                else
                {
                    list.Add(row.Cells[colName].Value.ToString());
                }
            }

            return true;
        }
        public static bool CheckUniqueColumnsValues(DataGridView dgv, List<string> colNames)
        {
            var list = new List<string>();
            string rowValue=null;

            foreach (DataGridViewRow row in dgv.Rows)
            {
                foreach (var colName in colNames)
                {
                    rowValue += row.Cells[colName].Value.ToString();
                }
                list.Add(rowValue);
                rowValue = null;
            }

            return CheckUniqueValues(list);
        }
        public static bool CheckUniqueCombination_2(DataGridView dgv, string colName)
        {
            var list = new List<string>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                list.Add(row.Cells[colName].Value.ToString());
            }

            return CheckUniqueValues(list);
        }
        public static bool CheckUniqueValues(List<string> list)
        {
            // check if list contains duplicates
            var listWithoutDuplicates = list.Distinct().ToList();
            if (listWithoutDuplicates.Count != list.Count)
            {
                return false;
            }
            return true;
        }
        public static void MoveRowsDown(List<DataGridViewRow> rowsToMove, DataGridView dgv)
        {
            // order row by index
            rowsToMove = rowsToMove.OrderByDescending(x => x.Index).ToList();
            // check if last row is not last in dgv
            if (rowsToMove[0].Index < dgv.Rows.Count - 1)
            {
                foreach (DataGridViewRow row in rowsToMove)
                {
                    int index = row.Index;
                    dgv.Rows.Remove(row);
                    dgv.Rows.Insert(index + 1, row);
                }
            }
        }
        /// <summary>
        /// move rows in DataGridView up
        /// </summary>
        /// <param name="rowsToMove"></param>
        /// <param name="dgv"></param>
        public static void MoveRowsUp(List<DataGridViewRow> rowsToMove, DataGridView dgv)
        {
            // order row by index
            rowsToMove = rowsToMove.OrderBy(x => x.Index).ToList();
            // check if first row is not first in dgv
            if (rowsToMove[0].Index > 0)
            {
                foreach (DataGridViewRow row in rowsToMove)
                {
                    int index = row.Index;
                    dgv.Rows.Remove(row);
                    dgv.Rows.Insert(index - 1, row);
                }
            }

        }
        public static void ReorderRows(DataGridViewRow row, int newOrder, DataGridView dgv)
        {
            // todo - check if newOrder is not out of range
            // to row in dgvMain set new order and change row index
            row.Cells["Order"].Value = newOrder;
            dgv.Rows.Remove(row);
            if (newOrder > dgv.Rows.Count)
                dgv.Rows.Insert(dgv.Rows.Count, row);
            else
                dgv.Rows.Insert(newOrder - 1, row);
            
            SetOrderColumnsValuesAsRowIndex(dgv);

            dgv.ClearSelection();
            row.Selected = true;
        }
        /// <summary>
        /// Set value to Order column in dgvMain as row index + 1
        /// </summary>
        /// <param name="dgv"></param>
        public static void SetOrderColumnsValuesAsRowIndex(DataGridView dgv)
        {
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                dgv.Rows[i].Cells["Order"].Value = i + 1;
            }
        }
        public static void SelectRows(List<DataGridViewRow> rowsToSelect, DataGridView dgv)
        {
            dgv.ClearSelection();
            foreach (var row in rowsToSelect)
            {
                row.Selected = true;
            }
        }
        public static DataGridViewRow CloneDataGridViewRow(DataGridViewRow originalRow)
        {
            DataGridViewRow newRow = (DataGridViewRow)originalRow.Clone();

            for (int i = 0; i < originalRow.Cells.Count; i++)
            {
                newRow.Cells[i].Value = originalRow.Cells[i].Value;
            }

            return newRow;
        }

    }
}
