using Pinaka.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinaka.BL
{
    public class WebParser
    {
        PinakaParameters Parameters { get; set; }
        public WebParser(PinakaParameters parameters)
        {
            this.Parameters = parameters;
        }
        public PinakaHttpClient webClient = new PinakaHttpClient();

        public async Task<string> GetSiteHtml()
        {
            string pageHTML = await this.webClient.GetStringAsync(Parameters.URL);
            return pageHTML;
        }
    }
}
