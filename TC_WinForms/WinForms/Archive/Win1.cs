
using static TC_WinForms.DataProcessing.Authorizator;

namespace TC_WinForms
{
    public partial class Win1 : Form
    {
        private void TestMode()
        {
            IsUserExust("bokarev.fic@gmail.com", "pass");
            AfterAuthorization();
        }

        bool isTcDesign = false;
        bool isTcEditng = false;
        public Win1()
        {
            InitializeComponent();
            if (Program.testMode) TestMode();

            // add some test data in lbxPreviousActionsTC
            lbxPreviousActionsTC.Items.Add("Добропожаловать в программу!");
            lbxPreviousActionsTC.Items.Add("Для начала работы необходимо");
            lbxPreviousActionsTC.Items.Add("авторизоваться в системе.");

        }

        private void btnAuthorization_Click(object sender, EventArgs e)
        {
            PerformAuthorization();
        }

        private void btnTcDesign_Click(object sender, EventArgs e)
        {
            DisingOrEdingActive(sender);
        }
        private void btnTcEditng_Click(object sender, EventArgs e)
        {
            DisingOrEdingActive(sender);
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (isTcDesign)
            {
                // check if Program.FormsForward is not null and not win2 clean it
                if (Program.FormsForward.Count != 0 && Program.FormsForward.Last() is Win2) WinProcessing.NextFormBtn(this);
                else
                {
                    Program.FormsForward.Clear();
                    WinProcessing.isDataToSave = false;
                    WinProcessing.NextFormBtn(new Win2(), this);
                }
            }
            else if (isTcEditng)
            {
                // todo - Make unable form while loading data (m.b. add loading circle)

                if (Program.FormsForward.Count != 0 && Program.FormsForward.Last() is Win3) WinProcessing.NextFormBtn(this);
                else
                {
                    Program.FormsForward.Clear();
                    WinProcessing.isDataToSave = false;
                    WinProcessing.NextFormBtn(new Win3(), this);
                }
            }
        }
        private void Win1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !WinProcessing.CloseAppMessage(e, out bool saveData);

        }
        private void PerformAuthorization()
        {
            try
            {
                txtLogin.Text = txtLogin.Text.Trim();
                txtPassword.Text = txtPassword.Text.Trim();
                if (txtLogin.Text.Length > 0 && txtPassword.Text.Length > 0)
                {
                    // throw an exception if password less than 4 symbols
                    if (txtPassword.Text.Length < 4) throw new Exception("Пароль должен состоять минимум из 4 символов.");
                    if (IsUserExust(txtLogin.Text, txtPassword.Text))
                    {
                        AfterAuthorization();
                    }
                    else MessageBox.Show("Пользователь не найден!");
                }
                else MessageBox.Show("Заполните все поля!");
            }
            catch (Exception e)
            {
                if (txtPassword.Text.Length < 4) MessageBox.Show(e.Message);
                else MessageBox.Show("Произошла ошибка при авторизации!");
            }
        }
        private void DisingOrEdingActive(object sender)
        {
            Button btn = (Button)sender;
            if (btn.Name == "btnTcDesign") isTcDesign = true;
            else if (btn.Name == "btnTcEditng") isTcDesign = false;
            isTcEditng = !isTcDesign;
            WinProcessing.ColorizeBtnAndEnableControl(sender, gbxFunctionalityChoice, btnNext);
        }

        private void AfterAuthorization()
        {
            gbxAuthorizationForm.Visible = false;
            pnlControlPanel.Enabled = true;
            gbxFunctionalityChoice.Enabled = true;

            lbxPreviousActionsTC.Items.Clear();

            lbxPreviousActionsTC.Items.Add("* Создание ТП: ТПМ точки трансформации (ТТ)");
            lbxPreviousActionsTC.Items.Add("* Создание ТК: ТК_Вынос осей (ТК_1.1)");
            lbxPreviousActionsTC.Items.Add("* Связь ТК с ТП: ТПМ точки трансформации (ТТ)");
            lbxPreviousActionsTC.Items.Add(" - ТК_1.1");
            lbxPreviousActionsTC.Items.Add("* Создание ТК: ТК_Комплекс земляных работ по ");
            lbxPreviousActionsTC.Items.Add("разработке котлована (ТК_1.4.1)");
            lbxPreviousActionsTC.Items.Add("* Связь ТК с ТП: ТПМ точки трансформации (ТТ) -");
            lbxPreviousActionsTC.Items.Add(" ТК_1.4.1");
        }


    }
}
