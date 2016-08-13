using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using static System.StringComparison;

namespace WordsLinks.Service
{
    static class TranslateService
    {
        public delegate void OnResult(string[] data);
        static char[] spliter = new char[] { '；', ';', '，', ',' };
        static string baseAPI = "https://fanyi.youdao.com/openapi.do?keyfrom={0}&key={1}&type=data&doctype=json&version=1.1&q=";
        static string APIurl;
        static TranslateService()
        {
            APIurl = string.Format(baseAPI, "WordsLinks", "57959088");
        }

        public static async void Eng2Chi(string eng, OnResult onRes)
		{
			JObject result;
			try
			{
                var ret = await NetService.client.GetStringAsync(APIurl + eng);
				result = JsonConvert.DeserializeObject<JObject>(ret);
			}
			catch (WebException e)
			{
				Debug.WriteLine($"failed: {e.Message}\n{e.StackTrace}");
				onRes(new string[] { $"failed: {e.Status}\n{e.Response}" });
				return;
			}
            catch (Exception e)
            {
                Debug.WriteLine($"exception: {e.GetType()}\nfailed: {e.Message}\n{e.StackTrace}");
                return;
            }
			HashSet<string> trans = new HashSet<string>();
			foreach(var tr in result["translation"])
				trans.Add(tr.ToString());
			//add basic translation
			{
				var basics = result["basic"]["explains"];
				foreach (var basic in basics)
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
			//add web translation
			foreach (var web in result["web"].Where(
				w => string.Equals(eng, w["key"].ToString(), CurrentCultureIgnoreCase)))
			{
				foreach (var val in web["value"])
					trans.Add(val.ToString());
			}
			onRes(trans.ToArray());
		}

		public static void Chi2Eng(string chi)
		{
			if (chi == null)
				return;
		}
	}
}
