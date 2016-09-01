using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xamarin.Forms;
using static Main.Util.BasicUtils;

namespace WordsLinks.ViewModel
{
    public class TextViewModel : BaseViewModel, IComparable<TextViewModel>
    {
        string text;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        bool isshow = true;
        public bool IsShow
        {
            get { return isshow; }
            set
            {
                isshow = value;
                OnPropertyChanged(nameof(IsShow));
            }
        }

        public TextViewModel() { }
        public TextViewModel(string str)
        {
            text = str;
        }

        public int CompareTo(TextViewModel other)
        {
            return text.CompareTo(other.text);
        }
    }

    public class WordGroupViewModel : SortedSet<TextViewModel>
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
        private string lastStr;
        private IEnumerable<string> bak = new string[0];
        private ObservableCollectionEx<WordGroupViewModel> datas = new ObservableCollectionEx<WordGroupViewModel>();

        public static SortedSet<TextViewModel> MakeData(IEnumerable<string> strs)
        {
            var ret = new SortedSet<TextViewModel>();
            foreach (var s in strs)
                ret.Add(new TextViewModel(s));
            return ret;
        }

        public WordGroupHelper()
        {
            for (char str = 'A'; str <= 'Z'; str++)
                datas.Add(new WordGroupViewModel() { Prefix = str.ToString() });
        }

        public void SetTo(ListView lv)
        {
            lv.IsGroupingEnabled = true;
            lv.ItemsSource = datas;
        }

        public void Set(IEnumerable<string> data)
        {
            bak = data;
            Search(null);
        }

        public bool Search(string str, bool isForce = false)
        {
            bool isGetLock = false;
            do 
            {
                isGetLock = Monitor.TryEnter(datas);
            }
            while (isForce && !isGetLock);
            if (!isGetLock)
                return false;
            try //Enter Lock
            {
                if (str != null && str == lastStr)
                    return true;
                if (string.IsNullOrWhiteSpace(str))//always reconstruct all
                {
                    foreach (var s in bak)
                        datas[GetPrefixIndex(s)].Add(new TextViewModel(s));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(lastStr) || str.Contains(lastStr))//append str, more restrict
                    {
                        foreach (var s in bak.Where(x => !x.Contains(str)))
                            datas[GetPrefixIndex(s)].Remove(new TextViewModel(s));
                    }
                    else
                    {
                        if (!lastStr.Contains(str))//no relation, reconstruct
                            foreach (var g in datas)
                                g.Clear();
                        foreach (var s in bak.Where(x => x.Contains(str)))
                            datas[GetPrefixIndex(s)].Add(new TextViewModel(s));
                    }
                }
                lastStr = str;
                datas.Refresh();
                return true;
            }
            finally //Leave Lock
            {
                Monitor.Exit(datas);
            }
        }
    }
}
