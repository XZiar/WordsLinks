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
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Core;

namespace WordsLinks.UWP.Util
{
    public class FileUtil_UWP : FileUtil
    {
        private static string documentsPath;
        private static string cachePath;
        internal static StorageFolder docFolder { get; private set; }
        internal static StorageFolder cacheFolder { get; private set; }

        static FileUtil_UWP()
        {
            docFolder = ApplicationData.Current.LocalFolder;
            documentsPath = docFolder.Path;
            cacheFolder = ApplicationData.Current.TemporaryFolder;
            cachePath = cacheFolder.Path;
        }
        public string GetCacheFilePath(string fileName)
        {
            CreateFile(cacheFolder, fileName);
            return Path.Combine(cachePath, fileName);
        }

        private async void CreateFile(StorageFolder folder, string fileName)
        {
            await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
        }

        public string GetFilePath(string fileName, bool isPrivate = false)
        {
            CreateFile(docFolder, fileName);
            return Path.Combine(documentsPath, fileName);
        }
    }

    class SQLiteUtil_UWP : SQLiteUtil
    {
        public SQLiteConnection GetSQLConn(string dbName)
        {
            string dbPath = SpecificUtils.fileUtil.GetFilePath(dbName, true);
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
        private TextBlock msgBar;
        private Border msgBox;
        private static Brush WhiteBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            BlackBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
            RedBrush = new SolidColorBrush(Color.FromArgb(64, 255, 0, 0)),
            GreenBrush = new SolidColorBrush(Color.FromArgb(64, 0, 255, 0)),
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
            Main.Util.BasicUtils.OnExceptionEvent += OnExceptionMsg;
        }

        private void OnExceptionMsg(Exception e, string log) =>
            Show(HUDType.Fail, e.Message, 2000);

        public void Dismiss()
        {
            if (!hasInit)
                Debug.WriteLine("Dismiss");
            else
                RunInUI(DismissHadler);
        }

        public void Show(HUDType type = HUDType.Loading, string msg = "", int duaration = 640)
        {
            if (!hasInit)
            {
                Debug.WriteLine($"{type} : {msg}");
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

