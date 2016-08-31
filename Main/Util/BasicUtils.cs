using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Main.Util
{
    public static class BasicUtils
    {
        public delegate void ExceptionHandler(Exception e, string log);
        public static ExceptionHandler OnExceptionEvent;
        public static void OnException(Exception e, string where = "", bool isShow = false)
        {
            var log = $"Exception when {where} :{e.GetType()}\r\n{e.Message}\r\n##at \t{e.Source}\r\n{e.StackTrace}";
            Logger(log, LogLevel.Exception);
            OnExceptionEvent?.Invoke(e, log);
        }
        public static void CopeWith(this Exception e, string where = "", bool isShow = false)
            => OnException(e, where, isShow);

        public static void Logger(string txt, LogLevel level = LogLevel.Verbose)
        {
#if DEBUG
            Debug.WriteLine(txt);
#endif
#if TRACE
            SpecificUtils.logUtil?.Log(txt, level);
#endif
        }

        public static Assembly assembly { get; } = typeof(BasicUtils).GetTypeInfo().Assembly;
        private static Dictionary<Type, Tuple<Assembly, string>> asmMap = new Dictionary<Type, Tuple<Assembly, string>>();
        private static Tuple<Assembly, string> GetAssembleInfo(Type type)
        {
            Tuple<Assembly, string> ret;
            if (!asmMap.TryGetValue(type, out ret))
            {
                var asm = type.GetTypeInfo().Assembly;
                ret = new Tuple<Assembly, string>(asm, asm.GetName().Name + ".");
                asmMap.Add(type, ret);
            }
            return ret;
        }
        public static Stream AssembleResource(string filename, object caller = null)
        {
            if (caller == null)
                return assembly.GetManifestResourceStream("Main." + filename);
            var asmInfo = GetAssembleInfo(caller is Type ? caller as Type : caller.GetType());
            return asmInfo.Item1.GetManifestResourceStream(asmInfo.Item2 + filename);
        }

        unsafe public static void Byte3To4(int len3, byte[] dat3, int offsetF, byte[] dat4, int offsetT, byte fill = 0xff)
        {
            int count = (len3 + 2) / 3, newlen = count * 4;
            if (offsetF + len3 > dat3.Length)
                throw new ArgumentException($"require {len3} from {offsetF}, overflow input's {dat3.Length}");
            if (offsetT + newlen > dat4.Length)
                throw new ArgumentException($"require {newlen} from {offsetT}, overflow input's {dat4.Length}");
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
            if (offsetT + len3 > dat3.Length)
                throw new ArgumentException($"require {len3} from {offsetT}, overflow input's {dat3.Length}");
            if (offsetF + oldlen > dat4.Length)
                throw new ArgumentException($"require {oldlen} from {offsetF}, overflow input's {dat4.Length}");
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

        private static ThreadLocal<byte[,]> matchMap = new ThreadLocal<byte[,]>(() => new byte[16, 16]);
        unsafe public static int LCS(string stra, string strb, out Tuple<int, int> pos)
        {
            byte[,] map = matchMap.Value;
            int lena = stra.Length, lenb = strb.Length, max = 0, posa = 0, posb = 0;
            if (lena > 16 || lenb > 16)
                throw new ArgumentException($"{stra} &&& {strb} \t : {lena} vs {lenb} overflow the limit of 16");
            fixed (char* cha = stra, chb = strb)
            {
                for (int a = 0; a < lena; a++)
                for (int b = 0; b < lenb; b++)
                    {
                        if (cha[a] != chb[b])
                        {
                            map[a, b] = 0;
                            continue;
                        }
                        map[a, b] = (a * b == 0) ? (byte)1 : (byte)(map[a - 1, b - 1] + 1);
                        if(map[a, b] > max)
                        {
                            max = map[a, b];
                            posa = a; posb = b;
                        }
                    }
            }
            pos = new Tuple<int, int>(posa, posb);
            return max;
        }

        public static void MinMax<T>(T a, T b, out T min, out T max) where T : IComparable<T>
        {
            if(a.CompareTo(b) < 0)
            {
                min = a; max = b;
            }
            else
            {
                min = b; max = a;
            }
        }

        public static string TryToLower(this string str, out bool? result)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                result = null;
                return str;
            }
            bool res = false;
            foreach (var ch in str)
                if (ch >= 'A' && ch <= 'Z')
                {
                    res = true;
                    break;
                }
            result = res;
            if (res)
                return str.ToLower();
            else
                return str;
        }

        public static int GetPrefixIndex(string str)
        {
            char ch = str[0];
            if (ch >= 'a' && ch <= 'z')
                return ch - 'a';
            if (ch >= 'A' && ch <= 'Z')
                return ch - 'A';
            else
                return 0;
        }
    }
}
