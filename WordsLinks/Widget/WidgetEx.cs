using System.Diagnostics;
using Xamarin.Forms;

namespace WordsLinks.Widget
{
    public enum BorderType { None, Line, Round, Rect }
    public enum ShadowPosition { Around, LowerRight }
    public class EntryEx : Entry
	{
        public BorderType Border { get; set; } = BorderType.Round;
	}

    public class FrameEx : Frame
    {
        public float ShadowWidth { get; set; } = 2.0f;
        public BorderType Border { get; set; } = BorderType.Round;
        public ShadowPosition ShadowPos { get; set; } = ShadowPosition.LowerRight;
    }
}
