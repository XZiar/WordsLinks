using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WordsLinks.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public BaseViewModel()
        {
        }
    }

    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        public void Refresh()
        {
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

}

