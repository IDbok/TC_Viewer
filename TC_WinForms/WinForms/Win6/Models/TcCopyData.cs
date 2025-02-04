using TC_WinForms.WinForms.Work;

namespace TC_WinForms.WinForms.Win6.Models;

public static class TcCopyData
{
	private static int? _copyTcId = null;
	/// <summary>
	/// Список скопированных строк целиком (если пользователь выделил целиком).
	/// </summary>
	public static List<TechOperationDataGridItem> FullItems { get; private set; } = new();

	/// <summary>
	/// Список «частичных копий», где в каждой копии указано, какие поля/столбцы были взяты.
	/// </summary>
	//public List<PartialCopyData> PartialItems { get; set; } = new();

	/// <summary>
	/// Может быть флаг или другой признак, что копируем именно строки/столбцы/ячейки и т.д.
	/// </summary>
	public static CopyScopeEnum CopyScope { get; private set; }

	public static void Clear()
	{
		FullItems.Clear();

		Clipboard.Clear();

		CopyScope = CopyScopeEnum.Text;
	}

	public static string? GetCopyText()
	{
		if (CopyScope == CopyScopeEnum.Text)
		{
			return Clipboard.GetText();
		}
		return null;
	}

	public static int? GetCopyTcId()
	{
		return _copyTcId;
	}

	public static void SetCopyText(string copyText)
	{
		CopyScope = CopyScopeEnum.Text;

		Clipboard.SetText(copyText);
	}
	public static void SetCopyDate(List<TechOperationDataGridItem> items, CopyScopeEnum? copyScope = null)
	{
		// Если пользователь не выделил ни одной строки, то ничего не делаем.
		if (items.Count == 0)
		{
			return;
		}

		var tcId = items.First().TechOperationWork?.TechnologicalCardId;

		if (tcId == null)
		{
			throw new Exception("Не удалось определить идентификатор технологической карты.");
		}

		_copyTcId = tcId.Value;

		// Определение все ли строки одного типа
		//var isAnyItemsToolOrComponent = ;
		if (items.Any(x => x.ItsTool || x.ItsComponent))
		{
			// Если все строки одного типа, то устанавливаем соответствующий флаг.
			//var isAllToolsOrComponents = items.All(x => x.ItsTool || x.ItsComponent);
			if (items.All(x => x.ItsTool || x.ItsComponent))
			{
				CopyScope = CopyScopeEnum.ToolOrComponents;
			}
			else
			{
				// Если строки разного типа, то выдаем ошибку.
				throw new Exception("Нельзя копировать строки с разными типами данных.");
			}
		}
		else
		{
			// Если строки не содержат инструменты и компоненты, то это строки с ТО. (возможно содержания ТО без ТП)
			
			// проверяем наличия хотя бы одного элемента с ТО
			if (items.All(x => x.executionWorkItem != null))
			{
				if (items.Count() == 1)
				{
					if (copyScope != null)
					{
						CopyScope = copyScope.Value;
					}
					else
					{
						CopyScope = CopyScopeEnum.Row;
					}
				}
				else
				{
					CopyScope = CopyScopeEnum.RowRange;
				}
			}
			else
			{
				// Если строки не содержат ТП, то это должна быть одна строка с ТО.
				if (items.Count() > 1)
				{
					throw new Exception("Нельзя копировать множетсво строк с без ТП.");
				}
				else
				{
					CopyScope = CopyScopeEnum.TechOperation;
				}
			}
		}

		FullItems = items;
	}
}

public enum CopyScopeEnum
{
	/// <summary>
	/// Копирование набора строк целиком.
	/// </summary>
	RowRange,

	/// <summary>
	/// Копирование строки целиком.
	/// </summary>
	Row,

	/// <summary>
	/// Копирование ячейки, как текста.
	/// </summary>
	Text,

	/// <summary>
	/// Копирование ячейки с персоналом.
	/// </summary>
	Staff,

	/// <summary>
	/// Копирование ячейки с машинами.
	/// </summary>
	Machines, // todo: это не используется

	/// <summary>
	/// Копирование ячейки с СЗ.
	/// </summary>
	Protections,

	/// <summary>
	/// Копирование ячейки с инструментом или компонентами.
	/// </summary>
	ToolOrComponents,

	/// <summary>
	/// Копирование ячейки с ТО.
	/// </summary>
	TechOperation,

}