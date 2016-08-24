using Main.Util;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using WordsLinks.UWP.View;
using WordsLinks.UWP.ViewModel;
using System;
using System.Threading;
using Windows.UI.Xaml.Media;
using Windows.UI;
using static WordsLinks.UWP.Util.BasicUtils;
using WordsLinks.UWP.Util;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WordsLinks.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageShell : Page
    {
        private List<NavPageItem> navList = new List<NavPageItem>
            {
                new NavPageItem()
                {
                    Label = "添加",
                    Source = "Assets/IconWrite.png",
                    PageType = typeof(WritePage)
                },
                new NavPageItem()
                {
                    Label = "记忆",
                    Source = "Assets/IconMemorize.png",
                    PageType = typeof(MemorizePage)
                },
                new NavPageItem()
                {
                    Label = "设置",
                    Source = "Assets/IconSetting.png",
                    PageType = typeof(SettingPage)
                },
            };

        public PageShell()
        {
            InitializeComponent();
            NavMenuList.ItemsSource = navList;
            NavMenuList.PageSelected += OnPageSelected;
            frame.NavigationFailed += (o, e) => e.Exception.CopeWith("page navigation");
            Loaded += (o, e) => NavMenuList.Select(0);
            Main.Util.BasicUtils.OnExceptionEvent += OnExceptionMsg;
            (SpecificUtils.hudPopup as HUDPopup_UWP).OnMsg += OnMsg;
        }

        private void OnMsg(bool isShow, HUDType type, string msg) => RunInUI(() =>
            {
                if (isShow)
                {
                    msgBar.Text = msg;
                    if (type == HUDType.Fail)
                        msgBar.Foreground = new SolidColorBrush(Color.FromArgb(64, 255, 0, 0));
                    else if (type == HUDType.Success)
                        msgBar.Foreground = new SolidColorBrush(Color.FromArgb(64, 0, 255, 0));
                    else
                        msgBar.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                else
                    msgBar.Text = "";
            });

        private void OnExceptionMsg(Exception e, string log) => RunInUI(() =>
            {
                msgBar.Text = e.Message;
                msgBar.Foreground = new SolidColorBrush(Color.FromArgb(64, 255, 0, 0));
                new Timer((o) => RunInUI(() => msgBar.Text = ""), null, 2000, Timeout.Infinite);
            });

        private void OnPageSelected(object sender, NavPageItem item)
        {
            frame.Navigate(item.PageType, null, new Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
        }

    }
}
