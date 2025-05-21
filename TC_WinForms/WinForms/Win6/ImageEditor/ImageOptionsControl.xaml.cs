using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using static TC_WinForms.WinForms.Win6.ImageEditor.ImageOptionsControl;
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
        public event Action<ImageOwner>? AfterSave;
        public ImageOptionsControl(TechnologicalCard technologicalCard, MyDbContext context, IImageHoldable? imageHolder = null, bool IsWindowEditor = true)
        {
            InitializeComponent();
            DataContext = this;
            this._imageHolder = imageHolder;
            this.techCardName = technologicalCard.Article;
            this.context = context; 
            tc = technologicalCard;
            LoadData(technologicalCard.ImageOwner);
            lblTcName.Text = techCardName;

            if (!IsWindowEditor && ImageDataGrid.Columns.Count > 0)
            {
                ImageDataGrid.Columns.RemoveAt(0); // Удаляем первый столбец
            }
        }

        private void LoadData(List<ImageOwner> items)
        {
            try
            {
                if (_imageHolder != null)
                {
                    var holderIds = _imageHolder?.ImageList?.ToHashSet() ?? new HashSet<ImageOwner>();

                    ImageItems = new ObservableCollection<ImageItem>(
                        items.OrderBy(i => i.Number)
                             .Select(i => new ImageItem
                             {
                                 Owner = i,
                                 IsSelected = holderIds.Contains(i),
                                 _parentControl = this
                             }));
                }
                else
                {
                    ImageItems = new ObservableCollection<ImageItem>(
                        items.OrderBy(i => i.Number)
                             .Select(i => new ImageItem
                             {
                                 Owner = i,
                                 IsSelected = false,
                                 _parentControl = this
                             }));
                }
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
            var oldNum = SelectedItem.Owner.Number;
            var objEditor = new ImageEditorWindow(SelectedItem.Owner);
            objEditor.AfterSave = async (editedObj) =>
            {
                UpdateObjectInDataGridView(editedObj, oldNum.Value);

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
            if (addedObj != null)
            {
                // Проверяем, что номер не выходит за допустимые границы
                if (addedObj.Number <= 0 || addedObj.Number > ImageItems.Count + 1)
                {
                    // Если номер некорректен, ставим его в конец
                    addedObj.Number = ImageItems.Count + 1;
                }

                var newItem = new ImageItem
                {
                    Owner = addedObj,
                    _parentControl = this
                };

                // Привязка ImageStorage
                if (addedObj.ImageStorage != null)
                {
                    context.Entry(addedObj.ImageStorage).State = EntityState.Added;
                }

                context.Entry(addedObj).State = EntityState.Added;

                // Вставляем на указанную позицию (с поправкой на индексацию с 0)
                int insertIndex = Math.Min(addedObj.Number.Value - 1, ImageItems.Count);
                ImageItems.Insert(insertIndex, newItem);
                tc.ImageOwner.Add(addedObj);

                // Корректируем номера остальных элементов
                for (int i = 0; i < ImageItems.Count; i++)
                {
                    if (i != insertIndex && ImageItems[i].Owner.Number != i + 1)
                    {
                        ImageItems[i].Owner.Number = i + 1;
                        if (context.Entry(ImageItems[i].Owner).State != EntityState.Added)
                            context.Entry(ImageItems[i].Owner).State = EntityState.Modified;
                    }
                }

                SelectedItem = newItem;
                ICollectionView view = CollectionViewSource.GetDefaultView(ImageItems);
                view.Refresh();
            }
        }

        private void UpdateObjectInDataGridView(ImageOwner editedObj, int oldNum)
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

                if (context.Entry(item.Owner).State != EntityState.Added)
                    context.Entry(item.Owner).State = EntityState.Modified;

                // Если номер изменился, перемещаем строку
                if (oldNum != editedObj.Number)
                {
                    MoveRowAndUpdateOrder(ImageItems, oldNum - 1, editedObj.Number.Value - 1);
                }

                // Уведомления
                item.OnPropertyChanged(nameof(item.Owner));
                OnPropertyChanged(nameof(ImageItems));
                ICollectionView view = CollectionViewSource.GetDefaultView(ImageItems);
                view.Refresh();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void MoveRowAndUpdateOrder(ObservableCollection<ImageItem> items, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex || oldIndex < 0 || oldIndex >= items.Count)
                return;

            // Корректируем newIndex если он за пределами
            newIndex = Math.Max(0, Math.Min(newIndex, items.Count - 1));

            var itemToMove = items[oldIndex];
            items.RemoveAt(oldIndex);
            items.Insert(newIndex, itemToMove);

            // Обновляем номера всех элементов
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Owner.Number != i + 1)
                {
                    items[i].Owner.Number = i + 1;
                    if (context.Entry(items[i].Owner).State != EntityState.Added)
                        context.Entry(items[i].Owner).State = EntityState.Modified;
                }
            }

            // Обновляем привязки
            OnPropertyChanged(nameof(ImageItems));
            var view = CollectionViewSource.GetDefaultView(ImageItems);
            view.Refresh();
        }

       


        private void BtnDeleteImage_Click(object sender, RoutedEventArgs e)
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
                // Удаляем связи
                imageOwner.ExecutionWorks.Clear();
                imageOwner.DiagramShags.Clear();

                // Обработка состояния Added
                if (context.Entry(imageOwner).State == EntityState.Added)
                {
                    // Для объектов в состоянии Added просто отсоединяем их
                    context.Entry(imageOwner).State = EntityState.Detached;

                    // Удаляем ImageStorage, если он тоже в состоянии Added
                    if (imageStorage != null && context.Entry(imageStorage).State == EntityState.Added)
                    {
                        context.Entry(imageStorage).State = EntityState.Detached;
                    }
                    else if(imageStorage != null && context.Entry(imageStorage).State == EntityState.Unchanged)
                    {
                        context.Entry(imageStorage).State = EntityState.Deleted;
                    }
                }
                else
                {
                    // Для существующих объектов помечаем на удаление
                    context.Entry(imageOwner).State = EntityState.Deleted;
                    context.Entry(imageStorage).State = EntityState.Deleted;
                    
                }

                // Удаляем из коллекций
                tc.ImageOwner.Remove(imageOwner);
                ImageItems.Remove(SelectedItem);

                // Обновляем нумерацию оставшихся элементов
                foreach (var item in ImageItems.Where(i => i.Owner.Number > imageOwner.Number))
                {
                    item.Owner.Number--;
                    if (context.Entry(item.Owner).State != EntityState.Added)
                        context.Entry(item.Owner).State = EntityState.Modified;
                }

                // Удаляем из _imageHolder.ImageList
                if (_imageHolder != null)
                    _imageHolder.ImageList.RemoveAll(i => i == imageOwner);

                SelectedItem = null;

                ICollectionView view = CollectionViewSource.GetDefaultView(ImageItems);
                view.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
