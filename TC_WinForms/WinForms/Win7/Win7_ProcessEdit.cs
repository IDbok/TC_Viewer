using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.WinForms.Work;
using TcDbConnector;
using TcModels.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using Microsoft.EntityFrameworkCore;
using System.Net;
using TC_WinForms.DataProcessing;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TcModels.Models.TechnologicalCard;

namespace TC_WinForms.WinForms
{
    public partial class Win7_ProcessEdit : Form
    {
        private readonly User.Role _accessLevel;

        public MyDbContext context;
        public TechnologicalProcess process;

        public Win7_ProcessEdit(int id = -1, User.Role accessLevel = User.Role.Lead)
        {
            _accessLevel = accessLevel;

            InitializeComponent();

            dataGridViewAllTP.CellClick += DataGridViewAllTP_CellClick;
            dataGridViewTPLocal.CellClick += DataGridViewTPLocal_CellClick;
                       

            context = new MyDbContext();
            if (id == -1)
            {
                process = new TechnologicalProcess();
                context.TechnologicalProcesses.Add(process);
                this.Text = "Новый проект";
            }
            else
            {
                process = context.TechnologicalProcesses.Include(i => i.TechnologicalCards).Single(s => s.Id == id);


                this.Text = process.Name;

                txtName.Text = process.Name;
                txtType.Text = process.Type;
                txtDescription.Text = process.Description;

            }

            if (_accessLevel != User.Role.Implementer)
            {
                dataGridViewAll();
            }

            dataGridViewLocalAll();

            AccessInitialization();
        }
        private void AccessInitialization()
        {
            var controlAccess = new Dictionary<User.Role, Action>
            {
                //[User.Role.Lead] = () => { },

                [User.Role.Implementer] = () => 
                {
                    // скрыть 1 и 2 столбец dgv
                    dataGridViewTPLocal.Columns[1].Visible = false;

                    txtDescription.ReadOnly = true;
                    txtName.ReadOnly = true;
                    txtType.ReadOnly = true;

                    btnSave.Visible = false;

                    btnCancel.Text = "Закрыть";

                },

                //[User.Role.ProjectManager] = () =>
                //{
                //    updateToolStripMenuItem.Visible = false;
                //},

                //[User.Role.User] = () =>
                //{
                //    updateToolStripMenuItem.Visible = false;
                //}
            };

            controlAccess.TryGetValue(_accessLevel, out var action);
            action?.Invoke();
        }
        private void DataGridViewTPLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 1)
                {
                    var Idd = (TechnologicalCard)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

                    process.TechnologicalCards.Remove(Idd);

                    dataGridViewAll();
                    dataGridViewLocalAll();
                }

                if (e.ColumnIndex == 2)
                {
                    var Idd = (TechnologicalCard)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

                    var editorForm = new Win6_new(Idd.Id, AuthorizationService.CurrentUser.UserRole(),viewMode: true);
                    editorForm.Show();

                }
            }
            catch (Exception)
            {

            }          
            
        }

        private void DataGridViewAllTP_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var Idd = (TechnologicalCard)dataGridViewAllTP.Rows[e.RowIndex].Cells[0].Value;

                process.TechnologicalCards.Add(Idd);

                dataGridViewAll();
                dataGridViewLocalAll();
            }
        }

        public void dataGridViewAll()
        {
            dataGridViewAllTP.Rows.Clear();

            // в проект можем добавить только выпущенные карты
            List<TechnologicalCard> allTehKart = context.TechnologicalCards
                    .Where(tc => tc.Status == TechnologicalCardStatus.Approved).ToList();

            foreach (TechnologicalCard card in allTehKart)
            {
                if (process.TechnologicalCards.SingleOrDefault(s => s == card) == null)
                {
                    if (textBoxPoisk.Text != "")
                    {
                        if (card.Article.ToLower().IndexOf(textBoxPoisk.Text.ToLower()) == -1)
                        {
                            if (card.Name?.ToLower().IndexOf(textBoxPoisk.Text.ToLower()) == -1)
                            {
                                continue;
                            }
                        }
                    }


                    List<object> listItem = new List<object>();
                    listItem.Add(card);

                    listItem.Add("Добавить");

                    listItem.Add(card.Article);
                    listItem.Add(card.Name);
                    dataGridViewAllTP.Rows.Add(listItem.ToArray());
                }
            }

        }

        public void dataGridViewLocalAll()
        {
            dataGridViewTPLocal.Rows.Clear();

            var all = process.TechnologicalCards.ToList();

            foreach (TechnologicalCard card in all)
            {
                List<object> listItem = new List<object>();
                listItem.Add(card);

                listItem.Add("Удалить");
                listItem.Add("Открыть");

                listItem.Add(card.Article);
                listItem.Add(card.Name);
                dataGridViewTPLocal.Rows.Add(listItem.ToArray());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            process.Name = txtName.Text;
            process.Type = txtType.Text;
            process.Description = txtDescription.Text;

            try
            {
                context.SaveChanges();
                MessageBox.Show("Сохранено");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBoxPoisk_TextChanged(object sender, EventArgs e)
        {
            dataGridViewAll();
        }
    }
}
