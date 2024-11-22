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
using TcDbConnector;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms.Diagram
{
    public partial class DiagramForm : Form, ISaveEventForm, IViewModeable
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
        //private void AddButton()
        //{
        //    Button newButton = new Button();
        //    newButton.Text = "Click Me";
        //    newButton.Location = new System.Drawing.Point(10, 10); // Set the location as per your requirement
        //    newButton.Click += new EventHandler(NewButton_Click);
        //    this.Controls.Add(newButton);
        //}
        //private void AddToolStrip()
        //{
        //    toolStrip = new ToolStrip();
        //    ToolStripButton newButton = new ToolStripButton("Click Me");
        //    newButton.Click += new EventHandler(NewButton_Click);
        //    toolStrip.Items.Add(newButton);
        //    toolStrip.Dock = DockStyle.Top; // Dock the ToolStrip at the top
        //    this.Controls.Add(toolStrip);
        //}
        //private void NewButton_Click(object sender, EventArgs e)
        //{
        //    MessageBox.Show("Button clicked!");
        //}

        //private void DiagramForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        //{
        //    // Check if the key combination Ctrl + Shift + A is pressed
        //    if (e.Control && e.Shift && e.KeyCode == Keys.A)
        //    {
        //        // Your code to handle the key combination
        //        MessageBox.Show("Ctrl + Shift + A was pressed!");
        //    }
        //}
        //private void WpfDiagram_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    // Create a new KeyEventArgs for the WinForms form
        //    var winFormsKeyEventArgs = new System.Windows.Forms.KeyEventArgs((Keys)KeyInterop.VirtualKeyFromKey(e.Key));
        //    DiagramForm_KeyDown(sender, winFormsKeyEventArgs);
        //}

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
    }
}
