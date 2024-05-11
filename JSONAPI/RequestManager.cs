using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TONWallet.JSONAPI
{
    public class RequestManager
    {
        public static async Task<T> GETRequestAsync<T>(string endpoint, Dictionary<string, string> args)
        {
            using (var httpClient = new HttpClient())
            {
                var r = await httpClient.GetAsync($"https://toncenter.com/api/v2{endpoint}{BuildQuery(args)}");
                Thread.Sleep(Utils.API_RATELIMIT);
                if (r.StatusCode != System.Net.HttpStatusCode.OK) throw new HttpProtocolException((long)r.StatusCode, r.StatusCode.ToString(), new Exception());

                var json = await r.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public static string BuildQuery(Dictionary<string, string> queryParams)
        {
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            return $"?{queryString}";
        }
    }
}
