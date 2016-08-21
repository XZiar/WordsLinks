using Xamarin.Forms;

namespace WordsLinks.Util
{
    class BasicUtils
    {
        public static ImageSource AssembleImage(string fname)
        {
            return ImageSource.FromResource("WordsLinks." + fname);
        }
    }
}
