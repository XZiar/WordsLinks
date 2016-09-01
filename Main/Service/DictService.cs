using Main.Model;
using Main.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Main.Util.BasicUtils;
using static Main.Util.SpecificUtils;
using static System.Text.Encoding;
using JKV = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Newtonsoft.Json.Linq.JToken>>;

namespace Main.Service
{
    public static class DictService
    {
        private static byte DBver = 0x1;
        private static SQLiteConnection db;

        private static Dictionary<WordStat, int> words = new Dictionary<WordStat, int>();
        private static Dictionary<WordStat, int> means = new Dictionary<WordStat, int>();
        private static SortedSet<WordStat> eles = new SortedSet<WordStat>();
        private static MultiValueDictionary<int, int> e2c = new MultiValueDictionary<int, int>();
        private static MultiValueDictionary<int, int> c2e = new MultiValueDictionary<int, int>();

        public static IEnumerable<string> Words { get { return words.Keys.Select(x=>x.str); } }
        public static IEnumerable<string> Meanings { get { return means.Keys.Select(x => x.str); } }
        public static int WordsCount { get { return words.Count; } }
        public static int MeansCount { get { return means.Count; } }
        public static long updTimeAll { get; private set; }
        public static long updTimeDetail { get; private set; }
        internal static int WrongCount { get; set; }
        public static bool isOutWrongCnt = false;

        static DictService()
        {
            db = sqlUtil.GetSQLConn("words.db");
        }

        public static void Init()
        {
            db.CreateTable<DBWord>();
            db.CreateTable<DBMeaning>();
            db.CreateTable<DBTranslation>();

            words.Clear(); means.Clear(); eles.Clear();
            WrongCount = 0;
            foreach(var w in db.Table<DBWord>())
            {
                var s = w.ToStat();
                words.Add(s, w.Id);
                eles.Add(s);
                if (s.wrong > 0)
                    WrongCount += s.wrong;
                WrongCount++;
            }
            foreach (var m in db.Table<DBMeaning>())
            {
                var s = m.ToStat();
                means.Add(s, m.Id);
                eles.Add(s);
                if (s.wrong > 0)
                    WrongCount += s.wrong;
                WrongCount++;
            }
            e2c.Clear(); c2e.Clear();
            foreach (var t in db.Table<DBTranslation>())
            {
                e2c.Add(t.Wid, t.Mid);
                c2e.Add(t.Mid, t.Wid);
            }
            updTimeAll = updTimeDetail = DateTime.Now.Ticks;
        }

        public static void Clear(bool isRefresh = true)
        {
            db.DropTable<DBWord>();
            db.DropTable<DBMeaning>();
            db.DropTable<DBTranslation>();
            if(isRefresh)
                Init();
            else
            {
                db.CreateTable<DBWord>();
                db.CreateTable<DBMeaning>();
                db.CreateTable<DBTranslation>();
            }
        }

        public static DBWord WordAt(int idx)
        {
            if (idx < 0 || idx >= WordsCount)
                return null;
            var dat = words.ElementAt(idx);
            return new DBWord() { Letters = dat.Key, Id = dat.Value };
        }
        public static DBMeaning MeanAt(int idx)
        {
            if (idx < 0 || idx >= MeansCount)
                return null;
            var dat = means.ElementAt(idx);
            return new DBMeaning() { Meaning = dat.Key, Id = dat.Value };
        }
        public static WordStat EleAt(int num)
        {
            int bak = num;
            foreach(var s in eles)
            {
                if(s.wrong >= 0)
                    num -= s.wrong + 1;
                if (num-- <= 1)
                    return s;
            }
            Logger($"Run out of num when EleAt: {bak}-{WrongCount}={num}", LogLevel.Warning);
            return eles.First();
        }

