using Serilog;
using SerilogTimings.Extensions;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms.Diagram
{
	public partial class DiagramForm : Form, ISaveEventForm, IViewModeable, IFormWithObjectId
    {
        private readonly ILogger _logger = Log.Logger.ForContext<DiagramForm>();
        private readonly TcViewState _tcViewState;

        private int tcId;
        private bool isViewMode;

        public WpfMainControl wpfDiagram;
        private ElementHost elementHost;

        private double verticalScrollPosition;
        private double horizontalScrollPosition;

        public DiagramForm()
        {
            InitializeComponent();
        }

        public DiagramForm(int tcId, TcViewState tcViewState, MyDbContext context)// bool isViewMode)
		{
			InitializeComponent();

			_tcViewState = tcViewState;

			this.tcId = tcId;
            using (_logger.TimeOperation("DiagramForm: InitializeElementHost"))
                InitializeElementHost();
            using (_logger.TimeOperation("DiagramForm: Create WpfMainControl"))
                AddElementToElementHost(new WpfMainControl(tcId, this, _tcViewState, context));

            this.Text = $"Блок схема {_tcViewState.TechnologicalCard.Name}({_tcViewState.TechnologicalCard.Article})";

            this.FormClosed += (sender, elementHost) => this.Dispose();

		}

        private void CleanElementHost()
		{
			elementHost.Child = null;
		}

        public void ReloadElementHost(WpfMainControl wpfDiagram)
        {
            using (_logger.TimeOperation("DiagramForm: ReloadElementHost"))
            {
                CleanElementHost();
                AddElementToElementHost(wpfDiagram);
            }
        }

		private void AddElementToElementHost(WpfMainControl wpfDiagram)
		{
			this.wpfDiagram = wpfDiagram;
			elementHost.Child = wpfDiagram;
		}

		private void InitializeElementHost()
		{
			elementHost = new ElementHost();
			elementHost.Dock = DockStyle.Fill;

			this.Controls.Add(elementHost);
		}

		public void UpdateVisualData() 
        {
            SaveScrollPosition(wpfDiagram.ZoomableScrollViewer);

            wpfDiagram.ReinitializeForm();

            RestoreScrollPosition(wpfDiagram.ZoomableScrollViewer);
        }

        public bool HasChanges { get; set; }

        public bool CloseFormsNoSave { get; set; } = false;

        public bool GetDontSaveData()
        {
            return HasChanges;
        }

        public async Task SaveChanges()
        {
            wpfDiagram.Save(saveContext: true);
        }


        public void SetViewMode(bool? isViewMode)
        {

        }
        private void DiagramForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            wpfDiagram.Save(saveContext: false);
        }

        public int GetObjectId()
        {
            return tcId;
        }

        private void SaveScrollPosition(ScrollViewer scrollViewer)
        {
            if (scrollViewer != null)
            {
                verticalScrollPosition = scrollViewer.VerticalOffset;
                horizontalScrollPosition = scrollViewer.HorizontalOffset;
            }
        }

        private void RestoreScrollPosition(ScrollViewer scrollViewer)
        {
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(verticalScrollPosition);
                scrollViewer.ScrollToHorizontalOffset(horizontalScrollPosition);
            }
        }
    }
}
