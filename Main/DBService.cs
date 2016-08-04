using SQLite;
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
		static DBService()
		{
			string dbPath = DependencyService.Get<FileUtil>().GetFilePath("words.db", true);
			Debug.WriteLine("open db at " + dbPath);
			db = DependencyService.Get<SQLiteUtil>().GetSQLConn("words.db");
			db.CreateTable<DBWord>();
			db.CreateTable<DBMeaning>();
			db.CreateTable<DBTranslation>();
		}
		public static void init()
		{
			return;
		}
		public static List<T> GetAll<T>() where T : new()
		{
			return db.Table<T>().ToList();
		}
	}
}
