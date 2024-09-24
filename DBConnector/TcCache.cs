using Microsoft.Extensions.Caching.Memory;
using TcDbConnector.Interfaces;
using TcDbConnector.Repositories;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TcDbConnector
{
    public class TCCache
    {
        TechnologicalCardRepository techCardRepository;
        TechOperationWorkRepository workRepository;

        IMemoryCache cache;
        private MemoryCacheEntryOptions timeOptions = new MemoryCacheEntryOptions();
        private object _locker = new object();

        public TCCache(TechnologicalCardRepository techCardRepository, TechOperationWorkRepository workRepository, IMemoryCache cache)
        {
            this.techCardRepository = techCardRepository;
            this.workRepository = workRepository;
            this.cache = cache;
            timeOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(40));
        }

        public async Task<TechnologicalCard>? GetTechnologicalCardAsync(int tcId)
        {
            cache.TryGetValue(tcId, out TechnologicalCard techCard);

            if (techCard == null)
            {
                techCard = await techCardRepository.GetObject(tcId);
                if (techCard != null)
                    cache.Set(tcId, techCard, timeOptions);
            }

            return techCard;
        }

        public async Task<List<TechOperationWork>>? GetTechOperationsAsync(int tcId)
        {
            cache.TryGetValue(tcId + "TO's", out List<TechOperationWork> techCard);

            if (techCard == null)
            {
                List<Task> finishedTasks = new List<Task>(); // { Task.Run() => ({ techCard = await workRepository.GetObjects(tcId); }) }
                finishedTasks.Add(Task.Run(async() =>
                {
                    techCard = await workRepository.GetObjects(tcId);
                }));

                techCard = await workRepository.GetObjects(tcId);
                if (techCard != null)
                    cache.Set(tcId + "TO's", techCard, timeOptions);
            }

            return techCard;
        }
    }
}
