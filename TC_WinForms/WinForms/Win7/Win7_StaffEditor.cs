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

    //public delegate Task PostSaveAction();
    public delegate Task PostSaveAction(Staff modelObject);
    public PostSaveAction? AfterSave { get; set; }

    private ConcurrencyBlockService<Staff> staffBlockService;

    private bool _isNewObject = false;
    public Win7_StaffEditor(Staff obj, bool isNewObject = false, User.Role accessLevel = User.Role.Lead) // todo: Сделать видимость не выпущенных объектов только для администратора или из карт в которых они были созданы
    {
        _accessLevel = accessLevel;

        _editingObj = obj;
        _isNewObject = isNewObject;


        InitializeComponent();
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
            
            var timerInterval = 1000 * 60 * 25;

            staffBlockService = new ConcurrencyBlockService<Staff>(_editingObj, timerInterval);
            staffBlockService.BlockObject();

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
        if (staffBlockService != null)
            staffBlockService.CleanBlockData();

        _editingObj.Name = txtName.Text;
        _editingObj.Type = txtType.Text;
        _editingObj.Functions = rtxtFunctions.Text;
        _editingObj.Qualification = rtxtQualification.Text;
        _editingObj.Comment = rtxtComment.Text;

        _editingObj.IsReleased = cbxIsReleased.Checked;
        _editingObj.ClassifierCode = txtClassifierCode.Text;

        // проверка _editingObj на то, что все необходимые заполнены
        if (!AreRequiredPropertiesFilled())
        {
            string fields = string.Join(", ", requiredPropertiesNames);
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
        if (staffBlockService != null)
            staffBlockService.CleanBlockData();

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

    public bool AreRequiredPropertiesFilled() // todo: добавить подцветку обязательных полей
    {
        Type modelType = _editingObj.GetType();

        List<string> requiredProperties;// = modelType.GetMethod("GetPropertiesRequired")
                                        //.Invoke(null, null) as List<string>;

        if (_editingObj is IRequiredProperties rp)
        {
            requiredProperties = rp.GetPropertiesRequired;
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
