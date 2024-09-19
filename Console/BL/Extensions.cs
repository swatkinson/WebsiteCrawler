using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pinaka.BL
{
    public static class Extensions
    {
        public static List<Cookie> GetCookies(this HttpResponseMessage message)
        {
            message.Headers.TryGetValues("Set-Cookie", out var cookiesHeader);
            var cookies = cookiesHeader?.Select(cookieString => CreateCookie(cookieString)).ToList();
            return cookies;
        }

        private static Cookie CreateCookie(string cookieString)
        {
            var properties = cookieString.Split(';', StringSplitOptions.TrimEntries);
            var name = properties[0].Split("=")[0];
            var value = properties[0].Split("=")[1];
            var path = properties[2].Replace("path=", "");
            var cookie = new Cookie(name, value, path)
            {
                Secure = properties.Contains("secure"),
                HttpOnly = properties.Contains("httponly"),
                Expires = DateTime.Parse(properties[1].Replace("expires=", ""))
            };
            return cookie;
        }

        public static void SetCookies(this HttpRequestMessage requestMessage, PinakaHttpClient client, CookieCollection cookies)
        {
            string? cookieDomainName = string.Empty;
            if (requestMessage.RequestUri != null)
                cookieDomainName = requestMessage.RequestUri?.GetLeftPart(UriPartial.Authority);
            else if(client.BaseAddress!=null)
                cookieDomainName = client.BaseAddress?.GetLeftPart(UriPartial.Authority);

            string[] cookieHeaders = { };
            var cookieContainer = client.CookieContainer;
            foreach (var cookie in cookies.AsEnumerable())
            {
                //if (cookies.Any(x => x.Name == cookie.Name))
                //{ 

                //}
                cookieContainer.Add(cookie);
                //cookieHeaders.Append(string.Join("=", new string[] { cookie.Name, cookie.Value }));
            }
            //cookieContainer.SetCookies(new Uri(cookieDomainName), string.Join(",", cookieHeaders));
        }
    }
}
