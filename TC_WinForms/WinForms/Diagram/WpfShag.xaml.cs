using System;
using System.Collections.Generic;
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

            ComboBoxComp.ItemsSource = selectedItem.ComponentWorks;
            ComboBoxTool.ItemsSource = selectedItem.ToolWorks;

            ListBoxAll.Items.Clear();

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

        private void ComboBoxComp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxComp.SelectedItem != null)
            {
                var itm = (ComponentWork)ComboBoxComp.SelectedItem;
                ListBoxAll.Items.Add(CraeteItem(itm.component.Name));
                ComboBoxComp.SelectedItem = null;
            }
        }

        private void ComboBoxTool_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxTool.SelectedItem != null)
            {
                var itm = (ToolWork)ComboBoxTool.SelectedItem;
                ListBoxAll.Items.Add(CraeteItem(itm.tool.Name));
                ComboBoxTool.SelectedItem = null;
            }
        }

        public object CraeteItem(string name)
        {
            StackPanel sp = new StackPanel();
            sp.Orientation = System.Windows.Controls.Orientation.Horizontal;

            sp.Children.Add(new TextBlock { Text = name });

            var but = new System.Windows.Controls.Button();
            but.Content = "X";
            but.Margin = new Thickness(10,0,0,0);
            but.Tag = sp;
            but.Click += But_Click;

            sp.Children.Add(but);

            return sp;
        }

        private void But_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ListBoxAll.Items.Remove(((System.Windows.Controls.Button)sender).Tag);
            }
            catch (Exception)
            {

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
    }
}
