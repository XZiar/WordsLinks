using CoreGraphics;
using System.ComponentModel;
using System.Diagnostics;
using UIKit;
using WordsLinks.Widget;
using WordsLinks.iOS.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using static WordsLinks.Widget.BorderType;
using static WordsLinks.Widget.TextCellEx;

[assembly: ExportRenderer(typeof(EntryEx), typeof(EntryExRenderer))]
[assembly: ExportRenderer(typeof(FrameEx), typeof(FrameExRenderer))]
[assembly: ExportRenderer(typeof(TextCellEx), typeof(TextCellExRenderer))]
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

    public class TextCellExRenderer : TextCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var obj = base.GetCell(item, reusableCell, tv);
            var cellEx = item as TextCellEx;
            if (!cellEx.IsShow)
            {
                obj.Accessory = UITableViewCellAccessory.None;
            }
            else
            {
                switch (cellEx.ShowIndicator)
                {
                case RightIndicator.Entry:
                    obj.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    break;
                case RightIndicator.Check:
                    obj.Accessory = UITableViewCellAccessory.Checkmark;
                    break;
                case RightIndicator.None:
                    obj.Accessory = UITableViewCellAccessory.None;
                    break;
                }
            }
            return obj;
        }
    }
}