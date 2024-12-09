namespace TcModels.Models.Helpers;

using System;
using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FirstLetterAttribute : ValidationAttribute
{
	private readonly char _requiredLetter;

	public FirstLetterAttribute(char requiredLetter, string? errorMessage = null)
	{
		_requiredLetter = requiredLetter;

		// Устанавливаем ErrorMessage для базового класса ValidationAttribute
		ErrorMessage = errorMessage ?? $"Код должен начинаться с буквы '{_requiredLetter}'.";
	}

	protected override ValidationResult IsValid(object value, ValidationContext validationContext)
	{
		if (value is string str && !string.IsNullOrWhiteSpace(str))
		{
			if (str[0] != _requiredLetter)
			{
				return new ValidationResult(ErrorMessage);
			}
		}

		return ValidationResult.Success;
	}
}

