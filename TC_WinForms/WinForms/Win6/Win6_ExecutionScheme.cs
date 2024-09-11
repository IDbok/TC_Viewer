using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Work;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using static System.Windows.Forms.DataFormats;

namespace TC_WinForms.WinForms
{
    public partial class Win6_ExecutionScheme : Form, IViewModeable, ISaveEventForm
    {
        private readonly TcViewState _tcViewState;
        private Win6_new _parent;
        private readonly TechnologicalCard _tc;
        private bool _isViewMode = true;

        public bool HasChanges { get; private set; } = false;

        public bool CloseFormsNoSave { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Win6_ExecutionScheme(TechnologicalCard tc, TcViewState tcViewState, Win6_new parent, bool viewerMode = false)
        {
            _tc = tc;
            _parent = parent;
            _tcViewState = tcViewState;
            //_isViewMode = viewerMode;

            InitializeComponent();

            this.Text = $"{_tc!.Name} ({_tc.Article}) - Схема исполнения";

            _tcViewState.ViewModeChanged += OnViewModeChanged;
        }

        private void Win6_ExecutionScheme_Load(object sender, EventArgs e)
        {
            SetViewMode();
            try
            {
                if (_parent.executionSchemeImage64 != null)
                {
                    DisplayImage(_parent.executionSchemeImage64, pictureBoxExecutionScheme);
                }
                else if (_tc.ExecutionSchemeImageId != null)
                {
                    using (var dbCon = new MyDbContext())
                    {
                        ImageStorage? image = new ImageStorage();

                        for (int i = 0; i < 3; i++)
                        {
                            image = dbCon.ImageStorage.Where(i => i.Id == _tc.ExecutionSchemeImageId).FirstOrDefault();
                            if (image != null)
                                break;
                            else if (i == 2)
                                throw new Exception("Не удалолсь получить изображение");
                        }

                        DisplayImage(image!.ImageBase64!, pictureBoxExecutionScheme);
                        _parent.executionSchemeImage64 = image.ImageBase64;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при загрузке данных: \n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void OnViewModeChanged()
        {
            SetViewMode();
        }
        public void SetViewMode(bool? isViewMode = false)
        {
            //if (isViewMode != null)
            //{
            //    _isViewMode = (bool)isViewMode;
            //}

            btnUploadExecutionScheme.Visible = !_tcViewState.IsViewMode;
            btnDeleteES.Visible = !_tcViewState.IsViewMode;
        }
        void DisplayImage(byte[] imageBytes, PictureBox pictureBox)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                pictureBox.Image = Image.FromStream(ms);
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }
        void DisplayImage(string base64String, PictureBox pictureBox)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(imageBytes))
            {
                pictureBox.Image = Image.FromStream(ms);
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void btnUploadExecutionScheme_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Title = "Выберите изображение";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var bytesImage = File.ReadAllBytes(openFileDialog.FileName);
                    string base64Image = Convert.ToBase64String(bytesImage);

                    var newImage = new ImageStorage
                    {
                        ImageBase64 = base64Image,
                        Category = ImageCategory.ExecutionScheme
                    };
                    _tc.ExecutionSchemeImage = newImage;

                    if (_tc.ExecutionSchemeImageId != null)
                    {
                        _tc.ExecutionSchemeImage.Id = (long)_tc.ExecutionSchemeImageId;
                    }
                    //_tc.ExecutionSchemeBase64 = base64Image;
                    DisplayImage(_tc.ExecutionSchemeImage.ImageBase64, pictureBoxExecutionScheme);

                    HasChanges = true;
                }
                _parent.executionSchemeImage64 = _tc.ExecutionSchemeImage.ImageBase64;
            }
        }

        public bool GetDontSaveData()
        {
            throw new NotImplementedException();
        }

        public async Task SaveChanges()
        {
            if (_tc.ExecutionSchemeImage != null || _tc.ExecutionSchemeImageId != null)
            {
                var dbCon = new DbConnector();
                if (_tc.ExecutionSchemeImage?.ImageBase64 != null)
                {
                    await dbCon.UpdateTcExecutionScheme(_tc.Id, _tc.ExecutionSchemeImage.ImageBase64);
                }
                else
                {
                    await dbCon.DeleteTcExecutionScheme(_tc.Id);
                }
            }
        }

        private void btnDeleteES_Click(object sender, EventArgs e)
        {
            _tc.ExecutionSchemeImage?.ClearBase64Image();
            pictureBoxExecutionScheme.Image = null;
            _parent.executionSchemeImage64 = null;
        }

    }
}
