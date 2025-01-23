using Microsoft.EntityFrameworkCore;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.TcContent;
using static TcModels.Models.TcContent.Outlay;

namespace TC_WinForms.Services
{
    public class CalculateOutlayService
    {
        enum GroupType
        {
            Single,
            Etap,
            ParallelIndex,
        }

        private List<Outlay> _outlayList = new List<Outlay>();

        public CalculateOutlayService() { }

        /// <summary>
        /// Рассчитывает затраты на основе актуальных данных из Технологической карты
        /// </summary>
        /// <param name="tcViewState">Объект класса TcViewState, хранящий в себе актуальыне данные ТК, которые могут быть не сохранены пользователем.</param>
        /// <returns>
        /// Список затрат типа <typeparamref name="Outlay"/>.
        /// </returns>
        public List<Outlay> GetOutlayList(TcViewState tcViewState)
        {
            CalculateTechCardOutlay(tcViewState);
            return _outlayList;
        }

        /// <summary>
        /// Рассчитывает затраты на основе актуальных данных из Технологической карты и сохраняет в БД новые данные, если такие имеются.
        /// </summary>
        /// <param name="tcViewState">Объект класса TcViewState, хранящий в себе актуальыне данные ТК, которые могут быть не сохранены пользователем.</param>
        public void TryRewriteOutlay(TcViewState tcViewState)
        {
            CalculateTechCardOutlay(tcViewState);

            using (MyDbContext context = new MyDbContext())
            {
                var existedOutlay = context.Set<Outlay>().Where(o => o.TcID == tcViewState.TechnologicalCard.Id).ToList();
                if (existedOutlay.Count == 0)
                {
                    context.OutlaysTable.AddRange(_outlayList);
                    context.SaveChanges();
                    return;
                }

                UpdateGroup(
                    existedOutlay.Where(o => o.Type == OutlayType.Mechine).ToList(),
                    _outlayList.Where(o => o.Type == OutlayType.Mechine).ToList(), context);

                UpdateGroup(
                    existedOutlay.Where(o => o.Type == OutlayType.Staff).ToList(),
                    _outlayList.Where(o => o.Type == OutlayType.Staff).ToList(), context);

                var existedComponent = existedOutlay.Where(s => s.Type == OutlayType.Components).First();
                var newComponent = _outlayList.Where(s => s.Type == OutlayType.Components).First();
                TryUpdateOutlayRecord(existedComponent, newComponent, context);

                var existedSummary = existedOutlay.Where(s => s.Type == OutlayType.SummaryTimeOutlay).First();
                var newSummary = _outlayList.Where(s => s.Type == OutlayType.SummaryTimeOutlay).First();
                TryUpdateOutlayRecord(existedSummary, newSummary, context);

                context.SaveChanges();
            }

        }

        /// <summary>
        ///  Если актуальные данные и данные из БД отличаются - обновляет данные о затратах в БД
        /// </summary>
        /// <param name="oldRecord">Объект класса Outlay, запись о затратах, полученная из базы данных.</param>
        /// <param name="newRecord">Объект класса Outlay, актуальная запись о затратах, полученная после пересчета текущих данных ТК (не сохраненных).</param>
        /// <param name="context">Объект класса MyDbContext, текущий контекст для работы с базой данных, с помощью которого вносятся изменения в запись.</param>
        private void TryUpdateOutlayRecord(Outlay oldRecord, Outlay newRecord, MyDbContext context)
        {
            if (newRecord.OutlayValue != oldRecord.OutlayValue)
            {
                context.OutlaysTable.Where(u => u.ID == oldRecord.ID)
                                                       .ExecuteUpdate(b => b.SetProperty(u => u.OutlayValue, newRecord.OutlayValue));
            }
        }

