﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.WinForms.Diagram
{
    public class ItemDataGridShagAdd : INotifyPropertyChanged
    {
        bool _add;
        string _AddText;
        public bool Add
        {
            get { return _add; }
            set
            {
                _add = value;
                OnPropertyChanged("Add");
            }
        }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string Count { get; set; }
        public string Comments { get; set; }
        public string AddText
        {
            get { return _AddText; }
            set
            {
                _AddText = value;
                OnPropertyChanged("AddText");
            }
        }

        public System.Windows.Media.Brush BrushBackground { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        //protected void OnPropertyChanged(string name)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        //}

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
