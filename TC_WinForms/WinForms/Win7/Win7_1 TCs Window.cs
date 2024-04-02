using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TcDbConnector;
using TcModels.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ComboBox = System.Windows.Forms.ComboBox;
using TextBox = System.Windows.Forms.TextBox;

namespace TC_WinForms.WinForms
{
    public partial class Win7_1_TCs_Window : Form
    {
        public MyDbContext context;

        private List<object> AllEllement = new List<object>();

        private TechnologicalCard LocalCard = null;

        public Win7_1_TCs_Window(int? tcId=null)
        {
            InitializeComponent();

            context = new MyDbContext();

            ConfigureComboBox();

            AllEllement.Add(textBox1);
            AllEllement.Add(comboBoxType);
            AllEllement.Add(comboBoxNetworkVoltage);

            if (tcId != null)
            {
                LocalCard = context.TechnologicalCards.Single(s => s.Id == tcId);
                load(LocalCard);
            }

        }


        public void load(TechnologicalCard LocalCard)
        {
            textBox1.Text = LocalCard.Article;
            comboBoxType.Text = LocalCard.Type;
            comboBoxNetworkVoltage.Text = LocalCard.NetworkVoltage.ToString();
            textBox4.Text = LocalCard.TechnologicalProcessType;
            textBox5.Text = LocalCard.TechnologicalProcessName;
            textBox6.Text = LocalCard.Parameter;
            textBox7.Text = LocalCard.FinalProduct;
            textBox8.Text = LocalCard.Applicability;
            textBox9.Text = LocalCard.Note;
            checkBox1.Checked = LocalCard.IsCompleted;
        }

        private void ConfigureComboBox()
        {
            comboBoxType.Items.AddRange(new object[] { "Ремонтная", "Монтажная", "Точка Трансформации", "Нет данных" });
            comboBoxNetworkVoltage.Items.AddRange(new object[] { 35f, 10f, 6f, 0.4f, 0f });
        }

        bool NoEmptiness()
        {
            bool ValueRet = true;

            foreach (object obj in AllEllement)
            {
                if (obj is TextBox)
                {
                    var tb = (TextBox)obj;
                    if (tb.Text == "")
                    {
                        tb.BackColor = Color.Red;
                        ValueRet = false;
                    }
                }

                if (obj is ComboBox)
                {
                    var cb = (ComboBox)obj;
                    if (cb.SelectedIndex == -1)
                    {
                        cb.BackColor = Color.Red;
                        ValueRet = false;
                    }
                }

            }

            return ValueRet;
        }

      
        bool Save()
        {
            if(LocalCard==null)
            {
                LocalCard = new TechnologicalCard();
                context.TechnologicalCards.Add(LocalCard);
            }

            LocalCard.Article = textBox1.Text;
            LocalCard.Type = comboBoxType.Text;
            LocalCard.NetworkVoltage = float.Parse(comboBoxNetworkVoltage.Text);
            LocalCard.TechnologicalProcessType = textBox4.Text;
            LocalCard.TechnologicalProcessName = textBox5.Text;
            LocalCard.Parameter = textBox6.Text;
            LocalCard.FinalProduct = textBox7.Text;
            LocalCard.Applicability = textBox8.Text;
            LocalCard.Note = textBox9.Text;
            LocalCard.IsCompleted = checkBox1.Checked;

            try
            {
                context.SaveChanges();
                StaticWinForms.Win7_new.UpdateTC();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return false;
            }
            return true;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (NoEmptiness())
            {
                if (Save())
                {
                    MessageBox.Show("Сохранено!");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (NoEmptiness())
            {
                if (Save())
                {
                    var nn = LocalCard.Id;
                    var editorForm = new Win6_new(nn);
                    editorForm.Show();
                    this.Close();
                }
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ((TextBox)sender).BackColor = Color.White;
        }

        private void comboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((ComboBox)sender).BackColor = Color.White;
        }

        
    }
}
