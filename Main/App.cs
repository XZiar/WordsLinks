using System;
using System.Diagnostics;
using WordsLinks.Service;
using WordsLinks.Util;
using WordsLinks.View;
using Xamarin.Forms;
using static WordsLinks.Util.BasicUtils;

namespace WordsLinks
{
	public class App : Application
	{
		public App()
		{
            try
            {
                DBService.Init();
                NetService.Init();
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
