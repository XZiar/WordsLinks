using BigTed;
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
using static BigTed.ProgressHUD;
using static WordsLinks.iOS.BasicUtils;

[assembly: Dependency(typeof(FileUtil_iOS))]
[assembly: Dependency(typeof(SQLiteUtil_iOS))]
[assembly: Dependency(typeof(ImageUtil_iOS))]
[assembly: Dependency(typeof(HUDPopup_iOS))]

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
                    Debug.Write($"select image {size}({w}*{h})");

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

        public Task<bool> SaveImage(Stream ins)
        {
            var tsk = new TaskCompletionSource<bool>();
            var img = UIImage.LoadFromData(NSData.FromStream(ins));
            RunInUI(() =>
                img.SaveToPhotosAlbum((image, err) =>
                {
                    Dispose(img, image);
                    System.Threading.Thread.Sleep(1000);
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
