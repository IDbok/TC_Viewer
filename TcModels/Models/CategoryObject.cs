using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models
{
    public class CategoryObject : IUpdatableEntity
    {
        public int Id { get; set; }
        public string ClassName { get; set; } //Наименование класса к которому относится категория
        public string Key { get; set; }//Ключевое название категории
        public string Type { get; set; }//Тип значения
        public string Value { get; set; }//Значение

        public void ApplyUpdates(IUpdatableEntity source)
        {
            if (source is CategoryObject category)
            {
                ClassName = category.ClassName;
                Key = category.Key;
                Type = category.Type;
                Value = category.Value;
            }
        }

        public string ClassNameConvert()
        {    
            switch (ClassName)
            {
                case "TechnologicalCard":
                    return "Технологическая карта";
                default:
                    return ClassName;
            }
        }

        public string KeyConert()
        {
            switch (Key)
            {
                case "Type":
                    return "Тип";
                case "NetworkVoltage":
                    return "Напряжение";
                default:
                    return Key;
            }
        }
    }
}
