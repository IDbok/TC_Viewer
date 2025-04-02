using System.Globalization;

public class TypeConverter
{
    private static readonly Dictionary<string, Func<string, object>> Converters =
        new Dictionary<string, Func<string, object>>(StringComparer.OrdinalIgnoreCase)
    {
        {"int", s => int.Parse(s, CultureInfo.InvariantCulture)},
        {"int32", s => int.Parse(s, CultureInfo.InvariantCulture)},
        {"float", s => float.Parse(s, CultureInfo.InvariantCulture)},
        {"single", s => float.Parse(s, CultureInfo.InvariantCulture)},
        {"double", s => double.Parse(s, CultureInfo.InvariantCulture)},
        {"bool", s => bool.Parse(s)},
        {"string", s => s},
        {"datetime", s => DateTime.Parse(s, CultureInfo.InvariantCulture)},
        {"decimal", s => decimal.Parse(s, CultureInfo.InvariantCulture)}
    };

    public static object ConvertValue(string typeName, string value)
    {
        if (Converters.TryGetValue(typeName.Trim().ToLower(), out var converter))
        {
            return converter(value);
        }
        throw new NotSupportedException($"Type '{typeName}' is not supported");
    }
}