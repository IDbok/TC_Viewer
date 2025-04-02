using TC_WinForms.Services;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Work
{
	public partial class CoefficientForm : Form
    {
        //public CoefficientForm()
        //{
        //    InitializeComponent();
        //}

        public CoefficientForm(TechTransition idd)
        {
            InitializeComponent();
            Idd = idd;

            label10.Text = idd.Name;
            label1.Text = idd.TimeExecution.ToString();

            label9.Text = idd.CommentTimeExecution ?? "";

            tbxCoefficient_TextChanged(null, null);
        }

        private TechTransition Idd { get; }

        private void btnAddCommand_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void tbxCoefficient_TextChanged(object? sender, EventArgs? e)
        {
            try
            {
                var time = Idd.TimeExecution.ToString().Replace(',', '.');
                var coef = tbxCoefficient.Text.Replace(',', '.');
                var expression = "1";

                // проверить нет ли в знака первым символом
                if (coef[0] == '+' || coef[0] == '-' ||
                    coef[0] == '*' || coef[0] == '/')
                {
                    expression = time + coef.Replace(',', '.');
                }
                else
                {
                    expression = time + "*" + coef.Replace(',', '.');
                }

                var bn = MathScript.EvaluateExpression(expression); //ee.Evaluate();

                //var bn = WorkParser.EvaluateExpression(Idd.TimeExecution.ToString().Replace(',', '.') + " " + textBox1.Text.Replace(',', '.')); // ee.Evaluate();
                label4.Text = bn.ToString();
                btnAddCommand.Enabled = true;
            }
            catch (Exception)
            {
                label4.Text = "Ошибка";
                btnAddCommand.Enabled = false;
            }

        }

        public string GetCoefficient
        {
            get { return tbxCoefficient.Text; }
        }


        public double GetValue
        {
            get { return Math.Round( double.Parse(label4.Text) , 2); }
        }

    }
}
