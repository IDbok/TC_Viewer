namespace TC_WinForms.DataProcessing;

using System;
using System.Collections.Generic;

public class Cache
{
    private Dictionary<string, object> _cache = new Dictionary<string, object>();
    private TimeSpan _cacheDuration;
    private Dictionary<string, DateTime> _cacheTimes = new Dictionary<string, DateTime>();
    private Dictionary<string, TimeSpan> _cacheDurations = new Dictionary<string, TimeSpan>();

    public Cache(TimeSpan cacheDuration)
    {
        _cacheDuration = cacheDuration;
    }

    // Получить значение из кэша
    public object Get(string key, Func<string, object> loadFunction)
    {
        // Проверяем, есть ли значение в кэше и не устарело ли оно
        if (_cache.ContainsKey(key) && DateTime.Now - _cacheTimes[key] < _cacheDuration)
        {
            return _cache[key];
        }

        // Загружаем значение, если его нет или оно устарело
        object value = loadFunction(key);
        _cache[key] = value;
        _cacheTimes[key] = DateTime.Now;
        return value;
    }

    // Добавить значение в кэш
    public void Set(string key, object value, TimeSpan? cacheDuration = null)
    {
        if(_cache.TryGetValue(key, out _))
        {
            _cache[key] = value;
            _cacheTimes[key] = DateTime.Now;
        }
        else
        {
            _cache.Add(key, value);
            _cacheTimes.Add(key, DateTime.Now);
        }

        AddDuration(key, cacheDuration);
    }
    private void AddDuration(string key, TimeSpan? cacheDuration = null)
    {
        TimeSpan cacheDurationNew = cacheDuration ?? _cacheDuration;

        if (_cacheDurations.ContainsKey(key))
        {
            _cacheDurations[key] = cacheDurationNew;
        }
        else
        {
            _cacheDurations.Add(key, cacheDurationNew);
        }
    }

    // Очистить кэш
    public void Clear()
    {
        _cache.Clear();
        _cacheTimes.Clear();
    }
}

