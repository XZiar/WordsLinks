using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WordsLinks.UWP.ViewModel
{
    class NavPageItem : INotifyPropertyChanged
    {
        public string Source { get; set; }
        public string Label { get; set; }
        public Type PageType { get; set; }
        public Page Page { get; set; }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                this.OnPropertyChanged(nameof(SelectedVis));
            }
        }

        public Visibility SelectedVis
        {
            get { return IsSelected? Visibility.Visible : Visibility.Collapsed; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
