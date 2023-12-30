

using TC_WinForms.DataProcessing;
using TcModels.Models;

namespace TC_WinForms
{
    public partial class Win2 : Form//, ISaveableForm
    {
        //static string titleTC = "Технологическая карта";
        public string ProjectType { get; set; }
        public TechnologicalProcess tp;
        public string GetPath() => @"Temp\ProjectData.json";
        public TechnologicalProcess DataToSave()
        {
            return tp;
        }
        public Win2()
        {
            InitializeComponent();

        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            UpdateProjectData();
            WinProcessing.BackFormBtn(this);
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            bool dataCompleted = WinProcessing.CheckAllTextBoxes(gbxProjectData);
            if (dataCompleted)
            {
                UpdateProjectData();
                WinProcessing.NextFormBtn(new Win4(), this);
            }
        }
        private void Win2_FormClosing(object sender, FormClosingEventArgs e)
        {
            WinProcessing.CloseingApp(e);
        }
        private Control[] GetEnableAfterChoise() => new Control[] { gbxProjectData, btnNext };
        private void btnTcTransPoint_Click(object sender, EventArgs e)
        {
            newProjectCreation(sender);
        }

        private void btnTcOilSub_Click(object sender, EventArgs e)
        {
            newProjectCreation(sender);
        }

        private void btnOpenSwitchgear_Click(object sender, EventArgs e)
        {
            newProjectCreation(sender);
        }


        private void newProjectCreation(object sender)
        {
            Button btn = (Button)sender;
            ProjectType = btn.Text;
            FillProjectData();
            WinProcessing.ColorizeBtnAndEnableControl(sender, gbxFunctionalityChoice, GetEnableAfterChoise());
        }
        private void FillProjectData()
        {
            if (tp == null) tp = new TechnologicalProcess();
            tp.Authors = new List<Author> { WinProcessing.GetAuthUser() };
            tp.Type = ProjectType;
            // todo - make an mehanism to get last num from db if previous project was saved
            DbConnector db = new DbConnector();

            //Random rnd = new Random();
            txtProjectNum.Text = (db.GetLastId<TechnologicalProcess>() + 1).ToString();//(db.GetLastId<TechnologicalProcess>()+1).ToString(); //rnd.Next(1000, 9999).ToString();
            txtProjectOperator.Text = tp.Authors[0].Name + " " + tp.Authors[0].Surname; // todo - how will work with multiple authors?
            txtProjectCreationDate.Text = tp.DateCreation.ToString();
            txtProjectVersion.Text = tp.Version;

            WinProcessing.isDataToSave = true;
        }
        private void UpdateProjectData()
        {
            if (tp != null)
            {
                tp.Name = txtProjectName.Text;
                tp.DateCreation = DateTime.Parse(txtProjectCreationDate.Text);
            }
        }
    }
}
