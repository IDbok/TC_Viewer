using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace TC_WinForms.WinForms.Win6.ImageEditor
{
    public partial class ImageOptionsControl : UserControl, INotifyPropertyChanged
    {
        private string techCardName = "ТестоваяТк";
        private int TcId = 451;
        private readonly IImageHoldable _imageHolder;
        private MyDbContext context;
        private TechnologicalCard tc;
        public class ImageItem : INotifyPropertyChanged
        {
            public ImageOwner Owner { get; set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        OnPropertyChanged();

                        // Обновление IImageHoldable
                        if (_parentControl != null)
                        {
                            if (value)
                            {
                                if (!_parentControl._imageHolder.ImageList.Any(o => o == Owner))
                                    _parentControl._imageHolder.ImageList.Add(Owner);
                            }
                            else
                            {
                                _parentControl._imageHolder.ImageList.RemoveAll(o => o == Owner);
                            }
                        }
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            internal ImageOptionsControl _parentControl; // установка при создании
        }


        private ObservableCollection<ImageItem> _imageItems;
        public ObservableCollection<ImageItem> ImageItems
        {
            get => _imageItems;
            set
            {
                _imageItems = value;
                OnPropertyChanged();
            }
        }

        private ImageItem _selectedItem;
        public ImageItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                UpdateImageDisplay();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageOptionsControl(IImageHoldable imageHolder, TechnologicalCard technologicalCard, MyDbContext context)
        {
            InitializeComponent();
            DataContext = this;
            this._imageHolder = imageHolder;
            this.techCardName = technologicalCard.Article;
            this.context = context; 
            tc = technologicalCard;
            LoadData(technologicalCard.ImageOwner);
            lblTcName.Text = techCardName;
        }

        private void LoadData(List <ImageOwner> items)
        {
            try
            {
                var holderIds = _imageHolder?.ImageList?.ToHashSet() ?? new HashSet<ImageOwner>();

                ImageItems = new ObservableCollection<ImageItem>(
                    items.Select(i => new ImageItem
                    {
                        Owner = i,
                        IsSelected = holderIds.Contains(i),
                        _parentControl = this
                    }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void UpdateImageDisplay()
        {
            if (SelectedItem?.Owner?.ImageStorage?.ImageBase64 == null)
            {
                ImageHolder.Source = null;
                ImageNumber.Text = "Изображение не выбрано";
                ImageName.Text = string.Empty;
                return;
            }

            try
            {
                byte[] imageBytes = Convert.FromBase64String(SelectedItem.Owner.ImageStorage.ImageBase64);
                using (var ms = new System.IO.MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    ImageHolder.Source = bitmap;
                }

                ImageNumber.Text = $"Рисунок {SelectedItem.Owner.Number}";
                ImageName.Text = SelectedItem.Owner.Name ?? "Без названия";
            }
            catch (Exception ex)
            {
                ImageHolder.Source = null;
                ImageNumber.Text = "Ошибка загрузки изображения";
                ImageName.Text = ex.Message;
            }
        }

        private void BtnAddImage_Click(object sender, RoutedEventArgs e)
        {
            var objEditor = new ImageEditorWindow();
            objEditor.AfterSave = async (editedObj) =>
            {
                AddObjectInDataGridView(editedObj);

                await Task.CompletedTask;
                
            };
            objEditor.ShowDialog();
        }

        private void BtnEditImage_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Выберите изображение для редактирования");
                return;
            }

            var objEditor = new ImageEditorWindow(SelectedItem.Owner);
            objEditor.AfterSave = async (editedObj) =>
            {
                UpdateObjectInDataGridView(editedObj);

                // Вручную принудить обновление SelectedItem (триггерит UpdateImageDisplay)
                var current = SelectedItem;
                SelectedItem = null;
                SelectedItem = current;

                await Task.CompletedTask;
            };
            objEditor.ShowDialog();
        }

        private void AddObjectInDataGridView(ImageOwner addedObj)
        {
            if(addedObj != null)
    {
                var newItem = new ImageItem
                {
                    Owner = addedObj
                };

                // Привязка ImageStorage, если он есть
                if (addedObj.ImageStorage != null)
                {
                    context.Entry(addedObj.ImageStorage).State = EntityState.Added;
                }

                context.Entry(addedObj).State = EntityState.Added;

                ImageItems.Add(newItem);
                tc.ImageOwner.Add(addedObj); // возможно лишнее, если контексту уже известен объект
                SelectedItem = newItem;
                OnPropertyChanged(nameof(ImageItems));
            }
        }
        private void UpdateObjectInDataGridView(ImageOwner editedObj)
        {
            var item = SelectedItem;
            if (item != null)
            {
                // Проверка изменений
                item.Owner.Name = editedObj.Name;
                item.Owner.Number = editedObj.Number;

                if (item.Owner.ImageStorage.IsChanged)
                {
                    item.Owner.ImageStorage = editedObj.ImageStorage;

                    if (item.Owner.ImageStorage != null)
                    {
                        // Новый ImageStorage
                        context.Entry(item.Owner.ImageStorage).State =
                            item.Owner.ImageStorage.Id == 0 ? EntityState.Added : EntityState.Modified;
                    }
                }

                if(context.Entry(item.Owner).State != EntityState.Added)
                    context.Entry(item.Owner).State = EntityState.Modified;

                // Уведомления
                item.OnPropertyChanged(nameof(item.Owner));
                OnPropertyChanged(nameof(ImageItems));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Выберите изображение для удаления.");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранное изображение?",
                                "Подтверждение удаления",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            var imageOwner = SelectedItem.Owner;
            var imageStorage = imageOwner.ImageStorage;

            try
            {
                imageOwner.ExecutionWorks.Clear();
                imageOwner.DiagramShags.Clear();

                // Проверяем, используется ли этот ImageStorage другими ImageOwner-ами
                bool isStorageShared = context.Set<ImageOwner>()
                    .Count(io => io.ImageStorageId == imageStorage.Id) > 1;

                // Удаляем ImageOwner из контекста и списка
                context.Entry(imageOwner).State = EntityState.Deleted;
                tc.ImageOwner.Remove(imageOwner);
                ImageItems.Remove(SelectedItem);

                // Если ImageStorage больше нигде не используется — удаляем и его
                if (!isStorageShared && imageStorage != null)
                {
                    context.Entry(imageStorage).State = EntityState.Deleted;
                }

                // Удаляем из _imageHolder.ImageList, если он там есть
                _imageHolder.ImageList.RemoveAll(i => i == imageOwner);

                SelectedItem = null;
                OnPropertyChanged(nameof(ImageItems));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
