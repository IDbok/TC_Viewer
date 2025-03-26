using TC_WinForms.Enums;
using TcModels.Models.TcContent;

namespace TC_WinForms.Extensions;
public static class TechTransitionExtensions
{
	public static bool IsRepeatTypeTransition(this TechTransition? techTransition)
	{
		if (techTransition == null) return false;

		return techTransition.Id == (int)SpetialTransactions.Repeat
			|| techTransition.Id == (int)SpetialTransactions.RepeatAsInTc;
	}

	public static bool IsRepeatAsInTcTransition(this TechTransition? techTransition)
	{
		if (techTransition == null) return false;

		return techTransition.Id == (int)SpetialTransactions.RepeatAsInTc;
	}

	public static bool IsRepeatTransition(this TechTransition? techTransition)
	{
		if (techTransition == null) return false;

		return techTransition.Id == (int)SpetialTransactions.Repeat;
	}
}

