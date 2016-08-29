using SQLite;
using System;
using System.Collections.Generic;

namespace Main.Model
{
    public interface WordElement
    {
        string GetStr();
        int GetId();
        WordStat ToStat();
    }

	[Table("Words")]
	public class DBWord : WordElement
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Unique, MaxLength(16)]
		public string Letters { get; set; }
        public short wrong { get; set; } = 1;
        public short right { get; set; }

        public DBWord() { }
        public DBWord(string letter, int id)
        { Letters = letter; Id = id; }
        public DBWord(WordStat s, int id)
        { Letters = s.str; wrong = s.wrong; Id = id; }

        public string GetStr() => Letters;
        public int GetId() => Id;
        public WordStat ToStat() =>
            new WordStat() { str = Letters, wrong = wrong, right = right };
    }

	[Table("Meanings")]
	public class DBMeaning : WordElement
    {
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Unique, MaxLength(16)]
		public string Meaning { get; set; }
        public short wrong { get; set; } = 1;
        public short right { get; set; }

        public DBMeaning() { }
        public DBMeaning(string meaning, int id)
        { Meaning = meaning; Id = id; }
        public DBMeaning(WordStat s, int id)
        { Meaning = s.str; wrong = s.wrong; Id = id; }

        public string GetStr() => Meaning;
        public int GetId() => Id;
        public WordStat ToStat() =>
            new WordStat() { str = Meaning, wrong = wrong, right = right };
    }

	[Table("Translations")]
	public class DBTranslation
	{
		public int Wid { get; set; }
		public int Mid { get; set; }

        public DBTranslation() { }
        public DBTranslation(int wid, int mid)
        { Wid = wid; Mid = mid; }
	}

    public class WordStat : IComparable<WordStat>
    {
        public string str;
        public short wrong, right;

        public static implicit operator string(WordStat obj) => obj.str;
        public static implicit operator WordStat(string txt) => 
            new WordStat() { str = txt };

        public int CompareTo(WordStat other)
        {
            if (wrong != other.wrong)
                return other.wrong.CompareTo(wrong);
            if (right != other.right)
                return right.CompareTo(other.right);
            return str.CompareTo(other.str);
        }

        public override int GetHashCode() => str.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is WordStat)
                return str == (obj as WordStat).str;
            else
                return GetHashCode() == obj.GetHashCode();
        }
    }
}
