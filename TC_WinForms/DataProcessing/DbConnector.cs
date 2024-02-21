using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.DataProcessing
{
    public class DbConnector
    {
        public async Task AddTcAsync(TechnologicalCard tc)
        {
            await AddTcAsync(new List<TechnologicalCard> { tc });
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
        public async Task AddObjectAsync<T>(List<T> tcs) where T : class, IIdentifiable
        {
            using (var db = new MyDbContext())
            {
                var objectIds = tcs.Select(t => t.Id).ToList();
                var existingObjects = await db.Set<T>()
                    .Where(t => objectIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                var newObjects = tcs.Where(t => !existingObjects.Contains(t.Id)).ToList();

                if (newObjects.Any())
                {
                    await db.Set<T>().AddRangeAsync(newObjects);
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task UpdateTcAsync(TechnologicalCard tc)
        {
            await UpdateTcListAsync(new List<TechnologicalCard> { tc });
        }
        public async Task UpdateTcListAsync(List<TechnologicalCard> updatedTcs)
        {
            using (var db = new MyDbContext())
            {
                foreach (var updatedTc in updatedTcs)
                {
                    var existingTc = await db.TechnologicalCards
                        .FirstOrDefaultAsync(t => t.Id == updatedTc.Id);

                    if (existingTc != null)
                    {
                        existingTc.Name = updatedTc.Name;
                        existingTc.Description = updatedTc.Description;
                        existingTc.Type = updatedTc.Type;
                        existingTc.NetworkVoltage = updatedTc.NetworkVoltage;
                        existingTc.TechnologicalProcessType = updatedTc.TechnologicalProcessType;
                        existingTc.TechnologicalProcessName = updatedTc.TechnologicalProcessName;
                        existingTc.Parameter = updatedTc.Parameter;
                        existingTc.FinalProduct = updatedTc.FinalProduct;
                        existingTc.Applicability = updatedTc.Applicability;
                        existingTc.Note = updatedTc.Note;
                        existingTc.IsCompleted = updatedTc.IsCompleted;
                        existingTc.Version = updatedTc.Version;
                        existingTc.TechnologicalProcessNumber = updatedTc.TechnologicalProcessNumber;
                        existingTc.DamageType = updatedTc.DamageType;
                        existingTc.RepairType = updatedTc.RepairType;
                        existingTc.Data = updatedTc.Data;
                        existingTc.Article = updatedTc.Article;
                        existingTc.TechnologicalProcess = updatedTc.TechnologicalProcess;
                    }
                }

                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task DeleteTcAsync(List<int> tcIds)
        {
            using (var db = new MyDbContext())
            {
                var tcsToDelete = await db.TechnologicalCards
                                          .Where(tc => tcIds.Contains(tc.Id))
                                          .ToListAsync();

                if (tcsToDelete.Any())
                {
                    db.TechnologicalCards.RemoveRange(tcsToDelete);

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
                    Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
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
                    Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                }

                //context.TechnologicalCards.Attach(updatingobj);

                context.Entry(updatingobj).State = EntityState.Modified;

                trackedEntities = context.ChangeTracker.Entries();
                foreach (var entry in trackedEntities)
                {
                    Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
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
                Console.WriteLine(e);
                throw;
            }
        }

        public List<T> GetObjectList<T>() where T : class, IIdentifiable
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    if (typeof(T) == typeof(TechnologicalProcess))
                        return context.Set<TechnologicalProcess>()
                                        //.Include(tp => tp.TechnologicalCards)
                                        .Cast<T>()
                                        .ToList();

                    else if (typeof(T) == typeof(TechnologicalCard))
                        return context.Set<TechnologicalCard>()
                                        //.Include(tc => tc.Staffs)
                                        //.Include(tc => tc.Components)
                                        //.Include(tc => tc.Tools)
                                        //.Include(tc => tc.Machines)
                                        //.Include(tc => tc.Protections)
                                        //.Include(tc => tc.WorkSteps)
                                        .Cast<T>()
                                        .ToList();
                    else return context.Set<T>().ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
                Console.WriteLine(e);
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
                else obj = context.Set<T>().Where(tc => tc.Id == id).FirstOrDefault();
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
    }
}
