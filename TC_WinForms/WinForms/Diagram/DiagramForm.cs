using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TC_WinForms.Interfaces;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms.Diagram
{
    public partial class DiagramForm : Form, ISaveEventForm, IViewModeable
    {
        private int tcId;
        private bool isViewMode;

       public WpfMainControl wpfDiagram;

        public DiagramForm()
        {
            InitializeComponent();
        }

        public DiagramForm(int tcId, bool isViewMode)
        {
            InitializeComponent();

            this.tcId = tcId;
            this.isViewMode = isViewMode;

            ElementHost elementHost = new ElementHost();
            elementHost.Dock = DockStyle.Fill;

            this.Controls.Add(elementHost);


            WpfMainControl wpfDiagram = new WpfMainControl(tcId, this);
            this.wpfDiagram = wpfDiagram;
            elementHost.Child = wpfDiagram;


        }

        public bool HasChanges { get; set; }

        public bool CloseFormsNoSave { get; set; } = false;

        public bool GetDontSaveData()
        {
            return HasChanges;
        }

        public async Task SaveChanges()
        {
            wpfDiagram.Save();
        }


        public void SetViewMode(bool? isViewMode)
        {
           
        }
    }
}
