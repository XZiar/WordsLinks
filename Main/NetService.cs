using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace WordsLinks
{
	static class NetService
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

		static NetService()
		{
			JArray choices = new JArray();
			#region Load network.json from embedded resource
			var assembly = typeof(NetService).GetTypeInfo().Assembly;
			try
			{
				Stream stream = assembly.GetManifestResourceStream("Main.network.json");
				using (var reader = new StreamReader(stream))
				{
					var json = reader.ReadToEnd();
					choices = JsonConvert.DeserializeObject<JArray>(json);
				}
			}
			catch(Exception e)
			{
				Debug.WriteLine("Failed when open {0} : {1}\n{2}", "network.json", e.Message, e.StackTrace);
			}
			#endregion
			foreach (JObject choice in choices)
			{
				var handler = new HttpClientHandler();
				var ub = new UriBuilder(choice["scheme"].ToString(), choice["host"].ToString());
				JToken port;
				if (choice.TryGetValue("port", out port))
					ub.Port = int.Parse(port.ToString());
				JToken jp;
				if (choice.TryGetValue("proxy", out jp))
				{
					var ubp = new UriBuilder(jp["type"].ToString(), jp["host"].ToString(), int.Parse(jp["port"].ToString()));
					handler.Proxy = new WebProxy(ubp.Uri);
					handler.UseProxy = true;
				}
				var server = new Tuple<string, Uri, HttpClient>(choice["name"].ToString(), ub.Uri,
					new HttpClient(handler) { Timeout = new TimeSpan(0, 0, 0, 3) });
				Debug.WriteLine("Server: " + server.Item1);
				servers.Add(server);
			}
			Choose(0);
		}

		public static void init() { }

		public static string[] GetChoices() => servers.Select(s => s.Item1).ToArray();

		public static void Choose(int idx)
		{
			if(idx < servers.Count)
				client = servers[0].Item3;
		}
	}
}
