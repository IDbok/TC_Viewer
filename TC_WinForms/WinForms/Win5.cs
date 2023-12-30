using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TC_WinForms
{
    //todo - make able to open only one form of this type
    public partial class Win5 : Form
    {
        object formSender;
        Form opendForm;
        string? prevChoice = null;
        string? ButtonName = null;
        string? ButtonText = null;
        public Win5(object sender, Form opendForm)
        {
            formSender = sender;
            this.opendForm = opendForm;
            InitializeComponent();
            if (sender is Button)
            {
                Button btn = (Button)sender;
                ButtonName = btn.Name;
                ButtonText = btn.Text;
                this.Text = "Выбор: " + ButtonText;
            }
            prevChoice = CheckIfWasChosen();
            ChoiceDataToLoad(sender);
        }
        private void ChoiceDataToLoad(object sender)
        {
            List<string> dataToLoad1 = new() { "firstchoice", "secondchoice", "thirdchoice" };
            List<string> dataToLoad2 = new() { "firstchoice another", "secondchoice another", "thirdchoice another" };
            if (ButtonName != null)
            {
                switch (ButtonName)
                {
                    case "btnSwitchgear35":
                        LoadDataInCombobox(cmbChoice, dataToLoad1);
                        break;
                    case "btnSwitchgear10_6":
                        LoadDataInCombobox(cmbChoice, dataToLoad2);
                        break;
                }
            }
        }
        private void LoadDataInCombobox(ComboBox comboBox, List<string> dataToLoad)
        {
            comboBox.Items.Clear();
            foreach (string data in dataToLoad)
            {
                comboBox.Items.Add(data);
            }
            if (prevChoice != null) comboBox.Text = prevChoice;
            // todo - make able to calcel activation of button if no choice was made

        }

        private void cmbChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnAccept.Enabled = true;
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (ButtonName != null)
            {
                Button btn = (Button)formSender;

                btn.Text += "\n" + cmbChoice.Text;
                WinProcessing.ColorizeChosenButton(formSender, Color.FromArgb(128, 255, 191));

                SaveDataToOpendForm(ButtonName, cmbChoice.Text);

            }

            this.Close();
        }
        private void SaveDataToOpendForm(string btnName, string choice)
        {
            if (opendForm is Win4)
            {
                Win4 win4 = (Win4)opendForm;
                win4.AddDataToDictionaryTC("btnSwitchgear35", cmbChoice.Text);
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private string? CheckIfWasChosen()
        {
            if (opendForm is Win4)
            {
                Win4 form = (Win4)opendForm;
                form.tc.Data.TryGetValue(ButtonName, out string value);
                return value;
            }
            return null;
        }
    }
}
