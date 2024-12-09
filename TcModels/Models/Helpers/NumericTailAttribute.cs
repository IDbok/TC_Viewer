namespace TcModels.Models.Helpers;

using System;
using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NumericTailAttribute : ValidationAttribute
{
	protected override ValidationResult IsValid(object value, ValidationContext validationContext)
	{
		if (value is string str && str.Length > 1)
		{
			// Проверяем, что все символы после первого являются цифрами
			var tail = str.Substring(1);
			if (!int.TryParse(tail, out _))
			{
				return new ValidationResult(ErrorMessage ?? "Все символы после первого должны быть цифрами.");
			}
		}

		return ValidationResult.Success;
	}
}
