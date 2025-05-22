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
using TC_WinForms.DataProcessing.Helpers;
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
                // Разделяем изображения на две группы
                var regularImages = items.Where(i => i.ImageRoleType != ImageType.ExecutionScheme)
                                       .OrderBy(i => i.Number)
                                       .ToList();

                var schemeImages = items.Where(i => i.ImageRoleType == ImageType.ExecutionScheme)
                                      .OrderBy(i => i.Number)
                                      .ToList();

                // Создаем объединенный список (обычные изображения сначала, схемы в конце)
                var combinedList = new List<ImageItem>();

                if (_imageHolder != null)
                {
                    var holderIds = _imageHolder?.ImageList?.ToHashSet() ?? new HashSet<ImageOwner>();

                    // Добавляем обычные изображения
                    combinedList.AddRange(regularImages.Select(i => new ImageItem
                    {
                        Owner = i,
                        IsSelected = holderIds.Contains(i),
                        _parentControl = this
                    }));

                    // Добавляем схемы выполнения
                    combinedList.AddRange(schemeImages.Select(i => new ImageItem
                    {
                        Owner = i,
                        IsSelected = holderIds.Contains(i),
                        _parentControl = this
                    }));
                }
                else
                {
                    combinedList.AddRange(regularImages.Select(i => new ImageItem
                    {
                        Owner = i,
                        IsSelected = false,
                        _parentControl = this
                    }));

                    combinedList.AddRange(schemeImages.Select(i => new ImageItem
                    {
                        Owner = i,
                        IsSelected = false,
                        _parentControl = this
                    }));
                }

                ImageItems = new ObservableCollection<ImageItem>(combinedList);
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
                // Определяем, в какую группу попадает новое изображение
                bool isScheme = addedObj.ImageRoleType == ImageType.ExecutionScheme;

                // Определяем базовый индекс для вставки
                int baseIndex = isScheme ?
                    ImageItems.Count(i => i.Owner.ImageRoleType == ImageType.ExecutionScheme) :
                    ImageItems.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme);

                // Проверяем, что номер не выходит за допустимые границы
                if (addedObj.Number <= 0 || addedObj.Number > baseIndex + 1)
                {
                    addedObj.Number = baseIndex + 1;
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

                // Находим правильную позицию для вставки
                int insertIndex = isScheme ?
                    ImageItems.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme) + addedObj.Number.Value - 1 :
                    addedObj.Number.Value - 1;

                insertIndex = Math.Min(insertIndex, ImageItems.Count);
                ImageItems.Insert(insertIndex, newItem);
                tc.ImageOwner.Add(addedObj);

                // Корректируем номера в соответствующей группе
                for (int i = 0; i < ImageItems.Count; i++)
                {
                    var currentItem = ImageItems[i];
                    bool currentIsScheme = currentItem.Owner.ImageRoleType == ImageType.ExecutionScheme;

                    if (currentIsScheme == isScheme &&
                        i != insertIndex &&
                        currentItem.Owner.Number != (currentIsScheme ? i - ImageItems.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme) + 1 : i + 1))
                    {
                        currentItem.Owner.Number = currentIsScheme ?
                            i - ImageItems.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme) + 1 :
                            i + 1;

                        if (context.Entry(currentItem.Owner).State != EntityState.Added)
                            context.Entry(currentItem.Owner).State = EntityState.Modified;
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
                bool wasScheme = item.Owner.ImageRoleType == ImageType.ExecutionScheme;
                bool isNowScheme = editedObj.ImageRoleType == ImageType.ExecutionScheme;

                // Обновляем свойства
                item.Owner.Name = editedObj.Name;
                item.Owner.Number = editedObj.Number;
                item.Owner.ImageRoleType = editedObj.ImageRoleType;

                if (item.Owner.ImageStorage.IsChanged)
                {
                    item.Owner.ImageStorage = editedObj.ImageStorage;
                    if (item.Owner.ImageStorage != null)
                    {
                        context.Entry(item.Owner.ImageStorage).State =
                            item.Owner.ImageStorage.Id == 0 ? EntityState.Added : EntityState.Modified;
                    }
                }

                if (context.Entry(item.Owner).State != EntityState.Added)
                    context.Entry(item.Owner).State = EntityState.Modified;

                // Если тип изображения изменился или номер изменился, перемещаем строку
                if (wasScheme != isNowScheme || oldNum != editedObj.Number)
                {
                    // Находим новую позицию
                    int newPosition;
                    if (isNowScheme)
                    {
                        // Перемещаем в группу схем
                        int schemeCount = ImageItems.Count(i => i.Owner.ImageRoleType == ImageType.ExecutionScheme);
                        newPosition = ImageItems.Count - schemeCount + Math.Min(editedObj.Number.Value - 1, schemeCount);
                    }
                    else
                    {
                        // Перемещаем в обычную группу
                        newPosition = Math.Min(editedObj.Number.Value - 1, ImageItems.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme));
                    }

                    MoveRowAndUpdateOrder(ImageItems, ImageItems.IndexOf(item), newPosition);
                }

                if(isNowScheme)
                    ImageHelper.SaveImageToTempFile(editedObj.ImageStorage.ImageBase64, tc.Id);


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

            var itemToMove = items[oldIndex];
            bool isScheme = itemToMove.Owner.ImageRoleType == ImageType.ExecutionScheme;

            // Ограничиваем newIndex в пределах соответствующей группы
            if (isScheme)
            {
                int schemeStartIndex = items.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme);
                newIndex = Math.Max(schemeStartIndex, Math.Min(newIndex, items.Count - 1));
            }
            else
            {
                int regularEndIndex = items.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme) - 1;
                newIndex = Math.Max(0, Math.Min(newIndex, regularEndIndex));
            }

            items.RemoveAt(oldIndex);
            items.Insert(newIndex, itemToMove);

            // Обновляем номера в соответствующей группе
            int groupStart = isScheme ? items.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme) : 0;
            int groupCount = isScheme ? items.Count - groupStart : groupStart;

            for (int i = groupStart; i < groupStart + groupCount; i++)
            {
                if (items[i].Owner.Number != i - groupStart + 1)
                {
                    items[i].Owner.Number = i - groupStart + 1;
                    if (context.Entry(items[i].Owner).State != EntityState.Added)
                        context.Entry(items[i].Owner).State = EntityState.Modified;
                }
            }

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
