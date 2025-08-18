using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TC_WinForms.ExceptionHandler
{
    public class ConcurrencyConflictHandler
    {
        private readonly ILogger _logger;
        private readonly DbContext _context;

        private Dictionary<string, string> entityDisplayNames = new Dictionary<string, string>
        {
            { "TechnologicalCard", "Технологическая карта" },
            { "DiagramShag", "Шаг диаграммы" },
            { "ExecutionWork", "Технологический переход" },
            { "TechOperationWork", "Технологическая операция" },
            { "Component_TC", "Компонент/материал" },
            { "ComponentWork", "Компонент/материал" },
            { "Tool_TC", "Инструмент" },
            { "ToolWork", "Инструмент" },
            { "Staff_TC", "Персонал" },
            { "Protection_TC", "СИЗ" },
            { "Machine_TC", "Механизм" },
        };

        private Dictionary<string, string> entityNameProperties = new Dictionary<string, string>
        {
            { "TechnologicalCard", "Article" },
            { "DiagramShag", "Nomer" },
            { "ExecutionWork", "techTransition" },
            { "TechOperationWork", "techOperation" },
            { "Component_TC", "Child" },
            { "ComponentWork", "component" },
            { "Tool_TC", "Child" },
            { "ToolWork", "tool" },
            { "Staff_TC", "Child" },
            { "Protection_TC", "Child" },
            { "Machine_TC", "Child" },
        };


        public ConcurrencyConflictHandler(ILogger logger, DbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        public async Task<bool> HandleConcurrencyExceptionAsync(DbUpdateConcurrencyException ex, int tcId)
        {
            bool retry = false;

            foreach (var entry in ex.Entries)
            {
                var entity = entry.Entity;
                var currentValues = entry.CurrentValues;
                var originalValues = entry.OriginalValues;
                var databaseValues = entry.GetDatabaseValues();

                var entityTypeName = entry.Metadata.Name.Split('.').Last();
                var displayName = entityDisplayNames.TryGetValue(entityTypeName, out var name) ? name : entityTypeName;

                var namePropertyKey = entityNameProperties.TryGetValue(entityTypeName, out var propertyName) ? propertyName : "Id";
                var namePropertyValue = entity.GetType().GetProperty(namePropertyKey)?.GetValue(entity)?.ToString()
                                        ?? currentValues["Id"]?.ToString() ?? "Неизвестно";

                if (databaseValues == null)
                {
                    _logger.Error($"Concurrency conflict: Entity {entry.Metadata.Name} with ID {currentValues["Id"]} was deleted for TcId={tcId}.");
                    var result = MessageBox.Show($"Объект {displayName} (Идентификатор={namePropertyValue}) был удалён. Создать заново?",
                                                "Конфликт", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        entry.State = EntityState.Detached;
                        var newEntity = Activator.CreateInstance(entry.Metadata.ClrType);
                        foreach (var property in currentValues.Properties)
                        {
                            entry.Entity.GetType().GetProperty(property.Name).SetValue(newEntity, currentValues[property]);
                        }
                        _context.Add(newEntity);
                        _logger.Information($"Объект {entry.Metadata.Name} (ID={currentValues["Id"]}) создан заново для TcId={tcId}.");
                        retry = true;
                    }
                    else
                    {
                        entry.State = EntityState.Detached;
                        _logger.Information($"Объект {entry.Metadata.Name} (ID={currentValues["Id"]}) удалён из локального контекста для TcId={tcId}.");
                        MessageBox.Show($"Объект {displayName}  ((Идентификатор={namePropertyValue}) был удалён и исключён из изменений. Повторите сохранение!");
                        return false; // Нет смысла продолжать, объект удалён
                    }
                }
                else
                {
                    _logger.Error($"Concurrency conflict: Entity {entry.Metadata.Name} with ID {currentValues["Id"]} was modified for TcId={tcId}.");
                    foreach (var property in currentValues.Properties)
                    {
                        var original = originalValues[property];
                        var current = currentValues[property];
                        var database = databaseValues[property];
                        if (!Equals(original, database))
                        {
                            _logger.Error($"Property {property.Name}: Original={original}, Current={current}, Database={database}");
                        }
                    }

                    var result = MessageBox.Show($"Объект {displayName} (Идентификатор={namePropertyValue}) был изменён. " +
                                                 "Перезаписать данными из базы?", "Конфликт", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        entry.OriginalValues.SetValues(databaseValues);
                        entry.CurrentValues.SetValues(databaseValues);
                        _logger.Information($"Объект {entry.Metadata.Name} (ID={currentValues["Id"]}) синхронизирован с данными из БД для TcId={tcId}.");
                        retry = true;
                    }
                    else
                    {
                        entry.OriginalValues.SetValues(databaseValues);
                        _logger.Information($"Объект {entry.Metadata.Name} (ID={currentValues["Id"]}) оставлен с изменениями пользователя для TcId={tcId}.");
                        retry = true;
                    }
                }
            }

            return retry;
        }
    }
}
