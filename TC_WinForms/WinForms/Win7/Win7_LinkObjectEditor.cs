using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using TC_WinForms.DataProcessing;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win7_LinkObjectEditor : Form
    {
        IModelStructure _editingObj;
        List<LinkEntety> _newLinks = new List<LinkEntety>();
        BindingList<LinkEntety> _links = new BindingList<LinkEntety>();

        List<string> requiredPropertiesNames = new List<string>();

        //public delegate Task PostSaveAction();
        public delegate Task PostSaveAction<TModel>(TModel modelObject) where TModel : IModelStructure;
        public PostSaveAction<IModelStructure> AfterSave { get; set; }

        private bool _isNewObject = false;
        public Win7_LinkObjectEditor(object obj, bool isNewObject = false)
        {
            if (!(obj is IModelStructure))
            {
                throw new ArgumentException("Объект не реализует интерфейс IModelStructure");
            }
            _editingObj = (IModelStructure)obj;
            _isNewObject = isNewObject;
            InitializeComponent();
        }

        private void Win7_LinkObjectEditor_Load(object sender, EventArgs e)
        {
            SetCbxUnits();
            SetCbxCategory();

            SetLinksDGVDataSources();

            if (_isNewObject)
            {
                this.Text = "Создание нового объекта";
                //PricelessObject();
            }
            else
            {
                this.Text = $"Редактирование объекта: {_editingObj.Name}";
                txtName.Text = _editingObj.Name;
                txtType.Text = _editingObj.Type;
                cbxUnit.SelectedIndex = cbxUnit.FindStringExact(_editingObj.Unit);
                txtPrice.Text = _editingObj.Price.ToString();
                rtxtDescription.Text = _editingObj.Description;
                rtxtManufacturer.Text = _editingObj.Manufacturer;
                txtClassifierCode.Text = _editingObj.ClassifierCode;

                if (_editingObj is ICategoryable categoryable)
                {
                    cbxCategory.SelectedIndex = cbxCategory.FindStringExact(categoryable.Categoty);
                }
            }

            _links = new BindingList<LinkEntety>(_editingObj.Links);
            dgvLinks.DataSource = _links;

            dgvLinks.Columns["Id"].Visible = false;

            SubscribeToChanges();
        }
        private void SetLinksDGVDataSources()
        {
            var linkColumn = new DataGridViewLinkColumn();
            linkColumn.HeaderText = "Ссылка";
            linkColumn.Name = "Link"; // Имя столбца
            linkColumn.DataPropertyName = nameof(LinkEntety.Link); // Связать столбец с данными

            var nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.HeaderText = "Название";
            nameColumn.Name = "Name"; // Имя столбца
            nameColumn.DataPropertyName = nameof(LinkEntety.Name); // Связать столбец с данными

            var IDColumn = new DataGridViewTextBoxColumn();
            IDColumn.HeaderText = "ID";
            IDColumn.Name = "Id"; // Имя столбца
            IDColumn.DataPropertyName = nameof(LinkEntety.Id); // Связать столбец с данными


            var isDefaultColumn = new DataGridViewCheckBoxColumn();
            isDefaultColumn.HeaderText = "";
            isDefaultColumn.Name = "IsDefault"; // Имя столбца
            isDefaultColumn.DataPropertyName = nameof(LinkEntety.IsDefault); // Связать столбец с данными
            isDefaultColumn.Width = 30;
            isDefaultColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;


            dgvLinks.Columns.Add(isDefaultColumn);
            dgvLinks.Columns.Add(nameColumn);
            dgvLinks.Columns.Add(linkColumn);

            dgvLinks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLinks.RowHeadersWidth = 20;
            dgvLinks.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            //dgvManufacturer.RowHeadersVisible = false;

            // Обработка события клика по ссылке
            dgvLinks.CellContentClick += dataGridView1_CellContentClick;

        }

        private void SetCbxUnits()
        {
            List<string> units;
            if (_editingObj is Machine machine)
            {
                units = new List<string> //шт.
                {
                    "шт.",
                };
            }
            else if (_editingObj is Protection protection)
            {
                units = new List<string>
                {
                    "шт.",
                };
            }
            else if (_editingObj is Tool tool) //шт.,компл.,м,,50 м,кг
            {
                units = new List<string>
                {
                    "шт.",
                    "компл.",
                    "м",
                    "50 м",
                    "кг",
                };
            }
            else if (_editingObj is TcModels.Models.TcContent.Component component)
            {
                units = new List<string> //шт.,м,компл.,кг,л,уп.,м3,,м?,лист,балл.,рул.,мл,г
                {
                    "шт.",
                    "м",
                    "компл.",
                    "кг",
                    "л",
                    "уп.",
                    "м3",
                    "м?",
                    "лист",
                    "балл.",
                    "рул.",
                    "мл",
                    "г",
                };
            }
            else
            {
                throw new ArgumentException("Неизвестный тип объекта");
            }

            cbxUnit.DataSource = units;
        }
        private void SetCbxCategory()
        {
            List<string> categories = new List<string>();

            if (_editingObj is Tool) //шт.,компл.,м,,50 м,кг
            {
                categories = new List<string> // Tool,Equip,Meas,AuxEquip
                {
                    "Tool",
                    "Equip",
                    "Meas",
                    "AuxEquip",
                };
            }
            else if (_editingObj is TcModels.Models.TcContent.Component)
            {
                categories = new List<string> // StandComp,StandDet,Material,OHLDet,OHLUnit,AuxMat,SubKit,SubMount,OHLProduct,OHLMount,OHLKit,SubUnit,SubDet
                {
                    "StandComp",
                    "StandDet",
                    "Material",
                    "OHLDet",
                    "OHLUnit",
                    "AuxMat",
                    "SubKit",
                    "SubMount",
                    "OHLProduct",
                    "OHLMount",
                    "OHLKit",
                    "SubUnit",
                    "SubDet",
                };
            }
            else
            {
                CategorylessObject();
            }

                cbxCategory.DataSource = categories;
        }

        private void PricelessObject()
        {
            txtPrice.Visible = false;
            lblPrice.Visible = false;
            lblPrice2.Visible = false;

            //int changePixelsY = -60;

            MoveItems(-60, 
                lblDescription, rtxtDescription,
                lblManufacturer, rtxtManufacturer, 
                lblLinks, dgvLinks, 
                btnAddLink, btnEditLink, btnDeleteLink, 
                btnSave, btnClose);

        }
        private void CategorylessObject()
        {
            cbxCategory.Visible = false;
            lblCategory.Visible = false;

            MoveItems(-60,
                lblClassifierCode, txtClassifierCode,
                lblPrice, txtPrice, lblPrice2,
                lblDescription, rtxtDescription,
                lblManufacturer, rtxtManufacturer,
                lblLinks, dgvLinks,
                btnAddLink, btnEditLink, btnDeleteLink,
                btnSave, btnClose);
        }

        private void MoveItems(int changePixelsY, params Control[] controls)
        {
            foreach (var control in controls)
            {
                control.Location = new Point(control.Location.X, control.Location.Y + changePixelsY);
            }
            this.Height += changePixelsY;
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            UrlClick(sender, e);
            IsDefaultClick(sender, e);
        }

        private void UrlClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvLinks.Columns[e.ColumnIndex] is DataGridViewLinkColumn && e.RowIndex >= 0)
            {
                //// Получение URL из текста ячейки
                var link = dgvLinks.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                if (link != null && Uri.TryCreate(link, UriKind.Absolute, out var uri))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = uri.AbsoluteUri,
                        UseShellExecute = true
                    };

                    // Открываем ссылку в браузере
                    System.Diagnostics.Process.Start(psi);
                }
            }
        }

        private void IsDefaultClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvLinks.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn && e.RowIndex >= 0)
            {
                bool isDefault = (bool)dgvLinks[e.ColumnIndex, e.RowIndex].Value;

                if (!isDefault)
                {
                    for (int i = 0; i < dgvLinks.Rows.Count; i++)
                    {
                        if (i != e.RowIndex)
                        {
                            dgvLinks[e.ColumnIndex, i].Value = false;
                        }
                    }
                    dgvLinks[e.ColumnIndex, e.RowIndex].Value = true;
                }
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            var priceCheck = float.TryParse(txtPrice.Text, out float price);

            if (txtPrice.Text!="" && !priceCheck)
            {
                // Ошибка при добавлении стоимости
                MessageBox.Show("Ошибка при добавлении стоимости.\nПроверьте формат введённых данных.", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _editingObj.Name = txtName.Text;
            _editingObj.Type = txtType.Text;
            _editingObj.Unit = cbxUnit.Text;
            _editingObj.Price = priceCheck? price : null;
            _editingObj.Description = rtxtDescription.Text;
            _editingObj.ClassifierCode = txtClassifierCode.Text;
            _editingObj.Manufacturer = rtxtManufacturer.Text;
            _editingObj.Links = _links.ToList();

            if(_editingObj is ICategoryable categoryable)
            {
                categoryable.Categoty = cbxCategory.Text;
            }

            // проверка _editingObj на то, что все необходимые заполнены
            if (!AreRequiredPropertiesFilled())
            {
                string fields = string.Join(", ", requiredPropertiesNames);
                MessageBox.Show("Для сохранения объекта необходимо заполнить обязательные поля:\n" + fields,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dbConnector = new DbConnector();
            if (_editingObj is Machine machine) 
            {
                var obj = machine;
                if(_isNewObject)
                {
                    await dbConnector.AddObjectAsync(obj);
                }
                else
                {
                    await dbConnector.UpdateObjectsAsync(obj);
                }
            }
            else if(_editingObj is Protection protection)
            {
                var obj = protection;
                if (_isNewObject)
                {
                    await dbConnector.AddObjectAsync(obj);
                }
                else
                {
                    await dbConnector.UpdateObjectsAsync(obj);
                }
            }
            else if (_editingObj is Tool tool)
            {
                var obj = tool;
                if (_isNewObject)
                {
                    await dbConnector.AddObjectAsync(obj);
                }
                else
                {
                    await dbConnector.UpdateObjectsAsync(obj);
                }
            }
            else if(_editingObj is TcModels.Models.TcContent.Component component)
            {
                var obj = component;
                if (_isNewObject)
                {
                    await dbConnector.AddObjectAsync(obj);
                }
                else
                {
                    await dbConnector.UpdateObjectsAsync(obj);
                }
            }
            else
            {
                throw new ArgumentException("Неизвестный тип объекта");
            }

            if (AfterSave != null)
            {
                await AfterSave(_editingObj);
            }

            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (HasChanges())
            {
                var result = MessageBox.Show("Закрыть без сохранения?", "Подтверждение", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }
        private void btnAddLink_Click(object sender, EventArgs e)
        {
            CreateSmallForm("Добавление новой ссылки").ShowDialog();
        }
        private Form CreateSmallForm(string formName, string? name = null, string? link = null, bool editLink = false)
        {
            var smallForm = new Form();
            smallForm.Size = new Size(450, 250);
            smallForm.StartPosition = FormStartPosition.CenterParent;
            smallForm.Text = formName;

            var lblName = new Label();
            lblName.Text = "Название:";
            lblName.Location = new Point(10, 10);
            smallForm.Controls.Add(lblName);

            var txtName = new TextBox();
            txtName.Location = new Point(10, 35);
            txtName.Size = new Size(400, 20);
            smallForm.Controls.Add(txtName);

            var lblLink = new Label();
            lblLink.Text = "Ссылка:";
            lblLink.Location = new Point(10, 70);
            smallForm.Controls.Add(lblLink);

            var txtLink = new TextBox();
            txtLink.Location = new Point(10, 95);
            txtLink.Size = new Size(400, 20);
            smallForm.Controls.Add(txtLink);

            var btnOk = new Button();
            btnOk.Location = new Point(10, 140);
            btnOk.Width = 400;
            btnOk.Height = 40;

            if (editLink)
            {
                btnOk.Text = "Редактировать";
            }
            else
            {
                btnOk.Text = "Добавить";
            }

            btnOk.Click += (s, ev) =>
            {
                if (txtName.Text != "" && txtLink.Text != "")
                {
                    var link = new LinkEntety();
                    link.Name = txtName.Text;
                    link.Link = txtLink.Text;

                    if (editLink)
                    {                        
                        var selectedRows = dgvLinks.SelectedRows[0];
                        _links[selectedRows.Index] = link;
                        
                    }else
                    {
                        _links.Add(link);
                    }

                    smallForm.Close();
                }
                else
                {
                    MessageBox.Show("Заполните все поля");
                }
            };
            smallForm.Controls.Add(btnOk);

            if (name != null)
            {
                txtName.Text = name;
            }
            if (link != null)
            {
                txtLink.Text = link;
            }

            return smallForm;
        }
        private void btnDeleteLink_Click(object sender, EventArgs e)
        {
            var selectedRows = dgvLinks.SelectedRows;
            foreach (DataGridViewRow row in selectedRows)
            {
                _links.RemoveAt(row.Index);
            }
        }
        private void btnEditLink_Click(object sender, EventArgs e)
        {
            var selectedRows = dgvLinks.SelectedRows;
            if (selectedRows.Count == 1)
            {
                var selectedRow = selectedRows[0];
                var link = _links[selectedRow.Index];
                CreateSmallForm("Редактирование ссылки", link.Name!, link.Link, true).ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите одну ссылку для редактирования");
            }
        }

        private void SubscribeToChanges()
        {
            txtClassifierCode.TextChanged += ComponentChanged;
            txtType.TextChanged += ComponentChanged;
            txtPrice.TextChanged += ComponentChanged;
            txtClassifierCode.TextChanged += ComponentChanged;

            cbxUnit.SelectedIndexChanged += ComponentChanged;
        }

        private void ComponentChanged(object sender, EventArgs e)
        {
            //if (HasChanges())
            //{
            //    btnSaveAndOpen.Text = "Сохранить и открыть";
            //}
            //else
            //{
            //    btnSaveAndOpen.Text = "Открыть";
            //}
        }
        private bool HasChanges()
        {
            if (_editingObj == null) return false;

            bool hasChanges = false;

            //hasChanges |= LocalCard.Article != txtArticle.Text;
            //hasChanges |= LocalCard.Type != cbxType.Text;
            //hasChanges |= LocalCard.NetworkVoltage.ToString() != cbxNetworkVoltage.Text;
            //hasChanges |= LocalCard.TechnologicalProcessType != txtTechProcessType.Text;
            //hasChanges |= LocalCard.TechnologicalProcessName != txtTechProcess.Text;
            //hasChanges |= LocalCard.Parameter != txtParametr.Text;
            //hasChanges |= LocalCard.FinalProduct != txtFinalProduct.Text;
            //hasChanges |= LocalCard.Applicability != txtApplicability.Text;
            //hasChanges |= LocalCard.Note != txtNote.Text;
            //hasChanges |= LocalCard.IsCompleted != chbxIsCompleted.Checked


            return hasChanges;
        }
        public bool AreRequiredPropertiesFilled() // todo: добавить подцветку обязательных полей
        {
            Type modelType = _editingObj.GetType();

            List<string> requiredProperties;// = modelType.GetMethod("GetPropertiesRequired")
                                            //.Invoke(null, null) as List<string>;

            if (_editingObj is IRequiredProperties rp) { requiredProperties = rp.GetPropertiesRequired;
                requiredPropertiesNames = rp.GetPropertiesNames.Where(x => requiredProperties.Contains(x.Key))
                    .Select(x => x.Value).ToList();
                    }
            else return false;

            foreach (string propertyName in requiredProperties)
            {
                PropertyInfo property = modelType.GetProperty(propertyName);
                if (property == null)
                {
                    throw new ArgumentException($"Property {propertyName} not found in {modelType.Name}");
                }

                object value = property.GetValue(_editingObj);
                if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
