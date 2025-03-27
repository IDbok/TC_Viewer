using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using TC_WinForms.Converters;
using TC_WinForms.DataProcessing.Utilities;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms.Win7
{
    public partial class Win7_Category : Form
    {
        private BindingList<CategoryObject> _uniqueCategories = new BindingList<CategoryObject>();
        private BindingList<CategoryObject> _categoryValues = new BindingList<CategoryObject>();
        private CategoryObject _selectedCategory = new CategoryObject();
        private List<CategoryObject> _allCategories = new List<CategoryObject>();
        public Win7_Category()
        {
            InitializeComponent();
            Load += Win7_Category_Load;
        }

        private void Win7_Category_Load(object? sender, EventArgs e)
        {
            SetDGVColumnsSettings();

            using (MyDbContext context = new MyDbContext())
            {
                _allCategories = context.CategoryObjects.Select(c => new CategoryObject
                {
                    Id = c.Id,
                    ClassName = DisplayNameConverter.ConvertToDisplay(c.ClassName, ConversionType.ClassName),
                    Key = DisplayNameConverter.ConvertToDisplay(c.Key, ConversionType.Key),
                    Type = DisplayNameConverter.ConvertToDisplay(c.Type, ConversionType.Type),
                    Value = c.Value,
                }).ToList();

                var uniqueCategories = _allCategories
                                            .AsEnumerable() // Переносим данные в память для клиентской группировки
                                            .GroupBy(c => new { c.ClassName, c.Key, c.Type }) // Группируем по трём полям
                                            .Select(g => g.First()) // Берём первый объект из группы
                                            .ToList();
                _uniqueCategories = new BindingList<CategoryObject>(uniqueCategories);
            }

            dgvCategory.DataSource = _uniqueCategories;

            dgvCategory.ClearSelection();
            dgvCategory.SelectionChanged += dgvCategory_SelectionChanged;
        }

        private void SetDGVColumnsSettings()
        {
            dgvCategory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvValue.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvCategory.AutoGenerateColumns = false;
            dgvValue.AutoGenerateColumns = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var objEditor = new Win7_CategoryEditor();
            objEditor.AfterSave = (createObj) => { UpdateOrAddObjectInGrid(createObj); return Task.CompletedTask; };
            objEditor.Show();
        }

        private void UpdateOrAddObjectInGrid(CategoryObject newCard)
        {
            newCard.ClassName = DisplayNameConverter.ConvertToDisplay(newCard.ClassName, ConversionType.ClassName);
            newCard.Type = DisplayNameConverter.ConvertToDisplay(newCard.Type, ConversionType.Type);
            newCard.Key = DisplayNameConverter.ConvertToDisplay(newCard.Key, ConversionType.Key);

            var existingCard = _categoryValues.FirstOrDefault(obj => obj.Id == newCard.Id);
            if (existingCard != null)
            {
                existingCard.Value = newCard.Value;
            }
            else if (newCard.ClassName == _selectedCategory.ClassName &&
                    newCard.Key == _selectedCategory.Key)
            {
                _categoryValues.Insert(0, newCard);
                _allCategories.Insert(0, newCard);
            }
            else
            {
                _allCategories.Insert(0, newCard);
            }
            dgvValue.Refresh();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvValue.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну запись для редактирования.");
                return;
            }

            var selectedRow = dgvValue.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();

            var objEditor = new Win7_CategoryEditor(Convert.ToInt32(selectedRow.Cells["IdDGVValue"].Value), true);
            objEditor.AfterSave = (editedObj) => { UpdateOrAddObjectInGrid(editedObj); return Task.CompletedTask; };
            objEditor.Show();
        }

        private void dgvCategory_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCategory.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну запись для отображения.");
                return;
            }

            var selectedRow = dgvCategory.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            _selectedCategory = _allCategories.Where(c => c.Id == Convert.ToInt32(selectedRow.Cells["Id"].Value)).FirstOrDefault();

            _categoryValues = new BindingList<CategoryObject>(_allCategories.Where(c => c.ClassName == _selectedCategory.ClassName &&
                                                       c.Key == _selectedCategory.Key &&
                                                       c.Type == _selectedCategory.Type)
                                            .ToList());

            dgvValue.DataSource = _categoryValues;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить запись?", "Удаление записи", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                var selectedRows = dgvValue.SelectedRows.Cast<DataGridViewRow>().ToList();
                foreach (var row in selectedRows)
                {
                    var selectedId = Convert.ToInt32(row.Cells["IdDGVValue"].Value);
                    using (MyDbContext context = new MyDbContext())
                    {
                        var deletedCategory= context.CategoryObjects.Where(c => c.Id == selectedId).FirstOrDefault();
                        context.CategoryObjects.Remove(deletedCategory);
                        context.SaveChanges();
                    }
                    var objToDelete = _allCategories.Where(c => c.Id == selectedId).FirstOrDefault();
                    _allCategories.Remove(objToDelete);
                    _categoryValues.Remove(objToDelete);
                }
            }
        }
    }
}
