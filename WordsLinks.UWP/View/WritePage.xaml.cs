using Main.Service;
using Main.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Input;
using WordsLinks.UWP.ViewModel;
using static WordsLinks.UWP.ViewModel.SelectItemGroup.SelectEventArgs;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WordsLinks.UWP.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WritePage : Page
    {
        private SelectItemGroup webTGroup, finTGroup;
        private bool isInPage;
        public WritePage()
        {
            NavigationCacheMode = NavigationCacheMode.Required;
            InitializeComponent();
            CoreApplication.MainView.CoreWindow.KeyDown += OnKeyDown;
            webTGroup = new SelectItemGroup(true, true);
            finTGroup = new SelectItemGroup(true, true);
            webTGroup.SetTo(webtrans);
            finTGroup.SetTo(fintrans);
            webTGroup.Select += OnSelectTrans;
            finTGroup.Select += OnSelectTrans;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            isInPage = true;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            isInPage = false;
        }

        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!isInPage)
                return;
            if (args.VirtualKey == VirtualKey.Back && word.FocusState == FocusState.Unfocused)
                OnDelClicked(del, null);
        }

        public void judgeAdd() =>
            add.IsEnabled = !string.IsNullOrWhiteSpace(word.Text) 
                && (webTGroup.SelectedItems.IsNotEmpty() || finTGroup.SelectedItems.IsNotEmpty());

        private void OnSelectTrans(object sender, SelectItemGroup.SelectEventArgs e)
        {
            if (e.msg == Message.Selected)
                judgeAdd();
        }

        private void OnKey(object sender, KeyRoutedEventArgs e)
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
                finTGroup.Set(await DictService.MatchMeanings(trans));
            }
            catch (Exception e)
            {
                e.CopeWith("searching");
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(word.Text))
                word.Text = word.Text.ToLower();
            webTGroup.ChooseNone();
            finTGroup.ChooseNone();
            add.IsEnabled = false;
        }

        private void OnAddClicked(object sender, TappedRoutedEventArgs args)
        {
            var chi = new HashSet<string>();
            foreach (var s in webTGroup.SelectedItems)
                chi.Add(s);
            foreach (var s in finTGroup.SelectedItems)
                chi.Add(s);
            try
            {
                DictService.AddWord(word.Text.ToLower(), chi);
            }
            catch(Exception e)
            {
                e.CopeWith("Add Word");
            }
        }

        private void OnDelClicked(object sender, TappedRoutedEventArgs args)
        {
            word.Text = "";
            word.Focus(FocusState.Pointer);
        }
    }
}
