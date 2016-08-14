using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using WordsLinks.Util;
using Xamarin.Forms;
using static WordsLinks.Util.BasicUtils;

namespace WordsLinks.Service
{
    static class TestService
    {
        static ImageUtil imgUtil;
        static TestService()
        {
            imgUtil = DependencyService.Get<ImageUtil>();
        }
    }
}
