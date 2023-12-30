using System.ComponentModel.DataAnnotations.Schema;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TcModels.Models
{
    public class TechnologicalCard: IIdentifiable
    {
        public int Id { get; set; }
        public string Article { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Version { get; set; } = "0.0.0.0";
        public List<Author> Authors { get; set; } = new();
        public List<TechnologicalProcess> TechnologicalProcess { get; set; } = new();
        [NotMapped]
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        public List<Staff> Staffs { get; set; } = new ();
        public List<Staff_TC> Staff_TCs { get; set; } = new();
        public List<Component> Components { get; set; } = new();
        public List<Component_TC> Component_TCs { get; set; } = new();
        public List<Machine> Machines { get; set; } = new();
        public List<Machine_TC> Machine_TCs { get; set; } = new();
        public List<Protection> Protections { get; set; } = new();
        public List<Protection_TC> Protection_TCs { get; set; } = new();
        public List<Tool> Tools { get; set; } = new();
        public List<Tool_TC> Tool_TCs { get; set; } = new();

        //public int? WorkStepsId { get; set; }
        //public List<WorkStep> WorkSteps { get; set; } = new();

        public Staff_TC ConnectObject(Staff staff, int order, string symbol)
        {
            Staff_TC staff_tc = new Staff_TC { Parent = this, Child = staff, Order = order, Symbol = symbol };
            Staff_TCs.Add(new Staff_TC { Parent = this, Child = staff, Order = order, Symbol = symbol });
            Staffs.Add(staff);
            return staff_tc;
        }
        /// <summary>
        /// Add object to TC and connect it with TC
        /// </summary>
        /// <typeparam name="T">Intermediate class (Tool_TC, Machine_TC etc)</typeparam>
        /// <typeparam name="C">Child class (Toll, Machine etc)</typeparam>
        /// <param name="obj">Child class object</param>
        /// <param name="order"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public T ConnectObject<T, C>(C obj, int order, int quantity)
            where T : class, IStructIntermediateTable<TechnologicalCard, C>, new()
            where C : class, IModelStructure
        {
            T obj_tc= new T { Parent = this, Child = obj, Order = order, Quantity = quantity };

            if (typeof(T) == typeof(Component_TC))
            {
                Component_TCs.Add(obj_tc as Component_TC);
                Components.Add(obj as Component);
            }
            else if (typeof(T) == typeof(Tool_TC))
            {
                Tool_TCs.Add(obj_tc as Tool_TC);
                Tools.Add(obj as Tool);
            }
            else if (typeof(T) == typeof(Machine_TC))
            {
                Machine_TCs.Add(obj_tc as Machine_TC);
                Machines.Add(obj as Machine);
            }
            else if (typeof(T) == typeof(Protection_TC))
            {
                Protection_TCs.Add(obj_tc as Protection_TC);
                Protections.Add(obj as Protection);
            }

            //else if (typeof(T) == typeof(WorkStep_TC)) // todo - add WorkStep_TC
            //{
            //    WorkStep_TCs.Add(new WorkStep_TC { Parent = this, Child = obj, Order = order, Quantity = quantity });
            //    WorkSteps.Add(obj as WorkStep);
            //}
            return obj_tc;
        }

        public override string ToString()
        {
            return $"{Id}.{Article} {Name}";
        }
        
    }
}
