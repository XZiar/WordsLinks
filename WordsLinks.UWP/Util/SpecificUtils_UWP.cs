using Main.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static Main.Util.BasicUtils;
using static WordsLinks.UWP.Util.BasicUtils;

namespace WordsLinks.UWP.Util
{
    class ThreadUtil_UWP : ThreadUtil
    {
        public void Sleep(int ms)
        {
            Task.Delay(ms).Wait();
        }
    }

    class FakeFile_UWP : FakeFile
    {
        public FakeFile_UWP(string fname) : base(fname) { }

        private static LauncherOptions opt = new LauncherOptions()
        { DisplayApplicationPicker = true };
        private static async void Open(string path)
        {
            await Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(path), opt);
        }
        public override void OpenWith()
        {
            Open(path);
        }
    }

    public class FileUtil_UWP : FileUtil
    {
        internal static StorageFolder docFolder { get; private set; }
        internal static StorageFolder cacheFolder { get; private set; }

        static FileUtil_UWP()
        {
            docFolder = ApplicationData.Current.LocalFolder;
            cacheFolder = ApplicationData.Current.TemporaryFolder;
        }

        private static async void CreateFile(StorageFolder folder, string fileName)
        {
            await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
        }
        internal static string GetPath(StorageFolder dir, string fname)
        {
            CreateFile(dir, fname);
            return Path.Combine(dir.Path, fname);
        }

        public FakeFile GetCacheFile(string fileName) => new FakeFile_UWP(GetPath(cacheFolder, fileName));

        public FakeFile GetFile(string fileName, bool isPrivate = false) => 
            new FakeFile_UWP(GetPath(docFolder, fileName));
    }

    public class LogUtil_UWP : LogUtil
    {
        internal static FakeFile LogFile;
        internal static Stream logStream { get; private set; }
        private static StreamWriter logWriter;
        public FakeFile logFile
        {
            get { return LogFile; }
        }

        public static async void Init()
        {
            LogFile = new FakeFile_UWP(FileUtil_UWP.GetPath(FileUtil_UWP.cacheFolder, "AppLog.log"));
            var file = await StorageFile.GetFileFromPathAsync(LogFile.path);
            logStream = await file.OpenStreamForWriteAsync();
            logStream.Position = logStream.Length;
            logWriter = new StreamWriter(logStream);
            TryLog("Init Logger");
        }

        private static void TryLog(string txt, LogLevel level = LogLevel.Verbose)
        {
            if (logWriter == null)
                return;
            lock (logWriter)
            {
                logWriter.Write($"{level} \t {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")}\r\n");
                logWriter.Write(txt + "\r\n\r\n");
                logWriter.Flush();
            }
        }

        public void Log(string txt, LogLevel level = LogLevel.Verbose) => TryLog(txt, level);
    }

    class SQLiteUtil_UWP : SQLiteUtil
    {
        public SQLiteConnection GetSQLConn(string dbName)
        {
            string dbPath = FileUtil_UWP.GetPath(FileUtil_UWP.docFolder, dbName);
            Logger("open db at " + dbPath);
            return new SQLiteConnection(dbPath);
        }
    }

    class ImageUtil_UWP : ImageUtil
    {
        public ImageUtil_UWP()
        {
            oPicker.FileTypeFilter.Add(".png");
            sPicker.FileTypeChoices.Add("图片文件", new List<string>() { ".png" });
        }

        private async void Encode(byte[] data, uint w, uint h, InMemoryRandomAccessStream stream)
        {
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Premultiplied, w, h, 96.0, 96.0, data);
            await encoder.FlushAsync();
        }
        public Stream CompressBitmap(byte[] data, int w, int h)
        {
            var memStream = new InMemoryRandomAccessStream();
            Encode(data, (uint)w, (uint)h, memStream);
            var stream = memStream.AsStreamForRead();
            Logger($"Log Encode stream for bugfix:{memStream.Size} => {stream.Length} bytes");
            return stream;
        }

        private static FileOpenPicker oPicker = new FileOpenPicker()
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };
        private static FileSavePicker sPicker = new FileSavePicker()
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };

        private static BitmapTransform nullTransform = new BitmapTransform();
        public Task<byte[]> GetImage()
        {
            var tsk = new TaskCompletionSource<byte[]>();
            RunInUI(async () =>
                {
                    var file = await oPicker.PickSingleFileAsync();
                    if (file == null)
                    {
                        tsk.SetResult(null);
                        return;
                    }

                    CachedFileManager.DeferUpdates(file);
                    var memStream = new InMemoryRandomAccessStream();
                    using (var writer = new DataWriter(memStream.GetOutputStreamAt(0)))
                    {
                        var buf = await FileIO.ReadBufferAsync(file);
                        writer.WriteBuffer(buf, 0, buf.Length);
                        await writer.StoreAsync();
                    }

                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(memStream);
                    var ret = await decoder.GetPixelDataAsync(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Premultiplied,
                        nullTransform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
                    tsk.SetResult(ret.DetachPixelData());
                    await CachedFileManager.CompleteUpdatesAsync(file);
                });
            return tsk.Task;
        }
        public Task<bool> SaveImage(byte[] data)
        {
            var tsk = new TaskCompletionSource<bool>();
            RunInUI(async () =>
                {
                    var file = await sPicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        CachedFileManager.DeferUpdates(file);
                        await FileIO.WriteBytesAsync(file, data);
                        var status = await CachedFileManager.CompleteUpdatesAsync(file);
                        tsk.SetResult(status == FileUpdateStatus.Complete);
                        //tsk.SetResult(true);
                    }
                    else
                        tsk.SetResult(false);
                });
            return tsk.Task;
        }
    }

    class HUDPopup_UWP : HUDPopup
    {
        private TextBlock msgBar;
        private Border msgBox;
        private static Brush WhiteBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            BlackBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
            RedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)),
            GreenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0)),
            DefaultBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 224));
        private bool hasInit = false;
        private DispatchedHandler DismissHadler;
        public void Init(TextBlock bar, Border box)
        {
            msgBar = bar;
            msgBox = box;
            DismissHadler = () =>
            {
                msgBar.Text = "";
                msgBox.Background = DefaultBrush;
            };
            hasInit = true;
            OnExceptionEvent += OnExceptionMsg;
        }

        private void OnExceptionMsg(Exception e, string log) =>
            Show(HUDType.Fail, e.Message, 2000);

        public void Dismiss()
        {
            if (!hasInit)
                Logger("Dismiss");
            else
                RunInUI(DismissHadler);
        }

        public void Show(HUDType type = HUDType.Loading, string msg = "", int duaration = 640)
        {
            if (!hasInit)
            {
                Logger($"{type} : {msg}");
                return;
            }

            RunInUI(() =>
            {
                msgBar.Text = msg;
                if (type == HUDType.Loading)
                {
                    msgBox.Background = DefaultBrush;
                    msgBar.Foreground = BlackBrush;
                }
                else
                {
                    new Timer((o) => Dismiss(), null, duaration, Timeout.Infinite);
                    msgBox.Background = type == HUDType.Fail ? RedBrush : GreenBrush;
                    msgBar.Foreground = WhiteBrush;
                }
            });
        }
    }
}

