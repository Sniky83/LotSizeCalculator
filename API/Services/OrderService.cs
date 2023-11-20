using API.JsonResults;
using API.Models;
using API.Services;
using LotSizeCalculator.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace API.Repositories
{
    public class OrderService
    {
        private readonly Dictionary<string, (int, string)> symbolsDicOther = new()
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

        private readonly Dictionary<string, string> symbolsDicCash = new()
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

        private KeyValuePair<string, (int, string)> foundSymbolOther = new();
        private KeyValuePair<string, string> foundSymbolCash = new();

        public OrderGetInfoResult GetInfo(OrderGetInfoDto orderGetInfoDto)
        {
            return GetInfoResult(orderGetInfoDto);
        }

        private OrderGetInfoResult GetInfoResult(OrderGetInfoDto orderGetInfoDto)
        {
            FillDictionaries(orderGetInfoDto);

            double castCurrency = ExchangeRateService.GetExchangeData(orderGetInfoDto.IsCash, foundSymbolOther, foundSymbolCash);

            LotAndOnePipValue lotAndOnePipValue = CalculateLotAndOnePipValue(orderGetInfoDto, castCurrency);

            return FillOrderGetInfoResult(orderGetInfoDto, lotAndOnePipValue);
        }

        private static OrderGetInfoResult FillOrderGetInfoResult(OrderGetInfoDto orderGetInfoDto, LotAndOnePipValue lotAndOnePipValue)
        {

            double finalProfit = lotAndOnePipValue.OnePipValue * orderGetInfoDto.TheoricalTpPips;
            double capitalPercentProfit = (finalProfit / orderGetInfoDto.Capital) * 100;
            double computeRR = capitalPercentProfit / orderGetInfoDto.MaxPercentCapital;
            double computeStopLoss = lotAndOnePipValue.OnePipValue * orderGetInfoDto.TheoricalSlPips;
            double computeStopLossPercent = (computeStopLoss / orderGetInfoDto.Capital) * 100;

            return new OrderGetInfoResult()
            {
                TotalTP = finalProfit,
                TotalSL = computeStopLoss,
                TotalPercentCapitalTP = capitalPercentProfit,
                TotalPercentCapitalSL = computeStopLossPercent,
                SinglePipValue = lotAndOnePipValue.OnePipValue,
                RR = computeRR,
                LotSize = lotAndOnePipValue.LotSize
            };
        }

        private LotAndOnePipValue CalculateLotAndOnePipValue(OrderGetInfoDto orderGetInfoDto, double castCurrency)
        {
            double totalMoneyLoss = (orderGetInfoDto.MaxPercentCapital / 100) * orderGetInfoDto.Capital;
            double onePipValue = totalMoneyLoss / orderGetInfoDto.TheoricalSlPips;
            double finalOnePipValue;
            double computedLot;

            if (orderGetInfoDto.IsCash)
            {
                double eurConversionOnePip = (1 / castCurrency) * onePipValue;
                computedLot = Math.Round(onePipValue / eurConversionOnePip, 2);
                finalOnePipValue = computedLot * eurConversionOnePip;
            }
            else
            {
                double oneCurExchFromEur = (1 / castCurrency);
                computedLot = Math.Round(onePipValue / (foundSymbolOther.Value.Item1 * oneCurExchFromEur), 2);
                finalOnePipValue = computedLot * (oneCurExchFromEur * foundSymbolOther.Value.Item1);
            }

            if (computedLot < 0.01)
            {
                throw new Exception($"Erreur : votre lot est inférieur à 0.01 ({computedLot:F3}). Veuillez augmenter votre capital ou alors diminuer le nombre de Pips pour votre SL ou encore augmenter votre pourcentage de pertes max de capital (peu recommandé)");
            }

            return new LotAndOnePipValue()
            {
                LotSize = computedLot,
                OnePipValue = finalOnePipValue
            };
        }

        private void FillDictionaries(OrderGetInfoDto orderGetInfoDto)
        {
            if (orderGetInfoDto.IsCash)
            {
                foundSymbolCash = symbolsDicCash.Where(kv => kv.Key.Contains(orderGetInfoDto.Symbol.ToUpper())).FirstOrDefault();

                if (foundSymbolCash.Key is null)
                {
                    throw new Exception("Erreur : le symbole recherché est introuvable");
                }
            }
            else
            {
                foundSymbolOther = symbolsDicOther.Where(kv => kv.Key.Contains(orderGetInfoDto.Symbol.ToUpper())).FirstOrDefault();

                if (foundSymbolOther.Key is null)
                {
                    throw new Exception("Erreur : le symbole recherché est introuvable");
                }
            }
        }
    }
}
