using System.Globalization;
using System.Windows.Data;

namespace TC_WinForms.Converters
{
    public class ZeroToEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                // Если значение 0 - возвращаем пустую строку
                if (doubleValue == 0)
                    return string.Empty;

                var numberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
                numberFormat.NumberDecimalSeparator = ",";

                // Проверяем, является ли число целым
                if (doubleValue == Math.Truncate(doubleValue))
                {
                    // Целое число - выводим без десятичных знаков
                    return doubleValue.ToString("0", numberFormat);
                }
                else
                {
                    // Дробное число - максимум 2 знака после запятой
                    return doubleValue.ToString("0.##", numberFormat);
                }
            }

            // Для других типов (на всякий случай)
            return value?.ToString();

            //return (value is double doubleValue && doubleValue == 0) ? string.Empty : value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
