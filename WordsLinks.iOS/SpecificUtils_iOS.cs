using CoreGraphics;
using Foundation;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using WordsLinks.iOS;
using WordsLinks.Util;
using Xamarin.Forms;
using static WordsLinks.Util.BasicUtils;
using static WordsLinks.iOS.BasicUtils;

[assembly: Dependency(typeof(FileUtil_iOS))]
[assembly: Dependency(typeof(SQLiteUtil_iOS))]
[assembly: Dependency(typeof(ImageUtil_iOS))]

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

    class ImageUtil_iOS : ImageUtil
    {
        static CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();

        public Stream CompressBitmap(byte[] data, int w, int h)
        {
            using (var ctx = new CGBitmapContext(data, w, h, 8, 4 * w, colorSpace,
                CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast))
            {
                using (var cgimg = ctx.ToImage())
                {
                    using (var img = UIImage.FromImage(cgimg))
                    {
                        return img.AsPNG().AsStream();
                    }
                }
            }
        }

        public Task<byte[]> GetImage()
        {
            var tsk = new TaskCompletionSource<byte[]>();
            var picker = new UIImagePickerController();
            EventHandler<UIImagePickerMediaPickedEventArgs> onSuc = (o, e) =>
            {
                picker.DismissModalViewController(true);
                if (e.Info[UIImagePickerController.MediaType].ToString() == "public.image")
                {
                    UIImage uiimg = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                    int w = (int)uiimg.Size.Width, h = (int)uiimg.Size.Height, size = w * h * 4;
                    byte[] data = new byte[size];
                    Debug.Write($"uuimage size : {w}*{h} with{NSThread.Current.IsMainThread}");

                    using (var ctx = new CGBitmapContext(data, w, h, 8, 4 * w, colorSpace,
                        CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast))
                    {
                        ctx.DrawImage(new CGRect(0, 0, w, h), uiimg.CGImage);
                        uiimg.Dispose();
                        tsk.SetResult(data);
                    }
                }
                else
                    tsk.SetResult(null);
            };
            EventHandler onCancel = (o, e) => { picker.DismissModalViewController(true); tsk.SetResult(null); };
            RunInUI(() =>
            {
                picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                picker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
                picker.FinishedPickingMedia += onSuc;
                picker.Canceled += onCancel;
                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentModalViewController(picker, true);
            });
            return tsk.Task;
        }

        public Task<bool> SaveImage(Stream ins)
        {
            var tsk = new TaskCompletionSource<bool>();
            using (var img = UIImage.LoadFromData(NSData.FromStream(ins)))
            {
                RunInUI(() => 
                img.SaveToPhotosAlbum((Image, err) =>
                    {
                        tsk.SetResult(err != null);
                    })
                );
            }
            return tsk.Task;
        }
    }
}
