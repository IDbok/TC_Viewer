using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram
{
    /// <summary>
    /// Логика взаимодействия для WpfShag.xaml
    /// </summary>
    public partial class WpfShag : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private readonly TcViewState _tcViewState;

        private TechOperationWork selectedItem;
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
        //public WpfShag( TcViewState tcViewState)
        //{
        //    InitializeComponent();
        //    DataContext = this;

        //    _tcViewState = tcViewState;

        //    _tcViewState.CommentViewModeChanged += OnCommentViewModeChanged;
        //    _tcViewState.ViewModeChanged += OnViewModeChanged;

        //    // Обновление привязки
        //    OnPropertyChanged(nameof(IsCommentViewMode));
        //    OnPropertyChanged(nameof(IsViewMode));
        //    OnPropertyChanged(nameof(IsHiddenInViewMode));
        //}
        private void OnCommentViewModeChanged()
        {
            OnPropertyChanged(nameof(IsCommentViewMode));
        }
        private void OnViewModeChanged()
        {
            OnPropertyChanged(nameof(IsHiddenInViewMode));
            OnPropertyChanged(nameof(IsViewMode));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Отписка от событий, чтобы избежать утечек памяти
        //~WpfShag()
        //{
        //    _tcViewState.CommentViewModeChanged -= OnCommentViewModeChanged;
        //    _tcViewState.ViewModeChanged -= OnViewModeChanged;
        //}

        public void SaveCollection()
        {
            if (diagramShag != null)
            {
                diagramShag.Deystavie = TextDeystShag.Text;
            }


            diagramShag.ImplementerComment = txtImplementerComment.Text;
            diagramShag.LeadComment = txtLeadComment.Text;

            var allVB = AllItemGrid.Where(w => w.Add).ToList();
            diagramShag.ListDiagramShagToolsComponent = new List<DiagramShagToolsComponent>();
            foreach (var item in allVB)
            {
                DiagramShagToolsComponent diagramShagToolsComponent = new DiagramShagToolsComponent();
                if(item.toolWork!=null)
                {
                    diagramShagToolsComponent.toolWork = item.toolWork;
                }
                else
                {
                    diagramShagToolsComponent.componentWork = item.componentWork;
                }

                try
                {
                    diagramShagToolsComponent.Quantity = double.Parse(item.AddText);
                }
                catch (Exception)
                {
                    diagramShagToolsComponent.Quantity = 0;
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
            WpfPosledovatelnost _wpfPosledovatelnost, 
            TcViewState tcViewState,
            DiagramShag _diagramShag=null)
        {
            InitializeComponent();

            _tcViewState = tcViewState;

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

                        ChangeImageVisibility(true);
                        var byt = Convert.FromBase64String(diagramShag.ImageBase64);
                        var bn = LoadImage(byt);
                        imageDiagram.Source = bn;
                    }
                    else
                    {
                        ChangeImageVisibility(false);
                    }
                }
                catch (Exception)
                {

                }

                DataContext = this;
                _tcViewState.CommentViewModeChanged += OnCommentViewModeChanged;

            }


            this.selectedItem = selectedItem;
            wpfPosledovatelnost = _wpfPosledovatelnost;

            if (selectedItem != null)
            {
                ComboBoxTeh.ItemsSource = selectedItem.executionWorks;
            }          

            AllItemGrid = new ObservableCollection<ItemDataGridShagAdd>();

            foreach (var item in selectedItem.ToolWorks)
            {
                ItemDataGridShagAdd itemDataGrid = new ItemDataGridShagAdd();
                itemDataGrid.Name = item.tool.Name;
                itemDataGrid.Type = item.tool.Type??"";
                itemDataGrid.Unit = item.tool.Unit;
                itemDataGrid.Count = item.Quantity.ToString();
                itemDataGrid.Comments = item.Comments.ToString();
                itemDataGrid.toolWork = item;
                itemDataGrid.AddText = "";

                System.Windows.Media.Brush brush = new SolidColorBrush(Colors.SkyBlue);
                itemDataGrid.BrushBackground = brush;

                AllItemGrid.Add(itemDataGrid);
            }

            foreach (var item in selectedItem.ComponentWorks)
            {
                ItemDataGridShagAdd itemDataGrid = new ItemDataGridShagAdd();
                itemDataGrid.Name = item.component.Name;
                itemDataGrid.Type = item.component.Type ?? "";
                itemDataGrid.Unit = item.component.Unit;
                itemDataGrid.Count = item.Quantity.ToString();
                itemDataGrid.Comments = item.Comments??"";
                itemDataGrid.componentWork = item;
                itemDataGrid.AddText = "";

                System.Windows.Media.Brush brush = new SolidColorBrush(Colors.LightPink);
                itemDataGrid.BrushBackground = brush;

                AllItemGrid.Add(itemDataGrid);
            }

            if(diagramShag.ListDiagramShagToolsComponent.Count>0)
            {
                foreach (DiagramShagToolsComponent itemS in diagramShag.ListDiagramShagToolsComponent)
                {
                    if(itemS.toolWork!=null)
                    {
                        var ty = AllItemGrid.SingleOrDefault(s => s.toolWork == itemS.toolWork);
                        if(ty!=null)
                        {
                            ty.Add = true;
                            ty.AddText = itemS.Quantity.ToString();
                        }
                    }

                    if (itemS.componentWork != null)
                    {
                        var ty = AllItemGrid.SingleOrDefault(s => s.componentWork == itemS.componentWork);
                        if (ty != null)
                        {
                            ty.Add = true;
                            ty.AddText = itemS.Quantity.ToString();
                        }
                    }

                }
            }


            DataGridToolAndComponentsAdd.ItemsSource = AllItemGrid;

            var vb = AllItemGrid.Where(w => w.Add).ToList();
            DataGridToolAndComponentsShow.ItemsSource = vb;

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
            if(ComboBoxTeh.SelectedItem != null)
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
                ChangeImageVisibility(false);
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
                wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            wpfPosledovatelnost.diagramPosledov.ListDiagramShag.Remove(diagramShag);
            wpfPosledovatelnost.DeleteItem(this);
            
            wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            wpfPosledovatelnost.Vniz(this);
            wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            wpfPosledovatelnost.Verh(this);
            wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
        }


        private void TG_Click(object sender, RoutedEventArgs e)
        {
            if(TG.IsChecked==true)
            {
                DataGridToolAndComponentsAdd.Visibility= Visibility.Visible;
                DataGridToolAndComponentsShow.Visibility= Visibility.Collapsed;
                wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
            }
            else
            {
                DataGridToolAndComponentsAdd.Visibility = Visibility.Collapsed;
                DataGridToolAndComponentsShow.Visibility = Visibility.Visible;

                var vb = AllItemGrid.Where(w=>w.Add).ToList();
                DataGridToolAndComponentsShow.ItemsSource = vb;
                wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
            }
        }

        private void TextDeystShag_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (diagramShag != null)
            {
                diagramShag.Deystavie = TextDeystShag.Text;

                if (wpfPosledovatelnost != null)
                {
                    wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
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
                    wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
                }
            }
        }

        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            ChangeImageVisibility(true);
            Image_MouseLeftButtonDown(sender, null);

        }
        private void ChangeImageVisibility(bool imageVisibility)
        {
            imageDiagram.Visibility = imageVisibility ? Visibility.Visible : Visibility.Collapsed;
            gridImageName.Visibility = imageVisibility ? Visibility.Visible : Visibility.Collapsed;

            if (IsViewMode)
            {
                
                return;
            }

            btnLoadImage.Visibility = imageVisibility ? Visibility.Collapsed : Visibility.Visible;
            btnDeleteImage.Visibility = imageVisibility ? Visibility.Visible : Visibility.Collapsed;
            //if(imageVisibility)
            //    imageDiagram.Source = "/WinForms/Diagram/Select.jpg";
        }
        
        private void btnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            //diagramShag.ImageBase64 = "";
            ChangeImageVisibility(false);
            //wpfPosledovatelnost.wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
        }

    }
}
