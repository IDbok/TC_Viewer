using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using TC_WinForms.Services;
using TcModels.Models;
using MessageBox = System.Windows.MessageBox;

namespace TC_WinForms.WinForms.Win6.ImageEditor
{
    public partial class ImageEditorWindow : Window, INotifyPropertyChanged
    {
        #region Поля

        private ImageOwner? _imageOwner;
        private string _imageName;
        private string _newImageName;
        private string _newImageNum;
        private bool IsNewImage { get; set; } = false;

        #endregion

        #region Свойства

        public string ImageStatus
        {
            get => _imageName != null ? $"Изображение {_imageName} загружено" : "Изображение не загружено";
            set
            {
                _imageName = value;
                OnPropertyChanged(nameof(ImageStatus));
            }
        }

        public string NewImageName
        {
            get => _newImageName;
            set
            {
                _newImageName = value;
                OnPropertyChanged(nameof(NewImageName));
            }
        }

        public string NewImageNum
        {
            get => _newImageNum;
            set
            {
                _newImageNum = value;
                OnPropertyChanged(nameof(NewImageNum));
            }
        }

        public delegate Task PostSaveAction<IModel>(IModel modelObject) where IModel : ImageOwner;
        public PostSaveAction<ImageOwner>? AfterSave { get; set; }

        #endregion

        #region События

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Конструктор

        public ImageEditorWindow(ImageOwner? image = null)
        {
            IsNewImage = image == null;
            _imageOwner = IsNewImage ? new ImageOwner() : image;
            NewImageName = _imageOwner?.Name;
            NewImageNum = _imageOwner?.Number.ToString();

            InitializeComponent();
            DataContext = this;

            this.Title = IsNewImage ? "Добавить изображение" : "Редактировать изображение";
            Closing += ImageEditorWindow_Closing;

            SetFieldsData();
        }

        #endregion

        #region Методы

        private void SetFieldsData()
        {
            NameTextBox.Text = NewImageName;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _imageOwner.Name = NameTextBox.Text;

            if (int.TryParse(NumberTextBox.Text, out var number))
            {
                _imageOwner.Number = number;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Введите корректный номер изображения");
                return;
            }

            if (AfterSave != null)
            {
                await AfterSave(_imageOwner);
            }

            Close();
        }

        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_imageOwner.ImageStorage != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите обновить изображение? Это удалит прошлое изображение и заменит на новое.", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
            }

            var openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            string filename = openFileDialog1.FileName;

            try
            {
                if (IsNewImage)
                {
                    var newImage = ImageService.CreateNewImageFromBase64(filename);
                    var number = int.TryParse(NumberTextBox.Text, out var parsedNumber) ? parsedNumber : 1;

                    _imageOwner = ImageService.CreateNewImageOwner(
                        newImage,
                        new TechnologicalCard(),
                        ImageType.Image,
                        number
                    );
                }
                else
                {
                    _imageOwner.ImageStorage = ImageService.UpdateImageFromBase64(
                        _imageOwner.ImageStorage,
                        filename
                    );
                }

                ImageStatus = System.IO.Path.GetFileName(filename);
            }
            catch (OutOfMemoryException)
            {
                System.Windows.Forms.MessageBox.Show("Файл не является изображением");
                return;
            }
        }

        private void ImageEditorWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (Content is ImageOptionsControl control && control.HasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "Вы хотите сохранить изменения перед выходом?",
                    "Сохранение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    control.CommitChanges(); // Метод сохранения
                }
                else if (result == MessageBoxResult.No)
                {
                    control.DiscardChanges(); // Метод отката (если нужен)
                }
            }
        }

        #endregion
    }
}
