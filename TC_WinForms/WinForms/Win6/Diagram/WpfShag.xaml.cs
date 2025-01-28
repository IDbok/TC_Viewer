using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TC_WinForms.WinForms.Win6.Models;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms.Diagram
{
	/// <summary>
	/// Логика взаимодействия для WpfShag.xaml
	/// </summary>
	public partial class WpfShag : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private readonly DiagramState _diagramState;
        private readonly TcViewState _tcViewState;


        private TechOperationWork? techOperationWork => _diagramState.DiagramToWork?.techOperationWork;
        WpfPosledovatelnost wpfPosledovatelnost;

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

            diagramShag.ImplementerComment = txtImplementerComment.Text;
            diagramShag.LeadComment = txtLeadComment.Text;

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
            TextImage.Text = $"Рисунок {nomer}";
        }

        public WpfShag(TechOperationWork selectedItem,
            DiagramState diagramState,
            DiagramShag? _diagramShag = null)
            : this(selectedItem,
                diagramState.WpfPosledovatelnost ?? throw new ArgumentNullException(nameof(diagramState.WpfPosledovatelnost)),
                diagramState.TcViewState, _diagramShag)
        {
            _diagramState = new DiagramState(diagramState);
            if (diagramState.DiagramToWork?.techOperationWork != null)
            {
                //techOperationWork = diagramState.DiagramToWork.techOperationWork;

                UpdateDataGrids();
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
                    txtLeadComment.Text = diagramShag.LeadComment ?? "";
                    txtImplementerComment.Text = diagramShag.ImplementerComment ?? "";
                }
                catch (Exception)
                {

                }

                try
                {
                    TBNameImage.Text = diagramShag.NameImage.ToString();
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
                        imageDiagram.Source = bn;

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

            // Добавление шагов в ComboBox
            ComboBoxTeh.ItemsSource = techOperationWork.executionWorks;

            // Устанавливаем источники данных для DataGrid
            DataGridToolAndComponentsAdd.ItemsSource = AllItemGrid;
            DataGridToolAndComponentsShow.ItemsSource = AllItemGrid.Where(i => i.Add).ToList();
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
                imag = System.Drawing.Image.FromFile(filename);
                isImageFile = true;

                byte[] bytes = File.ReadAllBytes(filename);
                string base64 = Convert.ToBase64String(bytes);
                diagramShag.ImageBase64 = base64;

                _diagramState.HasChanges();
            }
            catch (OutOfMemoryException)
            {
                isImageFile = false;

                System.Windows.Forms.MessageBox.Show("Файл не является картинкой");

                return;
            }

            imageDiagram.Source = ToImageSource(imag);
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (diagramShag != null)
            {
                diagramShag.NameImage = TBNameImage.Text;
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
            var isImage = imageDiagram.Source != null;

            imageDiagram.Visibility = isImage ? Visibility.Visible : Visibility.Collapsed;
            gridImageName.Visibility = isImage ? Visibility.Visible : Visibility.Collapsed;

            if (IsViewMode)
            {
                btnLoadImage.Visibility = Visibility.Collapsed;
                btnDeleteImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnLoadImage.Visibility = isImage ? Visibility.Collapsed : Visibility.Visible;
                btnDeleteImage.Visibility = isImage ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void CommentAccess()
        {
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
            imageDiagram.Source = null;
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
	}
}
