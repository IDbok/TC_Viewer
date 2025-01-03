namespace TC_WinForms.Extensions;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

public static class EnumExtensions
{
	public static string GetDescription(this Enum value)
	{
		return GetDescription(value);
		//FieldInfo fi = value.GetType().GetField(value.ToString());
		//DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

		//if (attributes != null && attributes.Length > 0)
		//{
		//	return attributes[0].Description;
		//}
		//else
		//{
		//	return value.ToString();
		//}
	}
	public static string GetDescription<T>(T value) where T : Enum
	{
		var field = typeof(T).GetField(value.ToString());
		var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
			.FirstOrDefault() as DescriptionAttribute;

		return attribute?.Description ?? value.ToString();
	}

	// Уникальный объект для обозначения "Все"
	private static readonly object AllKey = new object();

	public static List<KeyValuePair<object, string>> GetEnumWithAll<T>() where T : Enum
	{
		// Получаем все значения enum и их описания
		var enumValues = Enum.GetValues(typeof(T)).Cast<T>()
			.Select(value => new KeyValuePair<object, string>(value, GetDescription(value)))
			.ToList();

		// Добавляем элемент "Все"
		enumValues.Insert(0, new KeyValuePair<object, string>(AllKey, "Все"));

		return enumValues;
	}

	

	public static bool IsAllKey(object key)
	{
		return ReferenceEquals(key, AllKey);
	}
}


