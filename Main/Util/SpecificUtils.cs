using SQLite;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

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

    public enum HUDType { Loading, Success, Fail};
    public interface HUDPopup
    {
        void Show(HUDType type = HUDType.Loading, string msg = "", int duaration = 1000);
        void Dismiss();
    }

    public static class SpecificUtils
    {
        public static FileUtil fileUtil = DependencyService.Get<FileUtil>();
        public static ImageUtil imgUtil = DependencyService.Get<ImageUtil>();
        public static HUDPopup hudPopup = DependencyService.Get<HUDPopup>();
    }
}
