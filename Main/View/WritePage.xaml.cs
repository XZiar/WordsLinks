using System;
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

        private void OnSelectWebTrans(object sender, SelectCellGroup.SelectEventArgs e)
        {
            if(e.msg == Selected)
                finTGroup.Set((sender as SelectCellGroup).SelectedItems);
        }

        private void OnSelectFinTrans(object sender, SelectCellGroup.SelectEventArgs e)
        {
            if (e.msg == Selecting)
                Debug.WriteLine($"click {e.obj} at {e.idx} with {e.isSelect}");
            else if(e.msg == DataChanged)
            {
                add.IsEnabled = finTGroup.SelectedItems.Length > 0;
            }
        }

        private void OnClickSearch(object sender, EventArgs args)
        {
            if (word.Text != "")
                TranslateService.Eng2Chi(word.Text, strs => webTGroup.Set(strs));
        }

        private void OnAddClicked(object sender, EventArgs args)
		{
			
		}
	}
}
