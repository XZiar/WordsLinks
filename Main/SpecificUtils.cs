using SQLite;

namespace WordsLinks.Util
{
	public interface FileUtil
	{
		string GetFilePath(string fileName, bool isPrivate = false);
		string GetCacheFilePath(string fileName);
	}

	public interface SQLiteUtil
	{
		SQLiteConnection GetSQLConn(string dbName);
	}
}
