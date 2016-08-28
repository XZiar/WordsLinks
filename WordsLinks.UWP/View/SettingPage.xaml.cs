﻿using Main.Service;
using Main.Util;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using static Main.Util.BasicUtils;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            RefreshWords();
        }

        private void RefreshWords()
        {
            DBstatus.Text = $"单词本\t（{DictService.WordsCount}个单词）";
        }

        private MessageDialog modeDlg = new MessageDialog("是否覆盖现有单词本？") { Title = "导入方式" };
        private Task<bool> ImportChoose()
        {
            var tsk = new TaskCompletionSource<bool>();
            modeDlg.Commands.Clear();
            modeDlg.Commands.Add(new UICommand("覆盖", (c) => tsk.SetResult(true)));
            modeDlg.Commands.Add(new UICommand("合并", (c) => tsk.SetResult(false)));
            modeDlg.ShowAsync();
            return tsk.Task;
        }
        private async void OnDBTapped(object sender, TappedRoutedEventArgs args)
        {
            if (sender == exportDB)
            {
                try
                {
                    var ret = DictService.Export();
                    hudPopup.Show(msg: "导出中");
                    byte[] data = await ret;
                    //Logger($"before save {data.Length} bytes data");
                    if (await imgUtil.SaveImage(data))
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
                        var ret = DictService.Import(pic.Result, await ImportChoose());
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
                DictService.Clear();
                RefreshWords();
            }
        }

        private void OnLogTapped(object sender, TappedRoutedEventArgs args)
        {
            if (sender == showLog)
                openfileUtil.OpenFile(logUtil.GetLogFile());
        }
    }
}
