using Microsoft.EntityFrameworkCore;
using TcDbConnector.Interfaces;
using TcModels.Models;
using static TcModels.Models.TechnologicalCard;

namespace TcDbConnector.Repositories;

public class TechnologicalCardRepository: IRepository<TechnologicalCard>
{
    private readonly MyDbContext _db;
    //private readonly ILogger<TechnologicalCardRepository> _logger;

    public TechnologicalCardRepository(MyDbContext dbConnection)//, ILogger<TechnologicalCardRepository> logger)
    {
        _db = dbConnection;
        //_logger = logger;
    }
    public TechnologicalCardRepository()//, ILogger<TechnologicalCardRepository> logger)
    {
        _db = new MyDbContext();
        //_logger = logger;
    }
    public async Task CreateObject(TechnologicalCard tc)
    {
        await CreateObject(new List<TechnologicalCard> { tc });
    }

    public async Task<bool> CreateObject(List<TechnologicalCard> item)
    {
        try
        {
            using (var db = new MyDbContext())
            {
                var tcIds = item.Select(t => t.Id).ToList();
                var existingTcs = await db.TechnologicalCards
                    .Where(t => tcIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                var newTcs = item.Where(t => !existingTcs.Contains(t.Id)).ToList();

                if (newTcs.Any())
                {
                    await db.TechnologicalCards.AddRangeAsync(newTcs);
                    await db.SaveChangesAsync();
                }
                return true;
            };
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> DeleteObject(List<int> idList)
    {
        try
        {
            using (var db = new MyDbContext())
            {
                var tcsToDelete = await db.Set<TechnologicalCard>()
                                          .Where(tc => idList.Contains(tc.Id))
                                          .ToListAsync();

                if (tcsToDelete.Any())
                {
                    db.Set<TechnologicalCard>().RemoveRange(tcsToDelete);

                    await db.SaveChangesAsync();
                }
            }
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public IEnumerable<TechnologicalCard> GetListObjects()
    {
        try
        {
            using (var context = new MyDbContext())
            {
                var techCardList = context.TechnologicalCards;
                int i = 0;

                while (techCardList == null)
                {
                    techCardList = context.TechnologicalCards;
                    i = techCardList == null ? i++ : i;
                    if (i == 3)
                        throw new Exception();
                }

                return techCardList;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<TechnologicalCard> GetObject(int tcId)
    {
        try
        {
            using (var context = new MyDbContext())
            {
                int i = 0;
                var tc = context.Set<TechnologicalCard>()
                                        .Where(t => t.Id == tcId)
                                        .Include(t => t.Machines)
                                        .Include(t => t.Machine_TCs)
                                        .Include(t => t.Protection_TCs)
                                        .Include(t => t.Tool_TCs)
                                        .Include(t => t.Component_TCs)
                                        .Include(t => t.Staff_TCs).ThenInclude(t => t.Child)
                                        .SingleOrDefaultAsync();
                while (tc == null)
                {
                    tc = context.Set<TechnologicalCard>()
                                        .Where(t => t.Id == tcId)
                                        .Include(t => t.Machines)
                                        .Include(t => t.Machine_TCs)
                                        .Include(t => t.Protection_TCs)
                                        .Include(t => t.Tool_TCs)
                                        .Include(t => t.Component_TCs)
                                        .Include(t => t.Staff_TCs).ThenInclude(t => t.Child)
                                        .SingleOrDefaultAsync();
                    i = tc == null ? i++ : i;
                    if (i == 3)
                        throw new Exception();
                }

                return tc.Result;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public Task<List<TechnologicalCard>> GetObjects(int tcId)
    {
        try
        {
            using (var context = new MyDbContext())
            {
                int i = 0;
                var tc = context.Set<TechnologicalCard>()
                                        .Where(t => t.Id == tcId)
                                        .Include(t => t.Machines)
                                        .Include(t => t.Machine_TCs)
                                        .Include(t => t.Protection_TCs)
                                        .Include(t => t.Tool_TCs)
                                        .Include(t => t.Component_TCs)
                                        .Include(t => t.Staff_TCs).ThenInclude(t => t.Child)
                                        .ToListAsync();
                while (tc == null)
                {
                    tc = context.Set<TechnologicalCard>()
                                        .Where(t => t.Id == tcId)
                                        .Include(t => t.Machines)
                                        .Include(t => t.Machine_TCs)
                                        .Include(t => t.Protection_TCs)
                                        .Include(t => t.Tool_TCs)
                                        .Include(t => t.Component_TCs)
                                        .Include(t => t.Staff_TCs).ThenInclude(t => t.Child)
                                        .ToListAsync();
                    i = tc == null ? i++ : i;
                    if (i == 3)
                        throw new Exception();
                }

                return tc;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task UpdateObject(List<TechnologicalCard> itemList)
    {
        using (var context = new MyDbContext())
        {
            foreach (var item in itemList)
            {
                var existingTc = await context.Set<TechnologicalCard>()
                    .FirstOrDefaultAsync(t => t.Id == item.Id);

                if (existingTc != null)
                {
                    existingTc.ApplyUpdates(item);
                }
            }


            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
        }
    }

    public async Task UpdateStatusTc(TechnologicalCard tc, TechnologicalCardStatus newStatus)
    {
        using (var db = new MyDbContext())
        {
            var tcToUpdate = await db.TechnologicalCards
                .Where(t => t.Id == tc.Id)
                .FirstOrDefaultAsync();

            if (tcToUpdate != null)
            {
                tcToUpdate.Status = newStatus;
                await db.SaveChangesAsync();
            }
            else return;
        }

        tc.Status = newStatus;
    }
    public TechnologicalCard GetTechnologicalCard(int Id)
    {
        var tc = _db.TechnologicalCards.Where(tc => tc.Id == Id)
                .Include(tc => tc.Staff_TCs)
                .Include(tc => tc.Component_TCs)
                .Include(tc => tc.Tool_TCs)
                .Include(tc => tc.Machine_TCs)
                .Include(tc => tc.Protection_TCs)
                .Include(tc => tc.TechOperationWorks)
                .FirstOrDefault();

        if (tc == null)
        {
            throw new Exception($"Технологическая карта c id:{Id} не найдена");
        }

        return tc;
    }

    public void DeleteInnerEntitiesAsync(string article)
    {
        var tc = _db.TechnologicalCards.Where(tc => tc.Article == article)
                .Include(tc => tc.Staff_TCs)
                .Include(tc => tc.Component_TCs)
                .Include(tc => tc.Tool_TCs)
                .Include(tc => tc.Machine_TCs)
                .Include(tc => tc.Protection_TCs)
                .Include(tc => tc.TechOperationWorks)
                .FirstOrDefault();

        if (tc == null)
        {
           throw new Exception($"Технологическая карта {article} не найдена");
        }

        tc.Staff_TCs.Clear();
        tc.Component_TCs.Clear();
        tc.Tool_TCs.Clear();
        tc.Machine_TCs.Clear();
        tc.Protection_TCs.Clear();

        tc.TechOperationWorks.Clear();

        //tc.ExecutionSchemeBase64 = null;
        tc.ExecutionSchemeImageId = null;

        tc.Status = TechnologicalCard.TechnologicalCardStatus.Created;

        _db.SaveChanges();
    }

    public void UpdateStatus(string article, TechnologicalCard.TechnologicalCardStatus status)
    {
        var tc = _db.TechnologicalCards.Where(tc => tc.Article == article).FirstOrDefault();

        if (tc == null)
        {
            throw new Exception($"Технологическая карта {article} не найдена");
        }

        tc.Status = status;

        _db.SaveChanges();
    }
    public List<TechnologicalCard> GetAll() 
    {         
        return _db.TechnologicalCards.ToList();
    }
    public void UpdateStatus(List<TechnologicalCard> tcs, TechnologicalCard.TechnologicalCardStatus status)
    {
        var tcsToUpdate = _db.TechnologicalCards.Where(tc => tcs.Select(tc => tc.Id).Contains(tc.Id)).ToList();

        foreach (var tc in tcsToUpdate)
        {
            tc.Status = status;
        }
        _db.SaveChanges();
    }

    public void UpdateExecutionScheme(string article, byte[] executionScheme)
    {
        var tc = _db.TechnologicalCards.Where(tc => tc.Article == article).FirstOrDefault();

        if (tc == null)
        {
            throw new Exception($"Технологическая карта {article} не найдена");
        }

        var base64String = Convert.ToBase64String(executionScheme);
        var image = new ImageStorage
        {
            ImageBase64 = Convert.ToBase64String(executionScheme),
            Category = ImageCategory.ExecutionScheme
        };

        _db.ImageStorage.Add(image);
        _db.SaveChanges();

        tc.ExecutionSchemeImageId = image.Id;

        _db.SaveChanges();
    }

    //public async Task<IEnumerable<TechnologicalCard>> GetTechnologicalCardsAsync()
    //{
    //    var query = "SELECT * FROM TechnologicalCards";
    //    var technologicalCards = await _db.QueryAsync<TechnologicalCard>(query);
    //    return technologicalCards;
    //}

    //public async Task<TechnologicalCard> GetTechnologicalCardAsync(int id)
    //{
    //    var query = "SELECT * FROM TechnologicalCards WHERE Id = @Id";
    //    var technologicalCard = await _db.QueryFirstOrDefaultAsync<TechnologicalCard>(query, new { Id = id });
    //    return technologicalCard;
    //}

    //public async Task<int> CreateTechnologicalCardAsync(TechnologicalCard technologicalCard)
    //{
    //    var query = "INSERT INTO TechnologicalCards (Name, Article, Version, Type, NetworkVoltage, TechnologicalProcessType, TechnologicalProcessName, Parameter, FinalProduct, Applicability, Note, IsCompleted) VALUES (@Name, @Article, @Version, @Type, @NetworkVoltage, @TechnologicalProcessType, @TechnologicalProcessName, @Parameter, @FinalProduct, @Applicability, @Note, @IsCompleted)";
    //    var result = await _db.ExecuteAsync(query, technologicalCard);
    //    return result;
    //}

    //public async Task<int> UpdateTechnologicalCardAsync(TechnologicalCard technologicalCard)
    //{
    //    var query = "UPDATE TechnologicalCards SET Name = @Name, Article = @Article, Version = @Version, Type = @Type, NetworkVoltage = @NetworkVoltage, TechnologicalProcessType = @TechnologicalProcessType, TechnologicalProcessName = @TechnologicalProcessName, Parameter = @Parameter, FinalProduct = @FinalProduct, Applicability = @Applicability, Note = @Note, IsCompleted = @IsCompleted WHERE Id = @Id";
    //    var result = await _db.ExecuteAsync(query, technologicalCard);
    //    return result;
    //}

    //public async Task<int> DeleteTechnologicalCardAsync(int id)
    //{
    //    var query = "DELETE FROM TechnologicalCards WHERE Id = @Id";
    //    var
    //}
}
