using Microsoft.EntityFrameworkCore;
using TcModels.Models;

namespace TcDbConnector.Repositories;

public class TechnologicalCardRepository
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

    public string GetImageBase64(long? ExecutionSchemeImageId)
    {
        var imageBase64 = _db.ImageStorage.Where(i => i.Id == ExecutionSchemeImageId).Select(u => u.ImageBase64).FirstOrDefault();
        return imageBase64;
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
