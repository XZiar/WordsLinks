using Xamarin.Forms;

namespace WordsLinks.Widget
{
    public enum BorderType { None, Line, Round, Rect }
    public class EntryEx : Entry
	{
        public BorderType Border { get; set; } = BorderType.Round;
	}

    public class FrameEx : Frame
    {
        public float ShadowWidth { get; set; } = 4.0f;
        public BorderType Border { get; set; } = BorderType.Round;
    }
}
