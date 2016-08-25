using Foundation;
using Main.Util;
using System;
using Xamarin.Forms;
using static Main.Util.BasicUtils;

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
                Logger($"Exception when {where} :{e.Description}\n{e.LocalizedDescription}\n", LogLevel.Exception);
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