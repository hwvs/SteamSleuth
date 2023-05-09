using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamSleuthScraper.Net
{
    internal class Request
    {
        public static async Task<Stream> GetAsStreamAsync(string url)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStreamAsync();
                return content;
            }
        }
    }
}
