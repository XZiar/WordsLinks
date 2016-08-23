using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace WordsLinks.UWP.Util
{
    public static class BasicUtils
    {
        public static void RunInUI(DispatchedHandler act)
        {
            var disp = CoreApplication.MainView.Dispatcher;
            if (disp.HasThreadAccess)
                act.Invoke();
            else
                disp.RunAsync(CoreDispatcherPriority.Normal, act);
        }
    }
}
