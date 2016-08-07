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
        }

        public void OnTextChanged(object sender, EventArgs args)
		{
			Entry inputor = sender as Entry;
			if (inputor.Text == "")
			{
				add.BackgroundColor = Color.Gray;
				add.IsEnabled = false;
			}
			else
			{
				add.BackgroundColor = Color.Green;
				add.IsEnabled = true;
			}
		}

		public void OnAddClicked(object sender, EventArgs args)
		{
            Debug.WriteLine("BorderType is " + word.Border);
			TranslateService.Eng2Chi(word.Text, strs => 
			{
                //webtrans.ItemsSource = strs;
                webTGroup.Set(strs);
			});
		}
	}
}
