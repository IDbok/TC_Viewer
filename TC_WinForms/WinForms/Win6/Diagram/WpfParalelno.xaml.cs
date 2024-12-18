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
	/// Логика взаимодействия для WpfParalelno.xaml
	/// </summary>
	public partial class WpfParalelno : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private readonly DiagramState _diagramState;

        private readonly TcViewState _tcViewState;

        private TechOperationWork selectedItem;
       public WpfControlTO wpfControlTO;

        public DiagramParalelno diagramParalelno;
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
        public WpfParalelno()
        {
            InitializeComponent();
        }

        //public WpfParalelno(TechOperationWork selectedItem, DiagramState diagramState, DiagramParalelno _diagramParalelno = null) 
        //    : this(selectedItem, diagramState.WpfControlTO, diagramState.TcViewState, _diagramParalelno)
        //{
        //    _diagramState = new DiagramState(diagramState);
        //    _diagramState.WpfParalelno = this;
        //}

        //[Obsolete("Данный конструктор устарел, следует использовать конструктор с DiagramState")]
        //public WpfParalelno(TechOperationWork selectedItem, WpfControlTO _wpfControlTO, TcViewState tcViewState, DiagramParalelno _diagramParalelno = null)
        public WpfParalelno(TechOperationWork selectedItem, DiagramState diagramState, DiagramParalelno _diagramParalelno = null)
        {
            InitializeComponent();
            DataContext = this;
            this.IsEnabled = true;

            _diagramState = new DiagramState(diagramState);
            _diagramState.WpfParalelno = this;

            _tcViewState = _diagramState.TcViewState;

            var _wpfControlTO = _diagramState.WpfControlTO;

            if (_diagramParalelno == null)
            {
                diagramParalelno = new DiagramParalelno();
                diagramParalelno.techOperationWork = selectedItem;
                _wpfControlTO.diagamToWork.ListDiagramParalelno.Add(diagramParalelno);
            }
            else
            {
                diagramParalelno = _diagramParalelno;
            }

            this.selectedItem = selectedItem;
            wpfControlTO = _wpfControlTO;

            ListWpfPosledovatelnost.Children.Clear();

            if (diagramParalelno.ListDiagramPosledov.Count == 0)
            {
                ListWpfPosledovatelnost.Children.Add(new WpfPosledovatelnost(selectedItem, _diagramState!));
                    //this, _tcViewState));
            }
            else
            {
                foreach (DiagramPosledov diagramPosledov in diagramParalelno.ListDiagramPosledov.OrderBy(x => x.Order))
                {
                    ListWpfPosledovatelnost.Children.Add(new WpfPosledovatelnost(selectedItem, _diagramState!, diagramPosledov));
                    //this, _tcViewState, diagramPosledov));
                }
            }

            wpfControlTO.Nomeraciya();

            _tcViewState.ViewModeChanged += OnViewModeChanged;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfPosledovatelnost.Children.Add(new WpfPosledovatelnost(selectedItem, _diagramState!));
                //this, _tcViewState));
            wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
            wpfControlTO.Nomeraciya();
        }

        public void DeletePosledovatelnost(WpfPosledovatelnost wpfPosledovatelnost)
        {
            ListWpfPosledovatelnost.Children.Remove(wpfPosledovatelnost);
            
            wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;

            if (ListWpfPosledovatelnost.Children.Count==0)
            {
                wpfControlTO.diagamToWork.ListDiagramParalelno.Remove(diagramParalelno);
                wpfControlTO.DeteteParalelno(this);
            }
        }


        internal void Order(int v, WpfPosledovatelnost wpfPosledovatelnost)
        {
           if(v==1)
            {
                int ib = ListWpfPosledovatelnost.Children.IndexOf(wpfPosledovatelnost);
                if (ib < ListWpfPosledovatelnost.Children.Count - 1)
                {
                    var cv = ListWpfPosledovatelnost.Children[ib + 1];
                    ListWpfPosledovatelnost.Children.Remove(cv);
                    ListWpfPosledovatelnost.Children.Insert(ib, cv);

                    wpfControlTO.Nomeraciya();
                }
            }

           if(v==2)
            {
                int ib = ListWpfPosledovatelnost.Children.IndexOf(wpfPosledovatelnost);
                if (ib != 0)
                {
                    var cv = ListWpfPosledovatelnost.Children[ib];
                    ListWpfPosledovatelnost.Children.Remove(cv);
                    ListWpfPosledovatelnost.Children.Insert(ib - 1, cv);
                    wpfControlTO.Nomeraciya();
                }
            }

        }

        internal void Nomeraciya()
        {
            wpfControlTO.Nomeraciya();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            wpfControlTO.Order(1, this);

            wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            wpfControlTO.Order(2, this);
            wpfControlTO._wpfMainControl.diagramForm.HasChanges = true;
        }

        public void UpdateAllShag()
        {
            foreach (WpfPosledovatelnost wpfPosledovatelnost in ListWpfPosledovatelnost.Children)
            {

                wpfPosledovatelnost.UpdateAllShag();
            }
        }
    }
}
