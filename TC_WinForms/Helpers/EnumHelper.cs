using System.ComponentModel;
using System.Reflection;

namespace TC_WinForms.Helpers;

public static class EnumHelper
{
	public static string GetDescription<T>(T value) where T : Enum
	{
		var field = value.GetType().GetField(value.ToString());
		if (field == null) return value.ToString();

		var attribute = field.GetCustomAttribute<DescriptionAttribute>();
		return attribute?.Description ?? value.ToString();
	}

	public static Dictionary<T, string> GetEnumDescriptions<T>() where T : Enum
	{
		return Enum.GetValues(typeof(T)).Cast<T>()
			.ToDictionary(value => value, value => GetDescription(value));
	}
}
