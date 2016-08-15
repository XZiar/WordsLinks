using System;
using System.Diagnostics;
using WordsLinks.Service;
using WordsLinks.Util;
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

        private async void OnDBCellTapped(object sender, EventArgs args)
        {
            if (sender == exportCell)
            {
                try
                {
                    var ret = DBService.Export();
                    var res = await ret;
                    exportCell.TextColor = res ? Color.Green : Color.Red;
                }
                catch (Exception e)
                {
                    e.CopeWith("exportDB");
                }
            }
            else if (sender == importCell)
            {
                try
                {
                    var ret = DBService.Import();
                    var res = await ret;
                    importCell.TextColor = res ? Color.Green : Color.Red;
                }
                catch (Exception e)
                {
                    e.CopeWith("importDB");
                }
            }
            else if (sender == clearCell)
            {
                DBService.Clear();
            }
        }
    }
}
