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

namespace TC_WinForms.WinForms.Diagram
{
    public partial class DiagramForm : Form
    {
        private int tcId;
        private bool isViewMode;

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


            WpfMainControl wpfDiagram = new WpfMainControl(tcId);
                        
            elementHost.Child = wpfDiagram;


        }
    }
}
