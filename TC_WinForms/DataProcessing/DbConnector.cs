using Microsoft.EntityFrameworkCore;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.Work;
using static TcModels.Models.TechnologicalCard;

namespace TC_WinForms.DataProcessing
{
    public class DbConnector
    {
        public enum MessageType
        {
            Error,
            Warning,
            Info
        }

        public delegate void MessageToUI(string message, MessageType type);

        public event MessageToUI OnMessageToUI;

        public DbConnector()
        {
            OnMessageToUI += HandleMessage;
        }
        private void HandleMessage(string message, MessageType type)
        {
            switch (type)
            {
                case MessageType.Error:
                    MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case MessageType.Warning:
                    MessageBox.Show(message, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case MessageType.Info:
                    MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
        public async Task AddTcAsync(TechnologicalCard tc)
        {
            await AddTcAsync(new List<TechnologicalCard> { tc });
        }
        public async Task AddOrUpdateTCAsync(TechnologicalCard tc)
        {
            using (var db = new MyDbContext())
            {
                
                var existingTc = await db.Set<TechnologicalCard>()
                        .FirstOrDefaultAsync(t => t.Id == tc.Id);

                if (existingTc != null)
                    existingTc.ApplyUpdates(tc);
                else
                    await db.TechnologicalCards.AddAsync(tc);

                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task AddTcAsync(List<TechnologicalCard> tcs)
        {
            using (var db = new MyDbContext())
            {
                var tcIds = tcs.Select(t => t.Id).ToList();
                var existingTcs = await db.TechnologicalCards
                    .Where(t => tcIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                var newTcs = tcs.Where(t => !existingTcs.Contains(t.Id)).ToList();

                if (newTcs.Any())
                {
                    await db.TechnologicalCards.AddRangeAsync(newTcs);
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task AddObjectAsync<T>(T tc) where T : class, IIdentifiable
        {
            await AddObjectAsync(new List<T> { tc });
        }
        public async Task AddObjectAsync<T>(List<T> objects) where T : class, IIdentifiable
        {
            using (var db = new MyDbContext())
            {
                var objectIds = objects.Select(t => t.Id).ToList();
                var existingObjects = await db.Set<T>()
                    .Where(t => objectIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                var newObjects = objects.Where(t => !existingObjects.Contains(t.Id)).ToList();


                if (typeof(T) == typeof(Staff))
                {
                    foreach(var obj in newObjects)
                    {
                        var staff = obj as Staff;
                        if (staff != null)
                        {
                            var relatedStaffIds = staff.RelatedStaffs.Select(rs => rs.Id).ToList();
                            var existingRelatedStaffs = await db.Set<Staff>()
                                .Where(rs => relatedStaffIds.Contains(rs.Id))
                                .ToListAsync();


                            staff.ReplaceRelatedStaffs(existingRelatedStaffs);
                            
                        }
                    }
                }

                if (newObjects.Any())
                {
                    await db.Set<T>().AddRangeAsync(newObjects);
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task AddIntermediateObjectAsync(List<Staff_TC> objects) // where T : class, IIntermediateTableIds
        {
            try
            {
                using (var db = new MyDbContext())
                {

                    var objectIds = objects.Select(t => t.IdAuto).ToList();
                    var existingObjects = await db.Set<Staff_TC>()
                        .Where(t => objectIds.Contains(t.IdAuto))
                        .Select(t => t.IdAuto)
                        .ToListAsync();

                    var newObjects = objects.Where(t => !existingObjects.Contains(t.IdAuto)).ToList();


                    if (newObjects.Any())
                    {
                        await db.Set<Staff_TC>().AddRangeAsync(newObjects);
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public async Task AddIntermediateObjectAsync<T>(List<T> obj_TCs) where T : class, IIntermediateTableIds
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    var staffTCsIds = obj_TCs.Select(t => new { t.ParentId, t.ChildId }).ToList();

                    var existingCombinations = await db.Set<T>()
                        .Where(o => o.ParentId == obj_TCs[0].ParentId)
                        .Select(t => new { t.ParentId, t.ChildId })
                        .ToListAsync();

                    var newObjects = obj_TCs.Where(t => !existingCombinations
                        .Any(ec => ec.ParentId == t.ParentId && ec.ChildId == t.ChildId))
                        .ToList();

                    if (newObjects.Any())
                    {
                        await db.Set<T>().AddRangeAsync(newObjects);
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public async Task UpdateObjectsListAsync<T>(List<T> updatedObjects) where T : class, IUpdatableEntity, IIdentifiable
        {
            using (var db = new MyDbContext())
            {
                foreach (var updatedObject in updatedObjects)
                {
                    var existingTc = await db.Set<T>()
                        .FirstOrDefaultAsync(t => t.Id == updatedObject.Id);

                    if (existingTc != null)
                    {
                        existingTc.ApplyUpdates(updatedObject);
                    }
                }

                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task UpdateObjectsAsync<T>(T updatedObject) where T : class, IUpdatableEntity, IIdentifiable //, IModelStructure
        {
            using (var db = new MyDbContext())
            {
                T? existingObj = null;

                if(updatedObject is IModelStructure obj)
                {
                    existingObj = await db.Set<T>()
                    .Include(nameof(IModelStructure.Links))
                    .FirstOrDefaultAsync(t => t.Id == updatedObject.Id);
                }
                else if (updatedObject is Staff staff)
                {
                    existingObj = await db.Set<T>()
                        .Include(nameof(Staff.RelatedStaffs))
                        .FirstOrDefaultAsync(t => t.Id == updatedObject.Id);
                }
                else
                {
                    existingObj = await db.Set<T>()
                    .FirstOrDefaultAsync(t => t.Id == updatedObject.Id);
                }

                if (existingObj != null)
                {
                    existingObj.ApplyUpdates(updatedObject);
                }

                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task UpdateObjectImageAsync(Component updatedObject)//<T>(T updatedObject) where T : class, IIdentifiable //, IModelStructure
        {
            using (var db = new MyDbContext())
            {
                Component? existingObj = null;

                if (updatedObject is Component obj)
                {
                    existingObj = await db.Set<Component>()
                        .FirstOrDefaultAsync(t => t.Id == updatedObject.Id);
                }

                if (existingObj != null)
                {
                    existingObj.Image = updatedObject.Image;
                }

                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task UpdateIntermediateObjectAsync<T>(List<T> obj_TCs) where T : class, IIntermediateTableIds, IUpdatableEntity
        {
            using (var db = new MyDbContext())
            {
                foreach (var updatedObj_TC in obj_TCs)
                {
                    var existingStaffTC = await db.Set<T>()
                        .FirstOrDefaultAsync(st => st.ParentId == updatedObj_TC.ParentId && st.ChildId == updatedObj_TC.ChildId);

                    if (existingStaffTC != null)
                    {
                        existingStaffTC.ApplyUpdates(updatedObj_TC);
                    }
                }

                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task UpdateIntermediateObjectAsync(List<Staff_TC> updatedStaffTCs)
        { 
            using (var db = new MyDbContext())
            {
                foreach (var updatedObject in updatedStaffTCs)
                {
                    var existingTc = await db.Set<Staff_TC>()
                        .FirstOrDefaultAsync(t => t.IdAuto == updatedObject.IdAuto);

                    if (existingTc != null)
                    {
                        existingTc.ApplyUpdates(updatedObject);
                    }
                }

                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task<bool> DeleteObjectAsync<T>(List<int> objIds) where T : class, IIdentifiable
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    var tcsToDelete = await db.Set<T>()
                                              .Where(tc => objIds.Contains(tc.Id))
                                              .ToListAsync();

                    var deletedType = typeof(T);

                    if (tcsToDelete.Any())
                    {
                        switch (deletedType)
                        {
                            case Type t when t == typeof(TechTransition):
                            {
                                var TTToDele = tcsToDelete.Cast<TechTransition>().ToList();
                                var executionWorkToDelete = db.ExecutionWorks.Include(t => t.techTransition).Where(e => TTToDele.Contains(e.techTransition)).ToList();

                                if (executionWorkToDelete.Count > 0)
                                    db.ExecutionWorks.RemoveRange(executionWorkToDelete);

                                break;
                            }
                            case Type t when t == typeof(Component):
                            {
                                var componentToDelete = tcsToDelete.Cast<Component>().ToList();
                                var shagToolComp = db.DiagramShagToolsComponent.
                                    Include(d => d.componentWork).ThenInclude(tc => tc.component)
                                    .Where(d => componentToDelete.Contains(d.componentWork.component)).ToList();

                                if (shagToolComp.Count > 0)
                                    db.DiagramShagToolsComponent.RemoveRange(shagToolComp);

                                break;
                            }
                            case Type t when t == typeof(Tool):
                            {
                                var toolToDelete = tcsToDelete.Cast<Tool>().ToList();
                                var shagToolComp = db.DiagramShagToolsComponent.
                                    Include(d => d.toolWork).ThenInclude(tc => tc.tool)
                                    .Where(d => toolToDelete.Contains(d.toolWork.tool)).ToList();

                                if (shagToolComp.Count > 0)
                                    db.DiagramShagToolsComponent.RemoveRange(shagToolComp);

                                break;
                            }
                        }


                        db.Set<T>().RemoveRange(tcsToDelete);

                        await db.SaveChangesAsync();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
        public async Task DeleteIntermediateObjectAsync<T>(List<T> obj_TCs) where T : class, IIntermediateTableIds
        {
            using (var db = new MyDbContext())
            {
                var tcId = obj_TCs[0].ParentId;

                var obj_TCsIds = obj_TCs.Select(t =>  t.ChildId).ToList();

                var existingIds = await db.Set<T>()
                    .Where(o => o.ParentId == tcId)
                    .Select(t => t.ChildId)
                    .ToListAsync();

                var obj_TCsToDelete = obj_TCs.Where(t => existingIds
                    .Any(eIds =>  eIds == t.ChildId))
                    .ToList();

                if (obj_TCsToDelete.Any())
                {
                    // удаляем объекты из связанных данных ComponentWork, ToolWork
                    if (typeof(T) == typeof(Component_TC))
                    {
                        var cwToDelete = await db.Set<ComponentWork>()
                                .Include(cw => cw.techOperationWork)
                                .Where(cw => obj_TCsIds.Contains(cw.componentId) 
                                && cw.techOperationWork.TechnologicalCardId == tcId)
                            .ToListAsync();

                        var idsToDelete = cwToDelete.Select(cw => cw.Id).ToList();
                        if (cwToDelete.Any())
                        {
                            var cwDiagram = await db.Set<DiagramShagToolsComponent>()
                                .Where(cw => idsToDelete.Contains(cw.componentWorkId ?? 0))
                            .ToListAsync();

                            if (cwDiagram.Any())
                            {
                                db.Set<DiagramShagToolsComponent>().RemoveRange(cwDiagram);
                            }
                            db.Set<ComponentWork>().RemoveRange(cwToDelete);
                        }
                    }
                    else if (typeof(T) == typeof(Tool_TC))
                    {
                        var twToDelete = await db.Set<ToolWork>()
                            .Include(tw => tw.techOperationWork)
                            .Where(tw => obj_TCsIds.Contains(tw.toolId)
                            && tw.techOperationWork.TechnologicalCardId == tcId)
                            .ToListAsync();

                        var idsToDelete = twToDelete.Select(cw => cw.Id).ToList();
                        
                        if (twToDelete.Any())
                        {

                            var twDiagram = await db.Set<DiagramShagToolsComponent>()
                                .Where(cw => idsToDelete.Contains(cw.toolWorkId ?? 0))
                            .ToListAsync();

                            if (twDiagram.Any())
                            {
                                db.Set<DiagramShagToolsComponent>().RemoveRange(twDiagram);
                            }

                            db.Set<ToolWork>().RemoveRange(twToDelete);
                        }
                    }
                
                    db.Set<T>().RemoveRange(obj_TCsToDelete);

                    try
                    {
                        await db.SaveChangesAsync();
                    }

                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка при сохранении данных. Изменения не сохранены.\n \n" + e.Message);
                    }
                }
            }
        }

        public async Task ReplaceIntermediateObjectAsync<T>(List<T> old_TCs, List<T> new_TCs) where T : class, IIntermediateTableIds
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    var tcId = old_TCs[0].ParentId;

                    var obj_TCsIds = old_TCs.Select(t => t.ChildId).ToList();

                    var obj_TCsToReplace = await db.Set<T>()
                        .Where(o => o.ParentId == tcId 
                            && obj_TCsIds.Contains(o.ChildId))
                        .ToListAsync();
                    if (obj_TCsToReplace.Any())
                    {
                        // Выделяем объекты из связанных данных ComponentWork, ToolWork
                        List<ComponentWork> cwToReplace = new List<ComponentWork>();
                        List<ToolWork> twToReplace = new List<ToolWork>();
                        List<ExecutionWork> executionWorks = new List<ExecutionWork>();

                        if (typeof(T) == typeof(Component_TC))
                        {
                            cwToReplace = await db.Set<ComponentWork>()
                                    .Include(cw => cw.techOperationWork)
                                    .Where(cw => obj_TCsIds.Contains(cw.componentId)
                                    && cw.techOperationWork.TechnologicalCardId == tcId)
                                .ToListAsync();
                        }
                        else if (typeof(T) == typeof(Tool_TC))
                        {
                            twToReplace = await db.Set<ToolWork>()
                                .Include(tw => tw.techOperationWork)
                                .Where(tw => obj_TCsIds.Contains(tw.toolId)
                                && tw.techOperationWork.TechnologicalCardId == tcId)
                                .ToListAsync();
                        }
                        else if (typeof(T) == typeof(Machine_TC))
                        {
                            executionWorks = await db.Set<ExecutionWork>()
                                .Include(cw => cw.techOperationWork)

                                .Include(st => st.Machines)

                                .Where(ew => ew.techOperationWork.TechnologicalCardId == tcId &&
                                  ew.Machines.Any(p => obj_TCsIds.Contains(p.ChildId)))
                                .ToListAsync();
                        }
                        else if (typeof(T) == typeof(Protection_TC))
                        {
                            executionWorks = await db.Set<ExecutionWork>()
                                .Include(cw => cw.techOperationWork)

                                .Include(st => st.Protections)

                                .Where(ew => ew.techOperationWork.TechnologicalCardId == tcId &&
                                  ew.Protections.Any(p => obj_TCsIds.Contains(p.ChildId)))
                                .ToListAsync();
                        }

                        for (int i = 0; i < old_TCs.Count; i++)
                        {
                            var oldId = old_TCs[i].ChildId;
                            var newId = new_TCs[i].ChildId;


                            db.Set<T>().RemoveRange(obj_TCsToReplace);

                            db.Set<T>().AddRange(new_TCs);

                            if (cwToReplace.Any())
                            {
                                cwToReplace.Where(cw => cw.componentId == oldId).ToList().ForEach(cw => cw.componentId = newId);
                            }
                            else if (twToReplace.Any())
                            {
                                twToReplace.Where(tw => tw.toolId == oldId).ToList().ForEach(tw => tw.toolId = newId);
                            }

                            if (executionWorks.Any() &&
                                    (typeof(T) == typeof(Protection_TC) || typeof(T) == typeof(Machine_TC)))
                            {
                                foreach (var newTc in new_TCs)
                                {
                                    if (newTc is Protection_TC newProtectionTc)
                                    {
                                        executionWorks.ForEach(ew => ew.Protections.Add(newProtectionTc));
                                    }
                                    else if (newTc is Machine_TC newMachineTc)
                                    {
                                        executionWorks.ForEach(ew => ew.Machines.Add(newMachineTc));
                                    }
                                }
                            }

                        }

                        try
                        {
                            await db.SaveChangesAsync();
                        }

                        catch (Exception e)
                        {
                            MessageBox.Show("Ошибка при отправке запроса на сохранение данных.\n \n" + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка при сохранении данных\n\n" + e.Message);
            }
            
        }
        public async Task DeleteIntermediateObjectAsync(List<Staff_TC> staffTCs)
        {
            using (var db = new MyDbContext())
            {
                var obj_TCsIds = staffTCs.Select(t => t.IdAuto).ToList();

                var objsToDelete = await db.Set<Staff_TC>()
                                          .Where(tc => obj_TCsIds.Contains(tc.IdAuto))
                                          .ToListAsync();

                if (objsToDelete.Any())
                {
                    db.Set<Staff_TC>().RemoveRange(objsToDelete);

                    await db.SaveChangesAsync();
                }
            }
        }
        public void UpdateCurrentTc(int id)
        {
            Program.currentTc = GetObject<TechnologicalCard>(id);
        }

        public void UpdateTcList(List<TechnologicalCard> tcList)
        {
            using(var context = new MyDbContext())
            {
                context.UpdateRange(tcList);
                context.SaveChanges();
            }
        }

        public void Delete<T>(int id) where T : class, IIdentifiable
        {
            using (var context = new MyDbContext())
            {
                var deletingobj = context.Set<T>().Where(obj => obj.Id == id).FirstOrDefault();
                context.Remove(deletingobj);
                context.SaveChanges();
            }
        }
        public void Delete<T>(T obj)
        {
            using (var context = new MyDbContext())
            {
                context.Remove(obj);
                context.SaveChanges();
            }
        }
        public void Delete<T>(List<T> objs)
        {
            using (var context = new MyDbContext())
            {
                foreach (var obj in objs)
                {
                    context.Remove(obj);
                }
                context.SaveChanges();
            }
        }
        public void Add<T>(T addingobj)
        {
            using (var context = new MyDbContext())
            {
                context.Add(addingobj);
                context.SaveChanges();
            }
        }
        public void Add<T>(List<T> objs)
        {
            using (var context = new MyDbContext())
            {
                foreach (var obj in objs)
                {
                    context.Add(obj);
                }
                context.SaveChanges();
            }
        }
        /// <summary>
        /// Add only intermediate table object to db Parent and Child objects should be added before
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="objs"></param>
        public void Add<T,C>(List<T> objs)
            where T : class, IIntermediateTable<TechnologicalCard,C>
            where C : class
        {
            using (var context = new MyDbContext())
            {
                foreach (var obj in objs)
                {
                    // untrack child object
                    obj.Parent = null;
                    obj.Child = null;

                    context.Add(obj);
                }
                context.SaveChanges();
            }
        }

        public void Update<T>(T updatingobj) where T : class
        {
            using (var context = new MyDbContext())
            {
                context.Update(updatingobj);
                context.SaveChanges();
            }
        }
        public void Update<T>(List<T> updatingobjs) where T : class
        {
            using (var context = new MyDbContext())
            {
                foreach (var updatingobj in updatingobjs)
                {
                    context.Update(updatingobj);
                }
                context.SaveChanges();
            }
        }

        public void Update(ref TechnologicalCard updatingobj) //where T : class, IIdentifiable
        {
            using (var context = new MyDbContext())
            {

                var trackedEntities = context.ChangeTracker.Entries();
                foreach (var entry in trackedEntities)
                {
                    OnMessageToUI?.Invoke($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}", MessageType.Info);
                }
                // todo add mechanism to increase version of object if it was version field

                context.Entry(updatingobj).State = EntityState.Detached;
                
                // cetached all entities in collection
                foreach (var staff in updatingobj.Staffs)
                {
                    context.Entry(staff).State = EntityState.Detached;
                }
                foreach (var component in updatingobj.Components)
                {
                    context.Entry(component).State = EntityState.Detached;
                }
                foreach (var tool in updatingobj.Tools)
                {
                    context.Entry(tool).State = EntityState.Detached;
                }
                foreach (var machine in updatingobj.Machines)
                {
                    context.Entry(machine).State = EntityState.Detached;
                }
                foreach (var protection in updatingobj.Protections)
                {
                    context.Entry(protection).State = EntityState.Detached;
                }
                //foreach (var workStep in updatingobj.WorkSteps)
                    context.ChangeTracker.Clear();

                Thread.Sleep(100);

                trackedEntities = context.ChangeTracker.Entries();
                foreach (var entry in trackedEntities)
                {
                    OnMessageToUI?.Invoke($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}", MessageType.Info);
                }

                //context.TechnologicalCards.Attach(updatingobj);

                context.Entry(updatingobj).State = EntityState.Modified;

                trackedEntities = context.ChangeTracker.Entries();
                foreach (var entry in trackedEntities)
                {
                    OnMessageToUI?.Invoke($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}", MessageType.Info);
                }
                context.Update(updatingobj);

                // Сохраняем изменения в базе данных
                context.SaveChanges();
                
                
            }
        }
        

        public int GetLastId<T>() where T : class, IIdentifiable
        {
            using (var context = new MyDbContext())
            {
                int lastId;
                if (context.Set<T>().Count() == 0) lastId = 0;
                else lastId = context.Set<T>().OrderBy(a => a.Id).Last().Id;
                return lastId;
            }
        }
        public int GetIdByName<T>(string article) where T : class, INameable
        {
            using (var context = new MyDbContext())
            {
                int id = context.Set<T>().Where(obj => obj.Name == article).FirstOrDefault().Id;
                return id;
            }
        }
        public List<T> GetList<T>() where T : class, IIdentifiable
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    if (typeof(T) == typeof(TechnologicalProcess))
                        return context.Set<TechnologicalProcess>()
                                        .Include(tp => tp.TechnologicalCards)
                                        .Cast<T>()
                                        .ToList();

                    else if (typeof(T) == typeof(TechnologicalCard))
                        return context.Set<TechnologicalCard>()
                                        .Include(tc => tc.Staffs)
                                        .Include(tc => tc.Components)
                                        .Include(tc => tc.Tools)
                                        .Include(tc => tc.Machines)
                                        .Include(tc => tc.Protections)
                                        //.Include(tc => tc.WorkSteps)
                                        .Cast<T>()
                                        .ToList();
                    else return context.Set<T>().ToList();
                }
            }
            catch (Exception e)
            {
                OnMessageToUI?.Invoke("Произошла ошибка при попытки подключиться к БД.\n"+e.ToString(), MessageType.Error);
                throw;
            }
        }
        public async Task<TechnologicalCard?> GetTechnologicalCardToExportAsync(int id)
        {
            using (var db = new MyDbContext())
            {
                //var tc = db.TechnologicalCards.Where(tc => tc.Id == id)

                //    .Include(tc => tc.Staff_TCs).ThenInclude(tc => tc.Child)
                //    .Include(tc => tc.Component_TCs).ThenInclude(tc => tc.Child)
                //    .Include(tc => tc.Tool_TCs).ThenInclude(tc => tc.Child)
                //    .Include(tc => tc.Machine_TCs).ThenInclude(tc => tc.Child)
                //    .Include(tc => tc.Protection_TCs).ThenInclude(tc => tc.Child)

                //    .Include(tc => tc.TechOperationWorks).ThenInclude(tc => tc.techOperation)

                //    .FirstOrDefault();
                //var towIds = tc.TechOperationWorks.Select(tow => tow.Id).ToList();

                //var ew = await db.ExecutionWorks.Where(ew => towIds.Contains(ew.techOperationWorkId))
                //    .Include(ew => ew.Staffs)
                //    .Include(ew => ew.Machines)
                //    .Include(ew => ew.Protections)
                //    .Include(ew => ew.techTransition)
                //    .Include(ew => ew.ExecutionWorkRepeats)
                //    //.Include(ew => ew.ListexecutionWorkRepeat2)
                //    .ToListAsync();
                //var tw = await db.ToolWorks.Where(tw => towIds.Contains(tw.techOperationWorkId))
                //    .Include(tw => tw.tool)
                //    .ToListAsync();
                //var cw = await db.ComponentWorks.Where(cw => towIds.Contains(cw.techOperationWorkId))
                //    .Include(cw => cw.component)
                //    .ToListAsync();

                //foreach (var tow in tc.TechOperationWorks)
                //{
                //    var ewList = ew.Where(ew => ew.techOperationWorkId == tow.Id).ToList();
                //    var twList = tw.Where(tw => tw.techOperationWorkId == tow.Id).ToList();
                //    var cwList = cw.Where(cw => cw.techOperationWorkId == tow.Id).ToList();

                //    tow.executionWorks = ewList;
                //    tow.ToolWorks = twList;
                //    tow.ComponentWorks = cwList;
                //}

                //return tc;

                
                TechnologicalCard tc = await db.TechnologicalCards

                    .Include(t => t.Machine_TCs).ThenInclude(tc => tc.Child)
                    .Include(t => t.Protection_TCs).ThenInclude(tc => tc.Child)
                    .Include(t => t.Tool_TCs).ThenInclude(tc => tc.Child)
                    .Include(t => t.Component_TCs).ThenInclude(tc => tc.Child)
                    .Include(t => t.Staff_TCs).ThenInclude(t => t.Child)

                    .FirstAsync(s => s.Id == id);


                tc.TechOperationWorks = await db.TechOperationWorks
                    .Where(w => w.TechnologicalCardId == id)
                        .Include(i => i.techOperation)
                        .Include(r => r.ToolWorks).ThenInclude(r => r.tool)
                        .Include(i => i.ComponentWorks).ThenInclude(t => t.component)
                    .ToListAsync();

                foreach (var tow in tc.TechOperationWorks)
                {
                    // Загружаем executionWorks для текущего tow по частям с жадной загрузкой связанных данных
                    tow.executionWorks = await db.ExecutionWorks
                        .Where(ew => ew.techOperationWorkId == tow.Id)
                        .Include(ew => ew.techTransition)
                        .Include(ew => ew.Protections)
                        .Include(ew => ew.Machines)
                        .Include(ew => ew.Staffs)
                        .Include(ew => ew.ExecutionWorkRepeats)

                        .ToListAsync();
                }

                return tc;
            }
        }

        public async Task<List<Outlay>> GetOutlayListToExportAsync(int tcId)
        {
            using (var db = new MyDbContext())
            {
                var outlays = await db.OutlaysTable.Where(x => x.TcID == tcId).ToListAsync();

                return outlays;
            }
        }
        //public List<T> GetObjectList<T>(bool includeLinks = false) where T : class, IIdentifiable
        //{
        //    try
        //    {
        //        using (var context = new MyDbContext())
        //        {
        //            if (typeof(T) == typeof(TechnologicalProcess))
        //                return context.Set<TechnologicalProcess>()
        //                                //.Include(tp => tp.TechnologicalCards)
        //                                .Cast<T>()
        //                                .ToList();

        //            else if (typeof(T) == typeof(TechnologicalCard))
        //                return context.Set<TechnologicalCard>()
        //                                //.Include(tc => tc.Staffs)
        //                                //.Include(tc => tc.Components)
        //                                //.Include(tc => tc.Tools)
        //                                //.Include(tc => tc.Machines)
        //                                //.Include(tc => tc.Protections)
        //                                //.Include(tc => tc.WorkSteps)
        //                                .Cast<T>()
        //                                .ToList();
        //             return context.Set<T>().ToList();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        OnMessageToUI?.Invoke("Произошла ошибка при попытки подключиться к БД.\n" + e.ToString(), MessageType.Error);
        //        throw;
        //    }
        //}
        public List<T> GetObjectList<T>(bool includeLinks = false,
            bool includeRelatedStaffs = false
            ) where T : class, IIdentifiable
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    IQueryable<T> query = context.Set<T>();

                    if (typeof(T) == typeof(TechnologicalProcess))
                    {
                        query = query.Cast<TechnologicalProcess>()
                                     .Cast<T>();
                    }
                    else if (typeof(T) == typeof(TechnologicalCard))
                    {
                        query = query.Cast<TechnologicalCard>()
                                     .Cast<T>();
                    }

                    if (includeRelatedStaffs && typeof(T) == typeof(Staff) )
                    {
                        query = query.Include(nameof(Staff.RelatedStaffs));
                    }
                    if (includeLinks && typeof(IModelStructure).IsAssignableFrom(typeof(T)))
                    {
                        query = query.Include(nameof(IModelStructure.Links));
                    }

                    if (typeof(IVersionable).IsAssignableFrom(typeof(T)))
                    {
                        
                        //query = query.Include(nameof(IVersionable.Version));
                    }

                    return query.ToList();
                }
            }
            catch (Exception e)
            {
                OnMessageToUI?.Invoke("Произошла ошибка при попытке подключиться к БД.\n" + e.ToString(), MessageType.Error);
                throw;
            }
        }

        public List<T> GetIntermediateObjectList<T,C>(int parentId) where T : class, IIntermediateTable<TechnologicalCard, C>
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    
                    return context.Set<T>().Where(obj => obj.ParentId == parentId)
                                    .Include(tc => tc.Child)
                                    .Cast<T>()
                                    .ToList();
                }
            }
            catch (Exception e)
            {
                OnMessageToUI?.Invoke("Произошла ошибка при попытки подключиться к БД.\n" + e.ToString(), MessageType.Error);
                throw;
            }
        }

        //todo: настроить каскадное удаление, данный метод является заплаткой, удалить его
        public void DeleteRelatedToolComponentDiagram(int itemId, bool itsTool)//проверка является ли удаляемый объект инструментом, если нет - он компонент
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var relatedItems = context.DiagramShagToolsComponent.Where(s => (s.componentWorkId == itemId && !itsTool) || (s.toolWorkId == itemId && itsTool)).ToList(); //Добавлено булево условие чтобы случайно не удалить инструмент при удалении компонента, если совпадает значение id и наоборот
                    foreach (var item in relatedItems)
                    {
                        context.Remove(item);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                OnMessageToUI?.Invoke("Произошла ошибка при попытки подключиться к БД.\n" + e.ToString(), MessageType.Error);
                throw;
            }
        }

        public List<C> GetUnpublishedToOrTt<C>(int tcID) where C : class, IReleasable, IIdentifiable
        {
            List<C>? unpublishedObjs = new List<C>();
            C? unpublishedObj;

            try
            {
                using (var context = new MyDbContext())
                {
                    var allObjects = context.Set<TechOperationWork>().Where(tow => tow.TechnologicalCardId == tcID).ToList();

                    foreach (var obj in allObjects)
                    {
                        if(typeof(C) == typeof(TechOperation))
                        {
                            unpublishedObj = context.Set<TechOperation>()
                                                .Where(to => to.Id == obj.techOperationId && to.IsReleased == false)
                                                .Cast<C>()
                                                .FirstOrDefault();

                            if (unpublishedObj != null)
                                unpublishedObjs.Add(unpublishedObj);
                        }
                        else
                        {
                            var allExecutionWork = context.Set<ExecutionWork>()
                                                          .Where(ex => ex.techOperationWorkId == obj.Id)
                                                          .ToList();
                            foreach(var ex in allExecutionWork)
                            {
                                unpublishedObj = context.Set<TechTransition>()
                                                        .Where(tpUnpb => tpUnpb.Id == ex.techTransitionId && tpUnpb.IsReleased == false)
                                                        .Cast<C>()
                                                        .FirstOrDefault();
                                if (unpublishedObj != null)
                                    unpublishedObjs.Add(unpublishedObj);
                            }
                        }
                    }

                    return unpublishedObjs;
                }
            }
            catch (Exception e)
            {
                OnMessageToUI?.Invoke("Произошла ошибка при попытки подключиться к БД.\n" + e.ToString(), MessageType.Error);
                throw;
            }
        }

        public List<C>? GetUnpublishedObjectList<T, C>(int tcID) where T : class, IIntermediateTable<TechnologicalCard, C>
            where C : class, IReleasable, IIdentifiable
        {
            List<T>? allObjectsTC = null;
            List<C>? unpublishedObjs = new List<C>();
            C? unpublishedObj;
            try
            {
                using (var context = new MyDbContext())
                {
                    unpublishedObjs = new List<C>();
                    allObjectsTC = GetIntermediateObjectList<T, C>(tcID);                     

                    foreach (var obj in allObjectsTC)
                    {
                        unpublishedObj = context.Set<C>()
                                                .Where(tc => tc.Id == obj.ChildId && tc.IsReleased == false)
                                                .Cast<C>()
                                                .FirstOrDefault();
                        if (unpublishedObj != null)
                            unpublishedObjs.Add(unpublishedObj);
                    }

                    return unpublishedObjs;
                }
            }
            catch (Exception e)
            {
                OnMessageToUI?.Invoke("Произошла ошибка при попытки подключиться к БД.\n" + e.ToString(), MessageType.Error);
                throw;
            }
        }

        public T? GetObject<T>(int id) where T : class, IIdentifiable
        {
            T? obj = null;
            using (var context = new MyDbContext())
            {
                if (typeof(T) == typeof(TechnologicalProcess))
                    obj = context.Set<TechnologicalProcess>()
                                    .Where(tp => tp.Id == id)
                                    .Include(tp => tp.TechnologicalCards)
                                        .ThenInclude(tc => tc.Staffs)
                                    .Include(tp => tp.TechnologicalCards)
                                        .ThenInclude(tc => tc.Components)
                                    .Include(tp => tp.TechnologicalCards)
                                        .ThenInclude(tc => tc.Tools)
                                    .Include(tp => tp.TechnologicalCards)
                                        .ThenInclude(tc => tc.Machines)
                                    .Include(tp => tp.TechnologicalCards)
                                        .ThenInclude(tc => tc.Protections)
                                    //.Include(tp => tp.TechnologicalCards)
                                    //    .ThenInclude(tc => tc.WorkSteps)
                                    .Cast<T>()
                                    .FirstOrDefault();

                else if (typeof(T) == typeof(TechnologicalCard))
                    obj = context.Set<TechnologicalCard>()
                                    .Where(tc => tc.Id == id)
                                    .Include(tc => tc.Staffs)
                                    .Include(tc => tc.Components)
                                    .Include(tc => tc.Tools)
                                    .Include(tc => tc.Machines)
                                    .Include(tc => tc.Protections)
                                    //.Include(tc => tc.WorkSteps)
                                    .Cast<T>()
                                    .FirstOrDefault();
                else if (typeof(T) == typeof(Staff))
                    obj = context.Set<Staff>()
                                    .Where(tc => tc.Id == id)
                                    .Include(tc => tc.RelatedStaffs)
                                    .Cast<T>()
                                    .FirstOrDefault();
                else obj = context.Set<T>().Where(tc => tc.Id == id).FirstOrDefault();
            }
            return obj;
        }

        public T? GetObjectWithLinks<T>(int id) where T : class, IModelStructure
        {
            T? obj = null;
            using (var context = new MyDbContext())
            {
                obj = context.Set<T>().Where(tc => tc.Id == id)
                    .Include(x=> x.Links)
                    .FirstOrDefault();
            }
            return obj;
        }
        public T? GetObject<T, C>(int TC_id, int obj_id) 
            where T : class, IIntermediateTable<TechnologicalCard, C>
            where C : class, IIdentifiable
        {
            using (var context = new MyDbContext())
            {
                if (typeof(T) == typeof(Staff_TC))
                    return context.Set<Staff_TC>()
                                    .Where(sttc => sttc.ParentId == TC_id && sttc.ChildId == obj_id)
                                    .Include(sttc => sttc.Parent)
                                    .Include(sttc => sttc.Child)
                                    .Cast<T>()
                                    .FirstOrDefault();
                else if (typeof(T) == typeof(Component_TC))
                    return context.Set<Component_TC>()
                                    .Where(ctc => ctc.ParentId == TC_id && ctc.ChildId == obj_id)
                                    .Include(ctc => ctc.Parent)
                                    .Include(ctc => ctc.Child)
                                    .Cast<T>()
                                    .FirstOrDefault();
                else if (typeof(T) == typeof(Tool_TC))
                    return context.Set<Tool_TC>()
                                    .Where(ttc => ttc.ParentId == TC_id && ttc.ChildId == obj_id)
                                    .Include(ttc => ttc.Parent)
                                    .Include(ttc => ttc.Child)
                                    .Cast<T>()
                                    .FirstOrDefault();
                else if (typeof(T) == typeof(Machine_TC))
                    return context.Set<Machine_TC>()
                                    .Where(mtc => mtc.ParentId == TC_id && mtc.ChildId == obj_id)
                                    .Include(mtc => mtc.Parent)
                                    .Include(mtc => mtc.Child)
                                    .Cast<T>()
                                    .FirstOrDefault();
                else if (typeof(T) == typeof(Protection_TC))
                    return context.Set<Protection_TC>()
                                    .Where(ptc => ptc.ParentId == TC_id && ptc.ChildId == obj_id)
                                    .Include(ptc => ptc.Parent)
                                    .Include(ptc => ptc.Child)
                                    .Cast<T>()
                                    .FirstOrDefault();
                else return null;
            }
        }

        public T? GetObject2<T, C>(int TC_id, int obj_id)
            where T : class, IIntermediateTable<TechnologicalCard, C>
            where C : class, IIdentifiable
        {
            using (var context = new MyDbContext())
            {
                return context.Set<T>()
                                    .Where(sttc => sttc.ParentId == TC_id && sttc.ChildId == obj_id)
                                    .Include(sttc => sttc.Parent)
                                    .Include(sttc => sttc.Child)
                                    .Cast<T>()
                                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Check if object exist in db. If not - create new one. Return object from db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public T GetObjFromDbOrNew<T>(int id, string name, string type, string unit, float? price) where T : class, IModelStructure, new()
        {
            
            var objFromDb = GetObject<T>(id);

            T newobj = new T()
            {
                Id = objFromDb.Id != null ? objFromDb.Id : 0,
                Name = name,
                Type = type,
                Unit = unit,
                Price = price,
            };

            if (objFromDb == null)
            {
                Add(newobj);
                objFromDb = newobj;
            }
            else
            {
                if (!(objFromDb.Id == newobj.Id &&
                    objFromDb.Name == newobj.Name &&
                    objFromDb.Type == newobj.Type &&
                    objFromDb.Unit == newobj.Unit))
                {
                    Update(objFromDb);
                }
            }
            return objFromDb;
        }

        /// <summary>
        /// Get object from db or create new one. Return Intermediate table class object
        /// </summary>
        /// <typeparam name="T">Intermediate table class</typeparam>
        /// <typeparam name="C">Child object class</typeparam>
        /// <param name="tc"></param>
        /// <param name="childObj"></param>
        /// <returns></returns>
        public T GetObjFromDbOrNew<T, C>(ref TechnologicalCard tc, C childObj)
            where T : class, IStructIntermediateTable<TechnologicalCard, C>, new()
            where C : class, IModelStructure
        {
            var obj = GetObject<T, C>(tc.Id, childObj.Id);

            if (obj == null)
            {
                obj = tc.ConnectObject<T, C>(childObj, 0, 0);
            }
            return obj;
        }

        public Tool_TC GetObjFromDbOrNew2(ref TechnologicalCard tc, Tool childObj)
        {
            var obj = GetObject<Tool_TC, Tool>(tc.Id, childObj.Id);

            if (obj == null)
            {
                obj = tc.ConnectObject<Tool_TC, Tool>(childObj, 0, 0);
            }
            return obj;
        }

        public TechnologicalCard AddNewObjAndReturnIt(TechnologicalCard newObject) 
        {
            Add(newObject);
            // get id of new object
            newObject.Id = GetIdByName<TechnologicalCard>(newObject.Name);
            newObject.Name = "";

            return newObject;
        }

        public T AddNewObjAndReturnIt<T>(T newObject) where T : class, INameable
        {
            Add(newObject);

            newObject.Id = GetIdByName<T>(newObject.Name);
            newObject.Name = "";

            return newObject;
        }

        public List<int> FindStaff_TCIds(string[] staff_TcsSymbols, int tcId)
        {
            List<int> staffIds = new List<int>();

            using (var context = new MyDbContext())
            {
                foreach (var symbol in staff_TcsSymbols)
                {
                    var staff = context.Staff_TCs.Where(st => st.Symbol == symbol && st.ParentId == tcId).FirstOrDefault();
                    if (staff != null)
                    {
                        staffIds.Add(staff.IdAuto);
                    }
                }
            }

            return staffIds;
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

        /// <summary>
        /// Заменяет рисунок ханящийся в БД, если он не совпадает с текущим newES.
        /// Либо создаёт новую сущность ImageStorage и связывает её с ТК
        /// </summary>
        /// <param name="tcId"></param>
        /// <param name="newES">изображение в формате Base64</param>
        /// <returns></returns>
        public async Task UpdateTcExecutionScheme(int tcId, string newES)
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    var tcToUpdate = await db.TechnologicalCards
                        .Where(t => t.Id == tcId)
                        .FirstOrDefaultAsync();

                    if (tcToUpdate != null)
                    {
                        if (tcToUpdate.ExecutionSchemeImageId != null)
                        {
                            var img = db.ImageStorage.Where(i => i.Id == tcToUpdate.ExecutionSchemeImageId)
                                .FirstOrDefault();
                            if (img != null && img.ImageBase64 != newES)
                            {
                                img.ImageBase64 = newES;
                                db.ImageStorage.Update(img);
                            }
                        }
                        else
                        {
                            var img = new ImageStorage()
                            {
                                ImageBase64 = newES,
                                Category = ImageCategory.ExecutionScheme
                            };
                            db.ImageStorage.Add(img); 
                            await db.SaveChangesAsync();
                            tcToUpdate.ExecutionSchemeImageId = img.Id;
                            
                        }
                        //tcToUpdate.ExecutionSchemeBase64 = newES;
                        //tcToUpdate.Name = "";
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка при загрузке схемы исполнения.\nСхема в БД не сохранена.");
            }
            
        }
        public async Task DeleteTcExecutionScheme(int tcId)
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    var tcToUpdate = await db.TechnologicalCards
                        .Where(t => t.Id == tcId)
                        .FirstOrDefaultAsync();

                    if (tcToUpdate != null)
                    {
                        if (tcToUpdate.ExecutionSchemeImageId != null)
                        {
                            var img = db.ImageStorage.Where(i => i.Id == tcToUpdate.ExecutionSchemeImageId).FirstOrDefault();
                            if(img != null)
                                db.ImageStorage.Remove(img);
                            tcToUpdate.ExecutionSchemeImageId = null;

                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка при загрузке схемы исполнения.\nСхема в БД не сохранена.");
            }

        }

    }
}
