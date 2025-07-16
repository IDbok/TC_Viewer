using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms.Win6.ImageEditor
{
    public partial class Win6_ImageEditor : Form, IFormWithObjectId
    {
        #region Поля

        private List<ImageOwner> _originalImageList = new List<ImageOwner>();
        private ElementHost elementHost = new();
        private ImageOptionsControl? _imageControl;
        private IImageHoldable? imageHolder;
        private TechnologicalCard tc;
        private MyDbContext context;
        private bool isWindowEditor = true;
        private TcViewState tcViewState;
        public enum SaveResult
        {
            Save,
            DontSave,
            Cancel
        }

        public SaveResult UserChoice { get; private set; } = SaveResult.Save;
        #endregion

        #region Делегаты

        public delegate Task PostSaveAction<IModel>(IModel modelObject) where IModel : IImageHoldable;
        public PostSaveAction<IImageHoldable?>? AfterSave { get; set; }

        #endregion

        #region Конструктор

        public Win6_ImageEditor(IImageHoldable? imageHolder, TcViewState tcViewState, MyDbContext context, bool isWindowEditor = true)
        {
            InitializeComponent();
            this.tcViewState = tcViewState;
            this.imageHolder = imageHolder;
            this.tc = tcViewState.TechnologicalCard;
            this.context = context;
            this.isWindowEditor = isWindowEditor;

            if(imageHolder != null)
                _originalImageList = imageHolder.ImageList.ToList();//

            Load += Win6_ImageEditor_Load;
            FormClosing += Win6_ImageEditor_FormClosing;
        }

        #endregion

        #region Обработчики событий формы

        private void Win6_ImageEditor_Load(object? sender, EventArgs e)
        {
            _imageControl = new ImageOptionsControl(tc, context, tcViewState, imageHolder, isWindowEditor);
            InitializeWpfControl();
        }

        private void Win6_ImageEditor_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                UserChoice = SaveResult.Save;
                AfterSave?.Invoke(imageHolder);
                return;
            }


            if(HasListChanged())
            {
                // Показываем диалог: сохранить или нет
                var result = MessageBox.Show("Сохранить изменения?", "Подтверждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        UserChoice = SaveResult.Save;
                        AfterSave?.Invoke(imageHolder);
                        break;

                    case DialogResult.No:
                        UserChoice = SaveResult.DontSave;
                        if (imageHolder != null)
                            imageHolder.ImageList = _originalImageList;
                        break;

                    case DialogResult.Cancel:
                        UserChoice = SaveResult.Cancel;
                        e.Cancel = true; // отменить закрытие окна
                        break;
                }
            }
            else
            {
                // Если изменений не было, просто закрываем без вопросов
                UserChoice = SaveResult.DontSave;
            }
        }

        private bool HasListChanged()
        {
            if (imageHolder == null)
                return false;

            // Сравниваем текущий список с оригинальным
            if (imageHolder.ImageList.Count != _originalImageList.Count)
                return true;

            // Подробное сравнение элементов списка
            for (int i = 0; i < imageHolder.ImageList.Count; i++)
            {
                if (!imageHolder.ImageList[i].Equals(_originalImageList[i]))
                    return true;
            }

            return false;
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

        public int GetObjectId()
        {
           return tc.Id;
        }

        #endregion
    }
}
