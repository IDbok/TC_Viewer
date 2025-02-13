using static TcModels.Models.TcContent.Outlay;
using static TcModels.Models.TechnologicalCard;

namespace TC_WinForms.WinForms.Win7.Work
{
    public class SummaryOutlayDataGridItem
    {
        public int TcId { get; set; }
        public string TcName { get; set; }
        public string TechProcessType { get; set; }
        public string Parameter { get; set; }

        public List<(string StaffName, double StaffOutlay)> listStaffStr = new List<(string StaffName, double StaffOutlay)>();
        public List<(string MachineName, double MachineOutlay, int MachineId)> listMachStr = new List<(string MachineName, double MachineOutlay, int MachineId)>();
        public double ComponentOutlay { get; set; }
        public double SummaryOutlay { get; set; }
        public TechnologicalCardUnit UnitType { get; set; }
        public double SummaryOutlayCost { get; set; }

        //ТК имя
        //Стафф
        //Компоненты
        //Механизмы
        //Суммарное

        //Todo: добавить форомульные колонки
        //Ниже формульное
        //Ед изм затрат
        //Затраты
    }
}