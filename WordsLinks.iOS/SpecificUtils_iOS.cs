using SQLite;
using System;
using System.IO;
using WordsLinks.iOS;
using WordsLinks.Util;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileUtil_iOS))]
[assembly: Dependency(typeof(SQLiteUtil_iOS))]

namespace WordsLinks.iOS
{
	public class FileUtil_iOS : FileUtil
	{
		private static string documentsPath;
		private static string libraryPath;
		private static string cachePath;
		static FileUtil_iOS()
		{
			documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			libraryPath = Path.Combine(documentsPath, "..", "Library");
			cachePath = Path.Combine(libraryPath, "Caches");
		}

		public string GetCacheFilePath(string fileName)
		{
			return Path.Combine(cachePath, fileName);
		}

		public string GetFilePath(string fileName, bool isPrivate)
		{
			return Path.Combine(isPrivate ? documentsPath : libraryPath, fileName);
		}
	}

	class SQLiteUtil_iOS : SQLiteUtil
	{
		public SQLiteConnection GetSQLConn(string dbName)
		{
			string dbPath = new FileUtil_iOS().GetFilePath(dbName, true);
			return new SQLiteConnection(dbPath);
		}
	}
}
