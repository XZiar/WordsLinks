using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WordsLinks.Model;
using WordsLinks.Util;
using Xamarin.Forms;

namespace WordsLinks.Services
{
	static class DBService
	{
		private static SQLiteConnection db;
        private static bool _isChanged;
        public static bool isChanged
        {
            get
            {
                bool tmp = _isChanged;
                _isChanged = false;
                return tmp;
            }
            private set
            {
                _isChanged = value;
            }
        }
        private static Dictionary<string, int> words;
        private static Dictionary<string, int> means;
        public static string[] Meanings { get { return means.Keys.ToArray(); } }
        private static MultiValueDictionary<int, int> e2c, c2e;

        static DBService()
		{
			string dbPath = DependencyService.Get<FileUtil>().GetFilePath("words.db", true);
			Debug.WriteLine("open db at " + dbPath);
			db = DependencyService.Get<SQLiteUtil>().GetSQLConn("words.db");
		}

		public static void Init()
		{
            db.CreateTable<DBWord>();
            db.CreateTable<DBMeaning>();
            db.CreateTable<DBTranslation>();
            GetAll();
            isChanged = true;
        }

        private static void GetAll()
        {
            words = db.Table<DBWord>().ToDictionary(w => w.Letters, w => w.Id);
            means = db.Table<DBMeaning>().ToDictionary(w => w.Meaning, w => w.Id);
            e2c = new MultiValueDictionary<int, int>();
            c2e = new MultiValueDictionary<int, int>();
            foreach (var t in db.Table<DBTranslation>())
            {
                e2c.Add(t.Wid, t.Mid);
                c2e.Add(t.Mid, t.Wid);
            }

        }

        public static void Clear()
        {
            db.DropTable<DBWord>();
            db.DropTable<DBMeaning>();
            db.DropTable<DBTranslation>();
            Init();
        }

		public static DBMeaning[] GetMeansByWord(string eng)
        {
            int wid;
            if (!words.TryGetValue(eng, out wid))
                return null;
            IReadOnlyCollection<int> ids;
            if (!e2c.TryGetValue(wid, out ids))
                return null;
            if (ids.Count == 0)
                return null;
            return (from m in means
                    where ids.Contains(m.Value)
                    select new DBMeaning() { Id = m.Value, Meaning = m.Key })
                   .ToArray();
        }

        public static DBWord[] GetWordsByMean(string chi)
        {
            int mid;
            if (!means.TryGetValue(chi, out mid))
                return null;
            IReadOnlyCollection<int> ids;
            if (!c2e.TryGetValue(mid, out ids))
                return null;
            if (ids.Count == 0)
                return null;
            return (from m in words
                    where ids.Contains(m.Value)
                    select new DBWord() { Id = m.Value, Letters = m.Key })
                   .ToArray();
        }

        public static void AddWord(string eng, ICollection<string> chi)
        {
            int wid, mid;
            if(!words.TryGetValue(eng, out wid))//add word
            {
                wid = db.Insert(new DBWord() { Letters = eng });
                words[eng] = wid;
            }
            foreach(var str in chi)
            {
                if (!means.TryGetValue(str, out mid))//add meaning
                {
                    mid = db.Insert(new DBMeaning() { Meaning = str });
                    means[str] = mid;
                }
                try
                {
                    e2c.Add(wid, mid);
                    c2e.Add(mid, wid);
                }
                catch (ArgumentException) { }
            }
            isChanged = true;
        }
    }
}
