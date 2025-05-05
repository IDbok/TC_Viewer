using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TC_WinForms.Services;
using TcModels.Models;

namespace TC_WinForms.WinForms.Win6.ImageEditor
{
    public partial class ImageEditorWindow : Window, INotifyPropertyChanged
    {
        private ImageOwner? _imageOwner;

        private string _imageName;
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
        private string _newImageName;

        public string NewImageNum
        {
            get => _newImageNum;
            set
            {
                _newImageNum = value;
                OnPropertyChanged(nameof(NewImageNum));
            }
        }
        private string _newImageNum;

        private bool IsNewImage { get; set; } = false;
        public delegate Task PostSaveAction<IModel>(IModel modelObject) where IModel : ImageOwner;
        public PostSaveAction<ImageOwner>? AfterSave { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ImageEditorWindow(ImageOwner? image = null)
        {
            IsNewImage = image == null;
            _imageOwner = IsNewImage ? new ImageOwner() : image;
            NewImageName = _imageOwner?.Name;
            NewImageNum = _imageOwner?.Number.ToString();

            InitializeComponent();
            DataContext = this;

            this.Title = IsNewImage ? "Добавить изображение" : "Редактировать изображение";

            SetFieldsData();
        }

        private void NameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("NameTextBox получил фокус");
        }

        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("NameTextBox потерял фокус");
        }

        private void SetFieldsData()
        {
            NameTextBox.Text = NewImageName;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _imageOwner.Name = NameTextBox.Text;
            _imageOwner.Number = int.TryParse(NumberTextBox.Text, out var number) ? number : 1;

            if (AfterSave != null)
            {
                await AfterSave(_imageOwner);
            }

            Close();
        }

        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            string filename = openFileDialog1.FileName;

            try
            {
                if (IsNewImage)
                {
                    var newImage = ImageService.CreateNewImageFromBase64(filename);
                    var number = int.TryParse(NumberTextBox.Text, out var parsedNumber) ? parsedNumber : 1;

                    // Здесь создаём и сохраняем новый ImageOwner
                    _imageOwner = ImageService.CreateNewImageOwner(newImage, new TechnologicalCard(), ImageType.Image, number);
                }
                else
                {
                    _imageOwner.ImageStorage = ImageService.UpdateImageFromBase64(_imageOwner.ImageStorage, filename);
                }

                // Обновляем отображаемое имя файла
                ImageStatus = System.IO.Path.GetFileName(filename);
            }
            catch (OutOfMemoryException)
            {
                System.Windows.Forms.MessageBox.Show("Файл не является изображением");
                return;
            }
        }
    }
}
