using Microsoft.EntityFrameworkCore;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.DataProcessing
{
    public class DbConnector
    {
        //private MyDbContext context; //
        public void UpdateCurrentTc(int id)
        {
            Program.currentTc = GetObject<TechnologicalCard>(id);
        }
        public void Update<T>(T updatingobj) where T : class, IIdentifiable
        {
            using (var context = new MyDbContext())
            {
                //// todo add mecanism to increase version of object if it was version field
                //TechnologicalCard existingEntity = new();
                ////var local = GetObject<T>(updatingobj.Id);
                //if (typeof(T) == typeof(TechnologicalCard))
                //{
                //    updatingobj = updatingobj as TechnologicalCard;
                //    existingEntity = context.Set<TechnologicalCard>()
                //                    .Where(tc => tc.Id == ((TechnologicalCard)updatingobj).Id)
                //                    .Include(tc => tc.Staffs)
                //                    .Include(tc => tc.Components)
                //                    .Include(tc => tc.Tools)
                //                    .Include(tc => tc.Machines)
                //                    .Include(tc => tc.Protections)
                //                    //.Include(tc => tc.WorkSteps)
                //                    .Cast<T>()
                //                    .FirstOrDefault();

                //    if (existingEntity != null)
                //    {
                //        context.Entry(existingEntity).State = EntityState.Detached;

                //        context.TechnologicalCards.Attach(updatingobj as TechnologicalCard);

                //        context.Entry(updatingobj).State = EntityState.Modified;

                //        // Сохраняем изменения в базе данных
                //        context.SaveChanges();
                //    }
                //}
                //var existingEntity = GetObject<T>(updatingobj.Id);

                //// check if local is not null 
                //if (existingEntity != null)
                //{
                //    context.Entry(existingEntity).State = EntityState.Detached;

                //    context.TechnologicalProcesses.Attach(updatingobj);
                //}

                //local = updatingobj;
                //// set Modified flag in your entry
                //context.Entry(local).State = EntityState.Modified;

                ////context.Update(updatingobj);
                //context.SaveChanges();
            }
        }

        public void SaveChanges(TechnologicalCard tc)
        {
            using (var context = new MyDbContext())
            {
                //context.Entry(tc).State = EntityState.Modified;

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
                // todo add mecanism to increase version of object if it was version field

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
        public void Add<T>(T addingobj)
        {
            using (var context = new MyDbContext())
            {
                context.Add(addingobj);
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
        public List<T> GetList<T>() where T : class, IIdentifiable
        {
            try
            {
                // todo - Db connection error holder 
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

    }
}
