using Main.Service;
using Main.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Input;
using WordsLinks.UWP.ViewModel;
using static WordsLinks.UWP.ViewModel.SelectItemGroup.SelectEventArgs;
using Windows.System;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WordsLinks.UWP.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WritePage : Page
    {
        private SelectItemGroup webTGroup, finTGroup;
        public WritePage()
        {
            NavigationCacheMode = NavigationCacheMode.Required;
            InitializeComponent();
            webTGroup = new SelectItemGroup(true, true);
            finTGroup = new SelectItemGroup(true, true);
            webTGroup.SetTo(webtrans);
            finTGroup.SetTo(fintrans);
            webTGroup.Select += OnSelect;
            finTGroup.Select += OnSelect;
        }

        public void judgeAdd() =>
            add.IsEnabled = word.Text != null && (webTGroup.SelectedItems.Count() > 0 || finTGroup.SelectedItems.Count() > 0);

        private void OnSelect(object sender, SelectItemGroup.SelectEventArgs e)
        {
            if (e.msg == Message.Selected)
                judgeAdd();
        }

        private void OnChanged(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                OnClickSearch(search, null);
        }

        private async void OnClickSearch(object sender, TappedRoutedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(word.Text))
                return;
            try
            {
                var trans = await TranslateService.Eng2Chi(word.Text.ToLower());
                webTGroup.Set(trans);
                finTGroup.Set(await DBService.MatchMeanings(trans));
            }
            catch (Exception e)
            {
                e.CopeWith("searching");
            }
        }

        private void OnAddClicked(object sender, TappedRoutedEventArgs e)
        {
            var chi = new HashSet<string>();
            foreach (var s in webTGroup.SelectedItems)
                chi.Add(s);
            foreach (var s in finTGroup.SelectedItems)
                chi.Add(s);
            DBService.AddWord(word.Text.ToLower(), chi);
        }
    }
}
