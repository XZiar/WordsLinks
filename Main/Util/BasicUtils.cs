using System;
using System.Diagnostics;

namespace WordsLinks.Util
{
    public class BasicUtils
    {
        public static void OnException(Exception e, string where = "", bool isShow = false)
        {
            Debug.WriteLine($"Exception when {where}:{e.GetType()}\n{e.Message}\nat {e.Source}\n{e.StackTrace}\n");
        }

        unsafe public static void Byte3To4(int len3, byte[] dat3, int offsetF, byte[] dat4, int offsetT, byte fill = 0xff)
        {
            int count = (len3 + 2) / 3, newlen = count * 4;
            if (offsetF + len3 > dat3.Length || offsetT + newlen > dat4.Length)
                throw new ArgumentException();
            fixed (byte* d3 = dat3, d4 = dat4)
            {
                byte* f = d3 + offsetF, t = d4 + offsetT;
                for (int a = 1; a < count; a++)
                {
                    *((int*)t) = *((int*)f);
                    f += 3;
                    t += 4;
                    *(t - 1) = fill;
                }
                int b = len3 - (count - 1) * 3;
                byte* bak = t + 3;
                while(b-- > 0)
                    *t++ = *f++;
                *bak = fill;
            }
        }
        unsafe public static void Byte4To3(int len3, byte[] dat4, int offsetF, byte[] dat3, int offsetT)
        {
            int count = (len3 + 2) / 3, oldlen = count * 4;
            if (offsetT + len3 > dat3.Length || offsetF + oldlen > dat4.Length)
                throw new ArgumentException();
            fixed (byte* d3 = dat3, d4 = dat4)
            {
                byte* t = d3 + offsetT, f = d4 + offsetF;
                for (int a = 1; a < count; a++)
                {
                    *((int*)t) = *((int*)f);
                    f += 4;
                    t += 3;
                }
                int b = len3 - (count - 1) * 3;
                while (b-- > 0)
                    *t++ = *f++;
            }
        }
    }
}
