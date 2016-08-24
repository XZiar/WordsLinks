using Main.Service;
using Main.Util;
using System;
using System.Diagnostics;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static Main.Util.SpecificUtils;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WordsLinks.UWP.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            RefreshWords();
        }
        private void RefreshWords()
        {
            DBstatus.Text = $"单词本\t（{DBService.WordsCount}个单词）";
        }

        private async void OnDBTapped(object sender, TappedRoutedEventArgs args)
        {
            if (sender == exportDB)
            {
                try
                {
                    var ret = DBService.Export();
                    hudPopup.Show(msg: "导出中");
                    if (await ret)
                        hudPopup.Show(HUDType.Success, "导出成功");
                    else
                        hudPopup.Show(HUDType.Fail, "导出失败");
                }
                catch (Exception e)
                {
                    e.CopeWith("exportDB");
                }
            }
            else if (sender == importDB)
            {
                try
                {
                    var pic = imgUtil.GetImage();
                    if ((await pic) != null)
                    {
                        var ret = DBService.Import(pic.Result);
                        hudPopup.Show(msg: "导入中");
                        if (await ret)
                            hudPopup.Show(HUDType.Success, "导入成功");
                        else
                            hudPopup.Show(HUDType.Fail, "导入失败");
                        RefreshWords();
                    }
                }
                catch (Exception e)
                {
                    e.CopeWith("importDB");
                }
            }
            else if (sender == clearDB)
            {
                DBService.Clear();
                RefreshWords();
            }
        }

        private static LauncherOptions opt = new LauncherOptions()
        { DisplayApplicationPicker = true };
        private async void OnLogTapped(object sender, TappedRoutedEventArgs args)
        {
            if(sender == showLog)
                await Launcher.LaunchFileAsync(Util.BasicUtils.logFile, opt);
        }
    }
}
