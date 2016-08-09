using CoreGraphics;
using System.Diagnostics;
using UIKit;
using WordsLinks.Widget;
using WordsList.iOS.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using static WordsLinks.Widget.BorderType;

[assembly: ExportRenderer (typeof(EntryEx), typeof(EntryExRenderer))]
[assembly: ExportRenderer (typeof(FrameEx), typeof(FrameExRenderer))]
namespace WordsList.iOS.Renderer
{
    public class EntryExRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                var obj = Element as EntryEx;
                switch(obj.Border)
                {
                case None:
                    Control.BorderStyle = UITextBorderStyle.None;
                    break;
                case Round:
                    Control.BorderStyle = UITextBorderStyle.Line;
                    break;
                }
            }
        }
    }

    public class FrameExRenderer : FrameRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);
            if (Layer != null)
            {
                FrameEx obj = Element as FrameEx;
                Layer.BorderWidth = Layer.ShadowRadius = obj.ShadowWidth;
                Layer.ShadowOffset = new CGSize(obj.ShadowWidth, obj.ShadowWidth);
                Layer.ShadowOpacity = 0.8f;

                switch (obj.Border)
                {
                case None:
                    Layer.ShadowOpacity = 0;
                    break;
                case Rect:
                    Layer.CornerRadius = 0;
                    break;
                }
            }
        }
    }
}