using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TcModels.Models
{
    public class MyCachedDataProvider
    {
        IDataProvider _inner;
        ICache<List<TechnologicalCard>> _cacheTechCards;
        ICache<List<List<TechOperationWork>>> _cacheTechOperations;
        private object _locker = new object();

        public MyCachedDataProvider(IDataProvider inner, ICache<List<TechnologicalCard>> cache, ICache<List<List<TechOperationWork>>> cacheTO)
        {
            _inner = inner;
            _cacheTechCards = cache;
            _cacheTechOperations = cacheTO;
        }

        private static string GenerateKey(int param1, string param2)
        {
            return $"{param1} - {param2}";
        }

        public TechnologicalCard GetDataTechCard(int id, string CardName)
        {
            var key = GenerateKey(id, CardName);
            var result = _cacheTechCards.Get(key);
            TechnologicalCard res = result != null ? result[0] : null;
            if (res == null)
            {
                // лочим только чтобы 2 раза не генерить данные в многопоточном сценарии. 
                lock (_locker)
                {
                    result = _cacheTechCards.Get(key);
                    res = result != null ? result[0] : null;
                    if (res == null)
                    {
                        res = _inner.GetDataTechCard(id, CardName);
                        _cacheTechCards.Add(key, new List<TechnologicalCard> { res });
                    }
                }
            }
            return res;
        }

        public List<TechOperationWork> GetDataTechOperationList(int id, string CardName)
        {
            var key = GenerateKey(id, CardName);
            var result = _cacheTechOperations.Get(key);
            List<TechOperationWork> res = result != null ? result[0] : new List<TechOperationWork>();
            if (res == null || res.Count == 0)
            {
                // лочим только чтобы 2 раза не генерить данные в многопоточном сценарии. 
                lock (_locker)
                {
                    result = _cacheTechOperations.Get(key);
                    res = result != null ? result[0] : null;
                    if (res == null || res.Count == 0)
                    {
                        res = _inner.GetDataTechOperationList(id, CardName);
                        _cacheTechOperations.Add(key, new List<List<TechOperationWork>> { res });
                    }
                }
            }
            return res;
        }

    }
}
