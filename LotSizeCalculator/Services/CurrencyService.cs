using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LotSizeCalculator.Services
{
    internal static class CurrencyService
    {
        public static async Task<string> Exchange(string currency)
        {
            string url = $"https://open.er-api.com/v6/latest/{currency}";
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(result))
            {
                throw new Exception();
            }

            return result;
        }
    }
}
