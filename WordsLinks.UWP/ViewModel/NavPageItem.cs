using System;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace WordsLinks.UWP.ViewModel
{
    class NavPageItem : INotifyPropertyChanged
    {
        public string Source { get; set; }
        public string Label { get; set; }
        public Type PageType { get; set; }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(SelectedVis));
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
