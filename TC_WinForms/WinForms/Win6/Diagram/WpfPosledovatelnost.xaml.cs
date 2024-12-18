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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TC_WinForms.WinForms.Win6.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram
{
	/// <summary>
	/// Логика взаимодействия для WpfPosledovatelnost.xaml
	/// </summary>
	public partial class WpfPosledovatelnost : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private readonly DiagramState _diagramState;
        private readonly TcViewState _tcViewState;

        private TechOperationWork selectedItem;
        public WpfParalelno wpfParalelno;

        public DiagramPosledov diagramPosledov;
        public bool IsHiddenInViewMode => !_tcViewState.IsViewMode;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnViewModeChanged()
        {
            OnPropertyChanged(nameof(IsHiddenInViewMode));
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public WpfPosledovatelnost()
        {
            InitializeComponent();
        }

        //public WpfPosledovatelnost(TechOperationWork selectedItem, DiagramState diagramState, DiagramPosledov _diagramPosledov = null)
        //    : this(selectedItem, diagramState.WpfParalelno ?? throw new ArgumentNullException(nameof(diagramState.WpfParalelno))
        //          , diagramState.TcViewState, _diagramPosledov)
        //{
        //    _diagramState = new DiagramState(diagramState);
        //    _diagramState.WpfPosledovatelnost = this;
        //}
        //[Obsolete("Данный конструктор устарел, следует использовать конструктор с DiagramState")]
        //public WpfPosledovatelnost(TechOperationWork selectedItem, WpfParalelno _wpfParalelno, TcViewState tcViewState, DiagramPosledov _diagramPosledov=null)
        public WpfPosledovatelnost(TechOperationWork selectedItem, DiagramState diagramState, DiagramPosledov _diagramPosledov = null)
        {
            InitializeComponent();
            DataContext = this;

            _diagramState = new DiagramState(diagramState);
            _diagramState.WpfPosledovatelnost = this;

            _tcViewState = _diagramState.TcViewState;

            var _wpfParalelno = _diagramState.WpfParalelno ?? throw new ArgumentNullException(nameof(_diagramState.WpfParalelno));

            if (_diagramPosledov == null)
            {
                diagramPosledov = new DiagramPosledov();
                _wpfParalelno.diagramParalelno.ListDiagramPosledov.Add(diagramPosledov);
            }
            else
            {
                diagramPosledov = _diagramPosledov;

            }

            this.selectedItem = selectedItem;
            wpfParalelno = _wpfParalelno;

            ListWpfShag.Children.Clear();

            if (diagramPosledov.ListDiagramShag.Count == 0)
            {
                if (selectedItem != null)
                {
                    ListWpfShag.Children.Add(new WpfShag(selectedItem, _diagramState!));
                        //this, _tcViewState));
                }
            }
            else
            {
                foreach (DiagramShag diagramShag in diagramPosledov.ListDiagramShag.OrderBy(x => x.Order))
                {
                    ListWpfShag.Children.Add(new WpfShag(selectedItem, _diagramState!, diagramShag));
                    //this, _tcViewState, diagramShag));
                }
            }


            wpfParalelno.Nomeraciya();
            _tcViewState.ViewModeChanged += OnViewModeChanged;
            //OnViewModeChanged();
            Console.WriteLine( "Posled " + IsHiddenInViewMode);
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfShag.Children.Add(new WpfShag(selectedItem, _diagramState!));
            //this, _tcViewState));
            wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;

            wpfParalelno.Nomeraciya();
        }

        public void DeleteItem(WpfShag Item)
        {
            ListWpfShag.Children.Remove(Item);            

            wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;

            if (ListWpfShag.Children.Count==0)
            {
                wpfParalelno.diagramParalelno.ListDiagramPosledov.Remove(diagramPosledov);
                wpfParalelno.DeletePosledovatelnost(this);
            }

            wpfParalelno.Nomeraciya();
        }

        public void Vniz(WpfShag wpfShag)
        {
            int ib = ListWpfShag.Children.IndexOf(wpfShag);
            if(ib < ListWpfShag.Children.Count-1 )
            {
                var cv = ListWpfShag.Children[ib + 1];
                ListWpfShag.Children.Remove(cv);
                ListWpfShag.Children.Insert(ib, cv);

                wpfParalelno.Nomeraciya();
            }
        }

        public void Verh(WpfShag wpfShag)
        {
            int ib = ListWpfShag.Children.IndexOf(wpfShag);
            if (ib != 0)
            {
                var cv = ListWpfShag.Children[ib];
                ListWpfShag.Children.Remove(cv);
                ListWpfShag.Children.Insert(ib-1, cv);
                wpfParalelno.Nomeraciya();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            wpfParalelno.Order(1, this);
            wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            wpfParalelno.Order(2, this);
            wpfParalelno.wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
        }

        public void UpdateAllShag()
        {
            foreach (WpfShag wpfShag in ListWpfShag.Children)
            {
                wpfShag.UpdateDataGrids();
            }
        }
    }
}
