using Microsoft.EntityFrameworkCore;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using Timer = System.Timers.Timer;

namespace TC_WinForms.Services
{
    public class ConcurrencyBlockServise<T> where T : class, IIdentifiable
    {
        private bool? IsObjectInUse; //используется ли сейчас карта в программе другим пользователем
        private Timer UpdateDataTimer;
        private int ObjectId;
        private string ObjectType;
        private int TimerInterval;//интервал работы таймера в милисекундах
        public ConcurrencyBlockServise(T obj, int timerInterval)
        {
            ObjectType = obj.GetType().Name;
            ObjectId = obj.Id;
            TimerInterval = timerInterval;
        }
        public bool GetObjectUsedStatus() 
        {
            if (IsObjectInUse == null)
                CheckAreObjectBlock();
            return (bool)IsObjectInUse; 
        }
        public void BlockObject()
        {
            if (IsObjectInUse != null && (bool)IsObjectInUse)
                return; 

            using (MyDbContext dbContext = new MyDbContext())
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    var isBlocked = dbContext.BlockedConcurrencyObjects
                                                 .Any(b => b.ObjectId == ObjectId && b.ObjectType == ObjectType);

                    if (!isBlocked)
                    {
                        var blockedObject = new ObjectLocker
                        {
                            TimeStamp = DateTime.Now,
                            ObjectId = ObjectId,
                            ObjectType = ObjectType
                        };

                        dbContext.BlockedConcurrencyObjects.Add(blockedObject);
                        dbContext.SaveChanges();
                        transaction.Commit();
                    }
                }
            }
            SetTimer();
        }
        public void CleanBlockData()
        {
            if (IsObjectInUse != null && (bool)IsObjectInUse)
                return;

            using (MyDbContext dbContext = new MyDbContext())
            {
               
                    var blockedObject = dbContext.BlockedConcurrencyObjects.Where(s => s.ObjectId == ObjectId && s.ObjectType == ObjectType)
                                                                            .FirstOrDefault();
                    if (blockedObject != null)
                    {
                        dbContext.BlockedConcurrencyObjects.Remove(blockedObject);
                        dbContext.SaveChanges();
                    }
            }
            UpdateDataTimer.Dispose();

        }
        private void CheckAreObjectBlock()
        {
            using (MyDbContext dbContext = new MyDbContext())
            {
                IsObjectInUse = dbContext.BlockedConcurrencyObjects
                                       .Any(s => s.ObjectId == ObjectId && s.ObjectType == ObjectType);
            }
        }
        private void SetTimer()
        {
            // Создание таймера с отсчетом 25 минут
            UpdateDataTimer = new Timer();
            UpdateDataTimer.Interval = TimerInterval;
            // Hook up the Elapsed event for the timer. 
            UpdateDataTimer.Elapsed += UpdateDataTimer_Elapsed;
            UpdateDataTimer.AutoReset = true;
            UpdateDataTimer.Enabled = true;
        }

        private void UpdateDataTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            using (MyDbContext dbContext = new MyDbContext())
            {
                var blockedObject = dbContext.BlockedConcurrencyObjects
                                             .FirstOrDefault(s => s.ObjectId == ObjectId && s.ObjectType == ObjectType);

                if (blockedObject != null)
                {
                    blockedObject.TimeStamp = e.SignalTime;
                    dbContext.SaveChanges();
                }
                else
                {
                    UpdateDataTimer?.Dispose();
                }
            }
        }

    }
}
