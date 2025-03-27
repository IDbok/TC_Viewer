using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.Converters;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TcDbConnector;
using TcModels.Models;

namespace TC_WinForms.WinForms.Win7
{
    public partial class Win7_CategoryEditor : Form
    {
        private CategoryObject OriginCategory = null!;
        private CategoryObject LocalCategory = new();
        public MyDbContext context;
        private DbConnector dbCon = new DbConnector();
        public delegate Task PostSaveAction<TModel>(TModel modelObject) where TModel : CategoryObject;
        public PostSaveAction<CategoryObject>? AfterSave { get; set; }

        private bool isEditor = false;

        public Win7_CategoryEditor(int? categoryId = null, bool isEditor = false)
        {
            InitializeComponent();
            this.isEditor = isEditor;
            try
            {
                context = new MyDbContext();
            }
            catch (Exception ex)
            {
                throw;
            }


            SubscribeToChanges();

            if (categoryId != null)
            {
                try
                {
                    OriginCategory = context.CategoryObjects.Where(s => s.Id == categoryId)
                        .Select(c => new CategoryObject
                        {
                            Id = c.Id,
                            ClassName = DisplayNameConverter.ConvertToDisplay(c.ClassName, ConversionType.ClassName),
                            Key = DisplayNameConverter.ConvertToDisplay(c.Key, ConversionType.Key),
                            Type = DisplayNameConverter.ConvertToDisplay(c.Type, ConversionType.Type),
                            Value = c.Value,
                        }).FirstOrDefault();

                    if (OriginCategory != null)
                    {
                        LocalCategory.Id = OriginCategory.Id;
                        LocalCategory.ApplyUpdates(OriginCategory);
                        this.Text = "Редактирование значения категории";
                    }
                    else
                    {
                        MessageBox.Show("Технологическая карта не найдена");
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                OriginCategory = new CategoryObject();

                btnAddEdit.Text = "Добавить";
                this.Text = "Создание новой технологической карты";
            }

            ConfigureComboBoxes();
            FillContainersWithValues(LocalCategory);

            if (isEditor)
            {
                cbxClass.Enabled = false;
                cbxKey.Enabled = false;
            }
        }

        private void FillContainersWithValues(CategoryObject category)
        {
            cbxClass.SelectedItem = category.ClassName != null ? category.ClassName.ToString() : cbxClass.Items[0];
            cbxKey.SelectedItem = category.Key != null ? category.Key.ToString() : cbxKey.Items[0];
            txtValue.Text = category.Value;
        }

        private void ConfigureComboBoxes()
        {
            cbxClass.Items.AddRange(context.CategoryObjects.Select(s => DisplayNameConverter.ConvertToDisplay(s.ClassName, ConversionType.ClassName)).Distinct().ToArray());
            cbxKey.Items.AddRange(context.CategoryObjects.Where(s => s.ClassName == DisplayNameConverter.ConvertToInternal(LocalCategory.ClassName ?? "Технологическая карта", ConversionType.ClassName))
                .Select(s => DisplayNameConverter.ConvertToDisplay(s.Key, ConversionType.Key)).Distinct().ToArray());
        }

        private void SubscribeToChanges()
        {
            cbxClass.SelectedIndexChanged += (s, e) =>
            {
                LocalCategory.ClassName = cbxClass.SelectedItem.ToString();
                cbxKey.Items.Clear();
                cbxKey.Text = "";
                cbxKey.Items.AddRange(context.CategoryObjects.Where(s => s.ClassName == DisplayNameConverter.ConvertToInternal(LocalCategory.ClassName, ConversionType.ClassName))
                    .Select(s => DisplayNameConverter.ConvertToDisplay(s.Key, ConversionType.Key)).Distinct().ToArray());
            };
            cbxKey.SelectedIndexChanged += (s, e) =>
            {
                if (cbxKey.Items.Count == 0)
                {
                    return;
                }
                LocalCategory.Key = cbxKey.SelectedItem.ToString();
            };
        }

        private async void btnAddEdit_Click(object sender, EventArgs e)
        {
            if (await SaveAsync())
            {
                this.BringToFront();
                MessageBox.Show("Сохранено!");
                Close();
            }
        }

        async Task<bool> SaveAsync()
        {
            LocalCategory.ClassName = DisplayNameConverter.ConvertToInternal(cbxClass.SelectedItem.ToString(), ConversionType.ClassName);
            LocalCategory.Key = DisplayNameConverter.ConvertToInternal(cbxKey.SelectedItem.ToString(), ConversionType.Key);

            if (!isEditor)
                LocalCategory.Type = context.CategoryObjects.Where(s => s.ClassName == LocalCategory.ClassName && s.Key == LocalCategory.Key).Select(s => s.Type).First();
            else
                LocalCategory.Type = DisplayNameConverter.ConvertToInternal(LocalCategory.Type, ConversionType.Type);
            
            try
            {
                object convertedValue = TypeConverter.ConvertValue(LocalCategory.Type, txtValue.Text);
                LocalCategory.Value = convertedValue.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка, введите значение в правильном формате!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                OriginCategory.ApplyUpdates(LocalCategory);
                await dbCon.AddOrUpdateCategoryAsync(OriginCategory);

                if (AfterSave != null)
                {
                    await AfterSave(OriginCategory);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
