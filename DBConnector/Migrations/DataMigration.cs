using Microsoft.EntityFrameworkCore;

using oldCore = TcDbConnector.Migrations.OldCore.Models;
using newCore = TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;

namespace TcDbConnector.Migrations;

public class DataMigration
{
    private static OldDbContext _oldDbContext = new();
    private static MyDbContext _myDbContext = new();
    
    public static async void MigrateData(bool createNewDb)
    {
        if (_myDbContext.Database.CanConnect() && createNewDb)
        {
            _myDbContext.Database.EnsureDeleted();
        }

        // проверка на существование базы данных
        if (!_myDbContext.Database.CanConnect() && createNewDb)
        {
            //_myDbContext.Database.EnsureDeleted();
            _myDbContext.Database.EnsureCreated();
            Console.WriteLine("Новая БД создана!");
        }

        if (!_oldDbContext.Database.CanConnect() || !_myDbContext.Database.CanConnect())
        {
            throw new Exception("Не удалось подключиться к базе данных");
        }

        using (var transaction = _myDbContext.Database.BeginTransaction())
        {
            try
            {
                MigrateDictionaries();

                MigrateIntermediateTables();

                //MigrateDiagrams();


                _myDbContext.SaveChanges();

                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw;
            }
        }
        using (var transaction = _myDbContext.Database.BeginTransaction())
        {
            try
            {
                MigrateWorkTables();

                _myDbContext.SaveChanges();

                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw;
            }
        }
        using (var transaction = _myDbContext.Database.BeginTransaction())
        {
            try
            {
                MigrateDiagrams();

                _myDbContext.SaveChanges();

                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw;
            }
        }

        // Картинки переносятся по частям, чтобы не перегружать максимальный размер пакета в базе данных

        //рассчитать сколько всего картинок в таблице DiagramShag
        var count = _oldDbContext.DiagramShag.Count();
        // количество итераций
        var step = 2;
        var coefficient = count / step;
        var iterations = count % step == 0 ? coefficient : coefficient + 1;
        for (int i = 0; i < iterations; i++)
        {
            //Console.WriteLine($"Итерация { i + 1}");
            //Task.CompletedTask.Wait(1000);
            // транзакция для сохранения картинок
            using (var transaction = _myDbContext.Database.BeginTransaction())
            {
                try
                {
                    MigrateDiagramShagPictures(i, step);

                    _myDbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    #region Dictionaries
    static void MigrateDictionaries()
    {
        MigrateTechnologicalCards();
        MigrateStaff();
        MigrateComponents();
        MigrateTools();
        MigrateMachines();
        MigrateProtections();

        //MigrateTechnologicalProcesses();
        MigrateTechOperations();
        MigrateTechTransitions();

    }
    static void MigrateTechnologicalCards()
    {
        var oldTCs = _oldDbContext.TechnologicalCards.ToList();

        // преобразовать сущности из oldCore в newCore
        var newTCs = oldTCs.Select(tc =>
        {
            var newTC = new newCore.TechnologicalCard
            {
                Id = tc.Id,
                Article = tc.Article,
                Name = tc.Name,
                Description = tc.Description,
                Version = tc.Version,

                Type = tc.Type,
                NetworkVoltage = tc.NetworkVoltage,
                TechnologicalProcessType = tc.TechnologicalProcessType,
                TechnologicalProcessName = tc.TechnologicalProcessName,
                TechnologicalProcessNumber = tc.TechnologicalProcessNumber,
                Parameter = tc.Parameter,
                FinalProduct = tc.FinalProduct,
                Applicability = tc.Applicability,
                Note = tc.Note,
                DamageType = tc.DamageType,
                RepairType = tc.RepairType,
                IsCompleted = tc.IsCompleted,
                ExecutionScheme = tc.ExecutionScheme,
                Status = (newCore.TechnologicalCard.TechnologicalCardStatus)tc.Status,

            };

            return newTC;
        }).ToList();

        _myDbContext.TechnologicalCards.AddRange(newTCs);
        //_myDbContext.SaveChanges();
    }
    static void MigrateStaff()
    {
        var oldStaff = _oldDbContext.Staffs
            .Include(x => x.RelatedStaffs)
            //.Include(x => x.Li)
            .ToList();

        var newStaff = oldStaff.Select(staff =>
        {
            var newStaff = new newCore.TcContent.Staff
            {
                Id = staff.Id,
                Name = staff.Name,
                Type = staff.Type,
                Functions = staff.Functions,
                CombineResponsibility = staff.CombineResponsibility,
                Qualification = staff.Qualification,
                Comment = staff.Comment,
                IsReleased = staff.IsReleased,
                CreatedTCId = staff.CreatedTCId,
                OriginalId = staff.OriginalId,
                Version = staff.Version,
                UpdateDate = staff.UpdateDate,
                ClassifierCode = staff.ClassifierCode,
            };

            return newStaff;
        }).ToList();

        foreach (var staff in oldStaff)
        {
            if (staff.RelatedStaffs.Count > 0)
            {

                foreach (var relatedStaff in staff.RelatedStaffs)
                {
                    var newRelatedStaff = newStaff.FirstOrDefault(x => x.Id == relatedStaff.Id);
                    if (newRelatedStaff != null)
                    {
                        newStaff.FirstOrDefault(x => x.Id == staff.Id)?.RelatedStaffs.Add(newRelatedStaff);
                    }
                }
            }
        }

        _myDbContext.Staffs.AddRange(newStaff);
        //_myDbContext.SaveChanges();
    }
    static void MigrateComponents()
    {
        var oldComponents = _oldDbContext.Components
            .Include(x => x.Links)
            .ToList();
        var newComponents = oldComponents.Select(component =>
        {
            var newComponent = new newCore.TcContent.Component
            {
                Id = component.Id,
                Name = component.Name,
                Type = component.Type,
                Unit = component.Unit,
                Price = component.Price,
                Description = component.Description,
                Manufacturer = component.Manufacturer,
                Categoty = component.Categoty,
                ClassifierCode = component.ClassifierCode,
                IsReleased = component.IsReleased,
                CreatedTCId = component.CreatedTCId,
                Image = component.Image,
            };

            return newComponent;
        }).ToList();

        CopyLinks(oldComponents,
                  newComponents);

        _myDbContext.Components.AddRange(newComponents);
        //_myDbContext.SaveChanges();
    }
    static void CopyLinks<T,C>(List<T> oldComponents, List<C> newComponents) where T : IModelStructure
        where C : IModelStructure
    {
        foreach (var component in oldComponents)
        {
            if (component.Links.Count > 0)
            {
                foreach (var link in component.Links)
                {
                    var newLink = new TcModels.Models.IntermediateTables.LinkEntety
                    {
                        Id = link.Id,
                        Link = link.Link,
                        Name = link.Name,
                        IsDefault = link.IsDefault,
                    };
                    newComponents.FirstOrDefault(x => x.Equals(component))?.Links.Add(newLink);
                }
            }
        }
    }
    static void MigrateTools()
    {
        var oldTools = _oldDbContext.Tools
            .Include(x => x.Links)
            .ToList();

        var newTools = oldTools.Select(tool =>
        {
            var newTool = new newCore.TcContent.Tool
            {
                Id = tool.Id,
                Name = tool.Name,
                Type = tool.Type,
                Unit = tool.Unit,
                Price = tool.Price,
                Description = tool.Description,
                Manufacturer = tool.Manufacturer,
                Categoty = tool.Categoty,
                ClassifierCode = tool.ClassifierCode,
                IsReleased = tool.IsReleased,
                CreatedTCId = tool.CreatedTCId,
            };

            return newTool;
        }).ToList();

        CopyLinks(oldTools,
                  newTools);

        _myDbContext.Tools.AddRange(newTools);
        //_myDbContext.SaveChanges();
    }
    static void MigrateMachines()
    {
        var oldMachines = _oldDbContext.Machines
            .Include(x => x.Links)
            .ToList();
        var newMachines = oldMachines.Select(machine =>
        {
            var newMachine = new newCore.TcContent.Machine
            {
                Id = machine.Id,
                Name = machine.Name,
                Type = machine.Type,
                Unit = machine.Unit,
                Price = machine.Price,
                Description = machine.Description,
                Manufacturer = machine.Manufacturer,
                ClassifierCode = machine.ClassifierCode,
                IsReleased = machine.IsReleased,
                CreatedTCId = machine.CreatedTCId,
            };

            return newMachine;
        }).ToList();

        CopyLinks(oldMachines,
                  newMachines);
        _myDbContext.Machines.AddRange(newMachines);
        //_myDbContext.SaveChanges();
    }
    static void MigrateProtections()
    {
        var oldProtections = _oldDbContext.Protections
            .Include(x => x.Links)
            .ToList();
        var newProtections = oldProtections.Select(protection =>
        {
            var newProtection = new newCore.TcContent.Protection
            {
                Id = protection.Id,
                Name = protection.Name,
                Type = protection.Type,
                Unit = protection.Unit,
                Price = protection.Price,
                Description = protection.Description,
                Manufacturer = protection.Manufacturer,
                ClassifierCode = protection.ClassifierCode,
                IsReleased = protection.IsReleased,
                CreatedTCId = protection.CreatedTCId,
            };

            return newProtection;
        }).ToList();

        CopyLinks(oldProtections,
                  newProtections);

        _myDbContext.Protections.AddRange(newProtections);
        //_myDbContext.SaveChanges();
    }

    static void MigrateTechTransitions()
    {
        var oldTechTransitions = _oldDbContext.TechTransitions.ToList();

        var newTechTransitions = oldTechTransitions.Select(techTransition =>
        {
            var newTechTransition = new newCore.TcContent.TechTransition
            {
                Id = techTransition.Id,
                Name = techTransition.Name,
                TimeExecution = techTransition.TimeExecution,

                Category = techTransition.Category,
                TimeExecutionChecked = techTransition.TimeExecutionChecked,
                CommentName = techTransition.CommentName,
                CommentTimeExecution = techTransition.CommentTimeExecution,

                IsReleased = techTransition.IsReleased,
                CreatedTCId = techTransition.CreatedTCId,
            };

            return newTechTransition;
        }).ToList();

        _myDbContext.TechTransitions.AddRange(newTechTransitions);
        //_myDbContext.SaveChanges();
    }
    static void MigrateTechOperations()
    {
        var oldTechOperations = _oldDbContext.TechOperations
            .Include(x => x.techTransitionTypicals)
            .ToList();

        var newTechOperations = oldTechOperations.Select(techOperation =>
        {
            var newTechOperation = new newCore.TcContent.TechOperation
            {
                Id = techOperation.Id,
                Name = techOperation.Name,

                Category = techOperation.Category,

                IsReleased = techOperation.IsReleased,
                CreatedTCId = techOperation.CreatedTCId,

                
            };

            return newTechOperation;
        }).ToList();

        foreach(var techOperation in oldTechOperations)
        {
            if (techOperation.techTransitionTypicals.Count > 0)
            {
                foreach (var techTransitionTypical in techOperation.techTransitionTypicals)
                {
                    var newTechTransitionTypical = new newCore.TcContent.Work.TechTransitionTypical
                    {
                        Id = techTransitionTypical.Id,
                        TechOperationId = techTransitionTypical.TechOperationId,
                        TechTransitionId = techTransitionTypical.TechTransitionId,
                        
                        Etap = techTransitionTypical.Etap,
                        Posled = techTransitionTypical.Posled,

                        Coefficient = null,//techTransitionTypical.Coefficient,
                        Comments = null, //techTransitionTypical.Comments,
                    };
                    newTechOperations.FirstOrDefault(x => x.Id == techOperation.Id)?.techTransitionTypicals.Add(newTechTransitionTypical);
                }
            }
        }


        _myDbContext.TechOperations.AddRange(newTechOperations);
        
    }

    #endregion

    #region IntermediateTables

    static void MigrateIntermediateTables()
    {
        MigrateProtectionTC();
        MigrateStaffTC();
        MigrateComponentTC();
        MigrateToolTC();
        MigrateMachineTC();
    }

    static void MigrateStaffTC()
    {
        var oldStaffTCs = _oldDbContext.Staff_TCs.ToList();

        var newStaffTCs = oldStaffTCs.Select(staffTC =>
        {
            var newStaffTC = new newCore.IntermediateTables.Staff_TC
            {
                IdAuto = staffTC.IdAuto,
                ChildId = staffTC.ChildId,
                ParentId = staffTC.ParentId,
                Order = staffTC.Order,
                Symbol = staffTC.Symbol,

            };

            return newStaffTC;
        }).ToList();

        _myDbContext.Staff_TCs.AddRange(newStaffTCs);
    }

    static void MigrateComponentTC()
    {
        var oldComponentTCs = _oldDbContext.Component_TCs.ToList();

        var newComponentTCs = oldComponentTCs.Select(componentTC =>
        {
            var newComponentTC = new newCore.IntermediateTables.Component_TC
            {
                ChildId = componentTC.ChildId,
                ParentId = componentTC.ParentId,
                Order = componentTC.Order,
                Quantity = componentTC.Quantity,
                Note = componentTC.Note,
            };

            return newComponentTC;
        }).ToList();

        _myDbContext.Component_TCs.AddRange(newComponentTCs);
    }

    static void MigrateToolTC()
    {
        var oldToolTCs = _oldDbContext.Tool_TCs.ToList();

        var newToolTCs = oldToolTCs.Select(toolTC =>
        {
            var newToolTC = new newCore.IntermediateTables.Tool_TC
            {
                ChildId = toolTC.ChildId,
                ParentId = toolTC.ParentId,
                Order = toolTC.Order,
                Quantity = toolTC.Quantity,
                Note = toolTC.Note,
            };

            return newToolTC;
        }).ToList();

        _myDbContext.Tool_TCs.AddRange(newToolTCs);
    }

    static void MigrateMachineTC()
    {
        var oldMachineTCs = _oldDbContext.Machine_TCs.ToList();

        var newMachineTCs = oldMachineTCs.Select(machineTC =>
        {
            var newMachineTC = new newCore.IntermediateTables.Machine_TC
            {
                ChildId = machineTC.ChildId,
                ParentId = machineTC.ParentId,
                Order = machineTC.Order,
                Quantity = machineTC.Quantity,
                Note = machineTC.Note,
            };

            return newMachineTC;
        }).ToList();

        _myDbContext.Machine_TCs.AddRange(newMachineTCs);
    }

    static void MigrateProtectionTC()
    {
        var oldProtectionTCs = _oldDbContext.Protection_TCs.ToList();

        var newProtectionTCs = oldProtectionTCs.Select(protectionTC =>
        {
            var newProtectionTC = new newCore.IntermediateTables.Protection_TC
            {
                ChildId = protectionTC.ChildId,
                ParentId = protectionTC.ParentId,
                Order = protectionTC.Order,
                Quantity = protectionTC.Quantity,
                Note = protectionTC.Note,
            };

            return newProtectionTC;
        }).ToList();

        _myDbContext.Protection_TCs.AddRange(newProtectionTCs);
    }

    #endregion

    #region WorkTables

    static void MigrateWorkTables()
    {
        MigrateTechOperationWork();
        MigrateToolWork();
        MigrateComponentWork();

        MigrateExecutionWork();
    }

    static void MigrateTechOperationWork()
    {
        var oldTechOperationWorks = _oldDbContext.TechOperationWorks.ToList();

        var newTechOperationWorks = oldTechOperationWorks.Select(techOperationWork =>
        {
            var newTechOperationWork = new newCore.TcContent.TechOperationWork
            {
                Id = techOperationWork.Id,
                techOperationId = techOperationWork.techOperationId,
                TechnologicalCardId = techOperationWork.TechnologicalCardId,
                Order = techOperationWork.Order,
                Delete = techOperationWork.Delete,
                NewItem = techOperationWork.NewItem,
            };

            return newTechOperationWork;
        }).ToList();

        _myDbContext.TechOperationWorks.AddRange(newTechOperationWorks);
    }

    static void MigrateToolWork()
    {
        var oldToolWorks = _oldDbContext.ToolWorks.ToList();

        var newToolWorks = oldToolWorks.Select(toolWork =>
        {
            var newToolWork = new newCore.TcContent.ToolWork
            {
                Id = toolWork.Id,
                techOperationWorkId = toolWork.techOperationWorkId,
                toolId = toolWork.toolId,
                Quantity = toolWork.Quantity,
                Comments = toolWork.Comments,
            };

            return newToolWork;
        }).ToList();

        _myDbContext.ToolWorks.AddRange(newToolWorks);
    }
    static void MigrateComponentWork()
    {
        var oldComponentWorks = _oldDbContext.ComponentWorks.ToList();

        var newComponentWorks = oldComponentWorks.Select(componentWork =>
        {
            var newComponentWork = new newCore.TcContent.ComponentWork
            {
                Id = componentWork.Id,
                techOperationWorkId = componentWork.techOperationWorkId,
                componentId = componentWork.componentId,
                Quantity = componentWork.Quantity,
                Comments = componentWork.Comments,
            };

            return newComponentWork;
        }).ToList();

        _myDbContext.ComponentWorks.AddRange(newComponentWorks);
    }

    static void MigrateExecutionWork()
    {
        var oldExecutionWorks = _oldDbContext.ExecutionWorks
            .Include(x => x.Staffs)
            .Include(x => x.Protections)
            .Include(x => x.Machines)
            .Include(x => x.ListexecutionWorkRepeat2)
            .ToList();

        var newExecutionWorks = oldExecutionWorks.Select(executionWork =>
        {
            var newExecutionWork = new newCore.TcContent.ExecutionWork
            {
                Id = executionWork.Id,
                techOperationWorkId = executionWork.techOperationWorkId,
                techTransitionId = executionWork.techTransitionId,
                Repeat = executionWork.Repeat,
                sumEw = executionWork.sumEw,
                maxEw = executionWork.maxEw,
                Coefficient = executionWork.Coefficient,
                Value = executionWork.Value,
                Comments = executionWork.Comments,
                NewItem = executionWork.NewItem,
                Delete = executionWork.Delete,
                IdGuid = executionWork.IdGuid,
                Order = executionWork.Order,
                Etap = executionWork.Etap,
                Posled = executionWork.Posled,
                TempTimeExecution = executionWork.TempTimeExecution,
                Vopros = executionWork.Vopros,
                Otvet = executionWork.Otvet,
                //PictureName = executionWork.PictureName,
            };

            return newExecutionWork;
        }).ToList();

        foreach (var ew in oldExecutionWorks)
        {
            var newExecutionWork = newExecutionWorks.FirstOrDefault(x => x.Id == ew.Id);
            if (newExecutionWork != null)
            {
                if (ew.Staffs.Count > 0)
                {
                    foreach (var stafftc in ew.Staffs)
                    {
                        var newStaff = _myDbContext.Staff_TCs.FirstOrDefault(x => x.IdAuto == stafftc.IdAuto);
                        if (newStaff != null)
                        {
                            newExecutionWork.Staffs.Add(newStaff);
                        }
                        else
                        {
                            throw new Exception($"Staff_TC {stafftc.IdAuto} not found");
                        }
                    }
                }
                if (ew.Protections.Count > 0)
                {
                    foreach (var protectiontc in ew.Protections)
                    {
                        var newProtection = _myDbContext.Protection_TCs.FirstOrDefault(x => x.ChildId == protectiontc.ChildId && x.ParentId == protectiontc.ParentId && x.Order == protectiontc.Order);
                        if (newProtection != null)
                        {
                            newExecutionWork.Protections.Add(newProtection);
                        }
                        else
                        {
                            throw new Exception($"Protection_TC {protectiontc.Order}.{protectiontc.ChildId} - {protectiontc.ParentId} not found");
                        }
                    }
                }
                if (ew.Machines.Count > 0)
                {
                    foreach (var machinetc in ew.Machines)
                    {
                        var newMachine = _myDbContext.Machine_TCs.FirstOrDefault(x => x.ChildId == machinetc.ChildId && x.ParentId == machinetc.ParentId && x.Order == machinetc.Order);
                        if (newMachine != null)
                        {
                            newExecutionWork.Machines.Add(newMachine);
                        }
                        else
                        {
                            throw new Exception($"Machine_TC {machinetc.Order}.{machinetc.ChildId} - {machinetc.ParentId} not found");
                        }
                    }
                }

                if (ew.ListexecutionWorkRepeat2.Count > 0)
                {
                    foreach (var ewRepeat in ew.ListexecutionWorkRepeat2)
                    {
                        var newEwRepeat = newExecutionWorks.FirstOrDefault(x => x.Id == ewRepeat.Id);
                        if (newEwRepeat != null)
                        {
                            var newExecutionWorkRepeat = new newCore.TcContent.ExecutionWorkRepeat
                            {
                                ParentExecutionWorkId = ew.Id,
                                ChildExecutionWorkId = ewRepeat.Id,
                            };
                            newExecutionWork.ExecutionWorkRepeats.Add(newExecutionWorkRepeat);
                        }
                        else
                        {
                            throw new Exception($"ExecutionWork {ewRepeat.Id} not found");
                        }
                    }
                }

            }
        }

        _myDbContext.ExecutionWorks.AddRange(newExecutionWorks);
    }

    #region Diagrams

    static void MigrateDiagrams()
    {
        MigrateDiagramToWork();
        MigrateDiagramParalelno();
        MigrateDiagramPosledov();
        MigrateDiagramShag();
        MigrateDiagramShagToolsComponent();
    }
    static void MigrateDiagramToWork()
    {
        var oldDiagramToWorks = _oldDbContext.DiagamToWork.ToList();

        var newDiagramToWorks = oldDiagramToWorks.Select(diagramToWork =>
        {
            var newDiagramToWork = new newCore.TcContent.DiagamToWork
            {
                Id = diagramToWork.Id,
                techOperationWorkId = diagramToWork.techOperationWorkId,
                technologicalCardId = diagramToWork.technologicalCardId,
                Order = diagramToWork.Order,
            };

            return newDiagramToWork;
        }).ToList();

        _myDbContext.DiagamToWork.AddRange(newDiagramToWorks);
    }
    static void MigrateDiagramParalelno()
    {
        var oldDiagramToWorks = _oldDbContext.DiagramParalelno.ToList();

        var newDiagramToWorks = oldDiagramToWorks.Select(diagramToWork =>
        {
            var newDiagramToWork = new newCore.TcContent.DiagramParalelno
            {
                Id = diagramToWork.Id,
                techOperationWorkId = diagramToWork.techOperationWorkId,
                DiagamToWorkId = diagramToWork.DiagamToWorkId,
                Order = diagramToWork.Order,
            };

            return newDiagramToWork;
        }).ToList();

        _myDbContext.DiagramParalelno.AddRange(newDiagramToWorks);
    }
    static void MigrateDiagramPosledov()
    {
        var oldDiagramPosledovs = _oldDbContext.DiagramPosledov.ToList();

        var newDiagramPosledovs = oldDiagramPosledovs.Select(diagramPosledov =>
        {
            var newDiagramPosledov = new newCore.TcContent.DiagramPosledov
            {
                Id = diagramPosledov.Id,
                DiagramParalelnoId = diagramPosledov.DiagramParalelnoId,
                Order = diagramPosledov.Order,
            };

            return newDiagramPosledov;
        }).ToList();

        _myDbContext.DiagramPosledov.AddRange(newDiagramPosledovs);
    }
    static void MigrateDiagramShag()
    {
        var oldDiagramShags = _oldDbContext.DiagramShag.ToList();

        var newDiagramShags = oldDiagramShags.Select(diagramShag =>
        {
            var newDiagramShag = new newCore.TcContent.DiagramShag
            {
                Id = diagramShag.Id,
                Deystavie = diagramShag.Deystavie,
                // ImageBase64 = diagramShag.ImageBase64,
                NameImage = diagramShag.NameImage,
                Nomer = diagramShag.Nomer,
                DiagramPosledovId = diagramShag.DiagramPosledovId,
                Order = diagramShag.Order,
            };

            return newDiagramShag;
        }).ToList();

        _myDbContext.DiagramShag.AddRange(newDiagramShags);
    }
    static void MigrateDiagramShagToolsComponent()
    {
        var oldDiagramShagToolsComponents = _oldDbContext.DiagramShagToolsComponent.ToList();

        var newDiagramShagToolsComponents = oldDiagramShagToolsComponents.Select(diagramShagToolsComponent =>
        {
            var newDiagramShagToolsComponent = new newCore.TcContent.DiagramShagToolsComponent
            {
                Id = diagramShagToolsComponent.Id,
                toolWorkId = diagramShagToolsComponent.toolWorkId,
                componentWorkId = diagramShagToolsComponent.componentWorkId,

                Quantity = diagramShagToolsComponent.Quantity,
                DiagramShagId = diagramShagToolsComponent.DiagramShagId,
            };

            return newDiagramShagToolsComponent;
        }).ToList();

        _myDbContext.DiagramShagToolsComponent.AddRange(newDiagramShagToolsComponents);
    }

    static void MigrateDiagramShagPictures(int coefficient, int step)
    {
        var oldDiagramShags = _oldDbContext.DiagramShag.Skip(step * coefficient).Take(step).ToList();

        var newDiagramShags = _myDbContext.DiagramShag.ToList();

        foreach (var diagramShag in oldDiagramShags)
        {
            var newDiagramShag = newDiagramShags.FirstOrDefault(x => x.Id == diagramShag.Id);
            if (newDiagramShag != null)
            {
                newDiagramShag.ImageBase64 = diagramShag.ImageBase64;

                //// Декодируем base64 строку в массив байтов
                //byte[] imageBytes = Convert.FromBase64String(diagramShag.ImageBase64);

                //// Получаем вес изображения в байтах
                //int weightInBytes = imageBytes.Length;

                //Console.WriteLine($"Шаг: {diagramShag.Id}Вес изображения: {weightInBytes} байт");
            }
        }
    }

    #endregion

    #endregion

}
