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

		private TechnologicalCardRepository _tcRepository = new TechnologicalCardRepository();
		private readonly TechnologicalCard _tc;
		private bool _isViewMode = true;

		private int _tcId;
		public bool HasChanges { get; private set; } = false;

		public bool CloseFormsNoSave { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public Win6_ExecutionScheme(TcViewState tcViewState, bool viewerMode = false)
		{

			_tcViewState = tcViewState;
			_tc = tcViewState.TechnologicalCard
				  ?? throw new ArgumentNullException(nameof(tcViewState.TechnologicalCard));

			_tcId = _tc.Id;
			_logger = Log.Logger
				.ForContext<Win6_ExecutionScheme>()
				.ForContext("TcId", _tcId);

			_logger.Information("Инициализация окна.");

			InitializeComponent();
			SetupFormTitle();
			RegisterViewModeEvents();
		}

		private void SetupFormTitle()
		{
			// Пример: "НазваниеТК (Артикул) - Схема исполнения"
			this.Text = $"{_tc.Name} ({_tc.Article}) - Схема исполнения";
		}

		private void RegisterViewModeEvents()
		{
			// Подписываемся на изменение режима просмотра
			_tcViewState.ViewModeChanged += OnViewModeChanged;
			// Когда форма закрывается, диспозим ресурсы
			this.FormClosed += (s, e) => this.Dispose();
		}

		#region События формы

		private async void Win6_ExecutionScheme_Load(object sender, EventArgs e)
		{
			SetViewMode();
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				// 1) Сначала пытаемся загрузить из временного файла
				if (TryLoadFromTempFile())
				{
					return; // если успешно, завершаем
				}

				// 2) Если во временном файле нет — загружаем из самого объекта/БД
				await LoadFromObjectOrDbAsync(stopwatch);
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

		private void btnUploadExecutionScheme_Click(object sender, EventArgs e)
		{
			LogUserAction("Загрузка нового изображения схемы исполнения");
			UploadNewSchemeImage();
		}

		private void btnDeleteES_Click(object sender, EventArgs e)
		{
			LogUserAction("Удаление изображения схемы исполнения");

			DeleteSchemeImage();
		}

		#endregion

		#region Логика загрузки изображения

		/// <summary>
		/// Пытается загрузить изображение из временного файла.
		/// Если успешно, отображает и возвращает true, иначе false.
		/// </summary>
		private bool TryLoadFromTempFile()
		{
			var tempImage = ImageHelper.LoadImageFromTempFileAsBase64(_tc.Id);
			if (string.IsNullOrEmpty(tempImage))
				return false;

			_logger.Information("Изображение загружено из временных файлах.");
			DisplayImage(tempImage, pictureBoxExecutionScheme);
			return true;
		}

		/// <summary>
		/// Загружает изображение из _tc (если уже в ExecutionSchemeBase64) или из БД.
		/// </summary>
		private async Task LoadFromObjectOrDbAsync(System.Diagnostics.Stopwatch stopwatch)
		{
			// Если у ТК есть ExecutionSchemeImageId или есть сохраненный Base64 (при нулевом ID) 
			if (_tc.ExecutionSchemeImageId != null
				|| (_tc.ExecutionSchemeImageId == null && !string.IsNullOrEmpty(_tc.ExecutionSchemeBase64))
				|| (_tc.ExecutionSchemeImage != null && !string.IsNullOrEmpty(_tc.ExecutionSchemeImage.ImageBase64)))
			{
				string base64Image = await GetBase64ImageFromTcOrDbAsync(stopwatch);
				if (!string.IsNullOrEmpty(base64Image))
				{
					// Сохраняем изображение во временный файл
					ImageHelper.SaveImageToTempFile(base64Image, _tc.Id);
					_logger.Information("Изображение сохранено во временный файл.");

					DisplayImage(base64Image, pictureBoxExecutionScheme);
				}
				else
				{
					_logger.Information("Изображение не найдено ни в объекте, ни в БД.");
				}
			}
			else
			{
				_logger.Information("Отсутствуют данные об изображении (и ExecutionSchemeImageId, и Base64 = null).");
			}
		}

		/// <summary>
		/// Возвращает Base64 из _tc.ExecutionSchemeBase64 (если не пусто)
		/// или пытается загрузить из БД (если _tc.ExecutionSchemeImageId != null).
		/// </summary>
		private async Task<string> GetBase64ImageFromTcOrDbAsync(System.Diagnostics.Stopwatch stopwatch)
		{
			// 1) Если уже есть в ТК (устарело)
			if (!string.IsNullOrEmpty(_tc.ExecutionSchemeBase64))
			{
				_logger.Information("Изображение загружено из объекта TcViewState.");
				return _tc.ExecutionSchemeBase64;
			}

			// 2) Если уже есть в ТК
			if (_tc.ExecutionSchemeImage != null && !string.IsNullOrEmpty(_tc.ExecutionSchemeImage.ImageBase64))
			{
				_logger.Information("Изображение загружено из объекта ImageStorage.");
				return _tc.ExecutionSchemeImage.ImageBase64;
			}

			// 3) Иначе, пробуем загрузить из БД
			if (_tc.ExecutionSchemeImageId != null)
			{
				return await TryLoadImageBase64FromDbAsync((long)_tc.ExecutionSchemeImageId, stopwatch);
			}

			// Нет данных
			_logger.Debug("ExecutionSchemeBase64, ImageStorage и ExecutionSchemeImageId в ТК отсутствуют.");
			return string.Empty;
		}

		/// <summary>
		/// Пытается 3 раза подряд загрузить изображение Base64 из БД.
		/// Если удаётся — возвращает Base64, иначе бросает исключение.
		/// </summary>
		private async Task<string> TryLoadImageBase64FromDbAsync(long imageId, System.Diagnostics.Stopwatch stopwatch)
		{
			const int maxAttempts = 3;
			for (int attempt = 1; attempt <= maxAttempts; attempt++)
			{
				try
				{
					_logger.Information("Попытка {Attempt} загрузки изображения из БД.", attempt);

					var imageBase64 = await _tcRepository.GetImageBase64Async(imageId) ?? "";
					if (!string.IsNullOrEmpty(imageBase64))
					{
						_logger.Information("Изображение успешно загружено из БД за {Elapsed} мс.",
							stopwatch.ElapsedMilliseconds);
						_tc.ExecutionSchemeBase64 = imageBase64;
						return imageBase64;
					}

					if (attempt == maxAttempts)
					{
						throw new Exception("Изображение не удалось загрузить из БД после 3 попыток.");
					}
				}
				catch (Exception ex)
				{
					_logger.Error(ex, "Ошибка при загрузке изображения из БД (попытка {Attempt}).", attempt);
					if (attempt == maxAttempts)
						throw; // Если после 3 попыток всё ещё ошибка, пробрасываем дальше
				}
			}
			return string.Empty;
		}

		private void DisplayImage(string base64String, PictureBox pictureBox)
		{
			byte[] imageBytes = Convert.FromBase64String(base64String);
			using (var ms = new MemoryStream(imageBytes))
			{
				pictureBox.Image = Image.FromStream(ms);
				pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
			}

			_logger.Information("Изображение отображено в форме");
			// добавить первые 10 символов base64 строки, для проверки, что изображение загружено
			_logger.Debug("Первые 10 символов base64 строки: {FirstChars}",
						  base64String.Substring(0, Math.Min(10, base64String.Length)));
		}

		#endregion

		#region Загрузка/Удаление изображения

		private const int MAX_IMAGE_SIZE_BYTES = 1 * 1024 * 1024; // 1 MB условный лимит
		private void UploadNewSchemeImage()
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
				openFileDialog.Title = "Выберите изображение";

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						string filePath = openFileDialog.FileName;

						// 1) Считываем все байты
						byte[] bytesImage = File.ReadAllBytes(filePath);

						// 2) Проверяем, что файл не превышает условный лимит
						if (bytesImage.Length > MAX_IMAGE_SIZE_BYTES)
						{
							MessageBox.Show(
								$"Файл превышает максимально допустимый размер {MAX_IMAGE_SIZE_BYTES / (1024 * 1024)} МБ.\n" +
								"Не рекомендуется хранить такое изображение в БД.",
								"Размер файла слишком большой",
								MessageBoxButtons.OK,
								MessageBoxIcon.Warning
							);
							return;
						}

						// 3) Генерируем Base64
						string base64Image = Convert.ToBase64String(bytesImage);

						// 4) Устанавливаем новое изображение в ТК
						//SetNewImageData(base64Image); // todo: Проверить логику работы с изображением чисто через временные файлы

						// 5) Сохраняем во временный файл
						ImageHelper.SaveImageToTempFile(base64Image, _tc.Id);
						_logger.Information("Новое изображение загружено пользователем и сохранено во временный файл.");

						// 6) Отображаем в pictureBox
						DisplayImage(base64Image, pictureBoxExecutionScheme);

						// Ставим флаг, что есть несохранённые изменения
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

		private string FileToBase64(string filePath)
		{
			var bytes = File.ReadAllBytes(filePath);
			return Convert.ToBase64String(bytes);
		}

		/// <summary>
		/// Устанавливает новое изображение в _tc.ExecutionSchemeImage и т.д.
		/// </summary>
		private void SetNewImageData(string base64Image)
		{
			// Создаём новую структуру, чтобы привязать изображение к объекту
			var newImage = new ImageStorage
			{
				ImageBase64 = base64Image,
				Category = ImageCategory.ExecutionScheme
			};

			// Если уже есть Id, назначаем его вновь создаваемому ImageStorage
			if (_tc.ExecutionSchemeImageId != null)
			{
				newImage.Id = (long)_tc.ExecutionSchemeImageId;
			}

			_tc.ExecutionSchemeImage = newImage;
			//_tc.ExecutionSchemeBase64 = base64Image;
		}

		private void DeleteSchemeImage()
		{
			try
			{
				_tc.ExecutionSchemeImage?.ClearBase64Image();
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

		#endregion

		public void SetViewMode(bool? isViewMode = false)
        {
            btnUploadExecutionScheme.Visible = !_tcViewState.IsViewMode;
            btnDeleteES.Visible = !_tcViewState.IsViewMode;
        }

        public bool GetDontSaveData()
        {
            throw new NotImplementedException();
        }

		public async Task SaveChanges()
		{
			// проверка нет ли изображения во временных файлах
			var tempImage = ImageHelper.LoadImageFromTempFileAsBase64(_tc.Id);

			if (_tc.ExecutionSchemeImage != null
				|| _tc.ExecutionSchemeImageId != null
				|| !string.IsNullOrEmpty(tempImage))
			{
				var dbCon = new DbConnector();
				if (!string.IsNullOrEmpty(_tc.ExecutionSchemeImage?.ImageBase64))
                {
                    await dbCon.UpdateTcExecutionScheme(_tc.Id, _tc.ExecutionSchemeImage.ImageBase64);
                    //_tc.ExecutionSchemeBase64 = _tc.ExecutionSchemeImage.ImageBase64;
                    _logger.Information("Изображение сохранено в БД.");
                }
				else if (!string.IsNullOrEmpty(tempImage))
				{
					await dbCon.UpdateTcExecutionScheme(_tc.Id, tempImage);
					_logger.Information("Изображение сохранено в БД.");
				}
				else
                {
                    await dbCon.DeleteTcExecutionScheme(_tc.Id);
                    //_tc.ExecutionSchemeBase64 = null;
                    _logger.Information("Изображение удалено из БД.");
                }
            }
        }

        private void LogUserAction(string action)
		{
			_logger.LogUserAction(action);
		}
	}
}
