using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotSizeCalculator.Utils
{
    internal static class ConsoleUtil
    {
        public static char KeyPressed { get; set; }

        public static void OpenMsg(Dictionary<string, string> symbolsDicCash, Dictionary<string, (int, string)> symbolsDicOther)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[Welcome to IronFX Lot Size Calculator : Indices + Matières Premières (GOLD, SILVER, WTI, GAS NATUREL)]");
            Console.WriteLine("[Sur le graphique de TradingView par exemple, 1 Pip = 1 euro pour les indices et 1 Pip = 1 centime pour les matières premières]");
            Console.WriteLine("[Tous les symboles Cash valent : 1 Pip = 1 EUR/USD/GBP... et tous les symboles Z3 et autres ont des barèmes de prix au Pip propre à chaque symbole allant de 1 à 50 EUR/USD/GBP...]");
            Console.WriteLine("[Le calcul est donc casse tête pour les symboles en Z3 car chaque symbole a son barème alors que les symboles Cash ont tous le même barème mais avec une devise différente (EUR/USD/GBP...)]");
            Console.WriteLine("[Cela fait donc fluctuer le profit final lors de la conversion de la devise mère (ex -> USD) en EUR]");
            Console.WriteLine("[XAG = SILVER | XAU = GOLD | WTI = PETROLE USA]");
            Console.WriteLine("[L'application gère directement la conversion des devises en EUR pour simuler un gain final réel comme sur MT4]\n");

            Console.ResetColor();
            Console.WriteLine("Voici la liste des symboles disponibles en Cash :");
            Console.ForegroundColor = ConsoleColor.Green;

            foreach (var symbolDicCash in symbolsDicCash)
            {
                Console.WriteLine($"\t- {symbolDicCash.Key}");
            }

            Console.ResetColor();
            Console.WriteLine("\nVoici la liste des symboles disponibles en Z3 et autres :");
            Console.ForegroundColor = ConsoleColor.Green;

            foreach (var symbolDicOther in symbolsDicOther)
            {
                Console.WriteLine($"\t- {symbolDicOther.Key}");
            }

            Console.WriteLine();

            Console.ResetColor();
        }

        public static void ErrorMsg(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);

            LeaveApp();
        }

        public static void LogFinalMsg(double computedLot, double finalProfit, double capitalPercentProfit, double rr, double riskLoss, double riskLossPercent, double maxProfitPipsInputDouble, double eurConversionOnePip)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nVous pouvez utiliser un lot de [{computedLot:F2}] pour votre ordre\n");
            Console.ResetColor();
            Console.WriteLine($"1 Pip vaut [{eurConversionOnePip:F4}€]\n");
            Console.WriteLine($"Pour {maxProfitPipsInputDouble} Pips de misé sur votre trade :");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\t- Vous obtiendrez [{finalProfit:F2}€] de profit (TP) soit [{capitalPercentProfit:F2}%] de votre capital");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\t- Vous risquez cependant de perdre [{riskLoss:F2}€] sur votre SL soit [{riskLossPercent:F2}%] de votre capital");

            if (rr < 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (rr >= 1 && rr < 2)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (rr >= 2)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.WriteLine($"\t- Vous avez un RR de [{rr:F2}]");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t- Il a les swaps et les commissions à rajouter en plus du trade");

            LeaveApp();
        }

        public static string? CustomReadLine()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string? userInput = Console.ReadLine();
            Console.ResetColor();
            return userInput;
        }

        private static void LeaveApp()
        {
            Console.ResetColor();
            Console.WriteLine("\nVeuillez appuyer sur une touche pour relancer le programme ou [ECHAP] pour quitter l'application");
            KeyPressed = Console.ReadKey().KeyChar;

            if (KeyPressed != (char)ConsoleKey.Escape)
            {
                Console.Clear();
            }
        }
    }
}
