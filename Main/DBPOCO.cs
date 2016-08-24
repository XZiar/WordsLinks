using SQLite;
using System.Collections.Generic;

namespace Main.Model
{
    public interface WordElement
    {
        string GetStr();
        int GetId();
        int MissCount();
    }

	[Table("Words")]
	public class DBWord : WordElement
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Unique, MaxLength(16)]
		public string Letters { get; set; }
        public int Miss { get; set; }

        public DBWord() { }
        public DBWord(string letter, int id)
        { Letters = letter; Id = id; }

        public string GetStr() => Letters;
        public int GetId() => Id;
        public int MissCount() => Miss;
    }

	[Table("Meanings")]
	public class DBMeaning : WordElement
    {
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Unique, MaxLength(16)]
		public string Meaning { get; set; }
        public int Miss { get; set; }

        public DBMeaning() { }
        public DBMeaning(string meaning, int id)
        { Meaning = meaning; Id = id; }

        public string GetStr() => Meaning;
        public int GetId() => Id;
        public int MissCount() => Miss;
    }

	[Table("Translations")]
	public class DBTranslation
	{
		public int Wid { get; set; }
		public int Mid { get; set; }
	}

    public class DBEleComparer : IComparer<WordElement>
    {
        public static DBEleComparer Instance { get; private set; }
        static DBEleComparer()
        {
            Instance = new DBEleComparer();
        }

        public int Compare(WordElement x, WordElement y)
        {
            if (x.MissCount() == y.MissCount())
                return x.GetStr().CompareTo(y.GetStr());
            else
                return x.MissCount().CompareTo(y.MissCount());
        }
    }
}
