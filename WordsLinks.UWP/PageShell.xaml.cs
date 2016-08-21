using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WordsLinks.UWP.ViewModel;

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
                    Source = "Assets/IconWrite.png"
                },
                new NavPageItem()
                {
                    Label = "记忆",
                    Source = "Assets/IconMemorize.png"
                },
                new NavPageItem()
                {
                    Label = "设置",
                    Source = "Assets/IconSetting.png"
                },
            };
        public PageShell()
        {
            InitializeComponent();
            NavMenuList.ItemsSource = navList;
        }

        public Frame PageFrame { get { return frame; } }
    }
}
