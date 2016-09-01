using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Main.Util
{
    public static partial class BasicUtils
    {
        /// <summary>
        /// 将IList中元素打乱顺序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="rand">传入随机数发生器</param>
        public static IList<T> Shuffle<T>(this IList<T> list, Random rand)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        /// <summary>
        /// 将字符串转换为小写，并返回原字符串中是否存在大写字母
        /// </summary>
        /// <param name="str"></param>
        /// <param name="result">返回源字符串是否存在大写字母，null表示源字符串为空</param>
        /// <returns></returns>
        public static string TryToLower(this string str, out bool? result)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                result = null;
                return str;
            }
            result = false;
            char[] ch = new char[str.Length];
            int a = 0;
            foreach(var obj in str)
            {
                if (obj >= 'A' && obj <= 'Z')
                {
                    ch[a++] = (char)(obj + 32); // (char)('a' - 'A')
                    result = true;
                }
                else
                    ch[a++] = obj;
            }
            return new string(ch);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }

        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            return source.Any();
        }

        public static int ToInt(this JToken token) => token.ToString().ToInt();

        public static int ToInt(this string str) => int.Parse(str);
    }
}
