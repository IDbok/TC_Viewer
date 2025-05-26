using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        #region Fields

        private string techCardName = "ТестоваяТк";
        private int TcId = 451;
        private readonly IImageHoldable _imageHolder;
        private MyDbContext context;
        private TechnologicalCard tc;
        private ObservableCollection<ImageItem> _imageItems;
        private ImageItem _selectedItem;
        private bool _hasUnsavedChanges = false;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region ImageItem class

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
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            internal ImageOptionsControl _parentControl;
        }

        #endregion

        #region Properties
        public bool HasUnsavedChanges => _hasUnsavedChanges;
        public ObservableCollection<ImageItem> ImageItems
        {
            get => _imageItems;
            set
            {
                _imageItems = value;
                OnPropertyChanged();
            }
        }

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

        public event Action<ImageOwner>? AfterSave;

        #endregion

        #region Constructor

        public ImageOptionsControl(TechnologicalCard technologicalCard, MyDbContext context, IImageHoldable? imageHolder = null, bool IsWindowEditor = true)
        {
            InitializeComponent();
            DataContext = this;
            this._imageHolder = imageHolder;
            this.techCardName = technologicalCard.Article;
            this.context = context;
            this.tc = technologicalCard;
            LoadData(technologicalCard.ImageOwner);
            lblTcName.Text = techCardName;

            if (!IsWindowEditor && ImageDataGrid.Columns.Count > 0)
            {
                ImageDataGrid.Columns.RemoveAt(0);
            }
        }

        #endregion

        #region UI Events

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

                var current = SelectedItem;
                SelectedItem = null;
                SelectedItem = current;

                await Task.CompletedTask;
            };
            objEditor.ShowDialog();
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
                    else if (imageStorage != null && context.Entry(imageStorage).State == EntityState.Unchanged)
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

                _hasUnsavedChanges = true;
                SelectedItem = null;

                ICollectionView view = CollectionViewSource.GetDefaultView(ImageItems);
                view.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Load & Update

        private void LoadData(List<ImageOwner> items)
        {
            try
            {
                var regularImages = items.Where(i => i.ImageRoleType != ImageType.ExecutionScheme).OrderBy(i => i.Number).ToList();
                var schemeImages = items.Where(i => i.ImageRoleType == ImageType.ExecutionScheme).OrderBy(i => i.Number).ToList();

                var combinedList = new List<ImageItem>();

                if (_imageHolder != null)
                {
                    var holderIds = _imageHolder.ImageList?.ToHashSet() ?? new HashSet<ImageOwner>();

                    combinedList.AddRange(regularImages.Select(i => new ImageItem { Owner = i, IsSelected = holderIds.Contains(i), _parentControl = this }));
                    combinedList.AddRange(schemeImages.Select(i => new ImageItem { Owner = i, IsSelected = holderIds.Contains(i), _parentControl = this }));
                }
                else
                {
                    combinedList.AddRange(regularImages.Select(i => new ImageItem { Owner = i, IsSelected = false, _parentControl = this }));
                    combinedList.AddRange(schemeImages.Select(i => new ImageItem { Owner = i, IsSelected = false, _parentControl = this }));
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
                using var ms = new MemoryStream(imageBytes);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                ImageHolder.Source = bitmap;

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

        #endregion

        #region Image Manipulation (Add/Edit/Delete)

        private void AddObjectInDataGridView(ImageOwner addedObj)
        {
            if (addedObj == null) return;

            bool isScheme = addedObj.ImageRoleType == ImageType.ExecutionScheme;
            int baseIndex = isScheme
                ? ImageItems.Count(i => i.Owner.ImageRoleType == ImageType.ExecutionScheme)
                : ImageItems.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme);

            if (addedObj.Number <= 0 || addedObj.Number > baseIndex + 1)
                addedObj.Number = baseIndex + 1;

            var newItem = new ImageItem { Owner = addedObj, _parentControl = this };

            if (addedObj.ImageStorage != null)
                context.Entry(addedObj.ImageStorage).State = EntityState.Added;

            context.Entry(addedObj).State = EntityState.Added;

            int insertIndex = isScheme
                ? ImageItems.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme) + addedObj.Number.Value - 1
                : addedObj.Number.Value - 1;

            insertIndex = Math.Min(insertIndex, ImageItems.Count);
            ImageItems.Insert(insertIndex, newItem);
            tc.ImageOwner.Add(addedObj);

            UpdateAllImageNumbers(ImageItems);
            SelectedItem = newItem;

            _hasUnsavedChanges = true;
            CollectionViewSource.GetDefaultView(ImageItems).Refresh();
        }

        private void UpdateObjectInDataGridView(ImageOwner editedObj, int oldNum)
        {
            var item = SelectedItem;
            if (item == null) return;

            bool wasScheme = item.Owner.ImageRoleType == ImageType.ExecutionScheme;
            bool isNowScheme = editedObj.ImageRoleType == ImageType.ExecutionScheme;

            item.Owner.Name = editedObj.Name;
            item.Owner.Number = editedObj.Number;
            item.Owner.ImageRoleType = editedObj.ImageRoleType;

            if (item.Owner.ImageStorage.IsChanged)
            {
                item.Owner.ImageStorage = editedObj.ImageStorage;
                context.Entry(item.Owner.ImageStorage).State =
                    item.Owner.ImageStorage.Id == 0 ? EntityState.Added : EntityState.Modified;
            }

            if (context.Entry(item.Owner).State != EntityState.Added)
                context.Entry(item.Owner).State = EntityState.Modified;

            if (wasScheme != isNowScheme || oldNum != editedObj.Number)
            {
                int newPosition = isNowScheme
                    ? ImageItems.Count - ImageItems.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme) + Math.Min(editedObj.Number.Value - 1, ImageItems.Count(i => i.Owner.ImageRoleType == ImageType.ExecutionScheme))
                    : Math.Min(editedObj.Number.Value - 1, ImageItems.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme));

                MoveRowAndUpdateOrder(ImageItems, ImageItems.IndexOf(item), newPosition);
            }

            if (isNowScheme)
                ImageHelper.SaveImageToTempFile(editedObj.ImageStorage.ImageBase64, tc.Id);

            _hasUnsavedChanges = true;
            CollectionViewSource.GetDefaultView(ImageItems).Refresh();
        }

        #endregion

        #region Helpers

        private void MoveRowAndUpdateOrder(ObservableCollection<ImageItem> items, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex || oldIndex < 0 || oldIndex >= items.Count)
                return;

            var itemToMove = items[oldIndex];
            bool isScheme = itemToMove.Owner.ImageRoleType == ImageType.ExecutionScheme;

            int regularImagesCount = items.Count(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme);

            newIndex = isScheme
                ? Math.Max(regularImagesCount, Math.Min(newIndex, items.Count - 1))
                : Math.Max(0, Math.Min(newIndex, regularImagesCount - 1));

            items.RemoveAt(oldIndex);
            items.Insert(newIndex, itemToMove);

            UpdateAllImageNumbers(items);
            OnPropertyChanged(nameof(ImageItems));
            CollectionViewSource.GetDefaultView(ImageItems).Refresh();
        }

        private void UpdateAllImageNumbers(ObservableCollection<ImageItem> items)
        {
            int regularNumber = 1;
            foreach (var item in items.Where(i => i.Owner.ImageRoleType != ImageType.ExecutionScheme))
            {
                if (item.Owner.Number != regularNumber)
                {
                    item.Owner.Number = regularNumber;
                    if (context.Entry(item.Owner).State != EntityState.Added)
                        context.Entry(item.Owner).State = EntityState.Modified;
                }
                regularNumber++;
            }

            int schemeNumber = 1;
            foreach (var item in items.Where(i => i.Owner.ImageRoleType == ImageType.ExecutionScheme))
            {
                if (item.Owner.Number != schemeNumber)
                {
                    item.Owner.Number = schemeNumber;
                    if (context.Entry(item.Owner).State != EntityState.Added)
                        context.Entry(item.Owner).State = EntityState.Modified;
                }
                schemeNumber++;
            }
        }

        public void CommitChanges()
        {
            // Здесь можно вызвать SaveChanges
            context.SaveChanges();
            _hasUnsavedChanges = false;
        }

        public void DiscardChanges()
        {
            // Если нужно откатить изменения вручную
            foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }

            LoadData(tc.ImageOwner); // Перезагрузить UI
            _hasUnsavedChanges = false;
        }
        #endregion
    }
}
