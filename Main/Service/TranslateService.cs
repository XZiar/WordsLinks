using Main.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Main.Util.BasicUtils;
using static System.StringComparison;

namespace Main.Service
{
    public static class TranslateService
    {
        private static char[] spliter = new char[] { '；', ';', '，', ',' };
        private static Regex chsRex = new Regex(@"[\u4e00-\u9fa5]"),
            delEngRex = new Regex(@"[a-zA-Z. ]|\(([a-zA-Z. ]*|[\u4e00-\u9fa5])\)");
        private static string baseAPI = "https://fanyi.youdao.com/openapi.do?keyfrom={0}&key={1}&type=data&doctype=json&version=1.1&q=";
        private static string APIurl;
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
                    //Debug.WriteLine(ret);
                    result = JsonConvert.DeserializeObject<JObject>(ret);
                }
                catch (WebException e)
                {
                    e.CopeWith("Eng2Chi");
                    Logger($"web fail reason: {e.Status}\n{e.Response}", LogLevel.Warning);
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
                    foreach (string tr in result["translation"])
                        trans.Add(tr);
                }
                catch(Exception e)
                {
                    e.CopeWith("parse translation");
                }
                //add basic translation
                try
                {
                    foreach (string basic in result["basic"]["explains"])
                    {
                        foreach (var txt in delEngRex.Replace(basic, "").Split(spliter))
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
                        foreach (string val in web["value"])
                            if (chsRex.IsMatch(val))
                                trans.Add(val);
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
