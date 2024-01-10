using Microsoft.VisualBasic.Devices;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace TC_WinForms.WinForms
{
    public partial class Win6 : Form
    {
        EModelType? activeModelType = null;

        DbConnector db = new DbConnector();

        public Win6(object sender)
        {

            if (sender is not Button) this.Close();

            InitializeComponent();

            /////////////////////////////

            if (Program.ExistingCatds.Count == 0)
            {
                Program.ExistingProcces = db.GetList<TechnologicalProcess>();
                Program.CurrentTp = Program.ExistingProcces[0];
                Program.ExistingCatds = Program.CurrentTp.TechnologicalCards;
                Program.currentTc = Program.ExistingCatds[0];
            }

            ////////////////////////////////////

            //todo - ???? maybe better make unable to change TechProcessName in this form

            foreach (var item in Program.ExistingProcces)
            {
                cmbTechProcessName.Items.Add(item.Name);
            }
            cmbTechProcessName.Text = Program.CurrentTp.Name;

            Button btn = (Button)sender;
            if (btn.Name == "btnUpdateTC")
            {
                db.UpdateCurrentTc(Program.currentTc.Id);

                foreach (var item in Program.ExistingCatds)
                {
                    cmbTechCardName.Items.Add(item.Article);
                }
                cmbTechCardName.Text = Program.currentTc.Article;
            }
            else if (btn.Name == "btnAddNewTC")
            {
                cmbTechCardName.Visible = false;
                pnlNavigationTC.Controls.Add(new Label() { Name = "lblNewTc", Text = "Артикул ТК:", Location = new Point(350, 8) });
                // todo - add сheck if tbxNewTcName.Text already exists in db
                pnlNavigationTC.Controls.Add(new TextBox() { Name = "tbxNewTcName", Location = new Point(450, 4), Size = new Size(180, 28) });

                Program.NewTc = new TechnologicalCard();
            }

            btnShowStaffs_Click(btnShowStaffs, null);
            btnSaveChanges.Enabled = true; // todo - make check if changes were made
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            WinProcessing.BackFormBtn(this);
        }
        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            SaveDataFromDGV(activeModelType);

            //var db = new DbConnector();

            if (Program.currentTc.Id == 0)
            {
                MessageBox.Show("ТК не сохранена в БД. Сохранение невозможно.");
                //Program.CurrentTc.Article = ((TextBox)pnlNavigationTC.Controls["tbxNewTcName"]).Text;
                //Program.CurrentTc.TechnologicalProcessId = Program.CurrentTp.Id;
                //db.Add(Program.CurrentTc);
                //Program.ExistingCatds.Add(Program.CurrentTc);
                //Program.CurrentTp.TechnologicalCards.Add(Program.CurrentTc);
                //Program.CurrentTc = Program.ExistingCatds[Program.ExistingCatds.Count - 1];
                //cmbTechCardName.Items.Add(Program.CurrentTc.Article);
                //cmbTechCardName.Text = Program.CurrentTc.Article;
            }
            else
            {
                //db.Update(ref Program.currentTc);
                db.SaveChanges(Program.currentTc);
                MessageBox.Show("Изменения сохранены в БД.");
            }
        }

        private void Win6_FormClosing(object sender, FormClosingEventArgs e)
        {
            WinProcessing.CloseingApp(e);
        }

        private void cmbTechCardName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Program.currentTc != null && Program.currentTc.Article == cmbTechCardName.Text) return;
            else
            {
                db.UpdateCurrentTc(Program.ExistingCatds[cmbTechCardName.SelectedIndex].Id);
                //MessageBox.Show("Выбрана новая ТК: " + Program.CurrentTc.Article);
                btnShowStaffs_Click(null, null);
            }
        }
        /// <summary>
        /// Add new rows to Table typeof DataGridView from obj
        /// </summary>
        /// <typeparam name="T">It is intermediate table type</typeparam>
        /// <typeparam name="C">It is Child model that is implemented in intermediate table</typeparam>
        /// <param name="obj"></param>
        /// <param name="DGV"></param>
        

        private void btnShowStaffs_Click(object sender, EventArgs e)
        {
            // todo - save changes in dgvTcObjects if they are
            if (activeModelType == EModelType.Staff) return;
            SaveDataFromDGV(activeModelType);
            DGVNewStructure(EModelType.Staff);

            var obj = Program.currentTc.Staff_TCs.OrderBy(x => x.Order).ToList();

            WinProcessing.AddNewRowsToDGV(obj, dgvTcObjects);

            if (sender is Button)
                WinProcessing.ColorizeOnlyChosenButton(sender as Button, pnlControls);

        }

        private void btnShowComponents_Click(object sender, EventArgs e)
        {
            if (activeModelType == EModelType.Component) return;
            SaveDataFromDGV(activeModelType);
            DGVNewStructure(EModelType.Component);
            var obj = Program.currentTc.Component_TCs.OrderBy(x => x.Order).ToList();
            WinProcessing.AddNewRowsToDGV<Component_TC, TcModels.Models.TcContent.Component>(obj, dgvTcObjects);
            if (sender is Button)
                WinProcessing.ColorizeOnlyChosenButton(sender as Button, pnlControls);
        }

        private void btnShowMachines_Click(object sender, EventArgs e)
        {
            if (activeModelType == EModelType.Machine) return;
            SaveDataFromDGV(activeModelType);
            DGVNewStructure(EModelType.Machine);

            var obj = Program.currentTc.Machine_TCs.OrderBy(x => x.Order).ToList();
            WinProcessing.AddNewRowsToDGV<Machine_TC, Machine>(obj, dgvTcObjects);
            if (sender is Button)
                WinProcessing.ColorizeOnlyChosenButton(sender as Button, pnlControls);
        }

        private void btnShowProtections_Click(object sender, EventArgs e)
        {
            if (activeModelType == EModelType.Protection) return;
            SaveDataFromDGV(activeModelType);
            DGVNewStructure(EModelType.Protection);
            var obj = Program.currentTc.Protection_TCs.OrderBy(x => x.Order).ToList();
            WinProcessing.AddNewRowsToDGV<Protection_TC, Protection>(obj, dgvTcObjects);
            if (sender is Button)
                WinProcessing.ColorizeOnlyChosenButton(sender as Button, pnlControls);
        }

        private void btnShowTools_Click(object sender, EventArgs e)
        {
            if (activeModelType == EModelType.Tool) return;
            SaveDataFromDGV(activeModelType);
            DGVNewStructure(EModelType.Tool);
            var obj = Program.currentTc.Tool_TCs.OrderBy(x => x.Order).ToList();
            WinProcessing.AddNewRowsToDGV<Tool_TC, Tool>(obj, dgvTcObjects);
            if (sender is Button)
                WinProcessing.ColorizeOnlyChosenButton(sender as Button, pnlControls);
        }

        private void btnShowWorkSteps_Click(object sender, EventArgs e)
        {
            if (activeModelType == EModelType.WorkStep) return;
            //SaveDataFromDGV(activeModelType);
            DGVNewStructure(EModelType.WorkStep);
            activeModelType = EModelType.WorkStep;
            if (sender is Button)
                WinProcessing.ColorizeOnlyChosenButton(sender as Button, pnlControls);
        } // todo - make it work

        
        private void SaveDataFromDGV(EModelType? ModelType) // todo - ??? mb beter catch changes and save them in Program.CurrentTc
        {

            if (dgvTcObjects.Rows.Count == 1 || ModelType == null) return;

            int id = 0;
            int order = 0;

            if (ModelType == EModelType.Staff)
            {
                for (int i = 0; i < dgvTcObjects.Rows.Count - 1; i++)
                {

                    int.TryParse(dgvTcObjects.Rows[i].Cells["Id"].Value.ToString(), out id);
                    int.TryParse(dgvTcObjects.Rows[i].Cells["Num"].Value.ToString(), out order);

                    string name = dgvTcObjects.Rows[i].Cells["Title"].Value.ToString(); // todo - make null values enable to be in dgvTcObjects in not null columns
                    string type = dgvTcObjects.Rows[i].Cells["Type"].Value.ToString();
                    string combineResponsibility = dgvTcObjects.Rows[i].Cells["СombineResponsibility"].Value.ToString();
                    string qualification = dgvTcObjects.Rows[i].Cells["Qualification"].Value.ToString();

                    string symbol = dgvTcObjects.Rows[i].Cells["Symbol"].Value.ToString();

                    // check if staff is already exists in db
                    var staff = db.GetObject<Staff>(id);
                    Staff newStaff = new Staff()
                    {
                        Id = staff.Id != null ? staff.Id : 0,
                        Name = name,
                        Type = type,
                        CombineResponsibility = combineResponsibility,
                        Qualification = qualification,
                    };

                    if (staff == null)
                    {
                        db.Add(newStaff); // todo - make an storege of new staffs to add them all at once
                        staff = newStaff;
                    }
                    else
                    {
                        if (!(staff.Id == newStaff.Id &&
                            staff.Name == newStaff.Name &&
                            staff.Type == newStaff.Type &&
                            staff.CombineResponsibility == newStaff.CombineResponsibility &&
                            staff.Comment == newStaff.Comment &&
                            staff.Qualification == newStaff.Qualification 
                            //&&
                            //staff.ElSaftyGroup == newStaff.ElSaftyGroup &&
                            //staff.Grade == newStaff.Grade
                            ))
                        {
                            // db.Update(staff); // todo - update objects in object related to TurrentTc
                        }
                        else return; // end of method if staff is not changed
                    }

                    // Check if staff is already connected to TC. If not - add new Staff_TC object to db
                    var sttc = db.GetObject<Staff_TC, Staff>(Program.currentTc.Id, staff.Id);
                    if (sttc == null)
                    {
                        Program.currentTc.Staff_TCs.Add(new Staff_TC()
                        {
                            Order = order,
                            Child = staff,
                            Symbol = symbol
                        });
                    }
                    else
                    {
                        sttc.Order = order;
                        sttc.Symbol = symbol;
                        // db.Update(sttc);
                    }
                }
            }
            //else if (ModelType == EModelType.WorkStep)
            //{
            //    Program.CurrentTc.WorkSteps.Clear();
            //    for (int i = 0; i < dgvTcObjects.Rows.Count - 1; i++)
            //    {
            //        int.TryParse(dgvTcObjects.Rows[i].Cells["Num"].Value.ToString(), out int order);
            //        Program.CurrentTc.WorkSteps.Add(new WorkStep()
            //        {
            //            Order = order,
            //            Operation = dgvTcObjects.Rows[i].Cells["Operation"].Value.ToString(),
            //            Name = dgvTcObjects.Rows[i].Cells["Name"].Value.ToString(),
            //            StepTime = (int)dgvTcObjects.Rows[i].Cells["StepTime"].Value,
            //            Staff_TCs = new List<Staff_TC>(),
            //            Component_TCs = new List<Component_TC>(),
            //            Machine_TCs = new List<Machine_TC>(),
            //            Protection_TCs = new List<Protection_TC>(),
            //            Tool_TCs = new List<Tool_TC>()
            //        });
            //    }
            //}
            else
            {
                if (ModelType == EModelType.Component)
                {
                    var objects = WinProcessing.MappDataFromDGV<Component_TC, TcModels.Models.TcContent.Component>(dgvTcObjects, ref Program.currentTc);
                    WinProcessing.AddDataToTC<Component_TC, TcModels.Models.TcContent.Component>(objects, ref Program.currentTc);
                }
                else if (ModelType == EModelType.Machine)
                {
                    var objects = WinProcessing.MappDataFromDGV<Machine_TC, Machine>(dgvTcObjects, ref Program.currentTc);
                    WinProcessing.AddDataToTC<Machine_TC, Machine>(objects, ref Program.currentTc);
                }
                else if (ModelType == EModelType.Protection)
                {
                    var objects = WinProcessing.MappDataFromDGV<Protection_TC, Protection>(dgvTcObjects, ref Program.currentTc);
                    WinProcessing.AddDataToTC<Protection_TC, Protection>(objects, ref Program.currentTc);
                }
                else if (ModelType == EModelType.Tool)
                {
                    var objects = WinProcessing.MappDataFromDGV<Tool_TC,Tool>(dgvTcObjects, ref Program.currentTc);
                    WinProcessing.AddDataToTC<Tool_TC, Tool>(objects, ref Program.currentTc);
                }
            }
        }


        private void DGVStructure(Dictionary<string, string> columnNames)
        {
            dgvTcObjects.Columns.Clear();
            foreach (var item in columnNames)
            {
                dgvTcObjects.Columns.Add(item.Key, item.Value);
            }
        }
        /// <summary>
        /// Add new columns to dgvTcObjects and set activeModelType
        /// </summary>
        /// <param name="modelType"> Enum that represents models of TC tables structure (Staff, Tool, etc)</param>
        private void DGVNewStructure(EModelType modelType)
        {

            if (dgvTcObjects.Columns.Count != 0 && activeModelType == modelType) return;
            // todo - make check if dgvTcObjects is empty onece

            Dictionary<string, string> data = new()
                {
                    {"Id", "Индекс в БД" },
                    { "Num", "Порядковый номер" },
                    { "Title", "Наименование"}
                };
            if (modelType == EModelType.Staff)
            {
                data.Add("Type", "Тип");
                data.Add("СombineResponsibility", "Совмещение обазанностей");
                data.Add("Competence", "Квалификация");
                data.Add("Symbol", "Обознаяение в ТК");
            }
            else if (modelType == EModelType.WorkStep)
            {

                data.Add("Operation", "Технологические операции");
                data.Add("StepTime", "Время выполнения действия, мин");
                data.Add("Staff", "Персонал");
                data.Add("Component", "Материалы и комплектующие");
                data.Add("Machine", "Механизмы");
                data.Add("Protection", "Средства защиты");
                data.Add("Tool", "Инструменты и приспособления");

            }
            else
            {
                data.Add("Type", "Тип");
                data.Add("Unit", "Ед.Изм.");
                data.Add("Quantity", "Кол-во");
                data.Add("Price", "Стоимость, руб. без НДС");
            }

            DGVStructure(data);
            dgvTcObjects.Columns["Id"].ReadOnly = true;
            activeModelType = modelType;
        }

        private void dgvTcObjects_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvTcObjects.Columns[e.ColumnIndex].Name == "Num")
            {
                dgvTcObjects.Columns[e.ColumnIndex].ValueType = typeof(int);

                //dgvTcObjects.Sort(dgvTcObjects.Columns["Num"], ListSortDirection.Ascending);
                //Int32.TryParse
                //bool x = Int32.TryParse(str, out int x);

                Array.Sort(Array.ConvertAll(dgvTcObjects.Rows.Cast<DataGridViewRow>().ToArray(), x => x.Index + 1));
            }
        }

        private void ChangeRowsOrder(DataGridViewRow[] rows, int indexToInsert) // todo - can make from contextmenustrip in right mouse up on row (select all row) 
        {
            rows = rows.OfType<DataGridViewRow>().OrderBy(x => x.Index).ToArray();
            // create array of selected row's indexes
            int[] indexes = new int[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                indexes[i] = rows[i].Index;
            }
            foreach (DataGridViewRow row in rows)
            {
                dgvTcObjects.Rows.Remove(row);
            }
            //for (int i = 0; i < rows.Count(); i++)// (DataGridViewRow row in rows)
            //{
            //    dgvTcObjects.Rows.Insert(indexToInsert, rows[i]);
            //}
            //if (indexToInsert > dgvTcObjects.Rows.Count - 1) indexToInsert = dgvTcObjects.Rows.Count - 1;
            if (indexToInsert > (dgvTcObjects.Rows.Count - rows.Length)) indexToInsert = indexToInsert - rows.Length;
            for (int i = rows.Count() - 1; i >= 0; i--)// (DataGridViewRow row in rows)
            {
                dgvTcObjects.Rows.Insert(indexToInsert, rows[i]);
            }
        }

        /// <summary>
        /// method to change dgvTcObjects Num row's values in the existing order
        /// </summary>
        private void ChangeNumValues()
        {
            for (int i = 0; i < dgvTcObjects.Rows.Count - 1; i++)
            {
                dgvTcObjects.Rows[i].Cells["Num"].Value = i + 1;
            }
        }

        /// <summary>
        /// This field is used to check if right mouse down event is still processing
        /// </summary>
        bool mouseDown = false;
        DataGridViewRow[] selectedRows;

        private void dgvTcObjects_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            //MessageBox.Show($"CellMouseUp in {e.RowIndex}");
            if (mouseDown)
            {
                //ChangeRowsOrder(selectedRows, e.RowIndex);
                //foreach (DataGridViewRow item in selectedRows)
                //{
                //    item.Selected = true;
                //}
                selectedRows = null;
            }
        }
        private void dgvTcObjects_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            //var point = dgvTcObjects.PointToClient(MousePosition);
            //var info = dgvTcObjects.HitTest(point.X, point.Y);
            // check if mouse down on selected rows
            if (dgvTcObjects.SelectedRows.Count != 0 &&
                dgvTcObjects.SelectedRows.Contains(dgvTcObjects.Rows[e.RowIndex]) &&
                e.Button == MouseButtons.Left)
            {
                selectedRows = new DataGridViewRow[dgvTcObjects.SelectedRows.Count];

                for (int i = 0; i < dgvTcObjects.SelectedRows.Count; i++)
                {
                    selectedRows[i] = dgvTcObjects.SelectedRows[i];
                }


                mouseDown = true;
                Cursor.Current = Cursors.Hand;
                // make time delay to check if mouse is still down
                //Thread.Sleep(500);
                foreach (DataGridViewRow item in selectedRows)
                {
                    item.Selected = true;
                }
                // force to off mouse down event
                dgvTcObjects.CellMouseDown -= dgvTcObjects_CellMouseDown;
            }
        }

        private void dgvTcObjects_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            // get indexes of removed rows

            // !!! rows are removing one by one !!!


            //int index = e.RowIndex - 1;

            //int id = (int)dgvTcObjects.Rows[index].Cells["Id"].Value;
            //int order = (int)dgvTcObjects.Rows[index].Cells["Num"].Value;

            //MessageBox.Show($"RowRemoved: {string.Join(", ", index)}");
        }

        private void DeleteRelationInTC(int id, int order,EModelType? modelType)
        {
            if (modelType == EModelType.Staff)
            {
                var sttc = Program.currentTc.Staff_TCs.Find(x => x.ChildId == id && x.Order == order);
                Program.currentTc.Staff_TCs.Remove(sttc);
            }
            else if (modelType == EModelType.Component)
            {
                var sttc = Program.currentTc.Component_TCs.Find(x => x.ChildId == id);
                Program.currentTc.Component_TCs.Remove(sttc);
            }
            else if (modelType == EModelType.Machine)
            {
                var sttc = Program.currentTc.Machine_TCs.Find(x => x.ChildId == id);
                Program.currentTc.Machine_TCs.Remove(sttc);
            }
            else if (modelType == EModelType.Protection)
            {
                var sttc = Program.currentTc.Protection_TCs.Find(x => x.ChildId == id);
                Program.currentTc.Protection_TCs.Remove(sttc);
            }
            else if (modelType == EModelType.Tool)
            {
                var sttc = Program.currentTc.Tool_TCs.Find(x => x.ChildId == id);
                Program.currentTc.Tool_TCs.Remove(sttc);
            }
            else if (modelType == EModelType.WorkStep)
            {
                //var sttc = Program.CurrentTc.WorkSteps.Find(x => x.ChildId == id);
                //Program.CurrentTc.WorkSteps.Remove(sttc);
            }
        }

        private void dgvTcObjects_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            // get indexes of removing rows
            int index = e.Row.Index;


            int id = (int)dgvTcObjects.Rows[index].Cells["Id"].Value;
            int order = (int)dgvTcObjects.Rows[index].Cells["Num"].Value;

            DeleteRelationInTC(id, order, activeModelType);

            // MessageBox.Show($"UserDeletingRow: {string.Join(", ", index)}");
        }
    }

}

