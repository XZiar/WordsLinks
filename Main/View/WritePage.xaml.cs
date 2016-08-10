using System;
using System.Collections.Generic;
using System.Diagnostics;
using WordsLinks.Services;
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

        private void OnSelectWebTrans(object sender, SelectCellGroup.SelectEventArgs e)
        {
            if(e.msg == Selected)
                add.IsEnabled = webTGroup.SelectedItems.Length > 0 || finTGroup.SelectedItems.Length > 0;
        }

        private void OnSelectFinTrans(object sender, SelectCellGroup.SelectEventArgs e)
        {
            if (e.msg == Selected)
                add.IsEnabled = webTGroup.SelectedItems.Length > 0 || finTGroup.SelectedItems.Length > 0;
        }

        private void OnClickSearch(object sender, EventArgs args)
        {
            if (word.Text != "")
                TranslateService.Eng2Chi(word.Text, strs => webTGroup.Set(strs));
        }

        private void OnAddClicked(object sender, EventArgs args)
		{
            var chi = new HashSet<string>();
            foreach (var s in webTGroup.SelectedItems)
                chi.Add(s);
            foreach (var s in finTGroup.SelectedItems)
                chi.Add(s);
            DBService.AddWord(word.Text, chi);
            RefreshDB();
        }
	}
}
