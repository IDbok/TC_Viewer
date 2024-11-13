using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.Interfaces;

namespace TC_WinForms.Services
{
    public class CheckRequiredFieldsService<T> where T : class, IRequiredProperties
    {
        private List<Control> _requiredFields = new List<Control>();
        private Dictionary<string, string> requiredPropertiesNames = new Dictionary<string, string>();

        public void SetRequiredPropertiesList(T checkingObject)
        {
            foreach (Control control in _requiredFields)
            {
                requiredPropertiesNames.Add(control.Name, checkingObject.GetPropertiesNames.Where(x => control.Name.Contains(x.Key))
                .Select(x => x.Value).FirstOrDefault());
            }
        }

        public void SetRequiredFieldsList(T checkingObject, Panel mainPanel)
        {
            List<Control> requiredFields = new List<Control>();
            requiredFields.AddRange(mainPanel.Controls.OfType<TextBox>().ToList());
            requiredFields.AddRange(mainPanel.Controls.OfType<RichTextBox>().ToList());
            requiredFields.AddRange(mainPanel.Controls.OfType<ComboBox>().ToList());

            var _requiredPropertiesName = new List<String>();
            if (checkingObject is IRequiredProperties rp)
            {
                _requiredPropertiesName = rp.GetPropertiesRequired;
            }

            foreach (Control c in requiredFields)
            {
                foreach (string s in _requiredPropertiesName)
                {
                    if (c.Name.Contains(s))
                    {
                        _requiredFields.Add(c);
                        break;
                    }
                }
            }

        }
        
        public void SetRequiredFieldsList(T checkingObject, Form mainForm)
        {
            List<Control> requiredFields = new List<Control>();
            requiredFields.AddRange(mainForm.Controls.OfType<TextBox>().ToList());
            requiredFields.AddRange(mainForm.Controls.OfType<RichTextBox>().ToList());
            requiredFields.AddRange(mainForm.Controls.OfType<ComboBox>().ToList());

            var _requiredPropertiesName = new List<String>();
            if (checkingObject is IRequiredProperties rp)
            {
                _requiredPropertiesName = rp.GetPropertiesRequired;
            }

            foreach (Control control in requiredFields)
            {
                foreach (string name in _requiredPropertiesName)
                {
                    if (control.Name.Contains(name))
                    {
                        _requiredFields.Add(control);
                        break;
                    }
                }
            }

        }

        public List<String> ReturnEmptyFieldsName()
        {
            List<string> result = new List<string>();
            foreach (Control c in _requiredFields)
            {
                if (string.IsNullOrEmpty(c.Text))
                {
                    c.BackColor = Color.Crimson;
                    requiredPropertiesNames.TryGetValue(c.Name, out string nameProperty);
                    result.Add(nameProperty);
                }
            }
            return result;
        }
    }
}
