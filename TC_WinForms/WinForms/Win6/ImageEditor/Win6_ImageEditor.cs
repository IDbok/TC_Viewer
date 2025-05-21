using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TC_WinForms.WinForms.Win6.Models;
using TC_WinForms.WinForms.Win6.RoadMap;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms.Win6.ImageEditor
{
    public partial class Win6_ImageEditor : Form
    {
        private ElementHost elementHost = new();
        bool isWindowEditor = true;
        private ImageOptionsControl? _imageControl;
        private IImageHoldable? imageHolder;
        private TechnologicalCard tc;
        private MyDbContext context;
        public delegate Task PostSaveAction<IModel>(IModel modelObject) where IModel : IImageHoldable;
        public PostSaveAction<IImageHoldable?>? AfterSave { get; set; }
        public Win6_ImageEditor(IImageHoldable? imageHolder, TechnologicalCard tc, MyDbContext context, bool isWindowEditor = true)
        {
            InitializeComponent();
            this.imageHolder = imageHolder;
            this.tc = tc;
            this.context = context;
            this.isWindowEditor = isWindowEditor;
            Load += Win6_ImageEditor_Load;
            FormClosing += Win6_ImageEditor_FormClosing;
        }

        private void Win6_ImageEditor_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (AfterSave != null)
            {
                AfterSave(imageHolder);
            }
        }

        private void Win6_ImageEditor_Load(object? sender, EventArgs e)
        {
            _imageControl = new ImageOptionsControl(tc, context, imageHolder, isWindowEditor);
            InitializeWpfControl();
        }

        private void InitializeWpfControl()
        {
            InitializeElementHost();

            // Помещаем WPF-контрол в ElementHost
            elementHost.Child = _imageControl;
        }

        private void InitializeElementHost()
        {
            elementHost = new ElementHost();
            elementHost.Dock = DockStyle.Fill;

            this.Controls.Add(elementHost);
        }
    }
}
