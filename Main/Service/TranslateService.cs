using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WordsLinks.Util;
using static System.StringComparison;

namespace WordsLinks.Service
{
    static class TranslateService
    {
        //public delegate void OnResult(string[] data);
        static char[] spliter = new char[] { '；', ';', '，', ',' };
        static string baseAPI = "https://fanyi.youdao.com/openapi.do?keyfrom={0}&key={1}&type=data&doctype=json&version=1.1&q=";
        static string APIurl;
        static TranslateService()
        {
            APIurl = string.Format(baseAPI, "WordsLinks", "57959088");
        }

        public static Task<string[]> Eng2Chi(string eng)
            => Task.Run(async () =>
            {
                JObject result;
                try
                {
                    var ret = await NetService.client.GetStringAsync(APIurl + eng);
                    result = JsonConvert.DeserializeObject<JObject>(ret);
                }
                catch (WebException e)
                {
                    e.CopeWith("Eng2Chi");
                    Debug.WriteLine($"web fail reason: {e.Status}\n{e.Response}");
                    return new string[] { $"{e.Status}\n{e.Response}" };
                }
                catch (Exception e)
                {
                    e.CopeWith("Eng2Chi");
                    return null;
                }
                HashSet<string> trans = new HashSet<string>();
                try
                {
                    foreach (var tr in result["translation"])
                        trans.Add(tr.ToString());
                }
                catch(Exception e)
                {
                    e.CopeWith("parse translation");
                }
                //add basic translation
                try
                {
                    foreach (var basic in result["basic"]["explains"])
                    {
                        string[] explains = Regex.Replace(basic.ToString(), "[a-zA-Z.]", "").Split(spliter);
                        foreach (string txt in explains)
                        {
                            string str = txt.Trim();
                            if (str != "")
                                trans.Add(str);
                        }
                    }
                }
                catch (Exception e)
                {
                    e.CopeWith("parse basic translation");
                }
                //add web translation
                try
                {
                    foreach (var web in result["web"].Where(
                        w => string.Equals(eng, w["key"].ToString(), CurrentCultureIgnoreCase)))
                    {
                        foreach (var val in web["value"])
                            trans.Add(val.ToString());
                    }
                }
                catch (Exception e)
                {
                    e.CopeWith("parse web translation");
                }
                return trans.ToArray();
            });

		public static void Chi2Eng(string chi)
		{
			if (chi == null)
				return;
		}
	}
}
