using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Pinaka.BL
{
    public class PinakaHttpClient : HttpClient
    {
        public HttpClientHandler ClientHandler { get; set; }
        public CookieContainer CookieContainer { get; set; }

        public PinakaHttpClient(CookieContainer container, Dictionary<string, string> Headers)
            : this(container,new HttpClientHandler())
        {
            foreach (var keyVal in Headers)
            {
                this.DefaultRequestHeaders.Add(keyVal.Key, keyVal.Value);
            }
        }

        public PinakaHttpClient(bool flgAddContentType = true)
            : this(new CookieContainer(), new HttpClientHandler(), flgAddContentType)
        {

        }

        public PinakaHttpClient(CookieContainer container, HttpClientHandler handler, bool flgAddContentType = true): base(handler)
        {
            //this.Encoding = Encoding.UTF8;
            this.Timeout = TimeSpan.FromMinutes(3);//3600000 ticks
            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.MaxServicePointIdleTime = 2000;
            this.CookieContainer = container;
            this.ClientHandler = handler;

            if (flgAddContentType)
                this.DefaultRequestHeaders.Add("Content-Type", "application/json");//"application/x-www-form-urlencoded";
            this.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");// "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //this.Headers["Accept-Encoding"] = "gzip, deflate";
            this.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            this.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; rv:23.0) Gecko/20100101 Firefox/23.0");
            this.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            //this.Headers["Connection"] = "keep-alive";

            if (ConfigurationManager.AppSettings["ProxyEnabled"] != null)
            {
                bool proxyEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["ProxyEnabled"]);
                if (proxyEnabled)
                {
                    PinakaHttpClient.DefaultProxy = new WebProxy(ConfigurationManager.AppSettings["Proxy"] + ":" + ConfigurationManager.AppSettings["Port"], false);//comment
                    if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["UserName"]))
                    {
                        PinakaHttpClient.DefaultProxy.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["UserName"], ConfigurationManager.AppSettings["Password"]);//comment
                    }
                }
            }
        }
    }
}
