namespace TC_WinForms.DataProcessing.Helpers;

using System.Reflection;
using TcModels.Models.Interfaces;

public static class ValidationService
{
    public static bool AreRequiredPropertiesFilled(IValidatable validatable)
    {
        var requiredProperties = validatable.GetRequiredProperties();
        var type = validatable.GetType();

        foreach (var propertyName in requiredProperties)
        {
            var property = type.GetProperty(propertyName);
            if (property == null) continue;

            var value = property.GetValue(validatable);

            // Проверка на значение по умолчанию для данного типа
            if (IsDefaultValue(value, property.PropertyType))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsDefaultValue(object value, Type propertyType)
    {
        if (value == null)
        {
            return true;
        }

        // Отдельная проверка для enum: если не null, считаем заполненным
        if (propertyType.IsEnum)
        {
            return false;
        }

        // Для типов-значений создаем значение по умолчанию
        var defaultValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;

        return value.Equals(defaultValue);
    }
}

