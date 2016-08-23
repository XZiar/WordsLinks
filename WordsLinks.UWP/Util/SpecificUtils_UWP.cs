using Main.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using static WordsLinks.UWP.Util.BasicUtils;

namespace WordsLinks.UWP.Util
{
    public class FileUtil_UWP : FileUtil
    {
        private static string documentsPath;
        private static string cachePath;
        private static StorageFolder docFolder;
        private static StorageFolder cacheFolder;

        static FileUtil_UWP()
        {
            docFolder = ApplicationData.Current.LocalFolder;
            documentsPath = docFolder.Path;
            cacheFolder = ApplicationData.Current.TemporaryFolder;
            cachePath = cacheFolder.Path;
        }
        public string GetCacheFilePath(string fileName)
        {
            CreateFile(fileName);
            Debug.WriteLine($"{cachePath}");
            return Path.Combine(cachePath, fileName);
        }

        private async void CreateFile(string fileName)
        {
            await docFolder.CreateFileAsync(fileName);
        }

        public string GetFilePath(string fileName, bool isPrivate = false)
        {
            return Path.Combine(documentsPath, fileName);
        }
    }

    class SQLiteUtil_UWP : SQLiteUtil
    {
        public SQLiteConnection GetSQLConn(string dbName)
        {
            string dbPath = new FileUtil_UWP().GetFilePath(dbName, true);
            return new SQLiteConnection(dbPath);
        }
    }

    class ThreadUtil_UWP : ThreadUtil
    {
        public void Sleep(int ms)
        {
            Task.Delay(ms).Wait();
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
            return memStream.AsStream();
        }

        private FileOpenPicker oPicker = new FileOpenPicker()
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };
        private FileSavePicker sPicker = new FileSavePicker()
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
        public Task<bool> SaveImage(Stream ins)
        {
            var tsk = new TaskCompletionSource<bool>();
            byte[] dat = new byte[ins.Length];
            ins.Read(dat, 0, (int)ins.Length);
            RunInUI(async () =>
                {
                    var file = await sPicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        CachedFileManager.DeferUpdates(file);
                        await FileIO.WriteBytesAsync(file, dat);
                        var status = await CachedFileManager.CompleteUpdatesAsync(file);
                        tsk.SetResult(status == FileUpdateStatus.Complete);
                    }
                    else
                        tsk.SetResult(false);
                });
            return tsk.Task;
        }
    }

    class HUDPopup_UWP : HUDPopup
    {
        public void Dismiss()
        {
            throw new NotImplementedException();
        }

        public void Show(HUDType type = HUDType.Loading, string msg = "", int duaration = 640)
        {
            throw new NotImplementedException();
        }
    }
}
/*
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
*/
