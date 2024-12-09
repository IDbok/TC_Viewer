using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TcModels.Models.Helpers;

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
	[Required(ErrorMessage = "Код коэффициента обязателен")]
	[MaxLength(3, ErrorMessage = "Код коэффициента не должен превышать 3 символов")]
	[FirstLetter('Q')]
	[NumericTail(ErrorMessage = "Все символы после первого должны быть цифрами.")]
	public string Code { get; set; }

	// Значение коэффициента
	[Required(ErrorMessage = "Значение коэффициента обязательно")]
	public double Value { get; set; }

	// Наименование
	[MaxLength(100, ErrorMessage = "Наименование не должно превышать 100 символов")]
	public string? ShortName { get; set; }

	// Описание коэффициента
	[MaxLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
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
