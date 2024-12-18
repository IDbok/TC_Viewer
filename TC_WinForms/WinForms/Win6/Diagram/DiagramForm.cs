using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms.Diagram
{
	public partial class DiagramForm : Form, ISaveEventForm, IViewModeable, IFormWithObjectId
    {
        private readonly TcViewState _tcViewState;

        private int tcId;
        private bool isViewMode;

        public WpfMainControl wpfDiagram;

        public DiagramForm()
        {
            InitializeComponent();
        }

        public DiagramForm(int tcId, TcViewState tcViewState, MyDbContext context)// bool isViewMode)
        {
            InitializeComponent();

            _tcViewState = tcViewState;

            this.tcId = tcId;
            //this.isViewMode = isViewMode;

            ElementHost elementHost = new ElementHost();
            elementHost.Dock = DockStyle.Fill;

            this.Controls.Add(elementHost);


            WpfMainControl wpfDiagram = new WpfMainControl(tcId, this, _tcViewState, context);
            this.wpfDiagram = wpfDiagram;
            elementHost.Child = wpfDiagram;

            //this.KeyPreview = true;

            //this.KeyDown += DiagramForm_KeyDown;

            //wpfDiagram.KeyDown += WpfDiagram_KeyDown;

            //AddButton();

            this.FormClosed += (sender, elementHost) => this.Dispose();

        }

        public void UpdateVisualData() 
        {
            wpfDiagram.ReinitializeForm();
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
        private void DiagramForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            wpfDiagram.SaveOnDispose();
        }

        public int GetObjectId()
        {
            return tcId;
        }
    }
}
