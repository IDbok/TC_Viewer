using TcDbConnector;
using TcModels.Models.TcContent;
using TcModels.Models;

namespace TestConsoleAppTC
{
    public class DbConnectorTest
    {
        //private MyDbContext context; //
        public DbConnectorTest()
        {

        }
        public void UpdateCurrentTc(int id)
        {
            Program.CurrentTc = GetObject<TechnologicalCard>(id);
        }
        public void Update<T>(T updatingobj)
        {
            using (var context = new MyDbContext())
            {
                // todo add mecanism to increase version of object if it was version field

                context.Update(updatingobj);
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
                    //if (typeof(T) == typeof(TechnologicalProcess))
                    //    return context.Set<TechnologicalProcess>()
                    //                    .Include(tp => tp.TechnologicalCards)
                    //                    .Cast<T>()
                    //                    .ToList();

                    //else if (typeof(T) == typeof(TechnologicalCard))
                    //    return context.Set<TechnologicalCard>()
                    //                    .Include(tc => tc.Staffs)
                    //                    .Include(tc => tc.Components)
                    //                    .Include(tc => tc.Tools)
                    //                    .Include(tc => tc.Machines)
                    //                    .Include(tc => tc.Protections)
                    //                    //.Include(tc => tc.WorkSteps)
                    //                    .Cast<T>()
                    //                    .ToList();
                    //else 
                        return context.Set<T>().ToList();
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
            using (var context = new MyDbContext())
            {
                //if (typeof(T) == typeof(TechnologicalProcess))
                //    return context.Set<TechnologicalProcess>()
                //                    .Where(tp => tp.Id == id)
                //                    .Include(tp => tp.TechnologicalCards)
                //                        .ThenInclude(tc => tc.Staffs)
                //                    .Include(tp => tp.TechnologicalCards)
                //                        .ThenInclude(tc => tc.Components)
                //                    .Include(tp => tp.TechnologicalCards)
                //                        .ThenInclude(tc => tc.Tools)
                //                    .Include(tp => tp.TechnologicalCards)
                //                        .ThenInclude(tc => tc.Machines)
                //                    .Include(tp => tp.TechnologicalCards)
                //                        .ThenInclude(tc => tc.Protections)
                //                    //.Include(tp => tp.TechnologicalCards)
                //                    //    .ThenInclude(tc => tc.WorkSteps)
                //                    .Cast<T>()
                //                    .FirstOrDefault();

                //else if (typeof(T) == typeof(TechnologicalCard))
                //    return context.Set<TechnologicalCard>()
                //                    .Where(tc => tc.Id == id)
                //                    .Include(tc => tc.Staffs)
                //                    .Include(tc => tc.Components)
                //                    .Include(tc => tc.Tools)
                //                    .Include(tc => tc.Machines)
                //                    .Include(tc => tc.Protections)
                //                    //.Include(tc => tc.WorkSteps)
                //                    .Cast<T>()
                //                    .FirstOrDefault();
                //else 
                    return context.Set<T>().Where(tc => tc.Id == id).FirstOrDefault();
            }


        }
    }
}
