using Main.Service;
using Main.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        }
    }
}
