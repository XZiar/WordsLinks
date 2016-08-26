using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Main.Util.BasicUtils;

namespace WordsLinks.Util
{
    [ContentProperty("Source")]
    class ImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (null == Source)
                return null;
            return ImageSource.FromStream(() => AssembleResource(Source, this));
        }
    }
}
