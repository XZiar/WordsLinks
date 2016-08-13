using System;
using System.Diagnostics;

namespace WordsLinks.Util
{
    class BasicUtils
    {
        public static void OnException(Exception e, string where = "", bool isShow = false)
        {
            Debug.WriteLine($"Exception when {where}:{e.GetType()}\n{e.Message}\nat {e.Source}\n{e.StackTrace}\n");
        }
    }
}
