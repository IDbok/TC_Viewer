using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.RoadMap;

namespace TcDbConnector.Repositories;

public class TechnologicalCardRepository
{
    private readonly MyDbContext _db;
    private readonly ILogger _logger;

    public TechnologicalCardRepository(MyDbContext dbConnection)
    {
        _db = dbConnection;
        _logger = Log.Logger.ForContext<TechnologicalCardRepository>();
    }
    public TechnologicalCardRepository()
    {
        _db = new MyDbContext();
        _logger = Log.Logger.ForContext<TechnologicalCardRepository>();
    }

    public async Task<List<RoadMapItem>> GetRoadMapItemsDataAsync(List<int> towIds)
    {
        using (MyDbContext context = new MyDbContext())
        {
            return await context.Set<RoadMapItem>().Where(r => towIds.Contains(r.TowId)).ToListAsync();
        }
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

        var sw = Stopwatch.StartNew();
        long prev = 0; int step = 0; const int total = 21;

        long Lap() { var now = sw.ElapsedMilliseconds; var d = now - prev; prev = now; return d; }
        void Step(string name, int count) =>
            _logger.Information("DB[{Step}/{Total}] {Phase}: {Count} записей, +{Lap} мс, всего {Elapsed} мс ({Percent}%)",
                ++step, total, name, count, Lap(), sw.ElapsedMilliseconds, step * 100 / total);

        try
        {
            _logger.Information("DB загрузка ТК {TcId}: старт ({Total} шагов)", id, total);
            // Загрузка TC и его простых коллекций без Include
            var tc = await context.TechnologicalCards.FirstOrDefaultAsync(t => t.Id == id);
            Step("TechnologicalCards", tc is null ? 0 : 1);
            if (tc == null) return null;

            var machineTCs = await context.Machine_TCs.Where(x => x.ParentId == id).Include(t => t.Child).ToListAsync();
            Step("Machine_TCs", machineTCs.Count);

            var protectionTCs = await context.Protection_TCs.Where(x => x.ParentId == id).Include(t => t.Child).ToListAsync();
            Step("Protection_TCs", protectionTCs.Count);

            var toolTCs = await context.Tool_TCs.Where(x => x.ParentId == id).Include(t => t.Child).ToListAsync();
            Step("Tool_TCs", toolTCs.Count);

            var componentTCs = await context.Component_TCs.Where(x => x.ParentId == id).Include(t => t.Child).ToListAsync();
            Step("Component_TCs", componentTCs.Count);

            var staffTCs = await context.Staff_TCs.Where(x => x.ParentId == id).Include(t => t.Child).ToListAsync();
            Step("Staff_TCs", staffTCs.Count);

            var coefficients = await context.Coefficients.Where(x => x.TechnologicalCardId == id).ToListAsync();
            Step("Coefficients", coefficients.Count);

            await context.Entry(tc).Collection(t => t.ImageList).Query().Include(io => io.ImageStorage).LoadAsync();
            Step("Images", tc.ImageList?.Count ?? 0);

            // Подвязка вручную
            tc.Machine_TCs = machineTCs;
            tc.Protection_TCs = protectionTCs;
            tc.Tool_TCs = toolTCs;
            tc.Component_TCs = componentTCs;
            tc.Staff_TCs = staffTCs;
            tc.Coefficients = coefficients;

            // Загрузка TOW без include
            var towList = await context.TechOperationWorks
                .Where(w => w.TechnologicalCardId == id)
                .ToListAsync();
            Step("TechOperationWorks", towList.Count);

            var towIds = towList.Select(t => t.Id).ToList();

            var techOperations = await context.TechOperations
                                        .Where(op => towList.Select(t => t.techOperationId).Contains(op.Id))
                                        .ToListAsync();
            Step("TechOperations", techOperations.Count);

            // todo: кажется, этот кусок кода лишний, т.к. мы уже загрузили TechOperationWorks выше
            //var techOps = await context.TechOperationWorks.Where(op => towIds.Contains(op.Id)).ToListAsync(); // если связь по Id, иначе поправим
            
            var toolWorks = await context.ToolWorks.Where(t => towIds.Contains(t.techOperationWorkId)).ToListAsync();
            Step("ToolWorks", toolWorks.Count);
            var tools = await context.Tools.Where(t => toolWorks.Select(w => w.toolId).Contains(t.Id)).ToListAsync();
            Step("Tools", tools.Count);

            var componentWorks = await context.ComponentWorks.Where(c => towIds.Contains(c.techOperationWorkId)).ToListAsync();
            Step("ComponentWorks", componentWorks.Count);
            var components = await context.Components.Where(c => componentWorks.Select(w => w.componentId).Contains(c.Id)).ToListAsync();
            Step("Components", components.Count);

            var executionWorks = await context.ExecutionWorks
                                        .Where(e => towIds.Contains(e.techOperationWorkId))
                                        .Include(e => e.ImageList)
                                        .ToListAsync();
            Step("ExecutionWorks", executionWorks.Count);

            var execWorkIds = executionWorks.Select(e => e.Id).ToList();
            var transitionIds = executionWorks.Where(e => e.techTransitionId != null).Select(e => e.techTransitionId!.Value).ToHashSet();

            // Загружаем нужные сущности одним проходом
            var transitions = await context.TechTransitions
                .Where(t => transitionIds.Contains(t.Id))
                .ToListAsync();
            Step("TechTransitions", transitions.Count);

            var staffs = await context.Staff_TCs
                .Where(s => s.ParentId == id)
                .Include(s => s.ExecutionWorks)
                .Include(s => s.Child)
                .ToListAsync();
            Step("Staff_TCs with ExecutionWorks", staffs.Count);

            var machines = await context.Machine_TCs
                .Where(m => m.ParentId == id)
                .Include(s => s.Child)
                .Include(m => m.ExecutionWorks)
                .ToListAsync();
            Step("Machine_TCs with ExecutionWorks", machines.Count);

            var protections = await context.Protection_TCs
                .Where(p => p.ParentId == id)
                .Include(s => s.Child)
                .Include(p => p.ExecutionWorks)
                .ToListAsync();
            Step("Protection_TCs with ExecutionWorks", protections.Count);

            var repeats = await context.ExecutionWorkRepeats
                .Where(r => execWorkIds.Contains(r.ParentExecutionWorkId))
                .Include( r => r.ChildExecutionWork)
                .ToListAsync();
            Step("ExecutionWorkRepeats", repeats.Count);

            foreach (var tow in towList)
            {
                tow.techOperation = techOperations.FirstOrDefault(op => op.Id == tow.techOperationId);

                tow.ToolWorks = toolWorks.Where(tw => tw.techOperationWorkId == tow.Id).ToList();
                foreach (var tw in tow.ToolWorks)
                    tw.tool = tools.FirstOrDefault(t => t.Id == tw.toolId);

                tow.ComponentWorks = componentWorks.Where(cw => cw.techOperationWorkId == tow.Id).ToList();
                foreach (var cw in tow.ComponentWorks)
                    cw.component = components.FirstOrDefault(c => c.Id == cw.componentId);

                tow.executionWorks = executionWorks
                    .Where(e => e.techOperationWorkId == tow.Id)
                    .Select(e =>
                    {
                        // Привязываем TechTransition по Id
                        e.techTransition = e.techTransitionId != null
                            ? transitions.FirstOrDefault(t => t.Id == e.techTransitionId.Value)
                            : null;

                        // Остальные списки — через связывающие таблицы
                        e.Protections = protections.Where(p => p.ExecutionWorks.Any(w => w.Id == e.Id)).ToList();
                        e.Machines = machines.Where(m => m.ExecutionWorks.Any(w => w.Id == e.Id)).ToList();
                        e.Staffs = staffs.Where(s => s.ExecutionWorks.Any(w => w.Id == e.Id)).ToList();
                        e.ExecutionWorkRepeats = repeats.Where(r => r.ParentExecutionWorkId == e.Id).ToList();

                        return e;
                    }).ToList();
            }
            Step("Linking TOW children", towList.Count);

            tc.TechOperationWorks = towList;

            _logger.Information("DB загрузка ТК {TcId}: ГОТОВО за {Elapsed} мс", id, sw.ElapsedMilliseconds);
            return tc;
        }
        finally
        {
            if (isContextLocal) context.Dispose();
        }
    }

