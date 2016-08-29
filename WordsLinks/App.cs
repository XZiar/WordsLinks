using System;
using System.Diagnostics;
using Main.Service;
using Main.Util;
using WordsLinks.View;
using Xamarin.Forms;
using static Main.Util.BasicUtils;

namespace WordsLinks
{
    public class App : Application
    {
        public App()
        {
            try
            {
                SpecificUtils.Init(
                    DependencyService.Get<FileUtil>(),
                    DependencyService.Get<ImageUtil>(),
                    DependencyService.Get<HUDPopup>(),
                    DependencyService.Get<ThreadUtil>(),
                    DependencyService.Get<SQLiteUtil>(),
                    DependencyService.Get<LogUtil>()
                    );
                DictService.Init();
                NetService.Choose(0);
                MainPage = new MainPage();
            }
            catch (Exception e)
            {
                e.CopeWith("init app");
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
