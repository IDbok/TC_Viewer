using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.Converters
{
    public enum ConversionType { ClassName, Key, Type }
    public static class DisplayNameConverter
    {
        private static readonly BiDirectionalDictionary _classNameMap = new BiDirectionalDictionary
           {
                {"TechnologicalCard", "Технологическая карта" },
                {"Component", "Материалы" },
                {"Protection","Средства защиты" },
                {"Tool","Инструменты" },
                {"Machine","Механизмы" }
           };

        private static readonly BiDirectionalDictionary _keyMap = new BiDirectionalDictionary
           {
                { "Type", "Тип" },
                { "Categoty", "Категория" },
                { "NetworkVoltage", "Напряжение" },
                { "Unit", "Ед. измерения" },

           };

        private static readonly BiDirectionalDictionary _typeMap = new BiDirectionalDictionary
           {
               { "string", "Строка" },
               { "int", "Целое число" },
               { "float", "Число с точкой" },
           };

        public static string ConvertToDisplay(string internalName, ConversionType type)
        {
            return type switch
            {
                ConversionType.ClassName => _classNameMap.GetValue(internalName),
                ConversionType.Key => _keyMap.GetValue(internalName),
                ConversionType.Type => _typeMap.GetValue(internalName),

                _ => internalName
            };
        }

        public static string ConvertToInternal(string displayName, ConversionType type)
        {
            return type switch
            {
                ConversionType.ClassName => _classNameMap.GetKey(displayName),
                ConversionType.Key => _keyMap.GetKey(displayName),
                ConversionType.Type => _typeMap.GetKey(displayName),

                _ => displayName
            };
        }
    }

    public class BiDirectionalDictionary: IEnumerable
    {
        private readonly Dictionary<string, string> _forward = new();
        private readonly Dictionary<string, string> _reverse = new();

        public void Add(string key, string value)
        {
            _forward[key] = value;
            _reverse[value] = key;
        }

        public string GetValue(string key)
        {
            return _forward.TryGetValue(key, out var value)
                ? value
                : key; // или вернуть null/выбросить исключение
        }

        public string GetKey(string value)
        {
            return _reverse.TryGetValue(value, out var key)
                ? key
                : value; // или вернуть null/выбросить исключение
        }

        // Реализация инициализации через collection initializer
        public void Add(KeyValuePair<string, string> item) => Add(item.Key, item.Value);

        // Пустая реализация IEnumerable для поддержки инициализатора
        public IEnumerator GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
