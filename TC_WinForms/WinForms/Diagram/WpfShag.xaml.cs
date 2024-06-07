using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram
{
    /// <summary>
    /// Логика взаимодействия для WpfShag.xaml
    /// </summary>
    public partial class WpfShag : System.Windows.Controls.UserControl
    {
        private TechOperationWork selectedItem;
        WpfPosledovatelnost wpfPosledovatelnost;

        int Nomer = 0;

        ObservableCollection<ItemDataGridShagAdd> AllItemGrid;

        public WpfShag()
        {
            InitializeComponent();
        }


        public void SetNomer(int nomer)
        {
            TextShag.Text = $"№{nomer} шага";
            TextTable.Text = $"№{nomer} таблицы";
            TextImage.Text = $"№{nomer} рисунка";
        }

        public WpfShag(TechOperationWork selectedItem, WpfPosledovatelnost _wpfPosledovatelnost)
        {
            InitializeComponent();
            this.selectedItem = selectedItem;
            wpfPosledovatelnost = _wpfPosledovatelnost;

            ComboBoxTeh.ItemsSource = selectedItem.executionWorks;
                        

            AllItemGrid = new ObservableCollection<ItemDataGridShagAdd>();

            foreach (var item in selectedItem.ToolWorks)
            {
                ItemDataGridShagAdd itemDataGrid = new ItemDataGridShagAdd();
                itemDataGrid.Name = item.tool.Name;
                itemDataGrid.Type = item.tool.Type??"";
                itemDataGrid.Unit = item.tool.Unit;
                itemDataGrid.Count = item.Quantity.ToString();
                itemDataGrid.Comments = item.Comments.ToString();
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
                itemDataGrid.AddText = "";

                System.Windows.Media.Brush brush = new SolidColorBrush(Colors.LightPink);
                itemDataGrid.BrushBackground = brush;

                AllItemGrid.Add(itemDataGrid);
            }

            DataGridToolAndComponentsAdd.ItemsSource = AllItemGrid;
        }



        private void ComboBoxTeh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ComboBoxTeh.SelectedItem != null)
            {
                var work = (ExecutionWork)ComboBoxTeh.SelectedItem;

                TextDeystShag.Text += $"\n-{work?.techTransition?.Name}";

                ComboBoxTeh.SelectedItem = null;
            }
        }             


        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            // получаем выбранный файл
            string filename = openFileDialog1.FileName;

            System.Drawing.Image imag;

            bool isImageFile;
            try
            {
                imag = System.Drawing.Image.FromFile(filename);
                isImageFile = true;
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
            wpfPosledovatelnost.DeleteItem(this);
        }

        private void TG_Click(object sender, RoutedEventArgs e)
        {
            if(TG.IsChecked==true)
            {
                DataGridToolAndComponentsAdd.Visibility= Visibility.Visible;
                DataGridToolAndComponentsShow.Visibility= Visibility.Collapsed;
            }
            else
            {
                DataGridToolAndComponentsAdd.Visibility = Visibility.Collapsed;
                DataGridToolAndComponentsShow.Visibility = Visibility.Visible;

                var vb = AllItemGrid.Where(w=>w.Add).ToList();
                DataGridToolAndComponentsShow.ItemsSource = vb;
            }
        }
    }
}
