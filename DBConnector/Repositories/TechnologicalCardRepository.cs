using Microsoft.EntityFrameworkCore;
using TcModels.Models;
using TcModels.Models.TcContent;

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

    public async Task<TechnologicalCard> GetTechnologicalCardAsync(int id)
    {
        var tc = await GetTCDataAsyncCopy(id);
        tc.TechOperationWorks = await GetTOWDataAsync(id);
        tc.DiagamToWork = await GetDTWDataAsync(id);

        if (tc == null)
        {
            throw new Exception($"технологическая карта c id:{id} не найдена");
        }

        return tc;
    }

	public async Task<TechnologicalCard?> GetTCDataAsync(int id, MyDbContext dbCon = null)
	{
		bool isContextLocal = dbCon == null;
		MyDbContext context = dbCon ?? new MyDbContext();

		try
		{
			TechnologicalCard tc = await context.TechnologicalCards

				.Include(t => t.Machine_TCs).ThenInclude(tc => tc.Child)
				.Include(t => t.Protection_TCs).ThenInclude(tc => tc.Child)
				.Include(t => t.Tool_TCs).ThenInclude(tc => tc.Child)
				.Include(t => t.Component_TCs).ThenInclude(tc => tc.Child)
				.Include(t => t.Staff_TCs).ThenInclude(t => t.Child)
				.Include(t => t.Coefficients)
				.FirstAsync(s => s.Id == id);


			tc.TechOperationWorks = await context.TechOperationWorks
				.Where(w => w.TechnologicalCardId == id)
					.Include(i => i.techOperation)
					.Include(r => r.ToolWorks).ThenInclude(r => r.tool)
					.Include(i => i.ComponentWorks).ThenInclude(t => t.component)
				.ToListAsync();

			foreach (var tow in tc.TechOperationWorks)
			{
				// Загружаем executionWorks для текущего tow по частям с жадной загрузкой связанных данных
				tow.executionWorks = await context.ExecutionWorks
					.Where(ew => ew.techOperationWorkId == tow.Id)
					.Include(ew => ew.techTransition)
					.Include(ew => ew.Protections)
					.Include(ew => ew.Machines)
					.Include(ew => ew.Staffs)
					.Include(ew => ew.ExecutionWorkRepeats)
						.ThenInclude(ew => ew.ChildExecutionWork)
					.ToListAsync();
			}
			return tc;
		}
		catch (Exception)
		{
			// todo: добавить логгирование
			throw;
		}
		finally
		{
			if (isContextLocal)
			{
				context.Dispose();
			}
		}
	}

	public async Task<TechnologicalCard> GetTCDataAsyncCopy(int _tcId)//метод отличается другой структурой запроса, которая используется только для копирования карты
	{
		try
		{
			using (MyDbContext context = new MyDbContext())
			{
				var techCard = await context.TechnologicalCards
					.FirstAsync(t => t.Id == _tcId);


				// 2. Загружаем все связанные данные отдельными запросами

				// Machine_TCs
				var machineTcs = await context.Machine_TCs
					.Where(m => m.ParentId == _tcId)
					.ToListAsync();

				//// Protection_TCs
				var protectionTcs = await context.Protection_TCs
					.Where(pt => pt.ParentId == _tcId)
					.ToListAsync();

				// Tool_TCs
				var toolTcs = await context.Tool_TCs
					.Where(tt => tt.ParentId == _tcId)
					.ToListAsync();

				// Component_TCs
				var componentTcs = await context.Component_TCs
					.Where(ct => ct.ParentId == _tcId)
					.ToListAsync();

				// Staff_TCs
				var staffTcs = await context.Staff_TCs
					.Where(st => st.ParentId == _tcId)
					.ToListAsync();

				var coefficients = await context.Coefficients
					.Where(c => c.TechnologicalCardId == _tcId)
					.ToListAsync();

				return techCard;
			}
		}
		catch (Exception)
		{
			throw;
		}
		finally
		{

		}
	}

    public async Task<List<TechOperationWork>> GetTOWDataAsync(int _tcId)
    {
        using (MyDbContext context = new MyDbContext())
        {
            var techOperationWorkList = await context.TechOperationWorks.Where(w => w.TechnologicalCardId == _tcId)
                .ToListAsync();

            //список ID Технологических операций
            var towIds = techOperationWorkList.Select(t => t.Id).ToList();

            //Получаем список всех компонентов которые принадлежат карте
            var componentWorks = await context.ComponentWorks.Where(c => towIds.Any(o => o == c.techOperationWorkId))
                .ToListAsync();

            //Получаем список всех инструментов, которые принадлежат карте
            var toolWorks = await context.ToolWorks.Where(c => towIds.Any(o => o == c.techOperationWorkId))
                .ToListAsync();

            var executionWorks = await
                context.ExecutionWorks.Where(e => towIds.Any(o => o == e.techOperationWorkId))
                                        .Include(e => e.Protections)
                                        .Include(e => e.Machines)
                                        .Include(e => e.Staffs)
                                        .Include(e => e.ExecutionWorkRepeats).ThenInclude(e => e.ChildExecutionWork)
                                        .ToListAsync();


            return techOperationWorkList;
        }
    }

    public async Task<List<DiagamToWork>> GetDTWDataAsync(int _tcId, MyDbContext dbCon = null)
    {
        bool isContextLocal = dbCon == null;
        MyDbContext context = dbCon ?? new MyDbContext();

        try
        {

            var diagramToWorkList = await context.DiagamToWork.Where(w => w.technologicalCardId == _tcId)
                                                                        .Include(ie => ie.techOperationWork)
                                                                        .ToListAsync();


            var listDiagramParalelno = await context.DiagramParalelno.Where(p => diagramToWorkList.Select(i => i.Id).Contains(p.DiagamToWorkId))
                                                                        .ToListAsync();

            var listDiagramPosledov = await context.DiagramPosledov.Where(p => listDiagramParalelno.Select(i => i.Id).Contains(p.DiagramParalelnoId))
                .ToListAsync();

            var listDiagramShag = await context.DiagramShag.Where(d => listDiagramPosledov.Select(i => i.Id).Contains(d.DiagramPosledovId))
                .Include(q => q.ListDiagramShagToolsComponent)
                    .ThenInclude(e => e.toolWork)
                 .Include(q => q.ListDiagramShagToolsComponent)
                    .ThenInclude(e => e.componentWork)
                .ToListAsync();

            return diagramToWorkList;
        }
        finally
        {
            if (isContextLocal)
            {
                context.Dispose();
            }
        }
    }

    //public async Task<List<DiagamToWork>> GetDTWDataForPrint(int tcId)
    //{
    //    using (MyDbContext context = new MyDbContext())
    //    {
    //        var diagramToWorkList = await context.DiagamToWork.Where(w => w.technologicalCardId == tcId)
    //                                                                   .Include(ie => ie.techOperationWork)
    //                                                                   .ToListAsync();

    //        var listDiagramParalelno = await context.DiagramParalelno.Where(p => diagramToWorkList.Select(i => i.Id).Contains(p.DiagamToWorkId))
    //                                                                    .Include(t => t.techOperationWork)
    //                                                                    .ToListAsync();

    //        var listDiagramPosledov = await context.DiagramPosledov.Where(p => listDiagramParalelno.Select(i => i.Id).Contains(p.DiagramParalelnoId))
    //                                                               .ToListAsync();

    //        var listDiagramShag = await context.DiagramShag.Where(d => listDiagramPosledov.Select(i => i.Id).Contains(d.DiagramPosledovId))
    //            .Include(q => q.ListDiagramShagToolsComponent)
    //            .ToListAsync();

    //        return diagramToWorkList.OrderBy(o => o.Order).ToList();
    //    }
    //}

    public async Task<List<Outlay>> GetOutlayDataAsync(int tcId)
    {
        using (MyDbContext context = new MyDbContext())
        {
            return context.OutlaysTable.Where(o => o.TcId == tcId).ToList();
        }
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

    public async Task<string?> GetImageBase64Async(long ExecutionSchemeImageId)
    {
        var imageBase64 = await _db.ImageStorage
            .Where(i => i.Id == ExecutionSchemeImageId).Select(u => u.ImageBase64).FirstOrDefaultAsync();
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
