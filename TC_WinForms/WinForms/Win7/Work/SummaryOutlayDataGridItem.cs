using static TcModels.Models.TcContent.Outlay;
using static TcModels.Models.TechnologicalCard;

namespace TC_WinForms.WinForms.Win7.Work
{
    public class SummaryOutlayDataGridItem
    {
        public int TcId { get; set; }
        public string TcName { get; set; }
        public string TechProcess { get; set; }
        public string Parameter { get; set; }

        public List<SummaryOutlayStaff> listStaffStr = new List<SummaryOutlayStaff>();
        public List<SummaryOutlayMachine> listMachStr = new List<SummaryOutlayMachine>();
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

    public class SummaryOutlayStaff
    {
        public string StaffName { get; set; }
        public double StaffOutlay { get; set; }
        public int StaffId { get; set; }
        public double? StaffCost { get; set; }
    }

    public class SummaryOutlayMachine
    {
        public string MachineName {  get; set; }
        public double MachineOutlay {  get; set; }
        public int MachineId {  get; set; }
        public double? MachineCost { get; set; }
    }
}