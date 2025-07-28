using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TcModels.Models;

namespace TC_WinForms.Converters
{
    public class TechCodeComparer : IComparer<TechnologicalCard>
    {
        public int Compare(TechnologicalCard x, TechnologicalCard y)
        {
            if (x == null || y == null)
                return 0;

            var (prefixX, prefixNumX, restNumbersX) = SplitCode(x.Article);
            var (prefixY, prefixNumY, restNumbersY) = SplitCode(y.Article);

            // 1. Сравниваем буквенную часть ("ТК" == "ТК", "ТК" < "ТКП")
            int prefixCompare = string.Compare(prefixX, prefixY, StringComparison.Ordinal);
            if (prefixCompare != 0)
                return prefixCompare;

            // 2. Сравниваем число в префиксе (0 для "ТК" < 4 для "ТК4" < 10 для "ТК10")
            if (prefixNumX != prefixNumY)
                return prefixNumX.CompareTo(prefixNumY);

            // 3. Сравниваем оставшиеся числа после префикса (если есть)
            for (int i = 0; i < Math.Min(restNumbersX.Length, restNumbersY.Length); i++)
            {
                if (restNumbersX[i] != restNumbersY[i])
                    return restNumbersX[i].CompareTo(restNumbersY[i]);
            }

            return restNumbersX.Length.CompareTo(restNumbersY.Length);
        }

        private (string Prefix, int PrefixNumber, int[] RestNumbers) SplitCode(string code)
        {
            // Разделяем буквы и число в префиксе (например, "ТК10_1.2" → "ТК" + 10)
            var prefixMatch = Regex.Match(code, @"^([^\d]*)(\d*)");
            string prefix = prefixMatch.Groups[1].Value;

            // Если число в префиксе есть (ТК4 → 4, ТК10 → 10), иначе 0 (ТК → 0)
            int prefixNumber = prefixMatch.Groups[2].Success && !string.IsNullOrEmpty(prefixMatch.Groups[2].Value)
                ? int.Parse(prefixMatch.Groups[2].Value)
                : 0;

            // Оставшаяся часть (например, "_1.2" → [1, 2])
            string restPart = code.Substring(prefixMatch.Length);
            var restNumbers = Regex.Split(restPart, @"[^\d]+")
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(int.Parse)
                .ToArray();

            return (prefix, prefixNumber, restNumbers);
        }
    }
}
