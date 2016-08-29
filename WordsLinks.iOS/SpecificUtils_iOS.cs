using BigTed;
using CoreGraphics;
using Foundation;
using Main.Util;
using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using WordsLinks.iOS;
using Xamarin.Forms;
using static BigTed.ProgressHUD;
using static Main.Util.BasicUtils;
using static WordsLinks.iOS.BasicUtils;

[assembly: Dependency(typeof(FileUtil_iOS))]
[assembly: Dependency(typeof(SQLiteUtil_iOS))]
[assembly: Dependency(typeof(ImageUtil_iOS))]
[assembly: Dependency(typeof(HUDPopup_iOS))]
[assembly: Dependency(typeof(ThreadUtil_iOS))]
[assembly: Dependency(typeof(LogUtil_iOS))]

namespace WordsLinks.iOS
{
    class ThreadUtil_iOS : ThreadUtil
    {
        public void Sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }
    }

    class FakeFile_iOS : FakeFile
    {
        public FakeFile_iOS(string fname) : base(fname) { }

        public override void OpenWith()
        {
            NSUrl url = NSUrl.FromFilename(path);
            RunInUI(() =>
            {
                var ctrl = new UIActivityViewController(new NSObject[] { new NSString("Plain Text"), url }, null);
                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentModalViewController(ctrl, true);
            });
        }
    }

    class FileUtil_iOS : FileUtil
    {
        internal static string documentsPath;
        internal static string libraryPath;
        internal static string cachePath;
        static FileUtil_iOS()
        {
            documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            libraryPath = Path.Combine(documentsPath, "..", "Library");
            cachePath = Path.Combine(libraryPath, "Caches");
        }

        internal static string GetPath(string dir, string fname)
        {
            return Path.Combine(dir, fname);
        }

        public FakeFile GetCacheFile(string fileName) => new FakeFile_iOS(GetPath(cachePath, fileName));

        public FakeFile GetFile(string fileName, bool isPrivate) =>
            new FakeFile_iOS(GetPath(isPrivate ? documentsPath : libraryPath, fileName));
    }

    class LogUtil_iOS : LogUtil
    {
        internal static FakeFile LogFile;
        internal static FileStream logStream { get; private set; }
        private static StreamWriter logWriter;
        public FakeFile logFile
        {
            get { return LogFile; }
        }

        static LogUtil_iOS()
        {
            LogFile = new FakeFile_iOS(FileUtil_iOS.GetPath(FileUtil_iOS.cachePath, "AppLog.log"));
            logStream = new FileStream(LogFile.path, FileMode.OpenOrCreate);
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

    class SQLiteUtil_iOS : SQLiteUtil
    {
        public SQLiteConnection GetSQLConn(string dbName)
        {
            string dbPath = FileUtil_iOS.GetPath(FileUtil_iOS.documentsPath, dbName);
            Logger("open db at " + dbPath);
            return new SQLiteConnection(dbPath);
        }
    }

    class ImageUtil_iOS : ImageUtil
    {
        static CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
        static CGBitmapFlags bmpFlags = CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast;

        public Stream CompressBitmap(byte[] data, int w, int h)
        {
            var ctx = new CGBitmapContext(data, w, h, 8, 4 * w, colorSpace, bmpFlags);
            var cgimg = ctx.ToImage();
            var img = UIImage.FromImage(cgimg);
            var dat = img.AsPNG();
            Dispose(img, cgimg, ctx);
            return dat.AsStream();
        }

        private static UIImagePickerController picker = new UIImagePickerController()
        {
            SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
            AllowsEditing = false,
        };
        public Task<byte[]> GetImage()
        {
            var tsk = new TaskCompletionSource<byte[]>();
            EventHandler<UIImagePickerMediaPickedEventArgs> onSuc = null;
            EventHandler onCancel = null;
            onSuc = (o, e) =>
            {
                picker.FinishedPickingMedia -= onSuc;
                picker.Canceled -= onCancel;
                picker.DismissModalViewController(true);
                if (e.Info[UIImagePickerController.MediaType].ToString() == "public.image")
                {
                    UIImage uiimg = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                    int w = (int)uiimg.Size.Width, h = (int)uiimg.Size.Height, size = w * h * 4;
                    byte[] data = new byte[size];
                    Logger($"select image {size}({w}*{h})");

                    var ctx = new CGBitmapContext(data, w, h, 8, 4 * w, colorSpace, bmpFlags);
                    ctx.DrawImage(new CGRect(0, 0, w, h), uiimg.CGImage);
                    Dispose(ctx, uiimg);
                    tsk.SetResult(data);
                }
                else
                    tsk.SetResult(null);
            };
            onCancel = (o, e) => 
            {
                picker.FinishedPickingMedia -= onSuc;
                picker.Canceled -= onCancel;
                picker.DismissModalViewController(true);
                tsk.SetResult(null);
            };
            RunInUI(() =>
            {
                picker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
                picker.FinishedPickingMedia += onSuc;
                picker.Canceled += onCancel;
                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentModalViewController(picker, true);
            });
            return tsk.Task;
        }

        public Task<bool> SaveImage(byte[] data)
        {
            var tsk = new TaskCompletionSource<bool>();
            var img = UIImage.LoadFromData(NSData.FromArray(data));
            RunInUI(() =>
                img.SaveToPhotosAlbum((image, err) =>
                {
                    Dispose(img, image);
                    bool isSuc = (err == null);
                    if (!isSuc)
                        err.OnError("savePhoto");
                    tsk.SetResult(isSuc);
                })
            );
            return tsk.Task;
        }
    }

    class HUDPopup_iOS : HUDPopup
    {
        public void Dismiss()
        {
            BTProgressHUD.Dismiss();
        }

        public void Show(HUDType type, string msg, int duaration)
        {
            switch (type)
            {
            case HUDType.Loading:
                BTProgressHUD.Show(msg, maskType: MaskType.Black);
                break;
            case HUDType.Success:
                BTProgressHUD.ShowImage(UIImage.FromBundle("IconSuccess"), msg, duaration);
                break;
            case HUDType.Fail:
                BTProgressHUD.ShowImage(UIImage.FromBundle("IconFail"), msg, duaration);
                break;
            }
        }
    }
}
