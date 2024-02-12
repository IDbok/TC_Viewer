using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using System.DirectoryServices.ActiveDirectory;
using TC_WinForms.DataProcessing;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms
{
    internal static class WinProcessing
    {
        private static Author author = new();
        public static bool isDataToSave { get; set; } = false;
        private static void SaveData() 
        {

            // todo - save data from all forms
            Form[] allForms = Program.FormsBack.Concat(Program.FormsForward).ToArray();
            foreach (Form form in allForms)
            {
                if (form is Win2)
                { 
                    Win2 saveableForm = (Win2)form;
                    //DataJsonSerializer.Serialize<TcData>(saveableForm.tp, saveableForm.GetPath());
                }
            }
            //Form[] allForms = Program.FormsBack.Concat(Program.FormsForward).ToArray();
            //foreach (Form form in allForms)
            //{
            //    if (form is ISaveableForm)
            //    {
            //        ISaveableForm saveableForm = (ISaveableForm)form;
            //        DataJsonSerializer.Serialize(saveableForm.DataToSave(), saveableForm.GetPath());
            //    }
            //}
            MessageBox.Show("Данные сохранены", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        public static List<T> MappDataFromDGV<T,C>(DataGridView dgvTcObjects, ref TechnologicalCard tc) 
            where T : class, IStructIntermediateTable<TechnologicalCard, C>, new()
            where C : class, IModelStructure, new()
        {
            DbConnector db = new DbConnector();

            var objects = new List<T>();

            int quantity = 0;
            float price = 0;

            int id, order;

            for (int i = 0; i < dgvTcObjects.Rows.Count - 1; i++)
            {
                int.TryParse(dgvTcObjects.Rows[i].Cells["Id"].Value.ToString(), out id);
                int.TryParse(dgvTcObjects.Rows[i].Cells["Num"].Value.ToString(), out order);
                
                float.TryParse(dgvTcObjects.Rows[i].Cells["Price"].Value?.ToString(), out price);

                string name = dgvTcObjects.Rows[i].Cells["Title"].Value?.ToString(); // todo - make null values enable to be in dgvTcObjects in not null columns
                string type = dgvTcObjects.Rows[i].Cells["Type"].Value?.ToString();
                string unit = dgvTcObjects.Rows[i].Cells["Unit"].Value?.ToString();

                //if (IsEqual<T, C>(tc.Id, order, price, name, type, unit))
                //{
                    
                //}

                // check if object is already exists in db
                var obj = db.GetObjFromDbOrNew<C>(id, name, type, unit, price);

                // check if object is already connected to TC. If not - add new connection in TC
                var obj_tc = db.GetObjFromDbOrNew<T, C>(ref tc, obj);

                //db.GetObjFromDbOrNew<Tool_TC,Tool)>(ref tc, obj as Tool);
                obj_tc.Order = order;
                obj_tc.Quantity = quantity;

                objects.Add(obj_tc);
            }
            return objects;
        }

        // check if objects are equal
        public static bool IsEqual<T>(T obj1, int id, string name, string type, string unit, float? price) where T : class, IModelStructure
        {
            if (obj1 == null ) return false;

            if (!(obj1.Id == id &&
                obj1.Name == name &&
                obj1.Type == type &&
                obj1.Unit == unit))
                //if (price == null ||obj1.Price == price) // todo  - are we compare price? Can user change price in TC?
                    return true;
            return false;
        }
        public static bool IsEqual<T,C>(int tc_id, T obj1, int order,int quantity, int id, string name, string type, string unit, float? price) where T : class, IStructIntermediateTable<TechnologicalCard, C>
            where C : class, IModelStructure
        {
            if (obj1 == null) return false;

            if (IsEqual(obj1.Child, id, name, type, unit, price)) 
                if (obj1.ChildId == id &&
                    obj1.ParentId == tc_id &&
                    obj1.Order == order &&
                    obj1.Quantity == quantity) 
                    return true;
            return false;
        }

        public static void AddDataToTC<T, C>(List<T> objects, ref TechnologicalCard tc)
        {
            if (typeof(T) == typeof(Staff_TC))
            {
                tc.Staff_TCs.Clear();
                foreach (var item in objects)
                {
                    tc.Staff_TCs.Add(item as Staff_TC);
                }
            }
            else if (typeof(T) == typeof(Component_TC))
            {
                tc.Component_TCs.Clear();
                foreach (var item in objects)
                {
                    tc.Component_TCs.Add(item as Component_TC);
                }
            }
            else if (typeof(T) == typeof(Tool_TC))
            {
                tc.Tool_TCs.Clear();
                foreach (var item in objects)
                {
                    tc.Tool_TCs.Add(item as Tool_TC);
                }
            }
            else if (typeof(T) == typeof(Machine_TC))
            {
                tc.Machine_TCs.Clear();
                foreach (var item in objects)
                {
                    tc.Machine_TCs.Add(item as Machine_TC);
                }
            }
            else if (typeof(T) == typeof(Protection_TC))
            {
                tc.Protection_TCs.Clear();
                foreach (var item in objects)
                {
                    tc.Protection_TCs.Add(item as Protection_TC);
                }
            }
            
        }

        public static void AddNewRowsToDGV<T, C>(List<T> obj, DataGridView DGV)
            where T : IStructIntermediateTable<TechnologicalCard, C>
            where C : IModelStructure
        {
            for (int i = 0; i < obj.Count; i++)
            {
                DGV.Rows.Add(
                    (int)obj[i].ChildId,
                    (int)obj[i].Order,

                    obj[i].Child.Name,
                    obj[i].Child.Type,
                    obj[i].Child.Unit,

                    obj[i].Quantity);
                // todo - fix it
            }
        }
        
        public static bool CloseAppMessage(FormClosingEventArgs e, out bool saveDate)
        {
            saveDate = false;
            bool closeApp = true;
            //if (Program.testMode) return closeApp;
            if (isDataToSave)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Сохранить промежуточные результаты перед выходом?", 
                    "Выход из программы", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Yes) 
                { 
                    saveDate = true;
                    closeApp = true;
                }
                else if (dialogResult == DialogResult.No) closeApp = true;
                else closeApp = false;
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход из программы", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No) closeApp = false;
                
            }
            return closeApp;
        }
        public static void CloseingApp(FormClosingEventArgs e) 
        {
            bool closeApp = CloseAppMessage(e, out bool saveData);
            e.Cancel = !closeApp;
            if (saveData)
            {
                SaveData();
            }
            if (closeApp) Application.ExitThread();
        }
        // todo - ovverload colorization method with except buttons array (not change their color) (default olny one button)

        public static void ColorizeOnlyChosenButton(object sender, GroupBox gbx)
        {   
            foreach (Control btn2 in gbx.Controls)
            { btn2.BackColor = Color.FromArgb(255, 255, 255); }
            ColorizeChosenButton(sender, Color.FromArgb(128, 255, 191));
        }
        public static void ColorizeOnlyChosenButton(Button sender, Panel pnl ) // todo - make able to change colors
        {
            
            foreach (Control btn2 in pnl.Controls)
            { btn2.BackColor = Color.FromArgb(255, 255, 255); }
            ColorizeChosenButton(sender, Color.FromArgb(128, 255, 191));
        }
        public static void ColorizeChosenButton(object sender, Color btnColor)
        {
            Button btn = (Button)sender;
            btn.BackColor = btnColor;
        }
        public static void ColorizeBtnAndEnableControl(object sender, GroupBox gbxButtons, Control enableControl)
        {
            ColorizeBtnAndEnableControl(sender,  gbxButtons, new Control[] { enableControl });
        }
        public static void ColorizeBtnAndEnableControl(object sender, GroupBox gbxButtons, Control[] enableControl)
        {
            WinProcessing.ColorizeOnlyChosenButton(sender, gbxButtons);
            foreach (Control control in enableControl) control.Enabled = true;
        }
        public static bool CheckAllTextBoxes(GroupBox gbx)
        {
            foreach (Control txt in gbx.Controls)
            {
                if (txt is TextBox)
                {
                    if (txt.Text == "")
                    {
                        MessageBox.Show("Заполните все поля!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }
            return true;
        }
        public static Author GetAuthUser()
        {
            return Authorizator.author;
        }
        public static void NextFormBtn(Form newForm, Form thisForm)
        {
            Program.FormsBack.Add(thisForm);
            Program.MainForm = newForm;
            Program.MainForm.Show();
            thisForm.Hide();
        }
        public static void NextFormBtn(Form thisForm)
        {
            Program.FormsBack.Add(thisForm);
            Program.MainForm = Program.FormsForward.Last();
            Program.MainForm.Show();
            thisForm.Hide();
        }
        public static void BackFormBtn(Form thisForm)
        {
            Program.FormsForward.Add(thisForm);
            Program.MainForm = Program.FormsBack.Last();
            Program.FormsBack.RemoveAt(Program.FormsBack.Count - 1);
            Program.MainForm.Show();
            thisForm.Hide();
        }

        public static void AddItemsToListBox(ListBox listBox, List<string> items)
        {
            foreach (var item in items)
            {
                listBox.Items.Add(item);
            }
        }

        /// <summary>
        /// Set columns order and visibility in dgvMain
        /// </summary>
        /// <param name="columnOrder">is Dictionary with columnes names as keys and order as values. If order sets as "-1" that meant columns must be hiden</param>
        public static void SetTableColumnsOrder(Dictionary<string, int> columnOrder, DataGridView dgv)
        {
            columnOrder.Keys.ToList().ForEach(x =>
            {
                if (columnOrder[x] == -1)
                    dgv.Columns[x].Visible = false;

                else
                    dgv.Columns[x].DisplayIndex = columnOrder[x];
            });
        }
        public static void SetTableHeadersNames(Dictionary<string, string> columnNames, DataGridView dgv)
        {
            columnNames.Keys.ToList().ForEach(x => dgv.Columns[x].HeaderText = columnNames[x]);
        }
    }
}
