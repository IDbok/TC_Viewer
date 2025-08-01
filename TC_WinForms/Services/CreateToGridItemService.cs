using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC_WinForms.WinForms.Work;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.Services
{
    public static class CreateToGridItemService
    {
        private static int recordOrder = 1;

        public static List<TechOperationDataGridItem> PopulateTechOperationDataGridItems(List<TechOperationWork> techOperationWorkList, List<Machine_TC> machines) // todo: вставка по индексу?
        {
            recordOrder = 1;
            List <TechOperationDataGridItem> result = new List<TechOperationDataGridItem>();

            foreach (var techOperationWork in techOperationWorkList)
            {
                result.AddRange(CreateTechOperationDataGridItem(techOperationWork, machines));
            }

            CalculateEtapTimes(result);

            return result;
        }

        private static List<TechOperationDataGridItem> CreateTechOperationDataGridItem(TechOperationWork techOperationWork, List<Machine_TC> machines)
        {
            List<TechOperationDataGridItem> result = new List<TechOperationDataGridItem>();
            var executionWorks = techOperationWork.executionWorks.Where(w => !w.Delete).OrderBy(o => o.Order).ToList();

            if (executionWorks.Count == 0)
            {
                result.Add(new TechOperationDataGridItem(techOperationWork));
                recordOrder++;
            }

            foreach (var executionWork in executionWorks)
            {
                var item = CreateExecutionWorkItem(techOperationWork, machines, executionWork, recordOrder);
                result.Add(item);
                recordOrder++;
            }

            foreach (var toolWork in techOperationWork.ToolWorks.Where(t => t.IsDeleted == false).ToList())
            {
                result.Add(new TechOperationDataGridItem(techOperationWork, toolWork, recordOrder));
                recordOrder++;
            }

            foreach (var componentWork in techOperationWork.ComponentWorks.Where(t => t.IsDeleted == false).ToList())
            {
                result.Add(new TechOperationDataGridItem(techOperationWork, componentWork, recordOrder));
                recordOrder++;
            }

            return result;
        }

        private static TechOperationDataGridItem CreateExecutionWorkItem(
            TechOperationWork techOperationWork,
            List<Machine_TC> machines,
            ExecutionWork executionWork,
            int nomer)
        {
            // Генерация Guid, если он не задан
            if (executionWork.IdGuid == Guid.Empty)
            {
                executionWork.IdGuid = Guid.NewGuid();
            }

            // Установка порядкового номера строки
            executionWork.RowOrder = nomer;

            // Строка со списком персонала
            var staffStr = string.Join(",", executionWork.Staffs.Select(s => s.Symbol));

            // Строка с "диапазоном" номеров защит
            var protectList = executionWork.Protections.Select(p => p.Order).ToList();
            var protectStr = ConvertListToRangeString(protectList);

            // Список bool, отражающий, какие машины входят (пример в исходном коде)
            var mach = machines
                .Select(tc => executionWork.Machines.Contains(tc))
                .ToList();

            var item = new TechOperationDataGridItem(techOperationWork, executionWork, nomer, staffStr, mach, protectStr);

            // Дополнительная проверка значения
            if (item.TechTransitionValue == "-1")
            {
                item.TechTransitionValue = "Ошибка";
            }

            return item;
        }

        private static string ConvertListToRangeString(List<int> numbers)
        {
            if (numbers == null || !numbers.Any())
                return string.Empty;

            // Сортировка списка
            numbers.Sort();

            StringBuilder stringBuilder = new StringBuilder();
            int start = numbers[0];
            int end = start;

            for (int i = 1; i < numbers.Count; i++)
            {
                // Проверяем, идут ли числа последовательно
                if (numbers[i] == end + 1)
                {
                    end = numbers[i];
                }
                else
                {
                    // Добавляем текущий диапазон в результат
                    if (start == end)
                        stringBuilder.Append($"{start}, ");
                    else
                        stringBuilder.Append($"{start}-{end}, ");

                    // Начинаем новый диапазон
                    start = end = numbers[i];
                }
            }

            // Добавляем последний диапазон
            if (start == end)
                stringBuilder.Append($"{start}");
            else
                stringBuilder.Append($"{start}-{end}");

            return stringBuilder.ToString().TrimEnd(',', ' ');


        }


        private static void CalculateEtapTimes(List<TechOperationDataGridItem> result)
        {
            // Group items by ParallelIndex
            var parallelGroups = GroupByParallelIndex(result);

            // Process each group
            foreach (var group in parallelGroups)
            {
                var currentGroupType = group.Item1;
                var currentGroupList = group.Item2;
                var currentGroupProducts = group.Item3;

                switch (currentGroupType)
                {
                    case GroupType.Single:
                        ProcessSingleGroup(currentGroupList);
                        break;
                    case GroupType.Etap:
                        ProcessEtapGroup(currentGroupList);
                        break;
                    case GroupType.ParallelIndex:
                        ProcessParallelGroup(currentGroupList);
                        break;
                }
                currentGroupProducts.ForEach(i => i.TimeEtap = "-1");
            }
        }

        /// <summary>
        /// Группирует строки по Индексу параллельности или этапу. 
        /// При отсутствии одного из этих признаков добавляет в стак лист с один элементом
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private static Stack<(GroupType, List<TechOperationDataGridItem>, List<TechOperationDataGridItem>)>
            GroupByParallelIndex(List<TechOperationDataGridItem> items)
        {
            var parallelGroups = new Stack<(GroupType, List<TechOperationDataGridItem>, List<TechOperationDataGridItem>)>();

            var currentParallelIndex = 0;
            var currentEtap = "";

            foreach (var item in items)
            {
                if (item.Work)
                {
                    int? parallelIndex = item.techWork.techOperationWork.GetParallelIndex();

                    if (parallelIndex == null)
                    {
                        // проверка на наличие этапа
                        if (item.Etap == "" || item.Etap == "0")
                        {
                            // обнуляем этап
                            currentEtap = "";

                            parallelGroups.Push((GroupType.Single,
                                new List<TechOperationDataGridItem> { item },
                                new List<TechOperationDataGridItem>()));

                            continue;
                        }
                        else
                        {
                            // проверка на смену этапа
                            if (currentEtap != item.Etap)
                            {
                                currentEtap = item.Etap;

                                parallelGroups.Push((GroupType.Etap,
                                    new List<TechOperationDataGridItem> { item },
                                    new List<TechOperationDataGridItem>()));

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
                                new List<TechOperationDataGridItem> { item },
                                new List<TechOperationDataGridItem>()));
                        }
                        else
                        {
                            parallelGroups.Peek().Item2.Add(item);
                        }
                    }
                }
                else
                {
                    if (parallelGroups.Count == 0)
                    {
                        parallelGroups.Push((GroupType.Single,
                                    new List<TechOperationDataGridItem>(),
                                    new List<TechOperationDataGridItem> { item }));
                    }

                    parallelGroups.Peek().Item3.Add(item);
                }

            }

            return parallelGroups;
        }
        enum GroupType
        {
            Single,
            Etap,
            ParallelIndex,
        }

        private static double ProcessEtapGroup(List<TechOperationDataGridItem> etapGroup)
        {
            var executionWorks = new List<ExecutionWork>();

            foreach (var item in etapGroup)
            {
                if (item.Work)
                {
                    executionWorks.Add(item.techWork);
                }
            }

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

            // Adjust listMach if necessary
            if (etapGroup.Count > 1)
            {
                var col = etapGroup[0].listMach.Count;
                for (int i = 0; i < col; i++)
                {
                    bool tr = etapGroup.Any(item => item.listMach[i]);
                    if (tr)
                    {
                        foreach (var item in etapGroup)
                        {
                            item.listMach[i] = true;
                        }
                    }
                }
            }

            // Calculate the maximum TempTimeExecution among ExecutionWorks
            double maxTime = CalculateMaxEtapTime(etapGroup); //executionWorks.Max(m => m.TempTimeExecution);


            etapGroup.ForEach(item => item.TimeEtap = "-1");
            etapGroup[0].TimeEtap = maxTime.ToString();

            return maxTime;
        }

        private static void ProcessSingleGroup(List<TechOperationDataGridItem> etapGroup)
        {
            foreach (var item in etapGroup)
            {
                item.TimeEtap = item.techWork.Value.ToString();
            }
        }

        private static void ProcessParallelGroup(List<TechOperationDataGridItem> etapGroup)
        {
            var times = new List<double>();

            var groups2 = etapGroup.GroupBy(g => g.techWork.techOperationWork.GetSequenceGroupIndex()).ToList();
            foreach (var group in groups2)
            {
                // if (sequenceGroupIndex == null) то в группы выделяются ТО
                if (group.Key == null)
                {
                    var nullIndexGroups = group.ToList().GroupBy(g => g.techWork.techOperationWorkId).ToList();

                    foreach (var parGroup in nullIndexGroups)
                    {

                        times.Add(CalculateMaxEtapTime(parGroup.ToList()));
                    }
                }
                else
                {
                    var sequenceTimes = new List<double>();
                    var sequenceGroups = group.GroupBy(g => g.techWork.techOperationWorkId).ToArray();

                    foreach (var seqGroup in sequenceGroups)
                    {
                        sequenceTimes.Add(CalculateMaxEtapTime(seqGroup.ToList()));
                    }

                    times.Add(sequenceTimes.Sum());
                }
            }

            // присвоить это время первому шагу. Остальным присвоить -1
            etapGroup.ForEach(i => i.TimeEtap = "-1");
            etapGroup[0].TimeEtap = times.Max().ToString();
        }

        private static double CalculateMaxEtapTime(List<TechOperationDataGridItem> etapGroup)
        {
            // todo: проверка на наличие различных этапов внутри группы
            // Если есть разные этапы, то группируем по этапам и выполняем расчёт времени рекурсивно
            var etapGroups = etapGroup.GroupBy(g => g.Etap).ToList();
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
            if (etapGroup.All(a => a.Etap == "" || a.Etap == "0"
                //|| a.Etap == etapGroup[0].Etap
                ))
            {
                return etapGroup.Sum(s => s.techWork.Value);
            }

            var executionWorks = etapGroup.Where(w => w.Work).Select(s => s.techWork).ToList();

            var executionTimes = new List<double>();

            var sequenceTimes = new List<double>();

            foreach (var item in etapGroup)
            {
                if (item.Work)
                {
                    var executionWork = item.techWork;

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
            }

            executionTimes.Add(sequenceTimes.Sum());

            return executionTimes.Max();
        }
    }
}
