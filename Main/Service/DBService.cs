using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WordsLinks.Model;
using WordsLinks.Util;
using Xamarin.Forms;
using static System.Text.Encoding;
using static WordsLinks.Util.BasicUtils;

namespace WordsLinks.Service
{
    static class DBService
    {
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
        private static Dictionary<string, int> words;
        private static Dictionary<string, int> means;
        private static MultiValueDictionary<int, int> e2c, c2e;
        public static string[] Meanings { get { return means.Keys.ToArray(); } }
        public static int WordsCount { get { return words.Count; } }
        public static int MeansCount { get { return means.Count; } }

        private static ImageUtil imgUtil;
        static DBService()
        {
            string dbPath = DependencyService.Get<FileUtil>().GetFilePath("words.db", true);
            Debug.WriteLine("open db at " + dbPath);
            db = DependencyService.Get<SQLiteUtil>().GetSQLConn("words.db");
            imgUtil = DependencyService.Get<ImageUtil>();
        }

        public static void Init()
        {
            db.CreateTable<DBWord>();
            db.CreateTable<DBMeaning>();
            db.CreateTable<DBTranslation>();
            GetAll();
            isChanged = true;
        }

        private static void GetAll()
        {
            words = db.Table<DBWord>().ToDictionary(w => w.Letters, w => w.Id);
            means = db.Table<DBMeaning>().ToDictionary(w => w.Meaning, w => w.Id);
            e2c = new MultiValueDictionary<int, int>();
            c2e = new MultiValueDictionary<int, int>();
            foreach (var t in db.Table<DBTranslation>())
            {
                e2c.Add(t.Wid, t.Mid);
                c2e.Add(t.Mid, t.Wid);
            }
        }

        public static void Clear()
        {
            db.DropTable<DBWord>();
            db.DropTable<DBMeaning>();
            db.DropTable<DBTranslation>();
            Init();
        }

        public static Task<bool> Export() 
            => Task.Run(() =>
            {
                var tmp = new
                {
                    words = words,
                    means = means,
                    links = db.Table<DBTranslation>().ToList(),
                };
                string total = JsonConvert.SerializeObject(tmp);
                //Debug.WriteLine(total);

                byte[] txtdat = Unicode.GetBytes(total);
                int len = txtdat.Length;
                int wh = (int)Math.Ceiling(Math.Sqrt((len + 4) / 3.0));//3byte=>4byte
                byte[] dat = new byte[wh * wh * 4];
                Debug.WriteLine($"calc:{len} => {dat.Length}({wh}*{wh})");

                dat[0] = (byte)(len & 0xFF);
                dat[1] = (byte)((len & 0xFF00) >> 8);
                dat[2] = (byte)((len & 0xFF0000) >> 16);
                dat[3] = 0xFF;
                Byte3To4(len, txtdat, 0, dat, 4);

                using (Stream stream = imgUtil.CompressBitmap(dat, wh, wh))
                {
                    return imgUtil.ASaveImage(stream);
                }
            });

        public static void Import()
        {
            imgUtil.GetImage(bmp =>
            {
                if (bmp != null)
                {
                    int len = bmp[0] + (bmp[1] << 8) + (bmp[2] << 16);
                    Debug.WriteLine($"len:{len},size:{bmp.Length}");

                    byte[] dat = new byte[len];
                    Byte4To3(len, bmp, 4, dat, 0);

                    var total = Unicode.GetString(dat, 0, dat.Length);
                    //Debug.WriteLine(total);
                    var obj = JsonConvert.DeserializeObject<JObject>(total);
                }
                else
                    Debug.WriteLine("no image selected");
            });
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
                    catch (ArgumentException e) { OnException(e, "insert db"); }
                }
            }
            isChanged = true;
        }
    }
}
