
using System.Reflection;
using TC_WinForms.DataProcessing;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms;

public partial class Win7_TechTransitionEditor : Form
{
    private TechTransition _editingObj;

    public delegate Task PostSaveActionTP(TechTransition modelObject);
    public PostSaveActionTP AfterSave { get; set; }
    private bool _isNewObject = false;

    List<string> _requiredProperties = new List<string>()
        {
            "Name",
            "Category",
            "TimeExecution"
        };
    public Win7_TechTransitionEditor(object obj, bool isNewObject = false)
    {
        if (!(obj is TechTransition))
        {
            throw new ArgumentException("Объект не является TechTransition");
        }
        _editingObj = (TechTransition)obj;
        _isNewObject = isNewObject;
        InitializeComponent();
    }
    private void Win7_TechTransitionEditor_Load(object sender, EventArgs e)
    {
        SetCbxCategory();

        if (_isNewObject)
        {
            this.Text = "Создание нового объекта";
        }
        else
        {
            this.Text = $"Редактирование объекта: {_editingObj.Name}";
            txtName.Text = _editingObj.Name;
            cbxCategory.SelectedIndex = cbxCategory.FindStringExact(_editingObj.Category);
            txtTime.Text = _editingObj.TimeExecution.ToString();

            if (_editingObj.TimeExecutionChecked == true)
            {
                cbxTimeCheck.Checked = true;
            }
            else
            {
                cbxTimeCheck.Checked = false;
            }

            rtxtNameComment.Text = _editingObj.CommentName;
            rtxtTimeComment.Text = _editingObj.CommentTimeExecution;
        }
    }

    private void SetCbxCategory()
    {
        //Работа с техникой,Подготовка,Сборка/монтаж,Перемещение ,Действия с проводником,Земляные работы,Демонтаж,Действия с лебедкой,Установка стойки,Ссылки/нетиповые переходы

        cbxCategory.Items.AddRange(new string[] { 
            "Работа с техникой",
            "Подготовка",
            "Сборка/монтаж",
            "Перемещение",
            "Действия с проводником",
            "Земляные работы",
            "Демонтаж",
            "Действия с лебедкой",
            "Установка стойки",
            "Ссылки/нетиповые переходы"
        });
    }
    private async void btnSave_Click(object sender, EventArgs e)
    {
        var timeCheck = float.TryParse(txtTime.Text, out float time);

        if (txtTime.Text != "" && !timeCheck)
        {
            // Ошибка при добавлении стоимости
            MessageBox.Show("Ошибка при добавлении времени выполнения.\nПроверьте формат введённых данных.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }



        _editingObj.Name = txtName.Text;
        _editingObj.Category = cbxCategory.Text;
        _editingObj.TimeExecution = time;
        _editingObj.TimeExecutionChecked = cbxTimeCheck.Checked;
        _editingObj.CommentName = rtxtNameComment.Text;
        _editingObj.CommentTimeExecution = rtxtTimeComment.Text;


        // проверка _editingObj на то, что все необходимые заполнены
        if (!AreRequiredPropertiesFilled())
        {
            string fields = string.Join(", ", _requiredProperties);
            MessageBox.Show("Для сохранения объекта необходимо заполнить обязательные поля:\n" + fields,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

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
    private bool HasChanges()
    {
        return false;
    }

    public bool AreRequiredPropertiesFilled() // todo: добавить подцветку обязательных полей
    {
        Type modelType = _editingObj.GetType();
        

        foreach (string propertyName in _requiredProperties)
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
