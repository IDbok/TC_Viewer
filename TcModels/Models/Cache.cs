using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.Interfaces;

namespace TcModels.Models
{
    public class Cache<T> : ICache<T>
    {
        MemoryCache cache = new MemoryCache(Guid.NewGuid().ToString());
        TimeSpan expireTime;

        public Cache()
        {
            expireTime = TimeSpan.FromMinutes(5);
            //cache = MemoryCache.Default;
        }

        public void Add(string key, T value)
        {
            var cacheItem = new CacheItem(key, value);
            // Ваши политики протухания кеша
            var policy = new CacheItemPolicy() { SlidingExpiration = expireTime };
            //var policy = new CacheItemPolicy() {AbsoluteExpiration = DateTime.Now.Add(expireTime)};
            cache.Add(cacheItem, policy);
        }

        public T Get(string key)
        {
            var item = cache.GetCacheItem(key);
            var ii = cache[key];
            return (T)cache.Get(key);
        }
    }
}
