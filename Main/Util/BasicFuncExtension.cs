using System;
using System.Collections.Generic;

namespace WordsLinks.Util
{
    static class BasicFuncExtension
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
    }
}
