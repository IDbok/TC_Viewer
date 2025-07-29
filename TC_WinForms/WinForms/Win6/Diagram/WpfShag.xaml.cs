using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Win6.ImageEditor;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace TC_WinForms.WinForms.Diagram
{
	/// <summary>
	/// Логика взаимодействия для WpfShag.xaml
	/// </summary>
	public partial class WpfShag : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private readonly DiagramState _diagramState;
        private readonly TcViewState _tcViewState;
        public Dictionary<long?, string> _allPrintedTcDict = new Dictionary<long?, string>();


        private TechOperationWork? techOperationWork => _diagramState.DiagramToWork?.techOperationWork;
        WpfPosledovatelnost wpfPosledovatelnost;

        //Ивент для обновления изображений во всех шагах
        public static event EventHandler ImageUpdated;

        public event PropertyChangedEventHandler? PropertyChanged;
        public bool IsCommentViewMode => _tcViewState.IsCommentViewMode;
        public bool IsViewMode => _tcViewState.IsViewMode;
        public bool IsHiddenInViewMode => !IsViewMode;

        int Nomer = 0;

        ObservableCollection<ItemDataGridShagAdd> AllItemGrid;


        public DiagramShag diagramShag;
        public WpfShag()
        {
            InitializeComponent();
        }
        private void OnCommentViewModeChanged()
        {
            OnPropertyChanged(nameof(IsCommentViewMode));
        }
        private void OnViewModeChanged()
        {
            OnPropertyChanged(nameof(IsHiddenInViewMode));
            OnPropertyChanged(nameof(IsViewMode));

            ChangeImageVisibility();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SaveCollection()
        {
            if (diagramShag == null)
            {
                return;
            }

            diagramShag.Deystavie = TextDeystShag.Text;

            if(!diagramShag.IsRemarkClosed)
            {
                diagramShag.Reply = txtImplementerComment.Text;
                diagramShag.Remark = txtLeadComment.Text;
            }

            UpdateToolsComponentsList();
        }

        private void UpdateToolsComponentsList() // todo: зачем-то вызывается перед закрытием формы. Проверить нужно ли это
        {

            diagramShag.ListDiagramShagToolsComponent = new List<DiagramShagToolsComponent>();

            var allVB = AllItemGrid.Where(w => w.Add).ToList();
            foreach (var item in allVB)
            {
                DiagramShagToolsComponent diagramShagToolsComponent = new DiagramShagToolsComponent();
                if (item.toolWork != null)
                {
                    diagramShagToolsComponent.toolWork = item.toolWork;
                }
                else
                {
                    diagramShagToolsComponent.componentWork = item.componentWork;
                }

                try
                {
                    if(string.IsNullOrEmpty(item.AddText))
                    {
                        diagramShagToolsComponent.Quantity = 0;
                    }
                    else
                    {
                        diagramShagToolsComponent.Quantity = double.Parse(item.AddText);
                    }
                }
                catch
                {
                    // неверный формат данный
                    diagramShagToolsComponent.Quantity = 0;
                }

                var prevComment = diagramShagToolsComponent.toolWork != null ? diagramShagToolsComponent.toolWork.Comments : diagramShagToolsComponent.componentWork.Comments;

                if (prevComment != item.Comments)
                {
                    diagramShagToolsComponent.Comment = item.Comments;
                }

                diagramShag.ListDiagramShagToolsComponent.Add(diagramShagToolsComponent);
            }
        }

        public void SetNomer(int nomer)
        {
            if (diagramShag != null)
            {
                diagramShag.Nomer = nomer;
            }

            TextShag.Text = $"Шаг {nomer}";
            TextTable.Text = $"Таблица {nomer}";
            //TextImage.Text = $"Рисунок {nomer}";
        }

        public WpfShag(TechOperationWork selectedItem,
            DiagramState diagramState,
            DiagramShag? _diagramShag = null)
            : this(selectedItem,
                diagramState.WpfPosledovatelnost ?? throw new ArgumentNullException(nameof(diagramState.WpfPosledovatelnost)),
                diagramState.TcViewState, _diagramShag)
        {
            _diagramState = new DiagramState(diagramState);

            ImageUpdated += OnGlobalImageUpdated;

            if (diagramState.DiagramToWork?.techOperationWork != null)
            {
                //techOperationWork = diagramState.DiagramToWork.techOperationWork;

                UpdateDataGrids();
            }

            if (diagramShag != null)
            {
                UpdateRemarkDisplay();
                btnToggleRemark.Content = diagramShag.IsRemarkClosed ? "Открыть замечание" : "Закрыть замечание";
            }
        }
        [Obsolete("Данный конструктор устарел, следует использовать конструктор с DiagramState")]
        public WpfShag(TechOperationWork selectedItem,
            WpfPosledovatelnost _wpfPosledovatelnost,
            TcViewState tcViewState,
            DiagramShag _diagramShag = null)
        {
            InitializeComponent();


            DataContext = this;

            _tcViewState = tcViewState;

            _tcViewState.CommentViewModeChanged += OnCommentViewModeChanged;
            _tcViewState.ViewModeChanged += OnViewModeChanged;

            // Обновление привязки
            OnPropertyChanged(nameof(IsCommentViewMode));
            OnPropertyChanged(nameof(IsViewMode));
            OnPropertyChanged(nameof(IsHiddenInViewMode));

            if (_diagramShag == null)
            {
                diagramShag = new DiagramShag();
                _wpfPosledovatelnost.diagramPosledov.ListDiagramShag.Add(diagramShag);
            }
            else
            {
                diagramShag = _diagramShag;

                try
                {
                    TextDeystShag.Text = diagramShag.Deystavie.ToString();
                    txtLeadComment.Text = diagramShag.Remark ?? "";
                    txtImplementerComment.Text = diagramShag.Reply ?? "";
                }
                catch (Exception)
                {

                }

                try
                {
                    //TBNameImage.Text = diagramShag.NameImage.ToString();
                }
                catch (Exception)
                {

                }


                try
                {
                    if (diagramShag.ImageBase64 != "")
                    {
                        var byt = Convert.FromBase64String(diagramShag.ImageBase64);
                        var bn = LoadImage(byt);
                        //imageDiagram.Source = bn;

                        ChangeImageVisibility();//true);
                    }
                    else
                    {
                        ChangeImageVisibility();//false);
                    }
                }
                catch (Exception)
                {

                }

                //DataContext = this;
                //_tcViewState.CommentViewModeChanged += OnCommentViewModeChanged;

            }

            wpfPosledovatelnost = _wpfPosledovatelnost;

            CommentAccess();

            //SetIndxesToDiagramShag();

		}

        //     private void SetIndxesToDiagramShag()
        //     {
        //         if (_diagramState != null)
        //         {
        //             diagramShag.ParallelIndex = _diagramState?.WpfPosledovatelnost?.diagramPosledov.DiagramParalelnoId.ToString();
        //             diagramShag.SequenceIndex = _diagramState?.WpfParalelno?.diagramParalelno.Id.ToString();
        //}
        //     }

        public static void RaiseImageUpdated(object sender)
        {
            ImageUpdated?.Invoke(sender, EventArgs.Empty);
        }

        private void OnGlobalImageUpdated(object sender, EventArgs e)
        {
                // Обновляем изображения в текущем шаге
                RefreshImagePanel();
            
        }

        //Отписываемся от события при удалении шага
        ~WpfShag()
        {
            ImageUpdated -= OnGlobalImageUpdated;
        }

        public void UpdateDataGrids()
        {
            //var techOperationWork = this.techOperationWork;

            if (techOperationWork == null)
                return;


            // Создаем коллекцию для всех элементов
            AllItemGrid = new ObservableCollection<ItemDataGridShagAdd>();

            // Добавляем ToolWorks
            foreach (var toolWork in techOperationWork.ToolWorks)
            {
                var item = new ItemDataGridShagAdd
                {
                    Name = toolWork.tool.Name,
                    Type = toolWork.tool.Type ?? "",
                    Unit = toolWork.tool.Unit,
                    Count = toolWork.Quantity.ToString(),
                    Comments = toolWork.Comments ?? "",
                    toolWork = toolWork,
                    AddText = "",
                    BrushBackground = new SolidColorBrush(Colors.SkyBlue)
                };
                AllItemGrid.Add(item);
            }

            // Добавляем ComponentWorks
            foreach (var componentWork in techOperationWork.ComponentWorks)
            {
                var item = new ItemDataGridShagAdd
                {
                    Name = componentWork.component.Name,
                    Type = componentWork.component.Type ?? "",
                    Unit = componentWork.component.Unit,
                    Count = componentWork.Quantity.ToString(),
                    Comments = componentWork.Comments ?? "",
                    componentWork = componentWork,
                    AddText = "",
                    BrushBackground = new SolidColorBrush(Colors.LightPink)
                };
                AllItemGrid.Add(item);
            }

            // Обновление добавленных элементов
            foreach (var diagramShagToolsComponent in diagramShag.ListDiagramShagToolsComponent)
            {
                var existingItem = diagramShagToolsComponent.toolWork != null
                    ? AllItemGrid.SingleOrDefault(i => i.toolWork == diagramShagToolsComponent.toolWork)
                    : AllItemGrid.SingleOrDefault(i => i.componentWork == diagramShagToolsComponent.componentWork);

                if (existingItem != null)
                {
                    existingItem.Add = true;
                    existingItem.AddText = diagramShagToolsComponent.Quantity.ToString();
                    existingItem.Comments = diagramShagToolsComponent.Comment ?? existingItem.Comments;
                }
            }

            SetRelatedListBox();

            // Добавление шагов в ComboBox
            ComboBoxTeh.ItemsSource = techOperationWork.executionWorks;

            // Устанавливаем источники данных для DataGrid
            DataGridToolAndComponentsAdd.ItemsSource = AllItemGrid;
            DataGridToolAndComponentsShow.ItemsSource = AllItemGrid.Where(i => i.Add).ToList();

            if(diagramShag.ImageList.Count !=0)
                RefreshImagePanel();
        }

        private void SetRelatedListBox()
        {
            var relatedTCs = _tcViewState.TechOperationWorksList
                .SelectMany(t => t.executionWorks)
                .Where(e => e.RepeatsTCId != null)
                .Select(e => e.RepeatsTCId)
                .Distinct()
                .ToList();

            using (MyDbContext context = new MyDbContext())
            {
                relatedTCs.ForEach
                           (id =>
                           {
                               var tcArticle = context.TechnologicalCards
                               .Where(x => x.Id == id)
                               .Select(x => $"{x.Article}")
                               .FirstOrDefault();
                               _allPrintedTcDict.Add(id, $"{tcArticle}");
                           }
                           );
            }

            if(_allPrintedTcDict.Count == 0)
            {
                ComboBoxRelatedTc.IsEnabled = false;
                BtnOpenRelatedTC.IsEnabled = false;
            }
            else
                ComboBoxRelatedTc.ItemsSource = _allPrintedTcDict;
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void ComboBoxTeh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxTeh.SelectedItem != null)
            {
                var work = (ExecutionWork)ComboBoxTeh.SelectedItem;

                TextDeystShag.Text += $"\n-{work?.techTransition?.Name}. {work?.Comments};";

                ComboBoxTeh.SelectedItem = null;
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                ChangeImageVisibility();// false);
                return;
            }
            // получаем выбранный файл
            string filename = openFileDialog1.FileName;

            System.Drawing.Image imag;

            bool isImageFile;
            try
            {
                ImageStorage newImage = ImageService.CreateNewImageFromBase64(filename);
                ImageOwner imageOwner = ImageService.CreateNewImageOwner(newImage, techOperationWork?.technologicalCard, ImageRole.Image, (int)diagramShag.ImageList.Max(i => i.Number) + 1);
                diagramShag.ImageList.Add(imageOwner);
                imageOwner.DiagramShags.Add(diagramShag);
                imag = System.Drawing.Image.FromFile(filename);
                //_diagramState.WpfMainControl._diagramForm.wpfDiagram._dbContext.ImageOwners.Add(imageOwner);
                //_diagramState.WpfMainControl._diagramForm.wpfDiagram._dbContext.ImageStorage.Add(newImage);
                _diagramState.HasChanges();
            }
            catch (OutOfMemoryException)
            {
                isImageFile = false;

                System.Windows.Forms.MessageBox.Show("Файл не является картинкой");

                return;
            }

            //imageDiagram.Source = ToImageSource(imag);
        }

        public ImageSource ToImageSource(System.Drawing.Image image)
        {
            BitmapImage bitmap = new BitmapImage();

            using (MemoryStream stream = new MemoryStream())
            {
                // Save to the stream
                image.Save(stream, ImageFormat.Jpeg);

                // Rewind the stream
                stream.Seek(0, SeekOrigin.Begin);

                // Tell the WPF BitmapImage to use this stream
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            return bitmap;
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.Forms.MessageBox.Show("Вы действительно хотите удалить шаг?", 
                "Удаление шага", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                wpfPosledovatelnost.diagramPosledov.ListDiagramShag.Remove(diagramShag);
                wpfPosledovatelnost.DeleteItem(this);

                _diagramState.HasChanges();//wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
            }
            else
                return;
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            wpfPosledovatelnost.Vniz(this);
            _diagramState.HasChanges();  //wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
        }


        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            wpfPosledovatelnost.Verh(this);
            _diagramState.HasChanges();  //wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
        }


        private void TG_Click(object sender, RoutedEventArgs e)
        {
            if (TG.IsChecked == true)
            {
                DataGridToolAndComponentsAdd.Visibility = Visibility.Visible;
                DataGridToolAndComponentsShow.Visibility = Visibility.Collapsed;
                _diagramState.HasChanges(); //wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
            }
            else
            {
                DataGridToolAndComponentsAdd.Visibility = Visibility.Collapsed;
                DataGridToolAndComponentsShow.Visibility = Visibility.Visible;

                if (AllItemGrid != null && AllItemGrid.Count > 0)
                {
                    var vb = AllItemGrid.Where(w => w.Add).ToList();
                    DataGridToolAndComponentsShow.ItemsSource = vb;

                    UpdateToolsComponentsList();

                    _diagramState.HasChanges(); //wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
                }
            }
        }

        private void TextDeystShag_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (diagramShag != null)
            {
                diagramShag.Deystavie = TextDeystShag.Text;

                if (wpfPosledovatelnost != null)
                {
                    _diagramState.HasChanges(); //wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
                }
            }
        }

        private void TextLeadComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (diagramShag != null && !diagramShag.IsRemarkClosed)
            {
                diagramShag.Remark = txtLeadComment.Text;
                if (wpfPosledovatelnost != null)
                {
                    _diagramState.HasChanges();
                }
            }
        }

        private void TextImplementerComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (diagramShag != null && !diagramShag.IsRemarkClosed)
            {
                diagramShag.Reply = txtImplementerComment.Text;
                if (wpfPosledovatelnost != null)
                {
                    _diagramState.HasChanges();
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (diagramShag != null)
            {
                //diagramShag.NameImage = TBNameImage.Text;
                if (wpfPosledovatelnost != null)
                {
                    _diagramState.HasChanges(); //wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
                }
            }
        }
        private void TextBox_CommentTextChanged(object sender, TextChangedEventArgs e)
        {
            if (diagramShag != null)
            {
                ItemDataGridShagAdd gridItem = (ItemDataGridShagAdd)(((System.Windows.Controls.TextBox)sender).DataContext);

                if (gridItem != null)
                {
                    gridItem.Comments = ((System.Windows.Controls.TextBox)sender).Text;
                }

                if (wpfPosledovatelnost != null)
                {
                    _diagramState.HasChanges(); //wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
                }
            }
        }
        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            //ChangeImageVisibility();// true);
            Image_MouseLeftButtonDown(sender, null);
            ChangeImageVisibility();
        }
        private void ChangeImageVisibility()
        {
            var isImage = diagramShag.ImageList.Count > 0;

            //imageDiagram.Visibility = isImage ? Visibility.Visible : Visibility.Collapsed;
            //gridImageName.Visibility = isImage ? Visibility.Visible : Visibility.Collapsed;

            if (IsViewMode)
            {
                btnEditImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnEditImage.Visibility = Visibility.Visible;
            }
        }
        private void CommentAccess()
        {
            if (diagramShag.IsRemarkClosed)
            {
                txtLeadComment.IsEnabled = false;
                txtImplementerComment.IsEnabled = false;
                return;
            }

            if (_tcViewState.UserRole == User.Role.Lead)
            {
                txtLeadComment.IsReadOnly = false;
                txtLeadComment.IsEnabled = true;

                txtImplementerComment.IsReadOnly = false;
                txtImplementerComment.IsEnabled = true;
            }
            else if (_tcViewState.UserRole == User.Role.Implementer)
            {
                txtLeadComment.IsReadOnly = true;
                txtLeadComment.IsEnabled = false;

                txtImplementerComment.IsReadOnly = false;
                txtImplementerComment.IsEnabled = true;
            }
        }

        private void btnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            diagramShag.ImageBase64 = "";
            //imageDiagram.Source = null;
            ChangeImageVisibility();// false);

            _diagramState.HasChanges();
            //_tcViewState.WpfMainControl.diagramForm.HasChanges = true;
        }


        /// <summary>
        /// Передаём событие прокрутки полученное элементом родительскому элементу.
        /// Убираем "залипание" при прокрутке.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridToolAndComponentsShow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Останавливаем прокрутку самого DataGrid
            e.Handled = true;

            // Поднимаем событие вверх по визуальному дереву для прокрутки формы
            var parent = ((DataGrid)sender).Parent as UIElement;
            if (parent != null)
            {
                parent.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent
                });
            }
        }


        private void ComboBoxTeh_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!ComboBoxTeh.IsDropDownOpen)
            {
                e.Handled = true;
                var parent = ((System.Windows.Controls.ComboBox)sender).Parent as UIElement;
                parent?.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent
                });
            }
        }


        public void btnAddNewShag_Click(object sender, RoutedEventArgs e)
		{
			// проверить налиние более одного шага в последовательности
			var diagramPosledovChildren = _diagramState.WpfPosledovatelnost?.ListWpfShag.Children;
            if (diagramPosledovChildren == null) return;
            if(diagramPosledovChildren.Count > 1)
			{
				// если более одного шага, то добавить новый шаг в последовательность, если это не последний шаг
				if (diagramPosledovChildren[diagramPosledovChildren.Count - 1] != this)
				{ 
                    // получить позицию текущего шага в последовательности
					var index = diagramPosledovChildren.IndexOf(this);

					if (index == -1) return;

					_diagramState.WpfPosledovatelnost?.AddNewShag(index: ++index);

                    return;
				}
			}

			// если шаг в последовательности один или это последний шаг в последовательности,
			// то добавить новый шаг вне последовательности

			var shagConteiner = _diagramState.WpfParalelno;
			if (shagConteiner == null) return;
			var shagConteinerIndex = _diagramState.WpfControlTO?.Children.IndexOf(shagConteiner);
			if (shagConteinerIndex == null) return;

            if (_diagramState.WpfControlTO?.Children.Count == shagConteinerIndex + 1)
			{
				_diagramState.WpfControlTO?.AddNewShag();
			}
            else
            {
                _diagramState.WpfControlTO?.AddNewShag(++shagConteinerIndex);
            }
		}

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // Предотвращаем стандартное поведение Enter

                var dataGrid = sender as DataGrid;
                if (dataGrid == null) return;

                var currentCell = dataGrid.CurrentCell;
                var currentRowIndex = dataGrid.Items.IndexOf(currentCell.Item);
                var currentColumn = currentCell.Column;

                // Проверяем, есть ли следующая строка
                if (currentRowIndex + 1 < dataGrid.Items.Count)
                {
                    // Завершаем редактирование текущей ячейки и строки
                    dataGrid.CommitEdit(DataGridEditingUnit.Cell, true);

                    int nextRowIndex = currentRowIndex + 1;
                    var nextItem = dataGrid.Items[nextRowIndex];

                    dataGrid.CurrentCell = new DataGridCellInfo(nextItem, currentColumn);
                    dataGrid.SelectedItem = nextItem;

                    // Запускаем редактирование новой ячейки
                    dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Получаем ячейку
                        DataGridCell cell = GetCell(dataGrid, nextRowIndex, currentColumn.DisplayIndex);
                        if (cell != null)
                        {
                            //пытаемся найти TextBox внутри ячейки
                            var textBox = FindVisualChild<TextBox>(cell);
                            if (textBox != null)
                            {
                                textBox.Focus();
                                textBox.SelectAll();
                            }
                        }
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }
            }
        }

        private DataGridCell GetCell(DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            var rowContainer = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            if (rowContainer == null) return null;

            var presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
            if (presenter == null) return null;

            return presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;
                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }


        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Проверяем, что вводится только цифра или точка (для дробных чисел)
            if (!char.IsDigit(e.Text, e.Text.Length - 1) && e.Text != ".")
            {
                e.Handled = true; // Блокируем ввод
                return;
            }

            // Если вводится точка, проверяем, что её ещё нет в тексте
            var textBox = (TextBox)sender;
            if (e.Text == "." && textBox.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Разрешаем Backspace, Delete, стрелки и т.д.
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right)
            {
                return;
            }

            // Запрещаем пробел
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!text.All(c => char.IsDigit(c) || c == '.'))
                {
                    e.CancelCommand(); // Отменяем вставку
                }
            }
            else
            {
                e.CancelCommand();
            }
        }// Отменяем вставку, если это не текст

        private void btnEditImage_Click(object sender, RoutedEventArgs e)
        {
            var editor = new Win6_ImageEditor(diagramShag, _tcViewState,
                            _diagramState.WpfMainControl._diagramForm.wpfDiagram._dbContext);
            // Подписываемся на событие закрытия окна
            editor.Closed += (s, args) =>
            {
                // Обновляем панель с изображениями после закрытия редактора
                //RefreshImagePanel();
                ImageUpdated?.Invoke(this, EventArgs.Empty);
                // Помечаем изменения в состоянии
                _diagramState.HasChanges();
            };

            editor.Show();
        }

        private void RefreshImagePanel()
        {
            ImagePanel.Children.Clear();
            ImagePanel.Children.Add(btnEditImage);

            if (diagramShag?.ImageList == null || !diagramShag.ImageList.Any())
            {
                // Если изображений нет, можно добавить placeholder или просто выйти
                return;
            }

            // Предполагаем, что у вас есть текущее состояние шага с коллекцией ImageList
            // Разделяем изображения на две группы и сортируем
            var regularImages = diagramShag.ImageList
                .Where(img => img.Role == ImageRole.Image)
                .OrderBy(img => img.Number)
                .ToList();

            var schemeImages = diagramShag.ImageList
                .Where(img => img.Role == ImageRole.ExecutionScheme)
                .OrderBy(img => img.Number)
                .ToList();

            // Объединяем группы: сначала обычные изображения, потом схемы исполнения
            var orderedImages = regularImages.Concat(schemeImages).ToList();

            foreach (var owner in orderedImages)
            {
                try
                {
                    // контейнер с рамкой
                    var border = new Border
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(5),
                        Padding = new Thickness(5),
                        CornerRadius = new CornerRadius(4),
                        Background = System.Windows.Media.Brushes.White
                    };

                    // внутренняя панель
                    var stack = new StackPanel { Orientation = System.Windows.Controls.Orientation.Vertical };

                    // Определяем префикс в зависимости от типа изображения
                    string prefix = owner.Role == ImageRole.ExecutionScheme ? "Схема исполнения" : "Рисунок";

                    // 1) Номер + имя
                    var header = new TextBlock
                    {
                        Text = $"{prefix} {owner.Number}: {owner.Name ?? "Без названия"}",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 5),
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center
                    };
                    stack.Children.Add(header);

                    // Загрузка и отображение изображения
                    byte[] imageBytes = Convert.FromBase64String(owner.ImageStorage.ImageBase64);
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();

                        var imageControl = new System.Windows.Controls.Image
                        {
                            Source = bitmap,
                            MaxHeight = 450,
                            MaxWidth = 600,
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(0, 5, 0, 0)
                        };
                        stack.Children.Add(imageControl);
                    }

                    border.Child = stack;
                    ImagePanel.Children.Add(border);
                }
                catch (Exception ex)
                {
                    // В случае ошибки добавляем сообщение об ошибке
                    ImagePanel.Children.Add(new TextBlock
                    {
                        Text = $"Ошибка загрузки изображения {owner.Number}: {ex.Message}",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }


            }
        }

        private void BtnOpenRelatedTC_Click(object sender, RoutedEventArgs e)
        {
            long? selectedKey = ComboBoxRelatedTc.SelectedValue as long?;
            if (selectedKey != null)
            {
                var openedForm = CheckOpenFormService.FindOpenedForm<Win6_new>((int)selectedKey.Value);
                if (openedForm != null)
                {
                    openedForm.BringToFront();
                }
                else
                {
                    var a = new Win6_new((int)selectedKey, _tcViewState.UserRole, viewMode: true, startForm: EModelType.Diagram);
                    a.Show();
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var dataItem = (ItemDataGridShagAdd)((System.Windows.Controls.CheckBox)sender).DataContext;
            if(string.IsNullOrEmpty(dataItem.AddText))
            {
                dataItem.AddText = dataItem.componentWork == null ? dataItem.toolWork.Quantity.ToString() : dataItem.componentWork.Quantity.ToString();
            }
        }

        private void BtnToggleRemark_Click(object sender, RoutedEventArgs e)
        {
            // Переключаем состояние замечания
            diagramShag.IsRemarkClosed = !diagramShag.IsRemarkClosed;

            // Обновляем отображение
            UpdateRemarkDisplay();

            // Сохраняем изменения
            _diagramState.HasChanges();

            // Обновляем текст кнопки
            btnToggleRemark.Content = diagramShag.IsRemarkClosed ? "Открыть замечание" : "Закрыть замечание";
        }

        private void UpdateRemarkDisplay()
        {
            if (diagramShag.IsRemarkClosed)
            {
                // Показываем "Замечание закрыто" без изменения оригинальных данных
                txtLeadComment.Text = "Замечание закрыто";
                txtImplementerComment.Text = "Замечание закрыто";

                // Делаем текст серым
                txtLeadComment.Foreground = System.Windows.Media.Brushes.Gray;
                txtImplementerComment.Foreground = System.Windows.Media.Brushes.Gray;

                // Блокируем редактирование
                txtLeadComment.IsEnabled = false;
                txtImplementerComment.IsEnabled = false;
            }
            else
            {
                // Восстанавливаем оригинальные значения
                txtLeadComment.Text = diagramShag.Remark ?? "";
                txtImplementerComment.Text = diagramShag.Reply ?? "";

                // Восстанавливаем цвет
                txtLeadComment.Foreground = System.Windows.SystemColors.ControlTextBrush;
                txtImplementerComment.Foreground = System.Windows.SystemColors.ControlTextBrush;

                // Восстанавливаем доступность в зависимости от роли
                CommentAccess();
            }
        }
    }
}
