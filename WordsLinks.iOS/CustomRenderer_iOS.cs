using CoreGraphics;
using System.ComponentModel;
using System.Diagnostics;
using UIKit;
using WordsLinks.Widget;
using WordsLinks.iOS.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using static WordsLinks.Widget.BorderType;

[assembly: ExportRenderer(typeof(EntryEx), typeof(EntryExRenderer))]
[assembly: ExportRenderer(typeof(FrameEx), typeof(FrameExRenderer))]
namespace WordsLinks.iOS.Renderer
{
    public class EntryExRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control.ClearButtonMode = UITextFieldViewMode.Always;
                var obj = Element as EntryEx;
                switch (obj.Border)
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

            FrameEx obj = Element as FrameEx;
            Layer.ShadowRadius = obj.ShadowWidth;
            Layer.BorderWidth = 1;
            Layer.ShadowOpacity = 0.8f;
            switch (obj.Border)
            {
            case Rect:
                Layer.CornerRadius = 0;
                break;
            case None:
                Layer.BorderWidth = 0;
                break;
            }
            if (obj.ShadowPos == ShadowPosition.LowerRight)
                Layer.ShadowOffset = new CGSize(obj.ShadowWidth, obj.ShadowWidth);

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Debug.WriteLine($"prop changed:{e.PropertyName} by {sender}");
            switch (e.PropertyName)
            {
            case nameof(BackgroundColor):
                Layer.BackgroundColor = Element.BackgroundColor.ToCGColor();
                break;
            case nameof(Element.OutlineColor):
                Layer.BorderColor = Element.OutlineColor.ToCGColor();
                break;
            default:
                base.OnElementPropertyChanged(sender, e);
                break;
            }
        }

    }
}