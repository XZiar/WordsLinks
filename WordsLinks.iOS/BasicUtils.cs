using Foundation;
using System;
using System.Diagnostics;
using Main.Util;
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

        public static void OnError(this NSError e, string where = "", bool isShow = false)
        {
            try
            {
                Debug.WriteLine($"Exception when {where} :{e.Description}\n{e.LocalizedDescription}\n");
            }
            catch (Exception ex)
            {
                ex.CopeWith("log-NSerror");
            }
        }

        public static void Dispose(params IDisposable[] objs)
        {
            foreach (var o in objs)
                o.Dispose();
        }
    }
}