        /// <summary>
        ///  Сравнивает данные двух списков - актуальных затрат и затрат из БД и вносит изменения: удаляет неактуальные записи, добавляет новые и обновляет несовпадающие.
        /// </summary>
        /// <param name="oldListData">Список затрат полученный из базы данных.</param>
        /// <param name="newListData">Список затрат полученный после пересчета текущих данных ТК (не сохраненных).</param>
        /// <param name="context">Объект класса MyDbContext, текущий контекст для работы с базой данных, с помощью которого вносятся изменения в запись.</param>
        private void UpdateGroup(List<Outlay> oldListData, List<Outlay> newListData, MyDbContext context)
        {
            if (newListData == null && oldListData != null)
            {
                context.OutlaysTable.RemoveRange(oldListData);
                return;
            }
            else if(newListData == null)
            {
                return;
            }

            if (oldListData == null)
            {
                context.OutlaysTable.AddRange(newListData.ToList());
                return;
            }

            foreach (var item in oldListData)
            {
                var deleteFromOutlay = !newListData.Any(s => s.Name == item.Name);
                if (deleteFromOutlay)
                {
                    context.OutlaysTable.Remove(item);
                }
            }

            foreach(var item in newListData)
            {
                var addedInOutlay = !oldListData.Any(s => s.Name == item.Name);
                if (addedInOutlay)
                {
                    context.OutlaysTable.Add(item);
                    continue;
                }

                var updatedOutlay = oldListData.Where(s => s.Name.Equals(item.Name)).FirstOrDefault();
                if (updatedOutlay != null)
                {
                    TryUpdateOutlayRecord(updatedOutlay, item, context);
                }

            }
        }

        /// <summary>
        ///  Добавляет новую запись о затратах на основе данных редактируемой ТК в актуальный список затрат.
        /// </summary>
        private void AddNewOutlay(int tcId,OutlayType outlayType, UnitType unitType, double OutlayValue, string name = null)
        {
            var newOutlay = new Outlay
            {
                TcID = tcId,
                Type = outlayType,
                Name = name,
                OutlayUnitType = unitType,
                OutlayValue = Math.Round(OutlayValue, 2)
            };

            _outlayList.Add(newOutlay);
        }

        /// <summary>
        /// Рассчитывает затраты на основе актуальных данных из Технологической карты.
        /// </summary>
        /// <param name="tcViewState">Объект класса TcViewState, хранящий в себе актуальыне данные ТК, которые могут быть не сохранены пользователем.</param>
        private void CalculateTechCardOutlay(TcViewState tcViewState)
        {
            _outlayList.Clear();
            CalculateStaffOutlay(tcViewState);
            CalculateComponentOutlay(tcViewState);
            CalculateMachineOutlay(tcViewState);
            CalculateExecutionWorksOutlay(tcViewState);
        }

        /// <summary>
        /// Рассчитывает затраты персонала на основе актуальных данных из Технологической карты.
        /// </summary>
        /// <param name="tcViewState">Объект класса TcViewState, хранящий в себе актуальыне данные ТК, которые могут быть не сохранены пользователем.</param>
        private void CalculateStaffOutlay(TcViewState tcViewState)
        {
            double staffOutlay = 0;
            foreach (var staff in tcViewState.TechnologicalCard.Staff_TCs.Where(s =>  s.IsInOutlayCount).ToList())
            {
                foreach (var ew in staff.ExecutionWorks)
                {
                    staffOutlay += ew.Value;
                }
                staffOutlay = staffOutlay / 60;

                AddNewOutlay(tcViewState.TechnologicalCard.Id, OutlayType.Staff, UnitType.Hours, staffOutlay, $"{staff.Symbol} {staff.Child.Name}");
                staffOutlay = 0;
            }
        }

        /// <summary>
        /// Рассчитывает затраты материалов и компонентов на основе актуальных данных из Технологической карты.
        /// </summary>
        /// <param name="tcViewState">Объект класса TcViewState, хранящий в себе актуальыне данные ТК, которые могут быть не сохранены пользователем.</param>
        private void CalculateComponentOutlay(TcViewState tcViewState)
        {
            double componentOutlay = 0;
            foreach (var component in tcViewState.TechnologicalCard.Component_TCs)
            {
                componentOutlay += component.Child.Price == null
                    ? component.Quantity * 0
                    : component.Quantity * (double)component.Child.Price;
             }

            AddNewOutlay(tcViewState.TechnologicalCard.Id, OutlayType.Components, UnitType.Сurrency, componentOutlay);
        }

