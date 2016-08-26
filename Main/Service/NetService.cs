using Main.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using static Main.Util.BasicUtils;

namespace Main.Service
{
	public static class NetService
	{
		class WebProxy : IWebProxy
		{
			private readonly Uri _proxyUri;
			public ICredentials Credentials { get; set; }

			public WebProxy(Uri uri)
			{
				_proxyUri = uri;
			}

			public WebProxy(string uri) : this(new Uri(uri)) { }

			public Uri GetProxy(Uri destination)
			{
				return _proxyUri;
			}

			public bool IsBypassed(Uri destination)
			{
				return false;
			}
		}

		private static List<Tuple<string, Uri, HttpClient>> servers = new List<Tuple<string, Uri, HttpClient>>();
		public static HttpClient client { get; private set; }
        private static int curChoiceIdx;

		static NetService()
		{
			JArray choices = new JArray();
			#region Load network.json from embedded resource
			try
			{
                using (var stream = AssembleResource("network.json"))
				using (var reader = new StreamReader(stream))
				{
					var json = reader.ReadToEnd();
					choices = JsonConvert.DeserializeObject<JArray>(json);
				}
			}
			catch(Exception e)
			{
                e.CopeWith("open network.json");
                return;
			}
            #endregion
            string logtxt = "";
			foreach (JObject choice in choices)
			{
				var handler = new HttpClientHandler();
				var ub = new UriBuilder(choice["scheme"].ToString(), choice["host"].ToString());
				JToken port;
				if (choice.TryGetValue("port", out port))
					ub.Port = port.ToInt();
				JToken jp;
				if (choice.TryGetValue("proxy", out jp))
				{
					var ubp = new UriBuilder(jp["type"].ToString(), jp["host"].ToString(), jp["port"].ToInt());
					handler.Proxy = new WebProxy(ubp.Uri);
					handler.UseProxy = true;
				}
				var server = new Tuple<string, Uri, HttpClient>(choice["name"].ToString(), ub.Uri,
					new HttpClient(handler) { Timeout = new TimeSpan(0, 0, 0, 3) });
                logtxt += $"Server: {server.Item1}\r\n";
				servers.Add(server);
			}
            Logger(logtxt);
		}

        public static Tuple<int, IEnumerable<string>> GetChoices() 
            => new Tuple<int, IEnumerable<string>>(curChoiceIdx, servers.Select(s => s.Item1));

		public static void Choose(int idx)
		{
            if (idx < servers.Count)
                client = servers[curChoiceIdx = idx].Item3;
		}
	}
}
