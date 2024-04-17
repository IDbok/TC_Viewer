using System.Data;
using System.Reflection;
using TC_WinForms.DataProcessing;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms;

public partial class Win7_StaffEditor : Form
{
    Staff _editingObj;

    List<string> requiredPropertiesNames = new List<string>();

    //public delegate Task PostSaveAction();
    public delegate Task PostSaveAction(Staff modelObject);
    public PostSaveAction? AfterSave { get; set; }

    private bool _isNewObject = false;
    public Win7_StaffEditor(Staff obj, bool isNewObject = false)
    {
        _editingObj = obj;
        _isNewObject = isNewObject;

        InitializeComponent();
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        _editingObj.Name = txtName.Text;
        _editingObj.Type = txtType.Text;
        _editingObj.Functions = rtxtFunctions.Text;
        _editingObj.CombineResponsibility = rtxtCombineResponsibility.Text;
        _editingObj.Qualification = rtxtQualification.Text;
        _editingObj.Comment = rtxtComment.Text;


        // проверка _editingObj на то, что все необходимые заполнены
        if (!AreRequiredPropertiesFilled())
        {
            string fields = string.Join(", ", requiredPropertiesNames);
            MessageBox.Show("Для сохранения объекта необходимо заполнить обязательные поля:\n" + fields,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var dbConnector = new DbConnector();

        var obj = _editingObj;
        if (_isNewObject)
        {
            await dbConnector.AddObjectAsync(obj);
        }
        else
        {
            await dbConnector.UpdateObjectsAsync(obj);
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

    private void Win7_StaffEditor_Load(object sender, EventArgs e)
    {
        if (_isNewObject)
        {
            this.Text = "Создание нового объекта";
            //PricelessObject();
        }
        else
        {
            this.Text = $"Редактирование объекта: {_editingObj.Name} ({_editingObj.Type})";

            txtName.Text = _editingObj.Name;
            txtType.Text = _editingObj.Type;
            rtxtFunctions.Text = _editingObj.Functions;
            rtxtCombineResponsibility.Text = _editingObj.CombineResponsibility;
            rtxtQualification.Text = _editingObj.Qualification;
            rtxtComment.Text = _editingObj.Comment;
             
        }
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
}