        /// <summary>
        /// Рассчитывает затраты механизмов на основе актуальных данных из Технологической карты.
        /// </summary>
        /// <param name="tcViewState">Объект класса TcViewState, хранящий в себе актуальыне данные ТК, которые могут быть не сохранены пользователем.</param>
        private void CalculateMachineOutlay(TcViewState tcViewState)
        {
            double machineOutlay = 0;
            foreach (var machine in tcViewState.TechnologicalCard.Machine_TCs.Where(s => s.IsInOutlayCount).ToList())
            {
                machineOutlay = machine.ExecutionWorks == null|| machine.ExecutionWorks.Count == 0
                    ? 1
                    : CalculateEtapTimes(machine.ExecutionWorks);

                machineOutlay = machineOutlay / 60;

                AddNewOutlay(tcViewState.TechnologicalCard.Id, OutlayType.Mechine, UnitType.Hours, machineOutlay, machine.Child.Name);

                machineOutlay = 0;
            }
        }

        /// <summary>
        /// Рассчитывает суммарные затраты по времени на основе актуальных данных из Технологической карты.
        /// </summary>
        /// <param name="tcViewState">Объект класса TcViewState, хранящий в себе актуальыне данные ТК, которые могут быть не сохранены пользователем.</param>
        private void CalculateExecutionWorksOutlay(TcViewState tcViewState)
        {
            double etapOutlay = 0;
            List <ExecutionWork> executionWorks = new List<ExecutionWork>();
            foreach (var tow in tcViewState.TechOperationWorksList)
            {
                executionWorks.AddRange(tow.executionWorks);
            }

            etapOutlay = CalculateEtapTimes(executionWorks);
            etapOutlay = etapOutlay / 60;

            AddNewOutlay(tcViewState.TechnologicalCard.Id, OutlayType.SummaryTimeOutlay, UnitType.Hours, etapOutlay);
        }

        #region ExecutionWork Calculate

        private Stack<(GroupType, List<ExecutionWork>, List<ExecutionWork>)> GroupByParallelIndex(List<ExecutionWork> items)
        {
            var parallelGroups = new Stack<(GroupType, List<ExecutionWork>, List<ExecutionWork>)>();

            var currentParallelIndex = 0;
            var currentEtap = "";

            foreach (var item in items)
            {
                int? parallelIndex = item.techOperationWork.GetParallelIndex();
                if (parallelIndex == null)
                {
                    if (item.Etap == "" || item.Etap == "0")
                    {
                        // обнуляем этап
                        currentEtap = "";
                        parallelGroups.Push((GroupType.Single,
                            new List<ExecutionWork> { item },
                            new List<ExecutionWork>()));

                        continue;
                    }
                    else
                    {
                        // проверка на смену этапа
                        if (currentEtap != item.Etap)
                        {
                            currentEtap = item.Etap;
                            parallelGroups.Push((GroupType.Etap,
                                new List<ExecutionWork> { item },
                                new List<ExecutionWork>()));

                            continue;
                        }
                        else
                        {
                            // добавляем в текущий список
                            parallelGroups.Peek().Item2.Add(item);
                        }
                    }
                }
                else
                {
                    // обнуляем этап ????
                    currentEtap = "";

                    if (currentParallelIndex != parallelIndex)
                    {
                        currentParallelIndex = parallelIndex.Value;
                        parallelGroups.Push((GroupType.ParallelIndex,
                            new List<ExecutionWork> { item },
                            new List<ExecutionWork>()));
                    }
                    else
                    {
                        parallelGroups.Peek().Item2.Add(item);
                    }
                }
            }

            return parallelGroups;
        }

        private double CalculateEtapTimes(List<ExecutionWork> calculatedExecutionWorks)
        {
            // Group items by ParallelIndex
            var parallelGroups = GroupByParallelIndex(calculatedExecutionWorks);
            double timeOutlay = 0;
            // Process each group
            foreach (var group in parallelGroups)
            {
                var currentGroupType = group.Item1;
                var currentGroupList = group.Item2;
                var currentGroupProducts = group.Item3;

                switch (currentGroupType)
                {
                    case GroupType.Single:
                        {
                            timeOutlay += currentGroupList[0].Value;
                            break;
                        }
                    case GroupType.Etap:
                        {
                            timeOutlay += ProcessEtapGroup(currentGroupList);
                            break;
                        }
                    case GroupType.ParallelIndex:
                        {
                            timeOutlay += ProcessParallelGroup(currentGroupList);
                            break;
                        }
                }
            }

            var a = 0;
            return timeOutlay;
        }

