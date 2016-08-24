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
using System.Threading.Tasks;
using static System.Text.Encoding;
using static Main.Util.BasicUtils;
using static Main.Util.SpecificUtils;
using System.Collections.Specialized;
using System.Text;

namespace Main.Service
{
    public static class DBService
    {
        private static byte DBver = 0x1;
        private static SQLiteConnection db;
        private static bool _isChanged;
        public static bool isChanged
        {
            get
            {
                bool tmp = _isChanged;
                _isChanged = false;
                return tmp;
            }
            private set
            {
                _isChanged = value;
            }
        }
        private static Dictionary<string, int> words = new Dictionary<string, int>();
        private static Dictionary<string, int> means = new Dictionary<string, int>();
        private static SortedSet<WordElement> eles = new SortedSet<WordElement>(DBEleComparer.Instance);
        private static MultiValueDictionary<int, int> e2c = new MultiValueDictionary<int, int>();
        private static MultiValueDictionary<int, int> c2e = new MultiValueDictionary<int, int>();

        public static IEnumerable<string> Words { get { return words.Keys; } }
        public static IEnumerable<string> Meanings { get { return means.Keys; } }
        public static int WordsCount { get { return words.Count; } }
        public static int MeansCount { get { return means.Count; } }

        static DBService()
        {
            string dbPath = fileUtil.GetFilePath("words.db", true);
            Debug.WriteLine("open db at " + dbPath);
            db = sqlUtil.GetSQLConn("words.db");
        }

        public static void Init()
        {
            db.CreateTable<DBWord>();
            db.CreateTable<DBMeaning>();
            db.CreateTable<DBTranslation>();

            words.Clear(); means.Clear(); eles.Clear();
            foreach(var w in db.Table<DBWord>())
            {
                words.Add(w.Letters, w.Id);
                eles.Add(w);
            }
            foreach (var w in db.Table<DBMeaning>())
            {
                means.Add(w.Meaning, w.Id);
                eles.Add(w);
            }
            /*
            StringBuilder sb = new StringBuilder("Here Sort List:\n");
            foreach (var e in eles)
                sb.Append($"{e.MissCount()} : {e.GetStr()}\n");
            Debug.WriteLine(sb);
            */
            e2c = new MultiValueDictionary<int, int>();
            c2e = new MultiValueDictionary<int, int>();
            foreach (var t in db.Table<DBTranslation>())
            {
                e2c.Add(t.Wid, t.Mid);
                c2e.Add(t.Mid, t.Wid);
            }

            isChanged = true;
        }

        public static void Clear()
        {
            db.DropTable<DBWord>();
            db.DropTable<DBMeaning>();
            db.DropTable<DBTranslation>();
            Init();
        }

        public static DBWord WordAt(int idx)
        {
            var dat = words.ElementAt(idx);
            return new DBWord() { Letters = dat.Key, Id = dat.Value };
        }
        public static DBMeaning MeanAt(int idx)
        {
            var dat = means.ElementAt(idx);
            return new DBMeaning() { Meaning = dat.Key, Id = dat.Value };
        }

        public static Task<IEnumerable<string>> MatchMeanings(IEnumerable<string> checker)
            => Task.Run(() =>
            {
                List<string> ret = new List<string>();
                Tuple<int, int> pos;
                foreach (var mean in means.Keys)
                    foreach (var m in checker)
                    {
                        try
                        {
                            int len = LCS(mean, m, out pos);
                            int objlen = Math.Min(mean.Length, m.Length);
                            //Debug.WriteLine($"compare {mean} & {m} : {len} at {pos.Item1},{pos.Item2} of {objlen}");
                            if (len * 2 >= objlen)
                            {
                                if (pos.Item1 * pos.Item2 == 0)
                                    ret.Insert(0, mean);
                                else
                                    ret.Add(mean);
                                break;
                            }
                        }
                        catch(Exception e)
                        {
                            e.CopeWith("LCS matching");
                        }
                    }
                return ret as IEnumerable<string>;
            });

        public static Task<bool> Export()
            => Task.Run(() =>
            {
                var tmp = new
                {
                    words = words,
                    means = means,
                    links = e2c,
                };
                string total = JsonConvert.SerializeObject(tmp);
                Debug.WriteLine(total);

                byte[] txtdat = Unicode.GetBytes(total);
                int len = txtdat.Length;
                int wh = (int)Math.Ceiling(Math.Sqrt((len + 2) / 3 + 2));//3byte=>4byte
                byte[] dat = new byte[wh * wh * 4];
                Debug.WriteLine($"calc:{len} => {dat.Length}({wh}*{wh})");
                byte[] header = new byte[8]
                {
                    DBver, 0, 0, 0xFF,
                    (byte)len, (byte)(len >> 8), (byte)(len >> 16), 0xFF
                };
                Array.Copy(header, dat, 8);
                Byte3To4(len, txtdat, 0, dat, 8);

                using (Stream stream = imgUtil.CompressBitmap(dat, wh, wh))
                {
                    return imgUtil.SaveImage(stream);
                }
            });

