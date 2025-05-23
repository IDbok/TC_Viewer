using System;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms.Win6.ImageEditor
{
    public partial class Win6_ImageEditor : Form
    {
        #region Поля

        private ElementHost elementHost = new();
        private ImageOptionsControl? _imageControl;
        private IImageHoldable? imageHolder;
        private TechnologicalCard tc;
        private MyDbContext context;
        private bool isWindowEditor = true;

        #endregion

        #region Делегаты

        public delegate Task PostSaveAction<IModel>(IModel modelObject) where IModel : IImageHoldable;
        public PostSaveAction<IImageHoldable?>? AfterSave { get; set; }

        #endregion

        #region Конструктор

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

        #endregion

        #region Обработчики событий формы

        private void Win6_ImageEditor_Load(object? sender, EventArgs e)
        {
            _imageControl = new ImageOptionsControl(tc, context, imageHolder, isWindowEditor);
            InitializeWpfControl();
        }

        private void Win6_ImageEditor_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (AfterSave != null)
            {
                AfterSave(imageHolder);
            }
        }

        #endregion

        #region Инициализация WPF-контрола

        private void InitializeWpfControl()
        {
            InitializeElementHost();
            elementHost.Child = _imageControl;
        }

        private void InitializeElementHost()
        {
            elementHost = new ElementHost
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(elementHost);
        }

        #endregion
    }
}