        private double ProcessParallelGroup(List<ExecutionWork> executionWorks)
        {
            var times = new List<double>();

            var groupedEW = executionWorks.GroupBy(g => g.techOperationWork.GetSequenceGroupIndex()).ToList();
            foreach (var group in groupedEW)
            {
                // if (sequenceGroupIndex == null) то в группы выделяются ТО
                if (group.Key == null)
                {
                    var nullIndexGroups = group.ToList().GroupBy(g => g.techOperationWorkId).ToList();

                    foreach (var parGroup in nullIndexGroups)
                    {

                        times.Add(CalculateMaxEtapTime(parGroup.ToList()));
                    }
                }
                else
                {
                    var sequenceTimes = new List<double>();
                    var sequenceGroups = group.GroupBy(g => g.techOperationWorkId).ToArray();

                    foreach (var seqGroup in sequenceGroups)
                    {
                        sequenceTimes.Add(CalculateMaxEtapTime(seqGroup.ToList()));
                    }

                    times.Add(sequenceTimes.Sum());
                }
            }

            var result = times.Max();
            return result;
        }

        private double ProcessEtapGroup(List<ExecutionWork> executionWorks)
        {
            // Process ExecutionWorks based on Posled
            foreach (var executionWork in executionWorks)
            {
                if (!string.IsNullOrEmpty(executionWork.Posled) && executionWork.Posled != "0")
                {
                    var allSum = executionWorks
                        .Where(w => w.Posled == executionWork.Posled && w.Value != -1)
                        .Sum(s => s.Value);
                    executionWork.TempTimeExecution = allSum;
                }
                else
                {
                    executionWork.TempTimeExecution = executionWork.Value == -1 ? 0 : executionWork.Value;
                }
            }

            // Calculate the maximum TempTimeExecution among ExecutionWorks
            double maxTime = CalculateMaxEtapTime(executionWorks); //executionWorks.Max(m => m.TempTimeExecution);

            return maxTime;
        }

        private double CalculateMaxEtapTime(List<ExecutionWork> executionWorksList)
        {
            // todo: проверка на наличие различных этапов внутри группы
            // Если есть разные этапы, то группируем по этапам и выполняем расчёт времени рекурсивно
            var etapGroups = executionWorksList.GroupBy(g => g.Etap).ToList();
            if (etapGroups.Count > 1)
            {
                var times = new List<double>();
                foreach (var group in etapGroups)
                {
                    times.Add(CalculateMaxEtapTime(group.ToList()));
                }
                return times.Max();
            }

            // Если этап у всех операций равен 0 или "",
            // то суммировать время всех операций в группе
            if (executionWorksList.All(a => a.Etap == "" || a.Etap == "0"
                ))
            {
                return executionWorksList.Sum(s => s.Value);
            }

            var executionWorks = executionWorksList.ToList();
            var executionTimes = new List<double>();
            var sequenceTimes = new List<double>();
            foreach (var item in executionWorksList)
            {
                var executionWork = item;

                // Если у шага нет группы параллельности, то считаем его выполнение последовательным
                if (executionWork.Etap == "" || executionWork.Etap == "0")
                {
                    executionTimes.Add(executionWork.Value);
                    continue;
                }

                if (!string.IsNullOrEmpty(executionWork.Posled) && executionWork.Posled != "0")
                {
                    var allSum = executionWorks
                        .Where(w => w.Posled == executionWork.Posled && w.Value != -1
                            && w.Etap == executionWork.Etap && w.Etap != "0" && w.Etap != "")
                        .Sum(s => s.Value);

                    executionTimes.Add(allSum);
                }
                else
                {
                    executionTimes.Add(executionWork.Value);
                }

            }

            executionTimes.Add(sequenceTimes.Sum());
            return executionTimes.Max();
        }

        #endregion
    }
}
