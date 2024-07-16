using Microsoft.EntityFrameworkCore;

using oldCore = TcDbConnector.Migrations.OldCore.Models;
using newCore = TcModels.Models;
using TcModels.Models.Interfaces;

namespace TcDbConnector.Migrations;

public class DataMigration
{
    private static OldDbContext _oldDbContext = new();
    private static MyDbContext _myDbContext = new();
    
    public static void MigrateData(bool createNewDb)
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

                //MigrateTechnologicalProcesses();
                //MigrateTechOperations();
                //MigrateTechOperationWorks();
                //MigrateExecutionWorks();
                //MigrateProtectionTCs();
                //MigrateStaffTCs();
                //MigrateMachineTCs();
                //MigrateExecutionWorkRepeats();
                //MigrateDiagramParalelnos();
                //MigrateToolWorks();
                //MigrateComponentWorks();
                //MigrateTechTransitions();
                //MigrateConfigs();

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
}
