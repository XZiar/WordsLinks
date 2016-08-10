using SQLite;

namespace WordsLinks.Model
{
	[Table("Words")]
	public class DBWord
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Unique, MaxLength(16)]
		public string Letters { get; set; }
        public override int GetHashCode()
        {
            return Letters.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return (obj is string) ? (Letters == obj as string) : base.Equals(obj);
        }
    }
	[Table("Meanings")]
	public class DBMeaning
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Unique, MaxLength(16)]
		public string Meaning { get; set; }
    }
	[Table("Translations")]
	public class DBTranslation
	{
		public int Wid { get; set; }
		public int Mid { get; set; }
	}
}
