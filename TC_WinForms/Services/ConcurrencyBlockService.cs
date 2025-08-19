using Microsoft.EntityFrameworkCore;
using Serilog;
using TC_WinForms.WinForms;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using Timer = System.Timers.Timer;

namespace TC_WinForms.Services
{
    public class ConcurrencyBlockService<T> where T : class, IIdentifiable
    {
        private readonly ILogger _logger;

        private bool? IsObjectInUse; //используется ли сейчас карта в программе другим пользователем
        private Timer? UpdateDataTimer;
        private int ObjectId;
        private string ObjectType;
        private int TimerInterval;//интервал работы таймера в милисекундах
        private ObjectLocker blockedObject;
        public ConcurrencyBlockService(string ObjectType, int ObjectId, int timerInterval)
        {
            _logger = Log.Logger.ForContext<ConcurrencyBlockService<T>>();

            this.ObjectType = ObjectType;
            this.ObjectId = ObjectId;
            TimerInterval = timerInterval;

            Log.Information("Инициализация ConcurrencyBlockService для объекта {ObjectType} с ID={ObjectId}", ObjectType, ObjectId);
        }
        public bool GetObjectUsedStatus() 
        {
            if (blockedObject == null)
                CheckAreObjectBlock();

            _logger.Information("Статус блокировки объекта {ObjectType} с ID={ObjectId}: {IsObjectInUse}", ObjectType, ObjectId, IsObjectInUse);
            return (bool)IsObjectInUse!; 
        }
        public void BlockObject()
        {
            if (IsObjectInUse != null && (bool)IsObjectInUse)
            {
                _logger.Warning("Попытка заблокировать уже заблокированный объект {ObjectType} с ID={ObjectId}", ObjectType, ObjectId);
                return;
            }

            try
            {
                using (MyDbContext dbContext = new MyDbContext())
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        var isBlocked = dbContext.BlockedConcurrencyObjects
                                                 .Any(b => b.ObjectId == ObjectId && b.ObjectType == ObjectType);

                        if (!isBlocked)
                        {
                            blockedObject = new ObjectLocker
                            {
                                TimeStamp = DateTime.Now,
                                ObjectId = ObjectId,
                                ObjectType = ObjectType
                            };

                            dbContext.BlockedConcurrencyObjects.Add(blockedObject);
                            dbContext.SaveChanges();
                            transaction.Commit();

                            _logger.Information("Объект {ObjectType} с ID={ObjectId} успешно заблокирован", ObjectType, ObjectId);
                        }
                        else
                        {
                            _logger.Warning("Объект {ObjectType} с ID={ObjectId} уже заблокирован другим пользователем", ObjectType, ObjectId);
                        }
                    }
                }

                SetTimer();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при блокировке объекта {ObjectType} с ID={ObjectId}", ObjectType, ObjectId);
                throw;
            }
        }
        public void CleanBlockData()
        {
            if (IsObjectInUse != null && (bool)IsObjectInUse)
            {
                _logger.Warning("Попытка снять блокировку с объекта {ObjectType} с ID={ObjectId}, который используется другим пользователем", ObjectType, ObjectId);
                return;
            }

            try
            {
                using (MyDbContext dbContext = new MyDbContext())
                {
                    var blockedObject = dbContext.BlockedConcurrencyObjects
                                                 .FirstOrDefault(s => s.ObjectId == ObjectId && s.ObjectType == ObjectType);

                    if (blockedObject != null)
                    {
                        dbContext.BlockedConcurrencyObjects.Remove(blockedObject);
                        dbContext.SaveChanges();
                        _logger.Information("Блокировка снята для объекта {ObjectType} с ID={ObjectId}", ObjectType, ObjectId);
                    }
                    else
                    {
                        _logger.Warning("Попытка снять блокировку с объекта {ObjectType} с ID={ObjectId}, который не был заблокирован", ObjectType, ObjectId);
                    }
                }

                UpdateDataTimer?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при снятии блокировки объекта {ObjectType} с ID={ObjectId}", ObjectType, ObjectId);
                throw;
            }

        }
        private void CheckAreObjectBlock()
        {
            try
            {
                using (MyDbContext dbContext = new MyDbContext())
                {
                    IsObjectInUse = dbContext.BlockedConcurrencyObjects
                                             .Any(s => s.ObjectId == ObjectId && s.ObjectType == ObjectType);

                    _logger.Debug("Проверка блокировки завершена для объекта {ObjectType} с ID={ObjectId}. Статус: {IsObjectInUse}", ObjectType, ObjectId, IsObjectInUse);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при проверке блокировки объекта {ObjectType} с ID={ObjectId}", ObjectType, ObjectId);
                throw;
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
            
            _logger.Information("Таймер обновления блокировки установлен с интервалом {TimerInterval} мс для объекта {ObjectType} с ID={ObjectId}", TimerInterval, ObjectType, ObjectId);
        }

        private void UpdateDataTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                using (MyDbContext dbContext = new MyDbContext())
                {
                    var blockedObject = dbContext.BlockedConcurrencyObjects
                                                 .FirstOrDefault(s => s.ObjectId == ObjectId && s.ObjectType == ObjectType);

                    if (blockedObject != null)
                    {
                        dbContext.BlockedConcurrencyObjects.Where(u => u.Id == blockedObject.Id)
                                                           .ExecuteUpdate(b => b.SetProperty(u => u.TimeStamp, e.SignalTime));

                        dbContext.SaveChanges();
                        _logger.Debug("Таймер обновил метку времени для объекта {ObjectType} с ID={ObjectId} на {TimeStamp}", ObjectType, ObjectId, blockedObject.TimeStamp);
                    }
                    else
                    {
                        _logger.Warning("Таймер не смог найти объект {ObjectType} с ID={ObjectId} для обновления метки времени. Остановка таймера", ObjectType, ObjectId);
                        UpdateDataTimer?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при обновлении таймера блокировки для объекта {ObjectType} с ID={ObjectId}", ObjectType, ObjectId);
                UpdateDataTimer?.Dispose();
            }
        }

    }
}
