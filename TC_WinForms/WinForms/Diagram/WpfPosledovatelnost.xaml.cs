﻿using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для WpfPosledovatelnost.xaml
    /// </summary>
    public partial class WpfPosledovatelnost : System.Windows.Controls.UserControl
    {
        private TechOperationWork selectedItem;
       public WpfParalelno wpfParalelno;

       public DiagramPosledov diagramPosledov;

        public WpfPosledovatelnost()
        {
            InitializeComponent();
        }

        public WpfPosledovatelnost(TechOperationWork selectedItem, WpfParalelno _wpfParalelno, DiagramPosledov _diagramPosledov=null)
        {
            InitializeComponent();

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
                    ListWpfShag.Children.Add(new WpfShag(selectedItem, this));
                }
            }
            else
            {
                foreach (DiagramShag diagramShag in diagramPosledov.ListDiagramShag)
                {
                    ListWpfShag.Children.Add(new WpfShag(selectedItem, this, diagramShag));
                }
            }


            wpfParalelno.Nomeraciya();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListWpfShag.Children.Add(new WpfShag(selectedItem, this));
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
    }
}
