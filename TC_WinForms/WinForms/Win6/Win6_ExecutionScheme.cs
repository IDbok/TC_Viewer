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

namespace TC_WinForms.WinForms
{
    public partial class Win6_ExecutionScheme : Form, IViewModeable, ISaveEventForm
    {
        private readonly TcViewState _tcViewState;

        private readonly TechnologicalCard _tc;
        private bool _isViewMode = true;

        public bool HasChanges { get; private set; } = false;

        public bool CloseFormsNoSave { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Win6_ExecutionScheme(TechnologicalCard tc, TcViewState tcViewState, bool viewerMode = false)
        {
            _tc = tc;

            _tcViewState = tcViewState;
            //_isViewMode = viewerMode;

            InitializeComponent();

            this.Text = $"{_tc!.Name} ({_tc.Article}) - Схема исполнения";
        }

        private void Win6_ExecutionScheme_Load(object sender, EventArgs e)
        {
            SetViewMode();

            if (_tc.ExecutionSchemeImageId != null)
            {
                ImageStorage? image;
                // Загрузить изображение схемы выполнения
                using (var dbCon = new MyDbContext())
                {
                    image = dbCon.ImageStorage.Where(i => i.Id == _tc.ExecutionSchemeImageId).FirstOrDefault();
                }

                if (image != null && image.ImageBase64 != null)
                {
                    DisplayImage(image.ImageBase64, pictureBoxExecutionScheme);
                    //DisplayImage(_tc.ExecutionSchemeBase64, pictureBoxExecutionScheme);
                }
            }



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
        }
    }
}
