using Microsoft.EntityFrameworkCore;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using Timer = System.Timers.Timer;

namespace TC_WinForms.Services
{
    public class ConcurrencyBlockServise<T> where T : class, IIdentifiable
    {
        private bool IsCardUsing = false; //используется ли сейчас карта в программе другим пользователем
        private Timer UpdateDataTimer;
        private int ObjectId;
        private string ObjectType;

        public ConcurrencyBlockServise(T obj)
        {
            ObjectType = obj.GetType().Name;
            ObjectId = obj.Id;
        }
        public bool GetObjectUsedStatus() 
        {
            CheckAreObjectBlock();
            return IsCardUsing; 
        }
        public void BlockObject()
        {
            if (!IsCardUsing) 
            {
                using (MyDbContext dbContext = new MyDbContext())
                {
                    BlockedConcurrencyObjects blockedConcurrencyObjects = new BlockedConcurrencyObjects();
                    blockedConcurrencyObjects.TimeStamp = DateTime.Now;
                    blockedConcurrencyObjects.ObjectId = ObjectId;
                    blockedConcurrencyObjects.ObjectType = ObjectType;
                    dbContext.BlockedConcurrencyObjects.Add(blockedConcurrencyObjects);
                    dbContext.SaveChanges();
                }
                SetTimer();
            }
        }
        public void CleanBlockData()
        {
            if (!IsCardUsing)
            {
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

        }
        private void CheckAreObjectBlock()
        {
            using (MyDbContext dbContext = new MyDbContext())
            {
                var isObjectBlocked = dbContext.BlockedConcurrencyObjects.Where(s => s.ObjectId == ObjectId && s.ObjectType == ObjectType)
                                                                          .FirstOrDefault();
                if (isObjectBlocked != null)
                {
                    IsCardUsing = true;
                }
                else
                {
                    IsCardUsing = false;
                }
            }
        }
        private void SetTimer()
        {
            // Создание таймера с отсчетом 25 минут
            UpdateDataTimer = new Timer();
            UpdateDataTimer.Interval = 1000 * 60 * 25;
            // Hook up the Elapsed event for the timer. 
            UpdateDataTimer.Elapsed += UpdateDataTimer_Elapsed;
            UpdateDataTimer.AutoReset = true;
            UpdateDataTimer.Enabled = true;
        }

        private void UpdateDataTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            using (MyDbContext dbContext = new MyDbContext())
            {
                var cardStatus = dbContext.BlockedConcurrencyObjects.Where(s => s.ObjectId == ObjectId && s.ObjectType == ObjectType)
                                                                          .FirstOrDefault();
                if (cardStatus != null)
                {
                    dbContext.BlockedConcurrencyObjects.Where(s => s.ObjectId == ObjectId && s.ObjectType == ObjectType)
                                                       .ExecuteUpdate(b => b.SetProperty(u => u.TimeStamp, e.SignalTime));
                    dbContext.SaveChanges();
                }
            }
        }

    }
}
