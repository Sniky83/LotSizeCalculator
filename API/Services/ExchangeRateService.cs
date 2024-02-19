using LotSizeCalculator.Services;
using System.Globalization;
using System.Text.Json;

namespace API.Services
{
    public class ExchangeRateService
    {
        public static double GetExchangeData(bool isCash, KeyValuePair<string, (int, string)> foundSymbolOther, KeyValuePair<string, string> foundSymbolCash)
        {
            double castCurrency;

            if (foundSymbolCash.Value == "EUR" || foundSymbolOther.Value.Item2 == "EUR")
            {
                return 1;
            }

            string jsonResult = CurrencyService.Exchange("EURUS").Result;
            double.TryParse(jsonResult, NumberStyles.Any, CultureInfo.InvariantCulture, out castCurrency);

            if(castCurrency > 1.6 || castCurrency < 0.4)
            {
                throw new Exception($"Le prix d'échange en EUR/DOL est anormal. Vérifier la récupération de l'échange.\n Valeur actuelle : {castCurrency}");
            }

            return castCurrency;

            JsonElement ratesElement;

            try
            {
                JsonDocument jsonDoc = JsonDocument.Parse(jsonResult);
                JsonElement root = jsonDoc.RootElement;
                ratesElement = root.GetProperty("rates");
            }
            catch
            {
                throw new Exception("Erreur : les clés JSON de l'API ont changés. Il est donc impossible d'utiliser l'application. Veuillez contacter le créateur de celle-ci");
            }

            if (isCash)
            {
                if (foundSymbolCash.Value == "GBP" && ratesElement.TryGetProperty("GBP", out JsonElement gbpValue))
                {
                    castCurrency = gbpValue.GetDouble();
                }
                else if (foundSymbolCash.Value == "USD" && ratesElement.TryGetProperty("USD", out JsonElement usdValue))
                {
                    castCurrency = usdValue.GetDouble();
                }
                else if (foundSymbolCash.Value == "AUD" && ratesElement.TryGetProperty("AUD", out JsonElement audValue))
                {
                    castCurrency = audValue.GetDouble();
                }
                else if (foundSymbolCash.Value == "CHF" && ratesElement.TryGetProperty("CHF", out JsonElement chfValue))
                {
                    castCurrency = chfValue.GetDouble();
                }
                else if (foundSymbolCash.Value == "HKD" && ratesElement.TryGetProperty("HKD", out JsonElement hkdValue))
                {
                    castCurrency = hkdValue.GetDouble();
                }
                else if (foundSymbolCash.Value == "EUR")
                {
                    castCurrency = 1;
                }
                else
                {
                    throw new Exception($"Erreur : impossible de retrouver le symbole associé : {foundSymbolCash.Value}\nIl est donc impossible d'utiliser l'application. Veuillez contacter le créateur de celle-ci");
                }
            }
            else
            {
                if (foundSymbolOther.Value.Item2 == "GBP" && ratesElement.TryGetProperty("GBP", out JsonElement gbpValue))
                {
                    castCurrency = gbpValue.GetDouble();
                }
                else if (foundSymbolOther.Value.Item2 == "USD" && ratesElement.TryGetProperty("USD", out JsonElement usdValue))
                {
                    castCurrency = usdValue.GetDouble();
                }
                else if (foundSymbolOther.Value.Item2 == "AUD" && ratesElement.TryGetProperty("AUD", out JsonElement audValue))
                {
                    castCurrency = audValue.GetDouble();
                }
                else if (foundSymbolOther.Value.Item2 == "CHF" && ratesElement.TryGetProperty("CHF", out JsonElement chfValue))
                {
                    castCurrency = chfValue.GetDouble();
                }
                else if (foundSymbolOther.Value.Item2 == "HKD" && ratesElement.TryGetProperty("HKD", out JsonElement hkdValue))
                {
                    castCurrency = hkdValue.GetDouble();
                }
                else if (foundSymbolOther.Value.Item2 == "EUR")
                {
                    castCurrency = 1;
                }
                else
                {
                    throw new Exception($"Erreur : impossible de retrouver le symbole associé : {foundSymbolOther.Value.Item2}\nIl est donc impossible d'utiliser l'application. Veuillez contacter le créateur de celle-ci");
                }
            }

            return castCurrency;
        }
    }
}