        public static WordStat GetWordStat(string txt)
        {
            foreach (var s in eles)
                if (s.str == txt)
                    return s;
            return null;
        }
        public static DBMeaning[] GetMeansByWord(string eng)
        {
            int wid;
            if (!words.TryGetValue(eng, out wid))
                return null;
            return GetMeansByWId(wid);
        }
        public static DBMeaning[] GetMeansByWId(int wid)
        {
            IReadOnlyCollection<int> ids;
            if (!e2c.TryGetValue(wid, out ids))
                return null;
            if (ids.Count == 0)
                return new DBMeaning[0];
            return (from m in means
                    where ids.Contains(m.Value)
                    select new DBMeaning() { Id = m.Value, Meaning = m.Key })
                   .ToArray();
        }
        public static DBWord[] GetWordsByMean(string chi)
        {
            int mid;
            if (!means.TryGetValue(chi, out mid))
                return null;
            return GetWordsByMId(mid);
        }
        public static DBWord[] GetWordsByMId(int mid)
        {
            IReadOnlyCollection<int> ids;
            if (!c2e.TryGetValue(mid, out ids))
                return null;
            if (ids.Count == 0)
                return new DBWord[0];
            return (from m in words
                    where ids.Contains(m.Value)
                    select new DBWord() { Id = m.Value, Letters = m.Key })
                   .ToArray();
        }

        public static Task<IEnumerable<string>> MatchMeanings(IEnumerable<string> strs)
            => Task.Run(() =>
            {
                var ret = new List<string>();
                var chk = new List<string>();
                Tuple<int, int> pos;
                int fullMatchCount = 0, len = 0, minlen = 0, maxlen = 0;
                char last;
                foreach (var m in strs)
                {
                    if (m.Length >= 16)
                        continue;
                    last = m[m.Length - 1];
                    chk.Add((last == '的' || last == '地') ? m.Substring(0, m.Length - 1) : m);
                }
                foreach (string mean in means.Keys)
                {
                    last = mean[mean.Length - 1];
                    string tm = (last == '的' || last == '地') ? mean.Substring(0, mean.Length - 1) : mean;
                    foreach (var m in chk)
                    {
                        len = LCS(tm, m, out pos);
                        MinMax(tm.Length, m.Length, out minlen, out maxlen);

                        if (len == minlen)
                        {
                            ret.Insert(0, mean);
                            fullMatchCount++;
                        }
                        if (len * 2 >= minlen && len * 4 >= maxlen)
                        {
                            if (pos.Item1 * pos.Item2 == 0 && len > 1)
                                ret.Insert(fullMatchCount, mean);
                            else
                                ret.Add(mean);
                        }
                    }
                }
                return ret.Distinct() as IEnumerable<string>;
            });

        public static Task<byte[]> Export()
            => Task.Run(() =>
            {
                var tmp = new Dictionary<string,object>
                {
                    ["words"] = words.ToDictionary(x => x.Key.str, x => x.Value),
                    ["means"] = means.ToDictionary(x => x.Key.str, x => x.Value),
                    ["links"] = e2c
                };
                if (isOutWrongCnt)
                {
                    tmp.Add("wwcnt", db.Table<DBWord>().ToDictionary(x => x.Id, x => x.wrong));
                    tmp.Add("mwcnt", db.Table<DBMeaning>().ToDictionary(x => x.Id, x => x.wrong));
                }
                string total = JsonConvert.SerializeObject(tmp);
                Logger(total);

                byte[] txtdat = Unicode.GetBytes(total);
                int len = txtdat.Length;
                int wh = (int)Math.Ceiling(Math.Sqrt((len + 2) / 3 + 2));//3byte=>4byte
                byte[] dat = new byte[wh * wh * 4];
                Logger($"encode:{len} => {dat.Length}({wh}*{wh})");
                byte[] header = new byte[8]
                {
                    DBver, 0, 0, 0xFF,
                    (byte)len, (byte)(len >> 8), (byte)(len >> 16), 0xFF
                };
                Array.Copy(header, dat, 8);
                Byte3To4(len, txtdat, 0, dat, 8);

                using (Stream stream = imgUtil.CompressBitmap(dat, wh, wh))
                {
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, (int)stream.Length);
                    return data;
                }
            });

