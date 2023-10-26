using LotSizeCalculator.Services;
using LotSizeCalculator.Utils;
using System.Text;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;

Dictionary<string, (int, string)> symbolsDicOther = new()
{
    { "US500", (50, "USD") },
    { "US30", (5, "USD") },
    { "US100", (20, "USD") },
    { "JP225", (5, "USD") },
    { "UK100", (10, "GBP") },
    { "FRA40", (10, "EUR") },
    { "GER40", (10, "EUR") },
    { "CHINA50", (1, "USD") },
    { "AUS200", (1, "AUD") },
    { "XAGEUR", (5, "EUR") },
    { "XAUEUR", (1, "EUR") },
    { "XAGUSD", (5, "USD") },
    { "XAUUSD", (1, "USD") },
    { "USOIL", (10, "USD") },
    { "UKOIL", (10, "USD") },
    { "NAT.GAS", (10, "USD") }
};

Dictionary<string, string> symbolsDicCash = new()
{
    { "US500", "USD" },
    { "US30", "USD" },
    { "US100", "USD" },
    { "JP225", "USD" },
    { "UK100", "GBP" },
    { "EUR50", "EUR" },
    { "FRA40", "EUR" },
    { "NETH25", "EUR" },
    { "SPAIN35", "EUR" },
    { "SWISS20", "CHF" },
    { "GER40", "EUR" },
    { "CHINA50", "USD" },
    { "AUS200", "AUD" },
    { "HK50", "HKD" },
    { "PORT20", "EUR" },
    { "SWE30", "USD" },
    { "UK250", "GBP" },
    { "ITALY40", "EUR" },
    { "BRENT", "USD" },
    { "WTI", "USD" },
    { "NAT.GAS", "USD" }
};

while (ConsoleUtil.KeyPressed != (char)ConsoleKey.Escape)
{
    KeyValuePair<string, (int, string)> foundSymbolOther = new();
    KeyValuePair<string, string> foundSymbolCash = new();
    double capitalInput = 0;
    string symbolInput = "";
    double percentLossInput = 0;
    double qtyPipsLossInput = 0;
    double qtyPipsProfitInput = 0;
    bool isCashSymbol = false;

    ConsoleUtil.OpenMsg(symbolsDicCash, symbolsDicOther);

    try
    {
        capitalInput = UserInputsUtil.Capital();

        isCashSymbol = UserInputsUtil.IsCashSymbol();

        symbolInput = UserInputsUtil.Symbol();

        if(isCashSymbol)
        {
            foundSymbolCash = symbolsDicCash.Where(kv => kv.Key.Contains(symbolInput!.ToUpper())).FirstOrDefault();

            if (foundSymbolCash.Key is null)
            {
                ConsoleUtil.ErrorMsg("Erreur: le symbole recherché est introuvable");
                continue;
            }
        }
        else
        {
            foundSymbolOther = symbolsDicOther.Where(kv => kv.Key.Contains(symbolInput!.ToUpper())).FirstOrDefault();

            if (foundSymbolOther.Key is null)
            {
                ConsoleUtil.ErrorMsg("Erreur: le symbole recherché est introuvable");
                continue;
            }
        }

        percentLossInput = UserInputsUtil.PercentMaxCapitalLoss();

        qtyPipsLossInput = UserInputsUtil.QtyPipsLoss();

        qtyPipsProfitInput = UserInputsUtil.QtyPipsProfit();
    }
    catch
    {
        continue;
    }

    double castCurrency = 0;

    string jsonResult = "";

    JsonElement ratesElement;

    try
    {
        jsonResult = CurrencyService.Exchange("EUR").Result;
    }
    catch
    {
        ConsoleUtil.ErrorMsg("Erreur : connexion interrompue avec l'API de conversion des devises\nIl est donc impossible d'utiliser l'application. Veuillez contacter le créateur de celle-ci");
        break;
    }

    try
    {
        JsonDocument jsonDoc = JsonDocument.Parse(jsonResult);
        JsonElement root = jsonDoc.RootElement;
        ratesElement = root.GetProperty("rates");
    }
    catch
    {
        ConsoleUtil.ErrorMsg("Erreur : les clés JSON de l'API ont changés\nIl est donc impossible d'utiliser l'application. Veuillez contacter le créateur de celle-ci");
        break;
    }

    double totalMoneyLoss = (percentLossInput / 100) * capitalInput;
    double onePipValue = totalMoneyLoss / qtyPipsLossInput;
    double eurConversionOnePip = 0;
    double computedLot = 0;
    double finalOnePipValue = 0;

    if (isCashSymbol)
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
            ConsoleUtil.ErrorMsg($"Erreur : impossible de retrouver le symbole associé : {foundSymbolCash.Value}\nIl est donc impossible d'utiliser l'application. Veuillez contacter le créateur de celle-ci");
            break;
        }

        eurConversionOnePip = (1 / castCurrency) * onePipValue;
        computedLot = Math.Round(onePipValue / eurConversionOnePip, 2);
        finalOnePipValue = computedLot * eurConversionOnePip;
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
            ConsoleUtil.ErrorMsg($"Erreur : impossible de retrouver le symbole associé : {foundSymbolOther.Value.Item2}\nIl est donc impossible d'utiliser l'application. Veuillez contacter le créateur de celle-ci");
            break;
        }

        double oneCurExchFromEur = (1 / castCurrency);
        computedLot = Math.Round(onePipValue / (foundSymbolOther.Value.Item1 * oneCurExchFromEur), 2);
        finalOnePipValue = computedLot * (oneCurExchFromEur * foundSymbolOther.Value.Item1);
    }

    if (computedLot < 0.01)
    {
        ConsoleUtil.ErrorMsg($"Erreur : votre lot est inférieur à 0.01 ({computedLot:F3})\nVeuillez augmenter votre capital ou alors diminuer le nombre de Pips pour votre SL ou encore augmenter votre pourcentage de pertes max de capital (peu recommandé)");
        continue;
    }

    double finalProfit = finalOnePipValue * qtyPipsProfitInput;
    double capitalPercentProfit = (finalProfit / capitalInput) * 100;
    double computeRR = capitalPercentProfit / percentLossInput;
    double computeStopLoss = finalOnePipValue * qtyPipsLossInput;
    double computeStopLossPercent = (computeStopLoss / capitalInput) * 100;

    ConsoleUtil.LogFinalMsg(computedLot, finalProfit, capitalPercentProfit, computeRR, computeStopLoss, computeStopLossPercent, qtyPipsProfitInput, finalOnePipValue);
}