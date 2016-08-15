using WordsLinks.Util;
using Xamarin.Forms;

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
