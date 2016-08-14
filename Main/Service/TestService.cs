using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using WordsLinks.Util;
using Xamarin.Forms;
using static WordsLinks.Util.BasicUtils;

namespace WordsLinks.Service
{
    static class TestService
    {
        static ImageUtil imgUtil;
        static TestService()
        {
            imgUtil = DependencyService.Get<ImageUtil>();
        }
        public static void ImgTester()
        {
            var assembly = typeof(TestService).GetTypeInfo().Assembly;
            try
            {
                //Stream stream = assembly.GetManifestResourceStream("Main.search.png");
                byte[] dat = new byte[8 * 8 * 4];
                for (int a = 1, x = 0; a <= 8; a++)
                for (int b = 1; b <= 8; b++)
                    {
                        dat[x++] = (byte)(255 / a);
                        dat[x++] = (byte)(255 / b);
                        dat[x++] = (byte)(511 / (a + b));
                        dat[x++] = 255;
                    }
                Stream stream = imgUtil.CompressBitmap(dat, 8, 8);
                imgUtil.SaveImage(stream);
            }
            catch (Exception e)
            {
                OnException(e, "test image");
            }
        }
        public static void SaveTester(byte[] dat, int w, int h)
        {
            try
            {
                Stream stream = imgUtil.CompressBitmap(dat, w, h);
                imgUtil.SaveImage(stream);
            }
            catch (Exception e)
            {
                OnException(e, "test image");
            }
        }

        public static void ReadTester()
        {
            try
            {
                imgUtil.GetImage(data => 
                {
                    Debug.WriteLine($"callback: {data} === {data!=null}");
                    if (data != null)
                    {
                        try
                        {
                            DBService.Import(data);
                        }
                        catch (Exception e)
                        {
                            OnException(e, "db read image");
                        }
                    }
                });
            }
            catch (Exception e)
            {
                OnException(e, "read image");
            }
        }
    }
}