    public async Task<TechnologicalCard> GetTCDataAsyncCopy(int tcId)//метод отличается другой структурой запроса, которая используется только для копирования карты
	{
		try
		{
			using (MyDbContext context = new MyDbContext())
			{
				var techCard = await context.TechnologicalCards
					.FirstAsync(t => t.Id == tcId);


				// 2. Загружаем все связанные данные отдельными запросами

				// Machine_TCs
				var machineTcs = await context.Machine_TCs
					.Where(m => m.ParentId == tcId)
					.ToListAsync();

				//// Protection_TCs
				var protectionTcs = await context.Protection_TCs
					.Where(pt => pt.ParentId == tcId)
					.ToListAsync();

				// Tool_TCs
				var toolTcs = await context.Tool_TCs
					.Where(tt => tt.ParentId == tcId)
					.ToListAsync();

				// Component_TCs
				var componentTcs = await context.Component_TCs
					.Where(ct => ct.ParentId == tcId)
					.ToListAsync();

				// Staff_TCs
				var staffTcs = await context.Staff_TCs
					.Where(st => st.ParentId == tcId)
					.ToListAsync();

				var coefficients = await context.Coefficients
					.Where(c => c.TechnologicalCardId == tcId)
					.ToListAsync();

                var images = await context.ImageOwners
                    .Where(i => i.TechnologicalCardId == tcId)
                    .Include(i => i.ImageStorage).
                    ToListAsync();

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

    public async Task<List<TechOperationWork>> GetTOWDataAsync(int tcId)
    {
        using (MyDbContext context = new MyDbContext())
        {
            var techOperationWorkList = await context.TechOperationWorks.Where(w => w.TechnologicalCardId == tcId)
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
                                        .Include(e => e.ImageList).ThenInclude(e => e.ImageStorage)
                                        .ToListAsync();


            return techOperationWorkList;
        }
    }

    public async Task<List<DiagamToWork>> GetDTWDataAsync(int tcId, MyDbContext dbCon = null)
    {
        bool isContextLocal = dbCon == null;
        MyDbContext context = dbCon ?? new MyDbContext();

        var sw = Stopwatch.StartNew(); long prev = 0; int step = 0; const int total = 4;
        long Lap() { var now = sw.ElapsedMilliseconds; var d = now - prev; prev = now; return d; }
        void Step(string name, int count) =>
            _logger.Information("DB[{Step}/{Total}] {Phase}: {Count} записей, +{Lap} мс, всего {Elapsed} мс ({Percent}%)",
                ++step, total, name, count, Lap(), sw.ElapsedMilliseconds, step * 100 / total);

        try
        {
            _logger.Information("DB загрузка DTW для ТК {TcId}: старт", tcId);

            var diagramToWorkList = await context.DiagamToWork
                .Where(w => w.technologicalCardId == tcId)
                .Include(ie => ie.techOperationWork)
                .ToListAsync();
            Step("DiagamToWork", diagramToWorkList.Count);

            var dtwIds = diagramToWorkList.Select(i => i.Id).ToList();
            var listDiagramParalelno = await context.DiagramParalelno
                .Where(p => dtwIds.Contains(p.DiagamToWorkId))
                .ToListAsync();
            Step("DiagramParalelno", listDiagramParalelno.Count);

            var dpIds = listDiagramParalelno.Select(p => p.Id).ToList();
            var listDiagramPosledov = await context.DiagramPosledov
                .Where(p => dpIds.Contains(p.DiagramParalelnoId))
                .ToListAsync();
            Step("DiagramPosledov", listDiagramPosledov.Count);

            var dposIds = listDiagramPosledov.Select(p => p.Id).ToList();
            var listDiagramShag = await context.DiagramShag
                .Where(d => dposIds.Contains(d.DiagramPosledovId))
                .Include(q => q.ListDiagramShagToolsComponent)
                    .ThenInclude(e => e.toolWork)
                .Include(q => q.ListDiagramShagToolsComponent)
                    .ThenInclude(e => e.componentWork)
                .Include(s => s.ImageList)
                    .ThenInclude(i => i.ImageStorage)
                .ToListAsync();
            Step("DiagramShag", listDiagramShag.Count);

            // Привязку к diagramToWork можно сделать тут при необходимости

            _logger.Information("DB загрузка DTW для ТК {TcId}: ГОТОВО за {Elapsed} мс", tcId, sw.ElapsedMilliseconds);
            return diagramToWorkList;
        }
        finally
        {
            if (isContextLocal) context.Dispose();
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
	public async Task<List<TechnologicalCard>> GetAllAsync()
	{
		return await _db.TechnologicalCards.ToListAsync();
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
            Category = "ExecutionScheme"
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
