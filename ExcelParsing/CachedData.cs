using System.Collections.Generic;
using TcModels.Models.TcContent;

namespace ExcelParsing;

public class CachedData
{
    public readonly List<Staff> Staffs = new List<Staff>();
    public readonly List<Component> Components = new List<Component>();
    public readonly List<Tool> Tools = new List<Tool>();
    public readonly List<Machine> Machines = new List<Machine>();
    public readonly List<Protection> Protections = new List<Protection>();

    public readonly List<TechOperation> TechOperations = new List<TechOperation>();
    public readonly List<TechTransition> TechTransitions = new List<TechTransition>();

    public CachedData(List<Staff> staffs, List<Component> components, List<Tool> tools, List<Machine> machines, List<Protection> protections, List<TechOperation> techOperations, List<TechTransition> techTransitions)
    {
        Staffs = staffs;
        Components = components;
        Tools = tools;
        Machines = machines;
        Protections = protections;
        TechOperations = techOperations;
        TechTransitions = techTransitions;
    }
}
