using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
            return ImageSource.FromResource("WordsLinks." + Source);
        }
    }
}
