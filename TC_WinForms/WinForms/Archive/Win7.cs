
using System.ComponentModel;
using TC_WinForms.DataProcessing;
using TcModels.Models;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms
{

    public partial class Win7 : Form
    {
        private readonly User.Role _accessLevel;

        private DbConnector dbCon = new DbConnector();

        //private Win7_1_TCs winTCs = new Win7_1_TCs(3);



        private WinNumber activeWin = 0;

        private List<TechnologicalCard> tcList = new List<TechnologicalCard>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<TechnologicalCard> changedCards = new List<TechnologicalCard>();
        private TechnologicalCard newCard;


        public Win7(User.Role accessLevel)
        {
            _accessLevel = accessLevel;
            InitializeComponent();

            AccessInitialization();

            // btnTechCard_Click event
            btnTechCard_Click(null, null);

        }
        private void AccessInitialization()
        {
            // todo - reverse accessibility
            if (_accessLevel == User.Role.Lead)
            {
                activeWin = WinNumber.TC;
            }
            else if (_accessLevel == User.Role.Implementer) // TC editor
            {
                // false visibility of all buttons in pnlControlBtns except btnAddNewTC
                foreach (Control btn2 in pnlNavigationBtns.Controls)
                { btn2.Visible = false; }
                btnTechCard.Visible = true;

                activeWin = WinNumber.TC;

            }
            else if (_accessLevel == User.Role.ProjectManager) // Progect editor
            {
                foreach (Control btn2 in pnlNavigationBtns.Controls)
                { btn2.Visible = false; }
                btnProject.Visible = true;

                activeWin = WinNumber.Project;
            }
            else if (_accessLevel == User.Role.User) // viewer
            {
                // hide all buttons
                pnlControlBtns.Visible = false;
                pnlNavigationBtns.Visible = false;
                btnAddNewTC.Enabled = false;
                btnUpdateTC.Enabled = false;
                btnDeleteTC.Enabled = false;

                activeWin = WinNumber.TC;
            }

            MessageBox.Show($"Добро пожаловать, {_accessLevel}!");
        }

        private void UpdateChangedTcList()
        {
            foreach (var item in changedItemId)
            {
                changedCards.Add(tcList.Find(x => x.Id == item));
            }
        }



        /// <summary>
        /// Add new columns to dgvTcObjects and set activeModelType
        /// </summary>
        /// <param name="modelType"> Enum that represents models of TC tables structure (Staff, Tool, etc)</param>
        private void DGVNewStructure(WinNumber winNumber)
        {

            if (dgvMain.Columns.Count != 0 && winNumber == 0) return;
            // todo - make check if dgvTcObjects is empty onece

            Dictionary<string, string> data = new()
                {
                    { "Id", "ID" },
                    { "Num", "Порядковый номер" },
                    { "Title", "Наименование"}
                };
            if (winNumber == WinNumber.TC) //TC
            {
                data.Remove("Num");
                data.Remove("Title");

                data.Add("Article", "Артикул");
                data.Add("Version", "Версия");
                data.Add("Name", "Название");
                data.Add("Type", "Тип карты");
                data.Add("NetworkVoltage", "Сеть, кВ");
                data.Add("TechnologicalProcessType", "Тип тех. процесса");
                data.Add("TechnologicalProcessName", "Тех. процесс");
                data.Add("Parameter", "Параметр");
                data.Add("FinalProduct", "Конечный продукт (КП)");
                data.Add("Applicability", "Применимость техкарты");
                data.Add("Note", "Примечания");
                data.Add("IsCompleted", "Наличие");

                //dgvMain.Columns["Id"].Visible = false;
                //dgvMain.Columns["Version"].Visible = false;
                //dgvMain.Columns["Name"].Visible = false;


            }
            else if (winNumber == WinNumber.Project) // projects
            {

                data.Add("Operation", "Технологические операции");
                data.Add("StepTime", "Время выполнения действия, мин");
                data.Add("Staff", "Персонал");
                data.Add("Component", "Материалы и комплектующие");
                data.Add("Machine", "Механизмы");
                data.Add("Protection", "Средства защиты");
                data.Add("Tool", "Инструменты и приспособления");

            }
            else if (winNumber == WinNumber.Staff) //TC
            {
                data.Add("Type", "Тип");
                data.Add("СombineResponsibility", "Совмещение обазанностей");
                data.Add("Competence", "Квалификация");
                data.Add("Symbol", "Обознаяение в ТК");
            }
            else if (winNumber == WinNumber.Component) // projects
            {

                data.Add("Operation", "Технологические операции");
                data.Add("StepTime", "Время выполнения действия, мин");
                data.Add("Staff", "Персонал");
                data.Add("Component", "Материалы и комплектующие");
                data.Add("Machine", "Механизмы");
                data.Add("Protection", "Средства защиты");
                data.Add("Tool", "Инструменты и приспособления");

            }
            else if (winNumber == WinNumber.Machine) //TC
            {
                data.Add("Type", "Тип");
                data.Add("СombineResponsibility", "Совмещение обазанностей");
                data.Add("Competence", "Квалификация");
                data.Add("Symbol", "Обознаяение в ТК");
            }
            else if (winNumber == WinNumber.Protection) // projects
            {

                data.Add("Operation", "Технологические операции");
                data.Add("StepTime", "Время выполнения действия, мин");
                data.Add("Staff", "Персонал");
                data.Add("Component", "Материалы и комплектующие");
                data.Add("Machine", "Механизмы");
                data.Add("Protection", "Средства защиты");
                data.Add("Tool", "Инструменты и приспособления");

            }
            else if (winNumber == WinNumber.Machine) // projects
            {

                data.Add("Operation", "Технологические операции");
                data.Add("StepTime", "Время выполнения действия, мин");
                data.Add("Staff", "Персонал");
                data.Add("Component", "Материалы и комплектующие");
                data.Add("Machine", "Механизмы");
                data.Add("Protection", "Средства защиты");
                data.Add("Tool", "Инструменты и приспособления");

            }

            DGVStructure(data);
            dgvMain.Columns["Id"].ReadOnly = true;
            activeWin = winNumber;
        }
        private void DGVStructure(Dictionary<string, string> columnNames)
        {
            dgvMain.Columns.Clear();
            foreach (var item in columnNames)
            {
                dgvMain.Columns.Add(item.Key, item.Value);
            }
            // dgvMain.Columns["Id"].Visible = false;
            dgvMain.Columns["Id"].Width = 50;
        }


        private void btnTechCard_Click(object sender, EventArgs e)
        {
            //LoadFormInPanel(winTCs);


            //dgvMain.Columns.Clear();

            //tcList = dbCon.GetObjectList<TechnologicalCard>();

            //var bindingList = new BindingList<TechnologicalCard>(tcList);
            //dgvMain.DataSource = bindingList;

            //// set columns names
            //SetTableHeadersNames(TechnologicalCard.GetPropertiesNames());

            //// set columns order and visibility
            //SetTableColumnsOrder(TechnologicalCard.GetPropertiesOrder());
        }

        private void Win7_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (newCard != null)
            {
                dbCon.Delete<TechnologicalCard>(newCard.Id);
            }
            WinProcessing.ClosingApp(e);
        }

        private void dgvMain_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //display columns names
            MessageBox.Show(dgvMain.Columns[e.ColumnIndex].Name);
        }


        /// <summary>
        /// Set columns order and visibility in dgvMain
        /// </summary>
        /// <param name="columnOrder">is Dictionary with columnes names as keys and order as values. If order sets as "-1" that meant columns must be hiden</param>
        private void SetTableColumnsOrder(Dictionary<string, int> columnOrder)
        {
            columnOrder.Keys.ToList().ForEach(x =>
            {
                if (columnOrder[x] == -1)
                    dgvMain.Columns[x].Visible = false;

                else
                    dgvMain.Columns[x].DisplayIndex = columnOrder[x];
            });
        }
        private void SetTableHeadersNames(Dictionary<string, string> columnNames)
        {
            columnNames.Keys.ToList().ForEach(x => dgvMain.Columns[x].HeaderText = columnNames[x]);
        }



        private void dgvMain_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //check if it is checkbox column
            if (dgvMain.Columns[e.ColumnIndex].GetType() == typeof(DataGridViewCheckBoxColumn))
            {
                AddChangedItemToList((int)dgvMain.Rows[e.RowIndex].Cells["Id"].Value);
            }
        }

        private void dgvMain_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // check if dgvMain is empty
            if (e.RowIndex < 0) return;
            if (dgvMain.Rows.Count == 0) return;

            AddChangedItemToList((int)dgvMain.Rows[e.RowIndex].Cells["Id"].Value);

        }

        private void AddChangedItemToList(int id)
        {
            if (!changedItemId.Contains(id))
            {
                changedItemId.Add(id);
            }
        }

        private void btnUpdateTC_Click(object sender, EventArgs e)
        {
            UpdateChangedTcInDb();
        }

        private void btnDeleteTC_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count > 0)
            {
                List<DataGridViewRow> rowsToDelete = GetSelectedRows();

                // add articles to message
                string message = "Вы действительно хотите удалить выбранные карты?\n";
                foreach (var row in rowsToDelete)
                {
                    message += row.Cells["Article"].Value.ToString() + "\n";
                }

                string caption = "Удаление Технологических карт";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;
                result = MessageBox.Show(message, caption, buttons);


                if (result == DialogResult.Yes)
                {
                    foreach (var row in rowsToDelete)
                    {
                        dbCon.Delete<TechnologicalCard>((int)row.Cells["Id"].Value);
                    }
                    DeleteRows(rowsToDelete);
                }
            }
        }
        private void UpdateChangedTcInDb()
        {
            UpdateChangedTcList();
            dbCon.UpdateTcList(changedCards);

            changedItemId.Clear();
            changedCards.Clear();
        }
        private List<DataGridViewRow> GetSelectedRows()
        {
            List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dgvMain.SelectedRows)
            {
                selectedRows.Add(row);
            }
            return selectedRows;
        }
        public void DeleteRows(List<DataGridViewRow> rowsToDelete)
        {
            foreach (DataGridViewRow row in rowsToDelete)
            {
                dgvMain.Rows.Remove(row);
            }
        }
        enum WinNumber
        {
            TC = 1,
            Project = 2,
            Staff = 3,
            Component = 4,
            Machine = 5,
            Protection = 6,
            Tool = 7
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////

        

        private void btnAddNewTC_Click(object sender, EventArgs e)
        {
            if (newCard != null) // todo - add check for all required fields (ex. Type can be only as "Ремонтная", "Монтажная", "ТТ")
                if (newCard.Article == ""
                  || newCard.Type == ""
                  || newCard.NetworkVoltage == 0)
                {
                    MessageBox.Show($"Новая карта с id {newCard.Id} ещё не заполнена." +
                        $"\nВнесите обязательные поля (Артикул, Тип карты, Сеть, кВ)");
                    return;
                };

            // insert new row as the fist one to dgvMain as new item of tcList
            int newId = dbCon.GetLastId<TechnologicalCard>();
            tcList.Insert(0, new TechnologicalCard()
            {
                Id = newId + 1,
                Article = "",
                Version = "0.0.0.0",
                Name = "",
                Type = "",
                NetworkVoltage = 0,
                IsCompleted = false
            });
            dbCon.Add(tcList[0]);

            newCard = tcList[0];

            // scroll to new row
            dgvMain.FirstDisplayedScrollingRowIndex = 0;
            // refesh screen
            dgvMain.Refresh();
        }

        private void LoadFormInPanel(Form form)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            this.pnlDataViewer.Controls.Add(form);
            form.Show();
        }
    }
}
