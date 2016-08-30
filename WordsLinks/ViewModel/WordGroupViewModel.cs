using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace WordsLinks.ViewModel
{
    public class WordGroupViewModel : List<string>
    {
        public string Prefix { get; set; }
        public string Title
        {
            get { return Prefix; }
            set { Prefix = value; }
        }
    }

    public class WordGroupHelper
    {
        private WordGroupViewModel[] groups = new WordGroupViewModel[26];
        private ObservableCollection<WordGroupViewModel> datas = new ObservableCollection<WordGroupViewModel>();
        private ListView list;

        public WordGroupHelper()
        {
            int a = 0;
            for (char str = 'A'; str <= 'Z'; str++)
                groups[a++] = new WordGroupViewModel() { Prefix = str.ToString() };
        }

        public void SetTo(ListView lv)
        {
            list = lv;
            list.IsGroupingEnabled = true;
            list.ItemsSource = datas;
        }

        public void Set(IEnumerable<string> data)
        {
            datas.Clear();
            foreach (var g in groups)
                g.Clear();
            foreach (var s in data)
                groups[s.ToLower()[0] - 'a'].Add(s);
            foreach (var g in groups)
                datas.Add(g);
        }
    }
}
