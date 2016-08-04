using System;
using System.Diagnostics;
using WordsLinks.Services;
using Xamarin.Forms;

namespace WordsLinks.View
{
	public partial class WritePage : ContentPage
	{
		public WritePage()
		{
			InitializeComponent();
		}

		public void OnTextChanged(object sender, EventArgs args)
		{
			Entry inputor = sender as Entry;
			if (inputor.Text == "")
			{
				add.BackgroundColor = Color.Gray;
				add.IsEnabled = false;
			}
			else
			{
				add.BackgroundColor = Color.Green;
				add.IsEnabled = true;
			}
		}

		public void OnAddClicked(object sender, EventArgs args)
		{
			Debug.WriteLine("BorderType is " + word.Border);
			TranslateService.Eng2Chi(word.Text, strs => 
			{
				webtrans.ItemsSource = strs;
			});
		}
	}
}
