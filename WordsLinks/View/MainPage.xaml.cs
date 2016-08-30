using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using static Main.Util.BasicUtils;

namespace WordsLinks.View
{
    public partial class MainPage : TabbedPage
    {
        public NavigationPage theSettingPage { get { return settingPage; } }
        public MainPage()
        {
            InitializeComponent();

            if (Device.OS == TargetPlatform.iOS)
            {
                // Fix : status bar position
                Logger("Fix status bar position for iOS");
                foreach (var ch in Children.Where(x=>!(x is NavigationPage)))
                {
                    ch.Padding = new Thickness(0, 20, 0, 0);
                    ch.BackgroundColor = Color.FromHex("FFFCF8");
                }
            }
        }
    }
}
