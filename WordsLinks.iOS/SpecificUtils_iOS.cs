using CoreGraphics;
using Foundation;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UIKit;
using WordsLinks.iOS;
using WordsLinks.Util;
using Xamarin.Forms;
using static WordsLinks.Util.BasicUtils;

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
            var ctx = new CGBitmapContext(data, w, h, 8, 4 * w, colorSpace,
                CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast);
            return UIImage.FromImage(ctx.ToImage()).AsPNG().AsStream();
        }

        public void GetImage(GetImageResponde resp)
        {
            try
            {
                var picker = new UIImagePickerController();
                picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                picker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
                picker.FinishedPickingMedia += (o, e) =>
                {
                    try
                    {
                        picker.DismissModalViewController(true);
                        if (e.Info[UIImagePickerController.MediaType].ToString() == "public.image")
                        {
                            UIImage uiimg = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                            if (uiimg != null)
                            {
                                int w = (int)uiimg.Size.Width, h = (int)uiimg.Size.Height;
                                int size = w * h * 4;
                                Debug.Write($"uuimage size : {w} * {h}");
                                byte[] data = new byte[size];
                                var ctx = new CGBitmapContext(data, w, h, 8, 4 * w, colorSpace,
                                    CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast);
                                ctx.DrawImage(new CGRect(0, 0, w, h), uiimg.CGImage);

                                StringBuilder str = new StringBuilder("ptr:");
                                for (int a = 0; a < 32; a++)
                                    str.Append($"{data[a]},");
                                Debug.WriteLine(str);
                                str = new StringBuilder("reverse:");
                                for (int a = 0, b = size; a < 32; a++)
                                    str.Append($"{data[--b]},");
                                Debug.WriteLine(str);
                                resp(data);
                            }
                        }
                        else
                            resp(null);
                    }
                    catch (Exception ex)
                    {
                        OnException(ex, "finish pick");
                    }
                };
                picker.Canceled += (o, e) => { picker.DismissModalViewController(true); resp(null); };
                var vctrl = UIApplication.SharedApplication.KeyWindow.RootViewController;
                vctrl.PresentModalViewController(picker, true);
            }
            catch (Exception e)
            {
                OnException(e, "start pick");
            }
        }

        public void SaveImage(Stream ins)
        {
            var img = UIImage.LoadFromData(NSData.FromStream(ins));
            img.SaveToPhotosAlbum((image, err) =>
            {
                Debug.WriteLine($"error : {err}");
            });
        }
    }
}
