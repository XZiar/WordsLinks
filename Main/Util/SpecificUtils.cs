using SQLite;
using System.IO;
using System.Threading.Tasks;

namespace Main.Util
{
    public interface ThreadUtil
    {
        void Sleep(int ms);
    }

    public interface FileUtil
	{
		string GetFilePath(string fileName, bool isPrivate = false);
		string GetCacheFilePath(string fileName);
	}

    public enum LogLevel { Exception, Warning, Verbose};
    public interface LogUtil
    {
        void Log(string txt, LogLevel level = LogLevel.Verbose);
    }

	public interface SQLiteUtil
	{
		SQLiteConnection GetSQLConn(string dbName);
	}

    public interface ImageUtil
    {
        Stream CompressBitmap(byte[] data, int w, int h);
        Task<byte[]> GetImage();
        Task<bool> SaveImage(byte[] data);
    }

    public interface OpenFileUtil
    {
        void OpenFile(string path);
    }

    public enum HUDType { Loading, Success, Fail};
    public interface HUDPopup
    {
        void Show(HUDType type = HUDType.Loading, string msg = "", int duaration = 640);
        void Dismiss();
    }

    

    public static class SpecificUtils
    {
        public static FileUtil fileUtil { get; private set; }
        public static ImageUtil imgUtil { get; private set; }
        public static HUDPopup hudPopup { get; private set; }
        public static ThreadUtil threadUtil { get; private set; }
        public static SQLiteUtil sqlUtil { get; private set; }
        public static LogUtil logUtil { get; private set; }
        public static OpenFileUtil openfileUtil { get; private set; }

        public static void Init(params object[] utils)
        {
            foreach (var u in utils)
            {
                if (u is FileUtil)
                    fileUtil = u as FileUtil;
                else if (u is ImageUtil)
                    imgUtil = u as ImageUtil;
                else if (u is HUDPopup)
                    hudPopup = u as HUDPopup;
                else if (u is ThreadUtil)
                    threadUtil = u as ThreadUtil;
                else if (u is SQLiteUtil)
                    sqlUtil = u as SQLiteUtil;
                else if (u is LogUtil)
                    logUtil = u as LogUtil;
                else if (u is OpenFileUtil)
                    openfileUtil = u as OpenFileUtil;
            }
        }
    }
}
