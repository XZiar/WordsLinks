using WordsLinks.Services;
using WordsLinks.View;
using Xamarin.Forms;

namespace WordsLinks
{
	public class App : Application
	{
		public App()
		{
			// The root page of your application
			DBService.Init();
			NetService.Init();
            MainPage = new MainPage();
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
