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
            //{ "UK100", (10, "GBP") },
            { "FRA40", (10, "EUR") },
            { "GER40", (10, "EUR") },
            { "CHINA50", (1, "USD") },
            //{ "AUS200", (1, "AUD") },
            { "XAGEUR", (5, "EUR") },
            { "XAUEUR", (1, "EUR") },
            { "XAGUSD", (5, "USD") },
            { "XAUUSD", (1, "USD") },
            { "USOIL", (10, "USD") },
            { "UKOIL", (10, "USD") },
            { "NAT.GAS", (10, "USD") },
            { "BTCEUR", (1, "EUR") },
            { "BTCUSD", (1, "USD") }
        };

        private readonly Dictionary<string, string> symbolsDicCash = new()
        {
            { "US500", "USD" },
            { "US30", "USD" },
            { "US100", "USD" },
            { "JP225", "USD" },
            //{ "UK100", "GBP" },
            { "EUR50", "EUR" },
            { "FRA40", "EUR" },
            { "NETH25", "EUR" },
            { "SPAIN35", "EUR" },
            //{ "SWISS20", "CHF" },
            { "GER40", "EUR" },
            { "CHINA50", "USD" },
            //{ "AUS200", "AUD" },
            //{ "HK50", "HKD" },
            { "PORT20", "EUR" },
            { "SWE30", "USD" },
            //{ "UK250", "GBP" },
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

        private OrderGetInfoResult FillOrderGetInfoResult(OrderGetInfoDto orderGetInfoDto, LotAndOnePipValue lotAndOnePipValue)
        {
            double finalProfit = lotAndOnePipValue.OnePipValue * orderGetInfoDto.TheoricalTpPips;
            double capitalPercentProfit = (finalProfit / orderGetInfoDto.Capital) * 100;
            double computeRR = capitalPercentProfit / orderGetInfoDto.MaxPercentCapital;
            double computeStopLoss = (lotAndOnePipValue.OnePipValue * orderGetInfoDto.TheoricalSlPips);
            double computeStopLossPercent = (computeStopLoss / orderGetInfoDto.Capital) * 100;
            //1% d'écart est toléré
            double seuilTolerance = (orderGetInfoDto.MaxPercentCapital + 1);

            if (computeStopLossPercent > seuilTolerance && orderGetInfoDto.LotSize is null)
            {
                //On boucle jusqu'a trouver le prochain capital accessible par rapport à notre limite
                orderGetInfoDto.MaxPercentCapital -= 0.1;
                return GetInfo(orderGetInfoDto);
            }

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
            double onePipValue = totalMoneyLoss / (orderGetInfoDto.TheoricalSlPips * -1);
            double finalOnePipValue;
            double lotWithoutRound;
            double computedLot;

            int dividor = GetOnePipCoefficient(orderGetInfoDto.Symbol);
            double conversionEur = (1 / castCurrency);

            if (orderGetInfoDto.IsCash)
            {
                double eurConversionOnePip = conversionEur * onePipValue;
                double delta = onePipValue - eurConversionOnePip;
                lotWithoutRound = (onePipValue + delta) / dividor;

                if(orderGetInfoDto.LotSize is null)
                {
                    computedLot = Math.Round(lotWithoutRound, 2, MidpointRounding.ToPositiveInfinity);
                }
                else
                {
                    computedLot = (double)orderGetInfoDto.LotSize;
                }

                finalOnePipValue = (computedLot * conversionEur) * dividor;
            }
            else
            {
                double eurConversionOnePip = conversionEur * foundSymbolOther.Value.Item1;
                lotWithoutRound = (onePipValue / eurConversionOnePip) / dividor;

                if (orderGetInfoDto.LotSize is null)
                {
                    computedLot = Math.Round(lotWithoutRound, 2, MidpointRounding.ToPositiveInfinity);
                }
                else
                {
                    computedLot = (double)orderGetInfoDto.LotSize;
                }

                finalOnePipValue = (computedLot * eurConversionOnePip) * dividor;
            }

            if (lotWithoutRound < 0.01 && orderGetInfoDto.LotSize is null)
            {
                throw new Exception($"Erreur : votre lot est inférieur à 0.01 ({lotWithoutRound:F3}). Veuillez augmenter votre capital ou alors diminuer le nombre de Pips pour votre SL ou encore augmenter votre pourcentage de pertes max de capital (peu recommandé)");
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

        private static int GetOnePipCoefficient(string symbol)
        {
            if(symbol.Contains("XAU"))
            {
                return 10;
            }
            else if (symbol.Contains("WTI"))
            {
                return 10;
            }
            else if (symbol.Contains("XAG"))
            {
                return 1000;
            }
            else
            {
                return 1;
            }
        }
    }
}
