using SQLite;

namespace WordsLinks.Model
{
	[Table("Words")]
	public class DBWord
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[MaxLength(16)]
		public string Letters { get; set; }
	}
	[Table("Meanings")]
	public class DBMeaning
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[MaxLength(16)]
		public string Meaning { get; set; }
	}
	[Table("Translations")]
	public class DBTranslation
	{
		public int Wid { get; set; }
		public int Mid { get; set; }
	}
}
