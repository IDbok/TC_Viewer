using System.Globalization;
using System.Windows.Data;

namespace TC_WinForms.Converters
{
    public class ZeroToEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is double doubleValue && doubleValue == 0) ? string.Empty : value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
