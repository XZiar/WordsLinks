using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using WordsLinks.Widget;
using Xamarin.Forms;
using static WordsLinks.ViewModel.SelectCellGroup.SelectEventArgs;

namespace WordsLinks.ViewModel
{
    public class SelectViewModel : BaseViewModel
    {
        string text;
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        bool isselected = false;
        public bool IsSelected
        {
            get
            {
                return isselected;
            }
            set
            {
                isselected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public Color TextColor
        {
            get
            {
                return Color.Black;
            }
        }
    }

    public class SelectCellGroup
    {
        public class SelectEventArgs : EventArgs
        {
            public enum Message { Selecting, DataChanged, Selected};
            public Message msg { get; set; } = Message.Selecting;
            public SelectViewModel obj { get; internal set; }
            public int idx { get; internal set; }
            public bool isSelect { get; set; }
        }
        private ObservableCollection<SelectViewModel> datas = new ObservableCollection<SelectViewModel>();
        private TableSection sect;
        private bool MultiSelect, NullSelect;
        public delegate void SelectHandler(object sender, SelectEventArgs e);
        public event SelectHandler Select;

        public IEnumerable<string> Items
        {
            get
            {
                return datas.Select(x => x.Text);
            }
        }
        public IEnumerable<string> SelectedItems
        {
            get
            {
                return from x in datas where (x.IsSelected == true) select x.Text;
            }
        }

        public SelectCellGroup(bool mulSel = false, bool nullSel = false)
        {
            MultiSelect = mulSel;
            NullSelect = nullSel;
        }

        public void Set(IEnumerable<string> data)
        {
            datas.Clear();
            foreach (var str in data)
                datas.Add(new SelectViewModel() { Text = str });
            Select?.Invoke(this, new SelectEventArgs() { msg = Message.DataChanged });
        }

        public void SetTo(ListView list)
        {
            list.ItemTemplate = new DataTemplate(typeof(SelectCell));
            list.ItemsSource = datas;
            list.ItemTapped += OnItemTapped;
        }

        public void SetTo(TableSection ts)
        {
            sect = ts;
            buildView();
            Select += OnSelect;
        }

        private void buildView()
        {
            Debug.WriteLine($"build view : {datas.Count} ele");
            foreach (var c in sect)
                c.Tapped -= OnCellTapped;
            sect.Clear();
            foreach (var d in datas)
            {
                var cell = new SelectCell(d.Text, d.IsSelected);
                cell.Tapped += OnCellTapped;
                sect.Add(cell);
            }
        }

        private void OnCellTapped(object sender, EventArgs e)
        {
            OnItemTapped(null, new ItemTappedEventArgs(null, datas[sect.IndexOf(sender as Cell)]));
        }

        private void OnSelect(object sender, SelectEventArgs e)
        {
            switch (e.msg)
            {
            case Message.Selecting:
                SelectCell obj = sect[e.idx] as SelectCell;
                obj.IsSelected = e.isSelect;
                break;
            case Message.DataChanged:
                buildView();
                break;
            }
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if(sender != null)
                (sender as ListView).SelectedItem = null;
            Choose(e.Item as SelectViewModel);
        }

        public void Choose(SelectViewModel item)
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

