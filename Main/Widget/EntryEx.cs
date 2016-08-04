using Xamarin.Forms;

namespace WordsLinks.Widget
{
	class EntryEx : Entry
	{
		public enum BorderType { None, Line, Round, Rect }
		public BorderType Border
		{
			get;
			set;
		} = BorderType.Round;
	}
}
