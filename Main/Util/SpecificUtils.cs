using SQLite;
using System.IO;

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

    public delegate void GetImageResponde(byte[] data);
    public interface ImageUtil
    {
        Stream CompressBitmap(byte[] data, int w, int h);
        void GetImage(GetImageResponde resp);
        void SaveImage(Stream ins);
    }
}