        private static bool ReplaceImport(JObject wordJO, JObject meanJO, JObject linkJO,
            Dictionary<int, short> wwcnt, Dictionary<int, short> mwcnt)
        {
            Clear(false);
            var wMap = new Dictionary<int, int>();
            var mMap = new Dictionary<int, int>();
            {
                var ws = new DBWord[wordJO.Count];
                var ids = new int[wordJO.Count];
                int a = 0, tid;
                short wcnt;
                foreach (var jp in wordJO)
                {
                    ws[a] = new DBWord() { Letters = jp.Key.ToLower() };
                    if (wwcnt.TryGetValue(tid = jp.Value.ToInt(), out wcnt))
                        ws[a].wrong = wcnt;
                    ids[a++] = tid;
                }
                db.InsertAll(ws);
                a = 0;
                foreach (var w in ws)
                    wMap.Add(ids[a++], w.Id);
            }
            {
                var ms = new DBMeaning[meanJO.Count];
                var ids = new int[meanJO.Count];
                int a = 0, tid;
                short wcnt;
                foreach (var jp in meanJO)
                {
                    ms[a] = new DBMeaning() { Meaning = jp.Key.ToLower() };
                    if (mwcnt.TryGetValue(tid = jp.Value.ToInt(), out wcnt))
                        ms[a].wrong = wcnt;
                    ids[a++] = jp.Value.ToInt();
                }
                db.InsertAll(ms);
                a = 0;
                foreach (var m in ms)
                    mMap.Add(ids[a++], m.Id);
            }
            {
                var ts = new List<DBTranslation>();
                foreach (var jp in linkJO)
                {
                    int wid = wMap[jp.Key.ToInt()];
                    foreach (var ji in jp.Value as JArray)
                        ts.Add(new DBTranslation(wid, mMap[ji.ToInt()]));
                }
                db.InsertAll(ts);
            }
            Init();
            return true;
        }
        private static bool AddImport(JObject wordJO, JObject meanJO, JObject linkJO)
        {
            var wMap = new Dictionary<int, int>();
            var mMap = new Dictionary<int, int>();
            int tmp;
            var w = new DBWord();
            foreach (var jp in wordJO)
            {
                if (words.TryGetValue(w.Letters = jp.Key.ToLower(), out tmp))
                    wMap.Add(w.Id = jp.Value.ToInt(), tmp);
                else
                {
                    db.Insert(w);
                    wMap.Add(jp.Value.ToInt(), w.Id);
                    words.Add(w.Letters, w.Id);
                    eles.Add(w.ToStat());
                    WrongCount += w.wrong + 1;
                }
            }
            var m = new DBMeaning();
            foreach (var jp in meanJO)
            {
                if (means.TryGetValue(m.Meaning = jp.Key, out tmp))
                    mMap.Add(m.Id = jp.Value.ToInt(), tmp);
                else
                {
                    db.Insert(m);
                    mMap.Add(jp.Value.ToInt(), m.Id);
                    means.Add(m.Meaning, m.Id);
                    eles.Add(m.ToStat());
                    WrongCount += m.wrong + 1;
                }
            }
            var t = new DBTranslation();
            foreach (var jp in linkJO)
            {
                t.Wid = wMap[jp.Key.ToInt()];
                foreach (var ji in jp.Value as JArray)
                {
                    t.Mid = mMap[ji.ToInt()];
                    if (!e2c.Contains(t.Wid, t.Mid))
                    {
                        e2c.Add(t.Wid, t.Mid);
                        c2e.Add(t.Mid, t.Wid);
                        db.Insert(t);
                    }
                }
            }
            updTimeAll = updTimeDetail = DateTime.Now.Ticks;
            return true;
        }

