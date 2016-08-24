using Main.Util;
using System;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Storage;

namespace WordsLinks.UWP.Util
{
    public static class BasicUtils
    {
        public static async void Init()
        {
            logFile = await StorageFile.GetFileFromPathAsync(SpecificUtils.fileUtil.GetCacheFilePath("AppLog.log"));
            var log = await logFile.OpenStreamForWriteAsync();
            log.Position = log.Length;
            logWriter = new StreamWriter(log);
        }
        public static void RunInUI(DispatchedHandler act)
        {
            var disp = CoreApplication.MainView.Dispatcher;
            if (disp.HasThreadAccess)
                act.Invoke();
            else
                disp.RunAsync(CoreDispatcherPriority.Normal, act);
        }

        internal static StorageFile logFile { get; private set; }
        private static StreamWriter logWriter;
        public static void WriteLog(Exception e, string logstr)
        {
            lock(logWriter)
            {
                logWriter.Write(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + "\r\n");
                logWriter.Write(logstr + "\r\n\r\n");
                logWriter.Flush();
            }
        }
        private static void ClearLog()
        {
            lock(logWriter)
            {
                
            }
        }
    }
}
