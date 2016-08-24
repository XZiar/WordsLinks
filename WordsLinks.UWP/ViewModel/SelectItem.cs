using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static WordsLinks.UWP.ViewModel.SelectItemGroup.SelectEventArgs;

namespace WordsLinks.UWP.ViewModel
{
    public class SelectItem : INotifyPropertyChanged
    {
        public string Text { get; set; }

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
            get { return IsSelected ? Visibility.Visible : Visibility.Collapsed; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SelectItemGroup
    {
        public class SelectEventArgs : EventArgs
        {
            public enum Message { Selecting, DataChanged, Selected };
            public Message msg { get; set; } = Message.Selecting;
            public SelectItem obj { get; internal set; }
            public int idx { get; internal set; }
            public bool isSelect { get; set; }
        }
        private ListView list;
        public List<SelectItem> datas { get; private set; } = new List<SelectItem>();
        private bool MultiSelect, NullSelect;
        public delegate void SelectHandler(object sender, SelectEventArgs e);
        public event SelectHandler Select;

        public IEnumerable<string> SelectedItems
        {
            get { return from x in datas where (x.IsSelected == true) select x.Text; }
        }

        public SelectItemGroup(bool mulSel = false, bool nullSel = false)
        {
            MultiSelect = mulSel;
            NullSelect = nullSel;
        }

        public void Set(IEnumerable<string> data)
        {
            datas = new List<SelectItem>();
            foreach (var str in data)
                datas.Add(new SelectItem() { Text = str });
            list.ItemsSource = datas;
            Select?.Invoke(this, new SelectEventArgs() { msg = Message.DataChanged });
        }

        public void SetTo(ListView list)
        {
            this.list = list;
            list.ItemsSource = datas;
            list.ItemClick += OnItemClicked;
        }

        private void OnItemClicked(object sender, ItemClickEventArgs e)
        {
            if (sender != null)
                (sender as ListView).SelectedItem = null;
            Choose(e.ClickedItem as SelectItem);
        }

        public void ChooseNone()
        {
            if (NullSelect)
                foreach (var i in datas)
                    i.IsSelected = false;
        }

        public void Choose(SelectItem item)
        {
            var idx = datas.IndexOf(item);
            //Debug.WriteLine($"choose {item} is at {idx}");
            if (idx == -1)
                return;
            Choose(new SelectEventArgs()
            {
                obj = item,
                idx = idx,
                isSelect = !item.IsSelected,
            });
        }

        public void Choose(int idx)
        {
            if (datas.Count <= idx)
                return;
            var obj = datas[idx];
            Choose(new SelectEventArgs()
            {
                obj = obj,
                idx = idx,
                isSelect = !obj.IsSelected,
            });
        }

        private void Choose(SelectEventArgs e)
        {
            try
            {
                bool newStat = e.isSelect;
                //Debug.WriteLine($"choose {e.idx} one to {e.isSelect}, multi={MultiSelect}, null={NullSelect}");
                if (!newStat && !NullSelect)//deselect
                {
                    if (MultiSelect)
                        if (datas.Any(x => x != e.obj && x.IsSelected))//exists another selected
                            return;
                    e.isSelect = !newStat;
                }
                else if (newStat && !MultiSelect)//do select
                {
                    int idx = 0;
                    foreach (var c in datas)
                    {
                        if (c.IsSelected)
                        {
                            c.IsSelected = false;
                            Select?.Invoke(this, new SelectEventArgs()
                            {
                                obj = c,
                                idx = idx,
                                isSelect = false,
                            });
                        }
                        idx++;
                    }
                }
            }
            finally
            {
                Select?.Invoke(this, e);
                e.obj.IsSelected = e.isSelect;
                Select?.Invoke(this, new SelectEventArgs() { msg = Message.Selected });
            }
        }
    }
}
