using System;
using WordsLinks.Service;
using WordsLinks.ViewModel;
using Xamarin.Forms;

namespace WordsLinks.View
{
	public partial class SettingPage : ContentPage
    {
        SelectCellGroup netChoiceGroup;
        public SettingPage()
		{
			InitializeComponent();
            netChoiceGroup = new SelectCellGroup(false, false);
            var dat = NetService.GetChoices();
            netChoiceGroup.Set(dat.Item2);
            netChoiceGroup.SetTo(netSect);
            netChoiceGroup.Select += (sender, e) =>
            {
                if (e.isSelect)
                    NetService.Choose(e.idx);
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var dat = NetService.GetChoices();
            netChoiceGroup.Choose(dat.Item1);
        }

        private void OnDBCellTapped(object sender, EventArgs args)
        {
            if(sender == exportCell)
            {

            }
            else if (sender == importCell)
            {

            }
            else if (sender == clearCell)
            {
                DBService.Clear();
            }
        }
    }
}
