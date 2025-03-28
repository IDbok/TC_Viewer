using TC_WinForms.Enums;
using TcModels.Models.TcContent;

namespace TC_WinForms.Extensions;
public static class TechTransitionExtensions
{
	/// <summary>
	/// Проверяет, является ли переход типа "Повтор" или "Выполнить в соответствии с ТК"
	/// </summary>
	/// <param name="techTransition"></param>
	/// <returns></returns>
	public static bool IsRepeatTypeTransition(this TechTransition? techTransition)
	{
		if (techTransition == null) return false;

		return techTransition.Id == (int)SpetialTransactions.Repeat
			|| techTransition.Id == (int)SpetialTransactions.RepeatAsInTc;
	}

	/// <summary>
	/// Проверяет, является ли переход типа "Выполнить в соответствии с ТК"
	/// </summary>
	/// <param name="techTransition"></param>
	/// <returns></returns>
	public static bool IsRepeatAsInTcTransition(this TechTransition? techTransition)
	{
		if (techTransition == null) return false;

		return techTransition.Id == (int)SpetialTransactions.RepeatAsInTc;
	}

	/// <summary>
	/// Проверяет, является ли переход типа "Повтор"
	/// </summary>
	/// <param name="techTransition"></param>
	/// <returns></returns>
	public static bool IsRepeatTransition(this TechTransition? techTransition)
	{
		if (techTransition == null) return false;

		return techTransition.Id == (int)SpetialTransactions.Repeat;
	}
}

