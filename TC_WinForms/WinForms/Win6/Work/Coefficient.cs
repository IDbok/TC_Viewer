using ExcelParsing.DataProcessing;
//using NCalc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.Services;
using TcModels.Models.TcContent;
using static Antlr4.Runtime.Atn.SemanticContext;

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

            textBox1_TextChanged(null, null);
        }

        public TechTransition Idd { get; }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //Expression ee = new Expression(Idd.TimeExecution.ToString().Replace(',','.')+ " " + textBox1.Text.Replace(',', '.'));

            //WorkParser.EvaluateExpression(Idd.TimeExecution.ToString().Replace(',', '.') + " " + textBox1.Text.Replace(',', '.'));

            try
            {
                var time = Idd.TimeExecution.ToString().Replace(',', '.');
                var coef = textBox1.Text.Replace(',', '.');
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
                button1.Enabled = true;
            }
            catch (Exception)
            {
                label4.Text = "Ошибка";
                button1.Enabled = false;
            }

        }

        public string GetCoefficient
        {
            get { return textBox1.Text; }
        }


        public double GetValue
        {
            get { return Math.Round( double.Parse(label4.Text) , 2); }
        }

    }
}
