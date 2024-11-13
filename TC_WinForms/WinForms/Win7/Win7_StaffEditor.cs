using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.Services;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TC_WinForms.WinForms.Win6_Staff;

namespace TC_WinForms.WinForms;

public partial class Win7_StaffEditor : Form
{
    private readonly User.Role _accessLevel;

    Staff _editingObj;

    List<string> requiredPropertiesNames = new List<string>();
    private CheckRequiredFieldsService<Staff> _staffService = new CheckRequiredFieldsService<Staff>();
    //public delegate Task PostSaveAction();
    public delegate Task PostSaveAction(Staff modelObject);
    public PostSaveAction? AfterSave { get; set; }

private bool _isNewObject = false;
    public Win7_StaffEditor(Staff obj, bool isNewObject = false, User.Role accessLevel = User.Role.Lead) // todo: Сделать видимость не выпущенных объектов только для администратора или из карт в которых они были созданы
    {
        _accessLevel = accessLevel;

        _editingObj = obj;
        _isNewObject = isNewObject;

        InitializeComponent();

        _staffService.SetRequiredFieldsList(_editingObj, panel1);
        _staffService.SetRequiredPropertiesList(_editingObj);

        txtName.TextChanged += Txt_TextChanged;
        txtType.TextChanged += Txt_TextChanged;
        rtxtFunctions.TextChanged += Txt_TextChanged;
        rtxtQualification.TextChanged += Txt_TextChanged;

    }

    private void Txt_TextChanged(object? sender, EventArgs e)
    {
        var field = (Control)sender;
        if (field.BackColor != SystemColors.Window && !string.IsNullOrEmpty(field.Text))
            field.BackColor = SystemColors.Window;
    }

    private void Win7_StaffEditor_Load(object sender, EventArgs e)
    {
        SetDGVDataSources();

        if (_isNewObject)
        {
            this.Text = "Создание нового объекта";

        }
        else
        {
            this.Text = $"Редактирование объекта: {_editingObj.Name} ({_editingObj.Type})";

            txtName.Text = _editingObj.Name;
            txtType.Text = _editingObj.Type;
            rtxtFunctions.Text = _editingObj.Functions;
            rtxtQualification.Text = _editingObj.Qualification;
            rtxtComment.Text = _editingObj.Comment;

            cbxIsReleased.Checked = _editingObj.IsReleased;
            txtClassifierCode.Text = _editingObj.ClassifierCode;
        }

        dgvRelatedStaffs.DataSource = _editingObj.RelatedStaffs;

        AccessInitialization();
    }
    private void AccessInitialization()
    {
        var controlAccess = new Dictionary<User.Role, Action>
        {
            [User.Role.Lead] = () => { },

            [User.Role.Implementer] = () => { cbxIsReleased.Enabled = false; },

            //[User.Role.ProjectManager] = () =>
            //{

            //},

            //[User.Role.User] = () =>
            //{
            //}
        };

        controlAccess.TryGetValue(_accessLevel, out var action);
        action?.Invoke();
    }
    private async void btnSave_Click(object sender, EventArgs e)
    {
        _editingObj.Name = txtName.Text;
        _editingObj.Type = txtType.Text;
        _editingObj.Functions = rtxtFunctions.Text;
        _editingObj.Qualification = rtxtQualification.Text;
        _editingObj.Comment = rtxtComment.Text;

        _editingObj.IsReleased = cbxIsReleased.Checked;
        _editingObj.ClassifierCode = txtClassifierCode.Text;

        // проверка _editingObj на то, что все необходимые заполнены
        var emptyFields = _staffService.ReturnEmptyFieldsName();

        if (emptyFields.Count != 0)
        {
            string fields = string.Join(", ", emptyFields);
            MessageBox.Show("Для сохранения объекта необходимо заполнить обязательные поля:\n" + fields,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // проверка полей на уникальность
        if (!await UniqueFieldChecker<Staff>.IsPropertiesUnique(_editingObj))
            return;

        var dbConnector = new DbConnector();

        if (_isNewObject)
        {
            await dbConnector.AddObjectAsync(_editingObj);
        }
        else
        {
            await dbConnector.UpdateObjectsAsync(_editingObj);
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

    

    private void SetDGVDataSources()
    {

        var nameColumn = new DataGridViewTextBoxColumn();
        nameColumn.HeaderText = "Название";
        nameColumn.Name = nameof(Staff.Name); // Имя столбца
        nameColumn.DataPropertyName = nameof(Staff.Name); // Связать столбец с данными

        var typeColumn = new DataGridViewTextBoxColumn();
        typeColumn.HeaderText = "Тип";
        typeColumn.Name = nameof(Staff.Type); // Имя столбца
        typeColumn.DataPropertyName = nameof(Staff.Type); // Связать столбец с данными

        dgvRelatedStaffs.Columns.Add(nameColumn);
        dgvRelatedStaffs.Columns.Add(typeColumn);

        dgvRelatedStaffs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvRelatedStaffs.RowHeadersWidth = 20;
        dgvRelatedStaffs.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

        dgvRelatedStaffs.AutoGenerateColumns = false; // Отключаем автоматическое создание столбцов

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

    private void btnAddRelatedStaff_Click(object sender, EventArgs e)
    {
        var newForm = new Win7_3_Staff(this);
        newForm.ShowDialog();
    }

    private void btnDeleteRelatedStaff_Click(object sender, EventArgs e)
    {
        if (dgvRelatedStaffs.SelectedRows.Count > 0)
        {
            var staffsToRemove = new List<Staff>();
            foreach (DataGridViewRow row in dgvRelatedStaffs.SelectedRows)
            {
                if (row.DataBoundItem is Staff staff)
                {
                    staffsToRemove.Add(staff);
                }
            }

            foreach (var staff in staffsToRemove)
            {
                _editingObj.RemoveRelatedStaff(staff);
            }

            dgvRelatedStaffs.DataSource = null;
            dgvRelatedStaffs.DataSource = _editingObj.RelatedStaffs;

            dgvRelatedStaffs.Refresh();
        }
    }

    public void AddNewObjects(List<Staff> newObjs)
    {
        foreach (var obj in newObjs)
        {
            if (_editingObj.RelatedStaffs.Find(x => x.Id == obj.Id) != null) continue;
            if (obj.Id == _editingObj.Id) continue;

            _editingObj.AddRelatedStaff(obj);
        }

        dgvRelatedStaffs.DataSource = null;
        dgvRelatedStaffs.DataSource = _editingObj.RelatedStaffs;

        dgvRelatedStaffs.Refresh();
    }

    private void cbxIsReleased_CheckedChanged(object sender, EventArgs e)
    {

    }
}
