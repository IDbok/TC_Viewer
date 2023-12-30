using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcModels.Models;

namespace TC_WinForms
{
    public partial class Win4 : Form
    {
        public TechnologicalCard tc;
        public Dictionary<string, string> ButtonNames = new();
        public void AddDataToDictionaryTC(string btnName, string choice) 
        {
            if (tc.Data.ContainsKey(btnName))
                tc.Data[btnName] = choice;
            else
                tc.Data.Add(btnName, choice);
        }
        public Win4()
        {
            InitializeComponent();
            tc = new TechnologicalCard();
        }

        private void Win4_FormClosing(object sender, FormClosingEventArgs e)
        {
            WinProcessing.CloseingApp(e);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            WinProcessing.BackFormBtn(this);
        }

        private void btnSwitchgear35_Click(object sender, EventArgs e)
        {
            ButtonClick(sender);
        }

        private void btnSwitchgear10_6_Click(object sender, EventArgs e)
        {

        }

        private void btnTrans_Click(object sender, EventArgs e)
        {

        }

        private void btnLightningRod_Click(object sender, EventArgs e)
        {

        }

        private void btnGround_Click(object sender, EventArgs e)
        {

        }

        private void btnFences_Click(object sender, EventArgs e)
        {

        }

        private void btnFoundation_Click(object sender, EventArgs e)
        {

        }

        private void btnIllumination_Click(object sender, EventArgs e)
        {

        }

        private void btnTransBusbar_Click(object sender, EventArgs e)
        {

        }

        private void btnConnections_Click(object sender, EventArgs e)
        {

        }

        private void btnLineTrap_Click(object sender, EventArgs e)
        {

        }

        private void ButtonClick(object sender)
        {
            ChoceFormOpen(sender);
            
        }
        private void ChoceFormOpen(object sender)
        {
            //Button btn = (Button)sender;
            Form newForm = new Win5(sender, this);
            newForm.Show();
        }
    }
}
