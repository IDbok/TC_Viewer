using System.IO;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.Interfaces;
using TcDbConnector.Repositories;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms
{
    public partial class Win6_ExecutionScheme : Form, IViewModeable, ISaveEventForm
    {
        private readonly TcViewState _tcViewState;

        private TechnologicalCardRepository tcRepository = new TechnologicalCardRepository();
        private readonly TechnologicalCard _tc;
        private bool _isViewMode = true;

        public bool HasChanges { get; private set; } = false;

        public bool CloseFormsNoSave { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Win6_ExecutionScheme(TcViewState tcViewState, bool viewerMode = false)
        {
            _tcViewState = tcViewState;
            _tc = tcViewState.TechnologicalCard!;

            //_isViewMode = viewerMode;

            InitializeComponent();

            this.Text = $"{_tc!.Name} ({_tc.Article}) - Схема исполнения";

            _tcViewState.ViewModeChanged += OnViewModeChanged;

            this.FormClosed += (s, e) => this.Dispose();
        }

        private async void Win6_ExecutionScheme_Load(object sender, EventArgs e)
        {
            SetViewMode();

            // проверка нет ли во временных данных изображения
            var image = ImageHelper.LoadImageFromTempFileAsBase64(_tc.Id);
            if (!string.IsNullOrEmpty(image))
            {
                DisplayImage(image, pictureBoxExecutionScheme);
                return;
            }

            if (_tc.ExecutionSchemeImageId != null || 
                (_tc.ExecutionSchemeImageId == null 
                && _tcViewState.TechnologicalCard!.ExecutionSchemeBase64 != null))
            {
                string imageBase64 = "";

                if (_tc.ExecutionSchemeBase64 != null)
                    imageBase64 = _tc.ExecutionSchemeBase64 ?? "";
                else if(_tc.ExecutionSchemeImageId != null)
                {
                    try
                    {
                        for(int i = 1; i <=3; i++)//проверка на загрузку изображение, повторяем 3 раза
                        {
                            imageBase64 = 
                                await tcRepository.GetImageBase64Async((long)_tc.ExecutionSchemeImageId) ?? "";
                            

                            if (i == 3 && (imageBase64 == "" || imageBase64 == null))
                                throw new Exception("не удалось получить изображение");
                            else if (imageBase64 != null && imageBase64 != "")
                                break;
                        }
                        _tc.ExecutionSchemeBase64 = imageBase64;

                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        this.Close();
                    }
                }

                if (!string.IsNullOrEmpty(imageBase64))
                {
                    // Сохраняем изображение во временный файл
                    ImageHelper.SaveImageToTempFile(imageBase64, _tc.Id);

                    DisplayImage(imageBase64, pictureBoxExecutionScheme);
                    //DisplayImage(_tc.ExecutionSchemeBase64, pictureBoxExecutionScheme);
                }
            }

        }
        
        private void OnViewModeChanged()
        {
            SetViewMode();
        }
        public void SetViewMode(bool? isViewMode = false)
        {
            btnUploadExecutionScheme.Visible = !_tcViewState.IsViewMode;
            btnDeleteES.Visible = !_tcViewState.IsViewMode;
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

                    // Сохраняем изображение во временный файл
                    ImageHelper.SaveImageToTempFile(base64Image, _tc.Id);

                    DisplayImage(_tc.ExecutionSchemeImage.ImageBase64, pictureBoxExecutionScheme);
                    _tc.ExecutionSchemeBase64 = base64Image;

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
                    _tc.ExecutionSchemeBase64 = _tc.ExecutionSchemeImage.ImageBase64;

                }
                else
                {
                    await dbCon.DeleteTcExecutionScheme(_tc.Id);
                    _tc.ExecutionSchemeBase64 = null;
                }
            }
        }

        private void btnDeleteES_Click(object sender, EventArgs e)
        {
            _tc.ExecutionSchemeImage?.ClearBase64Image();       
            pictureBoxExecutionScheme.Image = null;

            // Удаляем временный файл
            TempFileCleaner.CleanUpTempFiles(TempFileCleaner.GetTempFilePath(_tc.Id));

            HasChanges = true;
        }
    }
}
