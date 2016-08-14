using SQLite;
using System.IO;
using System.Threading.Tasks;

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

    public interface ImageUtil
    {
        Stream CompressBitmap(byte[] data, int w, int h);
        Task<byte[]> GetImage();
        Task<bool> SaveImage(Stream ins);
    }
}
