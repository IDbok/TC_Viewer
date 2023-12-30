using System.ComponentModel.DataAnnotations.Schema;
using TcModels.Models.IntermediateTables;

namespace TcModels.Models.TcContent
{
    public class WorkStep : IIdentifiable
    {

        static private EModelType modelType = EModelType.WorkStep;
        static public EModelType ModelType { get { return modelType; } }
        int id;
        [NotMapped]
        static Dictionary<int, float> stagesDic = new Dictionary<int, float>();
        int num { get; set; }
        string description { get; set; }
        string? staff { get; set; }
        float stepExecutionTime { get; set; }
        int stage { get; set; }
        float stageExecutionTime { get; set; }
        float machineExecutionTime { get; set; }
        string? protections { get; set; }
        string? comments { get; set; }

        //public TechnologicalCard? TechnologicalCard { get; set; }
        //public int? TechnologicalCardId { get; set; }
        //public Operation Operation { get; set; } = new();

        //public List<TechnologicalCard> TechnologicalCards { get; set; } = new();
        public WorkStep()
        {
        }
        public WorkStep(int num, string description, string? staff, float stepExecutionTime, int stage, float stageExecutionTime,
            float machineExecutionTime, string? protections, string? comments)
        {
            this.num = num;
            this.description = description;
            this.staff = staff;
            this.stepExecutionTime = stepExecutionTime;
            this.stage = stage;
            // TODO: добавить этапы и операции
            this.stageExecutionTime = stageExecutionTime;
            this.machineExecutionTime = machineExecutionTime;
            this.protections = protections;
            this.comments = comments;
        }
        public int Id { get { return id; } set { id = value; } }
        public string Name { get; set; }
        public int Num { get { return num; } set { num = value; } }
        public int Order { get; set; }
        public string Description { get { return description; } set { description = value; } }

        public List<Staff_TC> Staff_TCs { get; set; } = new();
        public List<Component_TC> Component_TCs { get; set; } = new();
        public List<Machine_TC> Machine_TCs { get; set; } = new();
        public List<Protection_TC> Protection_TCs { get; set; } = new();
        public List<Tool_TC> Tool_TCs { get; set; } = new();

        public float StepExecutionTime { get { return stepExecutionTime; } set { stepExecutionTime = value; } }
        public int Stage { get { return stage; } set { stage = value; } }
        public float StageExecutionTime { get { return stageExecutionTime; } set { stageExecutionTime = value; } }
        public float MachineExecutionTime { get { return machineExecutionTime; } set { machineExecutionTime = value; } }
        public string? Protections { get { return protections; } set { protections = value; } }
        public string? Comments { get { return comments; } set { comments = value; } }

        public static int GetLastStageNum() => !stagesDic.Any() ? 0 : stagesDic.Last().Key;
        public static float GetLastStageTime() => stagesDic.Last().Value;
        public static void AddStage(int stageNum, float stageTime) => stagesDic.Add(stageNum, stageTime);


    }
}
