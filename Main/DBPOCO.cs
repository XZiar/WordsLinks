using System;
using SQLite;

namespace WordsLinks.Model
{
    public interface WordElement
    {
        string GetStr();
        int GetId();
    }

	[Table("Words")]
	public class DBWord : WordElement
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Unique, MaxLength(16)]
		public string Letters { get; set; }
        public string GetStr() => Letters;
        public int GetId() => Id;
    }
	[Table("Meanings")]
	public class DBMeaning : WordElement
    {
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Unique, MaxLength(16)]
		public string Meaning { get; set; }
        public string GetStr() => Meaning;
        public int GetId() => Id;
    }
	[Table("Translations")]
	public class DBTranslation
	{
		public int Wid { get; set; }
		public int Mid { get; set; }
	}
}
