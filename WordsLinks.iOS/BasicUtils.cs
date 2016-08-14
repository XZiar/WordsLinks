using Foundation;
using System;
using Xamarin.Forms;

namespace WordsLinks.iOS
{
    public static class BasicUtils
    {
        public static void RunInUI(Action act)
        {
            if (NSThread.Current.IsMainThread)
                act.Invoke();
            else
                Device.BeginInvokeOnMainThread(act);
        }
    }
}