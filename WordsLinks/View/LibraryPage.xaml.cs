using Main.Model;
using Main.Service;
using System.Linq;
using WordsLinks.ViewModel;
using WordsLinks.Widget;
using Xamarin.Forms;

namespace WordsLinks.View
{
    public partial class LibraryPage : ContentPage
    {
        private string keyword = null;
        private long dictTime = 0;
        private WordGroupHelper wgroups = new WordGroupHelper();
        public LibraryPage()
		{
			InitializeComponent();
            wgroups.SetTo(words);
            words.GroupHeaderTemplate = new DataTemplate(typeof(HeaderCell));
        }

        public LibraryPage(string obj)
        {
            InitializeComponent();
            keyword = obj;
            Title = keyword;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            RefreshWords();
        }

        private void RefreshWords()
        {
            if (dictTime >= DictService.updTime)
                return;
            dictTime = DictService.updTime;
            if (keyword == null)
            {
                wgroups.Set(DictService.Words);
            }
            else
            {
                var ms = DictService.GetMeansByWord(keyword);
                var datas = (ms?.Cast<WordElement>()) ?? DictService.GetWordsByMean(keyword).Cast<WordElement>();
                words.ItemsSource = datas.Select(x => x.GetStr());
            }
        }

        private async void OnItemSelect(object sender, ItemTappedEventArgs args)
        {
            await Navigation.PushAsync(new LibraryPage(args.Item.ToString()));
        }
    }
}
