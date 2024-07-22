using System.Windows;
using System.Windows.Controls;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram
{
    /// <summary>
    /// Логика взаимодействия для WpfTo.xaml
    /// </summary>
    public partial class WpfTo : System.Windows.Controls.UserControl
    {
        private WpfMainControl _wpfMainControl;
        private string? parallelIndex;

        public WpfTo()
        {
            InitializeComponent();
        }
        public WpfTo(WpfMainControl wpfMainControl,
        DiagamToWork? _diagramToWork = null)
        {
            InitializeComponent();

            _wpfMainControl = wpfMainControl;

            ListTOParalelno.Children.Clear();

            if (_diagramToWork != null)
                AddWpfControlTO(wpfMainControl, _diagramToWork);
        }

        public void AddParallelTO(DiagamToWork? diagamToWork)
        {
            AddWpfControlTO(_wpfMainControl, diagamToWork);

            if (diagamToWork != null)
            {
                if (diagamToWork.ParallelIndex != null)
                    parallelIndex = diagamToWork.ParallelIndex;
                else if (ListTOParalelno.Children.Count > 0)
                {
                    if (parallelIndex == null)
                        parallelIndex = new Random(10).Next(10).ToString();

                    diagamToWork.ParallelIndex = parallelIndex;

                    _wpfMainControl.diagramForm.HasChanges = true;
                }

                _wpfMainControl.Nomeraciya();
            }
        }

        private void btnAddTOParallel_Click(object sender, RoutedEventArgs e)
        {
            AddWpfControlTO(_wpfMainControl);

            //wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
            //wpfControlTO.Nomeraciya();
        }
        private void AddWpfControlTO(WpfMainControl _wpfMainControl,
            DiagamToWork? _diagamToWork = null)
        {
            var wpfControlTO = new WpfControlTO(_wpfMainControl, _diagamToWork);
            wpfControlTO.VerticalAlignment = VerticalAlignment.Top;
            wpfControlTO.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            ListTOParalelno.Children.Add(wpfControlTO);
        }
    }
}
