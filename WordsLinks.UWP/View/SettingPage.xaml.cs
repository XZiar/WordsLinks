using Main.Service;
using Main.Util;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using static Main.Util.BasicUtils;
using static Main.Util.SpecificUtils;
using Windows.Foundation;
using System.Windows.Input;
using Windows.UI.Xaml;

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
        private Task<bool?> ImportChoose()
        {
            var tsk = new TaskCompletionSource<bool?>();
            modeDlg.Commands.Clear();
            modeDlg.Commands.Add(new UICommand("合并", (c) => tsk.SetResult(false), 0));
            modeDlg.Commands.Add(new UICommand("覆盖", (c) => tsk.SetResult(true), 1));
            modeDlg.Commands.Add(new UICommand("取消", (c) => tsk.SetResult(null), 2));
            modeDlg.ShowAsync();
            return tsk.Task;
        }
        private ContentDialog cfmDlg = new ContentDialog()
        {
            Title = "清空单词本",
            Content = "此操作无法恢复",
            PrimaryButtonText = "确认",
            SecondaryButtonText = "不了",
            FullSizeDesired = false,
        };
        private Task<bool> clearConfirm()
        {
            var tsk = new TaskCompletionSource<bool>();
            TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> yes = null, no = null;
            yes = (o, a) =>
            { tsk.SetResult(true); cfmDlg.PrimaryButtonClick -= yes; cfmDlg.SecondaryButtonClick -= no; };
            no = (o, a) => 
            { tsk.SetResult(false); cfmDlg.PrimaryButtonClick -= yes; cfmDlg.SecondaryButtonClick -= no; };
            cfmDlg.PrimaryButtonClick += yes;
            cfmDlg.SecondaryButtonClick += no;
            cfmDlg.ShowAsync();
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
                        bool? confirm = await ImportChoose();
                        if (confirm.HasValue)
                        {
                            var ret = DictService.Import(pic.Result, confirm.Value);
                            hudPopup.Show(msg: "导入中");
                            if (await ret)
                                hudPopup.Show(HUDType.Success, "导入成功");
                            else
                                hudPopup.Show(HUDType.Fail, "导入失败");
                            RefreshWords();
                        }
                    }
                }
                catch (Exception e)
                {
                    e.CopeWith("importDB");
                }
            }
            else if (sender == clearDB)
            {
                if (await clearConfirm())
                {
                    DictService.Clear();
                    RefreshWords();
                }
            }
        }

        private void OnLogTapped(object sender, TappedRoutedEventArgs args)
        {
            if (sender == showLog)
                logUtil.logFile.OpenWith();
        }

        private void OnSwitch(object sender, RoutedEventArgs args)
        {
            if (sender == ExWCnt)
                DictService.isOutWrongCnt = ExWCnt.IsOn;
        }
    }
}
