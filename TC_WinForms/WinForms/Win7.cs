
namespace TC_WinForms.WinForms
{

    public partial class Win7 : Form
    {
        private int activeWin = 0;
        public Win7(int accessLevel)
        {

            InitializeComponent();
            AccessInitialization(accessLevel);
            DGVNewStructure(1);
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void AccessInitialization(int accessLevel)
        {
            if (accessLevel == 0) // viewer
            {
                // hide all buttons
                pnlControlBtns.Visible = false;
                pnlNavigationBtns.Visible = false;
                btnAddNewTC.Enabled = false;
                btnUpdateTC.Enabled = false;
                btnDeleteTC.Enabled = false;

            }
            else if (accessLevel == 1) // TC editor
            {
                // false visibility of all buttons in pnlControlBtns except btnAddNewTC
                foreach (Control btn2 in pnlNavigationBtns.Controls)
                { btn2.Visible = false; }
                btnTechCard.Visible = true;

            }
            else if (accessLevel == 2) // Progect editor
            {
                foreach (Control btn2 in pnlNavigationBtns.Controls)
                { btn2.Visible = false; }
                btnProject.Visible = true;
            }
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
        /// <summary>
        /// Add new columns to dgvTcObjects and set activeModelType
        /// </summary>
        /// <param name="modelType"> Enum that represents models of TC tables structure (Staff, Tool, etc)</param>
        private void DGVNewStructure(int winNumber)
        {

            if (dgvMain.Columns.Count != 0 && winNumber == 0) return;
            // todo - make check if dgvTcObjects is empty onece

            Dictionary<string, string> data = new()
                {
                    {"Id", "ID" },
                    { "Num", "Порядковый номер" },
                    { "Title", "Наименование"}
                };
            if (winNumber == 1) //TC
            {
                data.Add("Article", "Артикул");
                data.Add("TP", "Тех. процесс");
                data.Add("Information", "Доп. информация");
            }
            else if (winNumber == 2) // projects
            {

                data.Add("Operation", "Технологические операции");
                data.Add("StepTime", "Время выполнения действия, мин");
                data.Add("Staff", "Персонал");
                data.Add("Component", "Материалы и комплектующие");
                data.Add("Machine", "Механизмы");
                data.Add("Protection", "Средства защиты");
                data.Add("Tool", "Инструменты и приспособления");

            }
            else if (winNumber == 3) //TC
            {
                data.Add("Type", "Тип");
                data.Add("СombineResponsibility", "Совмещение обазанностей");
                data.Add("Competence", "Квалификация");
                data.Add("Symbol", "Обознаяение в ТК");
            }
            else if (winNumber == 4) // projects
            {

                data.Add("Operation", "Технологические операции");
                data.Add("StepTime", "Время выполнения действия, мин");
                data.Add("Staff", "Персонал");
                data.Add("Component", "Материалы и комплектующие");
                data.Add("Machine", "Механизмы");
                data.Add("Protection", "Средства защиты");
                data.Add("Tool", "Инструменты и приспособления");

            }
            else if (winNumber == 5) //TC
            {
                data.Add("Type", "Тип");
                data.Add("СombineResponsibility", "Совмещение обазанностей");
                data.Add("Competence", "Квалификация");
                data.Add("Symbol", "Обознаяение в ТК");
            }
            else if (winNumber == 6) // projects
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
            dgvMain.Columns["Id"].ReadOnly = true;
            activeWin = winNumber;
        }

        private void Win7_FormClosing(object sender, FormClosingEventArgs e)
        {
            WinProcessing.CloseingApp(e);
        }

    }
}
