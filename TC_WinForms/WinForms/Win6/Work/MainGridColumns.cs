using TC_WinForms.WinForms.Win6.Healpers;

namespace TC_WinForms.WinForms.Win6.Work
{
    internal sealed class MainGridColumns
    {
        public DataGridViewColumn ExecutionWorkItem { get; init; }
        public DataGridViewColumn Order { get; init; }
        public DataGridViewColumn TechOperationName { get; init; }   // "Технологические операции"
        public DataGridViewColumn Staff { get; init; }
        public DataGridViewColumn TechTransitionName { get; init; }  // "Технологические переходы"
        public DataGridViewColumn TimeValue { get; init; }           // "Время действ., мин."
        public DataGridViewColumn EtapValue { get; init; }           // "Время этапа, мин."
        public DataGridViewColumn Protection { get; init; }
        public DataGridViewColumn Comment { get; init; }
        public DataGridViewColumn PictureName { get; init; }
        public DataGridViewColumn Remark { get; init; }
        public DataGridViewColumn Response { get; init; }

        public static MainGridColumns Capture(DataGridView g) => new()
        {
            ExecutionWorkItem = g.Columns["ExecutionWorkItem"],
            Order = g.Columns["Order"],
            TechOperationName = g.Columns["TechOperationName"],
            Staff = g.Columns["Staff"],
            TechTransitionName = g.Columns["TechTransitionName"],
            TimeValue = g.Columns["TimeValue"],
            EtapValue = g.Columns["EtapValue"],
            Protection = g.Columns["Protection"],
            Comment = g.Columns["CommentColumn"],
            PictureName = g.Columns["PictureNameColumn"],
            Remark = g.Columns["RemarkColumn"],
            Response = g.Columns["ResponseColumn"],
        };

    }
}
