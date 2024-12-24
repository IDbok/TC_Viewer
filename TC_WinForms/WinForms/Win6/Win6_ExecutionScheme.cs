using Serilog;
using System.IO;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.Extensions;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector.Repositories;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms
{
    public partial class Win6_ExecutionScheme : Form, IViewModeable, ISaveEventForm
    {
        private readonly ILogger _logger;
        private readonly TcViewState _tcViewState;

        private TechnologicalCardRepository tcRepository = new TechnologicalCardRepository();
        private readonly TechnologicalCard _tc;
        private bool _isViewMode = true;

        private int _tcId;
        public bool HasChanges { get; private set; } = false;

        public bool CloseFormsNoSave { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Win6_ExecutionScheme(TcViewState tcViewState, bool viewerMode = false)
        {

            _tcViewState = tcViewState;
            _tc = tcViewState.TechnologicalCard!;
            _tcId = _tc.Id;

            _logger = Log.Logger
                .ForContext<Win6_ExecutionScheme>()
                .ForContext("TcId", _tcId);

            _logger.Information("Инициализация окна.");

            //_isViewMode = viewerMode;

            InitializeComponent();

            this.Text = $"{_tc!.Name} ({_tc.Article}) - Схема исполнения";

            _tcViewState.ViewModeChanged += OnViewModeChanged;

            this.FormClosed += (s, e) => this.Dispose();
        }

        private async void Win6_ExecutionScheme_Load(object sender, EventArgs e)
        {
            SetViewMode();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // проверка нет ли во временных данных изображения
                var tempImage = ImageHelper.LoadImageFromTempFileAsBase64(_tc.Id);
                if (!string.IsNullOrEmpty(tempImage))
                {
                    _logger.Information("Изображение найдено во временных файлах.");

                    DisplayImage(tempImage, pictureBoxExecutionScheme);
                    return;
                }

                if (_tc.ExecutionSchemeImageId != null ||
                    (_tc.ExecutionSchemeImageId == null
                    && _tcViewState.TechnologicalCard!.ExecutionSchemeBase64 != null))
                {
                    string imageBase64 = "";

                    if (_tc.ExecutionSchemeBase64 != null)
                    {
                        imageBase64 = _tc.ExecutionSchemeBase64 ?? "";
                        _logger.Information("Изображение загружено из объекта TcViewState.");
                    }
                    else if (_tc.ExecutionSchemeImageId != null)
                    {
                        for (int attempt = 1; attempt <= 3; attempt++)//проверка на загрузку изображение, повторяем 3 раза
                        {
                            try
                            {
                                _logger.Information("Попытка {Attempt} загрузки изображения из БД.", attempt);

                                imageBase64 =
                                    await tcRepository.GetImageBase64Async((long)_tc.ExecutionSchemeImageId) ?? "";

                                if (!string.IsNullOrEmpty(imageBase64))
                                {
                                    _logger.Information("Изображение успешно загружено из БД за {ElapsedMilliseconds} мс.",
                                        stopwatch.ElapsedMilliseconds);
                                    _tc.ExecutionSchemeBase64 = imageBase64;
                                    break;
                                }

                                if (attempt == 3)
                                    throw new Exception("Изображение не удалось загрузить из БД после 3 попыток.");
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Ошибка при загрузке изображения из БД.");
                                if (attempt == 3) throw;
                            }

                        }
                        _tc.ExecutionSchemeBase64 = imageBase64;
                    }

                    if (!string.IsNullOrEmpty(imageBase64))
                    {
                        // Сохраняем изображение во временный файл
                        ImageHelper.SaveImageToTempFile(imageBase64, _tc.Id);
                        _logger.Information("Изображение сохранено во временный файл.");

                        DisplayImage(imageBase64, pictureBoxExecutionScheme);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке изображения");
                MessageBox.Show(ex.Message);
                this.Close();
            }
            finally
            {
                stopwatch.Stop();
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

            _logger.Information("Изображение отображено в форме");
            // добавить первые 10 символов base64 строки, для проверки, что изображение загружено
            _logger.Debug("Первые 10 символов base64 строки: {First10Chars}", base64String.Substring(0, 10));
        }

        private void btnUploadExecutionScheme_Click(object sender, EventArgs e)
        {
			LogUserAction("Загрузка нового изображения схемы исполнения");

			using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Title = "Выберите изображение";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
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
                        _logger.Information("Новое изображение загружено пользователем и сохранено во временный файл");

                        DisplayImage(_tc.ExecutionSchemeImage.ImageBase64, pictureBoxExecutionScheme);
                        _tc.ExecutionSchemeBase64 = base64Image;

                        HasChanges = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Ошибка при загрузке нового изображения");
                        MessageBox.Show("Ошибка при загрузке изображения.");
                    }
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
                    _logger.Information("Изображение сохранено в БД.");
                }
                else
                {
                    await dbCon.DeleteTcExecutionScheme(_tc.Id);
                    _tc.ExecutionSchemeBase64 = null;
                    _logger.Information("Изображение удалено из БД.");
                }
            }
        }

        private void btnDeleteES_Click(object sender, EventArgs e)
        {
			LogUserAction("Удаление изображения схемы исполнения");

			try
            {
                _tc.ExecutionSchemeImage?.ClearBase64Image(); // что делает данное поле?
                _tc.ExecutionSchemeImage = null;
                _tc.ExecutionSchemeBase64 = null;
                _tc.ExecutionSchemeImageId = null;
                pictureBoxExecutionScheme.Image = null;

                // Удаляем временный файл
                TempFileCleaner.CleanUpTempFiles(TempFileCleaner.GetTempFilePath(_tc.Id));
                _logger.Information("Изображение удалено из формы и временных файлов.");

                HasChanges = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при удалении изображения.");
                MessageBox.Show("Ошибка при удалении изображения.");
            }
        }

        private void LogUserAction(string action)
		{
			_logger.LogUserAction(action);
		}
	}
}
