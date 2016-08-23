using Main.Service;
using Main.Util;
using System;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static Main.Util.SpecificUtils;

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

        private void RefreshWords()
        {
            //wordsSect.Title = $"单词本\t（{DBService.WordsCount}个单词）";
        }

        private async void OnDBTapped(object sender, TappedRoutedEventArgs args)
        {
            if (sender == exportDB)
            {
                try
                {
                    var ret = DBService.Export();
                    if (await ret)
                        Debug.WriteLine("导出成功");
                    else
                        Debug.WriteLine("导出失败");
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
                        Debug.WriteLine("导入中");
                        if (await ret)
                            Debug.WriteLine("导入成功");
                        else
                            Debug.WriteLine("导入失败");
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
    }
}
