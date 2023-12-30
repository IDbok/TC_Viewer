using TC_WinForms.DataProcessing;
using TC_WinForms.WinForms;
using TcModels.Models;

namespace TC_WinForms
{
    public partial class Win3 : Form
    {
        //public List<TechnologicalProcess> tps;
        //public TechnologicalProcess tp;
        //public List<TechnologicalCard> tcs;
        //public TechnologicalCard tc;
        public Win3()
        {
            // todo - need to add some ather class to communicate with db not in the forms
            
            InitializeComponent();
            
            DbConnector db = new DbConnector();
            Program.ExistingProcces = db.GetList<TechnologicalProcess>();

            foreach (var item in Program.ExistingProcces)
            {
                cmbTechProcessName.Items.Add(item.Name);
            }

        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            WinProcessing.BackFormBtn(this);
        }

        private void Win3_FormClosing(object sender, FormClosingEventArgs e)
        {
            WinProcessing.CloseingApp(e);
        }

        private void btnUpdateTC_Click(object sender, EventArgs e)
        {
            if (dgvTcInTp.SelectedRows.Count == 0) return;
            //Program.CurrentTp = Program.ExistingProcces[cmbTechProcessName.SelectedIndex];
            WinProcessing.NextFormBtn(new Win6(sender), this);
        }

        private void cmbTechProcessName_SelectedIndexChanged(object sender, EventArgs e)
        {
            // get selected item number
            //tp = Program.ExistingProcces[cmbTechProcessName.SelectedIndex];
            Program.CurrentTp = Program.ExistingProcces[cmbTechProcessName.SelectedIndex];
            //tcs = Program.CurrentTp.TechnologicalCards;
            Program.ExistingCatds = Program.CurrentTp.TechnologicalCards; // todo - delete dublicates of data storages

            dgvTcInTp.Rows.Clear();
            for (int i = 0; i < Program.ExistingCatds.Count; i++)
            {
                dgvTcInTp.Rows.Add(i, Program.ExistingCatds[i].Name, Program.ExistingCatds[i].Article);
            }
        }
        private void dgvTcInTp_SelectionChanged(object sender, EventArgs e)
        {

            if (dgvTcInTp.SelectedRows.Count == 0) return;
            // get selected item number
            int selectedRow = dgvTcInTp.SelectedRows[0].Index;
            Program.currentTc = Program.ExistingCatds[selectedRow];
            dgvTcInTp.Rows[dgvTcInTp.SelectedRows[0].Index].Selected = true;
        }
        private void dgvTcInTp_ColumnRemoved(object sender, DataGridViewColumnEventArgs e)
        {
            MessageBox.Show("ColumnRemoved");
        }

        private void btnAddNewTC_Click(object sender, EventArgs e)
        {
            WinProcessing.NextFormBtn(new Win6(sender), this);
        }

        private void btnDeleteTC_Click(object sender, EventArgs e)
        {

        }
    }
}
