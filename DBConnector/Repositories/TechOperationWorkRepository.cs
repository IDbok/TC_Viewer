using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcDbConnector.Interfaces;
using TcModels.Models.TcContent;

namespace TcDbConnector.Repositories
{
    public class TechOperationWorkRepository : IRepository<TechOperationWork>
    {
        public async Task CreateObject(TechOperationWork item)
        {
            await CreateObject(new List<TechOperationWork> { item });
        }

        public async Task<bool> CreateObject(List<TechOperationWork> item)
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    var toWorkIds = item.Select(t => t.Id).ToList();
                    var existingtoWorks = await db.TechOperationWorks
                        .Where(t => toWorkIds.Contains(t.Id))
                        .Select(t => t.Id)
                        .ToListAsync();

                    var newTos = item.Where(t => !existingtoWorks.Contains(t.Id)).ToList();

                    if (newTos.Any())
                    {
                        await db.TechOperationWorks.AddRangeAsync(newTos);
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
                    var tosToDelete = await db.Set<TechOperationWork>()
                                              .Where(tc => idList.Contains(tc.Id))
                                              .ToListAsync();

                    if (tosToDelete.Any())
                    {
                        db.Set<TechOperationWork>().RemoveRange(tosToDelete);

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

        public IEnumerable<TechOperationWork> GetListObjects()
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var techOperationsList = context.TechOperationWorks;
                    int i = 0;

                    while (techOperationsList == null)
                    {
                        techOperationsList = context.TechOperationWorks;
                        i = techOperationsList == null ? i++ : i;
                        if (i == 3)
                            throw new Exception();
                    }

                    return techOperationsList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TechOperationWork> GetObject(int id)
        {
            var allTO = GetListObjects();
            var result = allTO.Where(t => t.Id == id).SingleOrDefault();
            return result;
        }

        public async Task<List<TechOperationWork>> GetObjects(int id)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    int i = 0;
                    var tc = await context.Set<TechOperationWork>()
                                            .Where(t => t.TechnologicalCardId == id)
                                            .Include(i => i.techOperation)
                                            .Include(i => i.ComponentWorks).ThenInclude(t => t.component)
                                            .Include(r => r.executionWorks).ThenInclude(t => t.techTransition)
                                            .Include(r => r.executionWorks).ThenInclude(t => t.Protections)
                                            .Include(r => r.executionWorks).ThenInclude(t => t.Machines)
                                            .Include(r => r.executionWorks).ThenInclude(t => t.Staffs)
                                            .Include(r => r.executionWorks).ThenInclude(t => t.ExecutionWorkRepeats)
                                            .Include(r => r.ToolWorks).ThenInclude(r => r.tool)
                                            .ToListAsync();
                    while (tc == null)
                    {
                        tc = await context.Set<TechOperationWork>()
                                            .Where(t => t.TechnologicalCardId == id)
                                            .Include(i => i.techOperation)
                                            .Include(i => i.ComponentWorks).ThenInclude(t => t.component)
                                            .Include(r => r.executionWorks).ThenInclude(t => t.techTransition)
                                            .Include(r => r.executionWorks).ThenInclude(t => t.Protections)
                                            .Include(r => r.executionWorks).ThenInclude(t => t.Machines)
                                            .Include(r => r.executionWorks).ThenInclude(t => t.Staffs)
                                            .Include(r => r.executionWorks).ThenInclude(t => t.ExecutionWorkRepeats)
                                            .Include(r => r.ToolWorks).ThenInclude(r => r.tool)
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

        public async Task UpdateObject(List<TechOperationWork> itemList)
        {
            using (var context = new MyDbContext())
            {
                foreach (var item in itemList)
                {
                    var existingTc = await context.Set<TechOperationWork>()
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
    }
}
