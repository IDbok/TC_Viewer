using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TcModels.Models.TcContent;

[Index(nameof(Code), nameof(TechnologicalCardId), IsUnique = true)]
public class Coefficient
{
	[Key]
	public int Id { get; set; }

	// Ссылка на родительский объект TechnologicalCard
	[ForeignKey(nameof(TechnologicalCard))]
	public int TechnologicalCardId { get; set; }
	public TechnologicalCard TechnologicalCard { get; set; }

	// Шифр коэффициента
	[Required]
	[MaxLength(3)]
	public string Code { get; set; }

	// Значение коэффициента
	[Required]
	public double Value { get; set; }

	// Краткое название
	[MaxLength(100)]
	public string? ShortName { get; set; }

	// Описание коэффициента
	[MaxLength(500)]
	public string? Description { get; set; }

	public int GetNumber()
	{
		// Возвращает номер коэффициента (его код, за исключением первого символа)

		return int.Parse(Code.Substring(1));
	}
	public char GetLetter()
	{
		return Code[0];
	}

	public override string ToString()
	{
		return $"{Code}: {Value} ({ShortName})";
	}
}