        private static JObject Decoder(byte[] bmp)
        {
            if (DBver != bmp[0])
                return null;
            int len = bmp[4] + (bmp[5] << 8) + (bmp[6] << 16);
            Debug.WriteLine($"decode:{bmp.Length} => {len}");

            byte[] dat = new byte[len];
            Byte4To3(len, bmp, 8, dat, 0);

            var total = Unicode.GetString(dat, 0, dat.Length);
            Debug.WriteLine(total);
            return JsonConvert.DeserializeObject<JObject>(total);
        }

        private static bool ReplaceImport(JObject obj)
        {
            Clear();
            foreach (var jp in obj["words"] as JObject)
            {
                var w = new DBWord(jp.Key.ToLower(), jp.Value.ToInt());
                words.Add(w.Letters, w.Id);
                eles.Add(w);
                db.Insert(w);
            }
            foreach (var jp in obj["means"] as JObject)
            {
                var m = new DBMeaning(jp.Key, jp.Value.ToInt());
                means.Add(m.Meaning, m.Id);
                eles.Add(m);
                db.Insert(m);
            }
            var t = new DBTranslation();
            foreach (var jp in obj["links"] as JObject)
            {
                t.Wid = jp.Key.ToInt();
                foreach (var ji in jp.Value as JArray)
                {
                    e2c.Add(t.Wid, t.Mid = ji.ToInt());
                    c2e.Add(t.Mid, t.Wid);
                    db.Insert(t);
                }
            }
            isChanged = true;
            return true;
        }
        private static bool AddImport(JObject obj)
        {
            var wMap = new Dictionary<int, int>();
            var mMap = new Dictionary<int, int>();
            int tmp;
            foreach (var jp in obj["words"] as JObject)
            {
                var w = new DBWord(jp.Key.ToLower(), jp.Value.ToInt());
                //string txt = $"Find Word {w.Letters} = {w.Id} : ";
                if (words.TryGetValue(w.Letters, out tmp))
                {
                    wMap.Add(w.Id, tmp);
                    //txt += $"Exist {w.Id}->{tmp}";
                }
                else
                {
                    tmp = w.Id;
                    db.Insert(w);
                    wMap.Add(tmp, w.Id);
                    //txt += $"Absent {tmp}->{w.Id}";
                    words.Add(w.Letters, w.Id);
                    eles.Add(w);
                }
                //Debug.WriteLine(txt);
            }
            foreach (var jp in obj["means"] as JObject)
            {
                var m = new DBMeaning(jp.Key, jp.Value.ToInt());
                //string txt = $"Find Mean {m.Meaning} = {m.Id} : ";
                if (means.TryGetValue(m.Meaning, out tmp))
                {
                    mMap.Add(m.Id, tmp);
                    //txt += $"Exist {m.Id}->{tmp}";
                }
                else
                {
                    tmp = m.Id;
                    db.Insert(m);
                    mMap.Add(tmp, m.Id);
                    //txt += $"Absent {tmp}->{m.Id}";
                    means.Add(m.Meaning, m.Id);
                    eles.Add(m);
                }
                //Debug.WriteLine(txt);
            }
            var t = new DBTranslation();
            foreach (var jp in obj["links"] as JObject)
            {
                t.Wid = wMap[jp.Key.ToInt()];
                foreach (var ji in jp.Value as JArray)
                {
                    //string txt = $"Link {jp.Key.ToInt()}-{ji.ToInt()} : ";
                    if (!e2c.Contains(t.Wid, t.Mid = mMap[ji.ToInt()]))
                    {
                        e2c.Add(t.Wid, t.Mid);
                        c2e.Add(t.Mid, t.Wid);
                        //txt += $"Absent {t.Wid}-{t.Mid}";
                        db.Insert(t);
                    }
                    //else
                        //txt += "Exist";
                    //Debug.WriteLine(txt);
                }
            }
            isChanged = true;
            return true;
        }

        public static Task<bool> Import(byte[] bmp, bool isReplace)
            => Task.Run(() =>
            {
                var obj = Decoder(bmp);
                if (obj == null)
                    return false;
                return isReplace ? ReplaceImport(obj) : AddImport(obj);
            });

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

        public static void AddWord(string eng, ICollection<string> chi)
        {
            int wid, mid;
            if (!words.TryGetValue(eng, out wid))//add word
            {
                var word = new DBWord() { Letters = eng };
                db.Insert(word);
                words[eng] = wid = word.Id;
            }
            foreach (var str in chi)
            {
                if (!means.TryGetValue(str, out mid))//add meaning
                {
                    var mean = new DBMeaning() { Meaning = str };
                    db.Insert(mean);
                    means[str] = mid = mean.Id;
                }
                if (!e2c.Contains(wid, mid))
                {
                    try
                    {
                        db.Insert(new DBTranslation() { Wid = wid, Mid = mid });
                        e2c.Add(wid, mid);
                        c2e.Add(mid, wid);
                    }
                    catch (ArgumentException e) { e.CopeWith("insert db"); }
                }
            }
            isChanged = true;
        }
    }
}
