using Main.Model;
using Main.Service;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordsLinks.ViewModel;
using WordsLinks.Widget;
using Xamarin.Forms;
using static Main.Util.BasicUtils;
using static Main.Util.SpecificUtils;

namespace WordsLinks.View
{
    public partial class LibraryPage : ContentPage
    {
        private string keyword = null;
        private long dictTime = 0;
        private WordGroupHelper wgroups = new WordGroupHelper();
        private CancellationTokenSource searchTask;

        private void init()
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("FFFCF8");
        }
        public LibraryPage()
		{
            try
            {
                init();
                wgroups.SetTo(words);
                words.GroupHeaderTemplate = new DataTemplate(typeof(HeaderCell));
                search.SearchButtonPressed += OnSearch;
                theword.IsVisible = wcnt.IsVisible = false;
            }
            catch(Exception e)
            {
                e.CopeWith("init libraryPage");
            }
        }
        public LibraryPage(string obj)
        {
            init();
            keyword = obj;
            theword.Text = Title = keyword;
            search.IsVisible = false;
        }

        private long searcherTime = 0;
        private bool shouldSearch = false;
        private Action<object> Searcher = (o) =>
        {
            var self = o as LibraryPage;
            int sleepTime = 0;
            while (!self.searchTask.IsCancellationRequested)
            {
                if (self.shouldSearch)
                {
                    long differ = DateTime.Now.Ticks - self.searcherTime;
                    if (differ < 1300000)// wait longer
                        sleepTime = (int)(160 - differ / 10000);
                    else
                    {
                        self.shouldSearch = false;
                        try
                        {
                            self.wgroups.Search(self.search.Text);
                        }
                        catch(Exception e)
                        {
                            e.CopeWith("thread do search");
                        }
                        sleepTime = 170;
                    }
                }
                else
                    sleepTime = 170;
                threadUtil.Sleep(sleepTime);
            }
        };

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (keyword == null)
            {
                searchTask = searchTask ?? new CancellationTokenSource();
                Task.Factory.StartNew(Searcher, this, searchTask.Token,
                      TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            RefreshWords();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            searchTask?.Cancel();
            searchTask = null;
        }

        private void RefreshWords()
        {
            if (keyword == null)
            {
                if (dictTime >= DictService.updTimeAll)
                    return;
                dictTime = DictService.updTimeAll;
                wgroups.Set(DictService.Words);
            }
            else
            {
                if (dictTime >= DictService.updTimeDetail)
                    return;
                dictTime = DictService.updTimeDetail;
                var ms = DictService.GetMeansByWord(keyword);
                var datas = (ms?.Cast<WordElement>()) ?? DictService.GetWordsByMean(keyword).Cast<WordElement>();
                words.ItemsSource = WordGroupHelper.MakeData(datas.Select(x => x.GetStr()));
                wcnt.Text = $"生疏程度 ：  {DictService.GetWordStat(keyword).wrong}";
            }
        }

        private async void OnItemSelect(object sender, ItemTappedEventArgs args)
        {
            var str = (args.Item as TextViewModel).Text;
            await Navigation.PushAsync(new LibraryPage(str));
        }

        private void OnSearchText(object sender, TextChangedEventArgs args)
        {
            try
            {
                bool? isChged;
                search.Text = search.Text.TryToLower(out isChged);
                isChged = isChged ?? false;
                if (!isChged.Value)
                {
                    searcherTime = DateTime.Now.Ticks;
                    shouldSearch = true;
                }
            }
            catch(Exception e)
            {
                e.CopeWith("searchText");
            }
        }
        private void OnSearch(object sender, EventArgs e)
        {
            wgroups.Search(search.Text);
        }
    }
}