        private static JObject Decoder(byte[] bmp)
        {
            if (DBver != bmp[0])
                return null;
            int len = bmp[4] + (bmp[5] << 8) + (bmp[6] << 16);
            Logger($"decode:{bmp.Length} => {len}");

            byte[] dat = new byte[len];
            Byte4To3(len, bmp, 8, dat, 0);

            var total = Unicode.GetString(dat, 0, dat.Length);
            Logger(total);
            return JsonConvert.DeserializeObject<JObject>(total);
        }
        public static Task<bool> Import(byte[] bmp, bool isReplace, bool isImWCnt = false)
            => Task.Run(() =>
            {
                var obj = Decoder(bmp);
                if (obj == null)
                    return false;
                JObject wordJO = obj["words"] as JObject, meanJO = obj["means"] as JObject, linkJO = obj["links"] as JObject;
                if (isReplace)
                {
                    Dictionary<int, short> wwcnt = null, mwcnt = null;
                    if(isImWCnt)
                    {
                        wwcnt = (obj["wwcnt"] as JKV)?.ToDictionary(x => x.Key.ToInt(), x => (short)x.Value.ToInt());
                        mwcnt = (obj["mwcnt"] as JKV)?.ToDictionary(x => x.Key.ToInt(), x => (short)x.Value.ToInt());
                    }
                    wwcnt = wwcnt ?? new Dictionary<int, short>();
                    mwcnt = mwcnt ?? new Dictionary<int, short>();
                    return ReplaceImport(wordJO, meanJO, linkJO, wwcnt, mwcnt);
                }
                else
                    return AddImport(wordJO, meanJO, linkJO);
            });

        public static void AddWord(string eng, ICollection<string> chi)
        {
            int wid, mid;
            if (!words.TryGetValue(eng, out wid))//add word
            {
                var word = new DBWord() { Letters = eng };
                db.Insert(word);
                words[eng] = wid = word.Id;
                eles.Add(word.ToStat());
                WrongCount += word.wrong + 1;
            }
            foreach (var str in chi)
            {
                if (!means.TryGetValue(str, out mid))//add meaning
                {
                    var mean = new DBMeaning() { Meaning = str };
                    db.Insert(mean);
                    means[str] = mid = mean.Id;
                    eles.Add(mean.ToStat());
                    WrongCount += mean.wrong + 1;
                }
                if (!e2c.Contains(wid, mid))
                {
                    db.Insert(new DBTranslation() { Wid = wid, Mid = mid });
                    e2c.Add(wid, mid);
                    c2e.Add(mid, wid);
                }
            }
            updTimeAll = updTimeDetail = DateTime.Now.Ticks;
        }

        public static void Report(string str, short minus)
        {
            var obj = eles.First(x => x.str == str);
            if (obj == null)
                return;
            //Debug.WriteLine($"Report {str}({obj.wrong}) for {minus}");
            eles.Remove(obj);
            bool wFitst = obj.wrong > 0;
            obj.wrong += minus;
            if (wFitst)
            {
                if (obj.wrong < 0)
                    minus += obj.wrong;
                else if (obj.wrong > 120)
                {
                    short tmp = (short)(obj.wrong / 2);
                    minus += tmp;
                    obj.wrong -= tmp;
                }
                WrongCount -= minus;
            }
            else
            {
                if (obj.wrong > 0)
                    WrongCount += obj.wrong;
                else if (obj.wrong < -120)
                    obj.wrong /= 2;
            }
            int id;
            if (words.TryGetValue(obj, out id))
                db.Update(new DBWord(obj, id));
            else if (means.TryGetValue(obj, out id))
                db.Update(new DBMeaning(obj, id));
            eles.Add(obj);
            updTimeDetail = DateTime.Now.Ticks;
        }

        public static string debugInfo()
        {
            StringBuilder sb = 
                new StringBuilder($"Total wrong: {WrongCount} for {WordsCount} words and {MeansCount} means\r\n");
            foreach (var e in eles)
                sb.Append($"{e.wrong} \t {e.str}\r\n");
            return sb.ToString();
        }

        public static void updateDB()
        {
            int ret = db.Execute("update Words set wrong=wrong+15");
            Debug.WriteLine($"affect {ret} records");
            ret = db.Execute("update Meanings set wrong=wrong+15");
            Debug.WriteLine($"affect {ret} records");
            Init();
        }
    }
}
