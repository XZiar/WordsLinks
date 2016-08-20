using System;
using System.Collections.Generic;
using System.Diagnostics;
using WordsLinks.Service;
using WordsLinks.ViewModel;
using Xamarin.Forms;
using static WordsLinks.ViewModel.SelectCellGroup.SelectEventArgs.Message;


namespace WordsLinks.View
{
    public partial class WritePage : ContentPage
	{
        SelectCellGroup webTGroup, finTGroup;
        public WritePage()
		{
            InitializeComponent();
            webTGroup = new SelectCellGroup(true, true);
            finTGroup = new SelectCellGroup(true, true);
            webTGroup.SetTo(webtrans);
            finTGroup.SetTo(fintrans);
            webTGroup.Select += OnSelectWebTrans;
            finTGroup.Select += OnSelectFinTrans;
        }

        private void RefreshDB()
        {
            if (DBService.isChanged)
                finTGroup.Set(DBService.Meanings);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshDB();
        }

        public void judgeAdd() =>
            add.IsEnabled = word.Text != null && (webTGroup.SelectedItems.Length > 0 || finTGroup.SelectedItems.Length > 0);

        private void CapChecker(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.NewTextValue))
                word.Text = e.NewTextValue.ToLower();
            judgeAdd();
        }

        private void OnSelectWebTrans(object sender, SelectCellGroup.SelectEventArgs e)
        {
            if (e.msg == Selected)
                judgeAdd();
        }

        private void OnSelectFinTrans(object sender, SelectCellGroup.SelectEventArgs e)
        {
            if (e.msg == Selected)
                judgeAdd();
        }

        private async void OnClickSearch(object sender, EventArgs args)
        {
            if (string.IsNullOrWhiteSpace(word.Text))
                return;
            var ret = TranslateService.Eng2Chi(word.Text.ToLower());
            webTGroup.Set(await ret);
        }

        private void OnAddClicked(object sender, EventArgs args)
		{
            var chi = new HashSet<string>();
            foreach (var s in webTGroup.SelectedItems)
                chi.Add(s);
            foreach (var s in finTGroup.SelectedItems)
                chi.Add(s);
            DBService.AddWord(word.Text.ToLower(), chi);
            RefreshDB();
        }
	}
}
