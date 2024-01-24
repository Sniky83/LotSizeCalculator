using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LotSizeCalculator.Services
{
    internal static class CurrencyService
    {
        public static async Task<string> Exchange(string currency)
        {
            string url = $"https://www.boursorama.com/bourse/action/graph/ws/UpdateCharts?symbol=1x{currency}&period=-1";
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(jsonResult))
            {
                throw new Exception("Impossible d'extraire le taux de change sur le site");
            }

            JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);

            JsonElement root = jsonDocument.RootElement;
            JsonElement dArray = root.GetProperty("d");

            foreach (JsonElement element in dArray.EnumerateArray())
            {
                JsonElement cValue = element.GetProperty("c");
                return cValue.ToString();
            }

            throw new Exception("Impossible de récupérer le taux de change");
        }
    }
}
