using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotSizeCalculator.Utils
{
    internal class UserInputsUtil
    {
        private static double GenericUserInput(string ihmMsg, string errorMsg)
        {
            Console.Write(ihmMsg);

            string? genericInput = ConsoleUtil.CustomReadLine();

            if (!double.TryParse(genericInput, out double genericDouble))
            {
                ConsoleUtil.ErrorMsg(errorMsg);
                throw new Exception();
            }

            return genericDouble;
        }

        public static bool IsCashSymbol()
        {
            string ihmMsg = "Est-ce que votre symbole est un Cash (ex -> US30Cash) (Oui / Non) ? : ";
            string errorMsg = "Erreur : veuillez sasir OUI ou NON";

            Console.Write(ihmMsg);

            string? cashOrNotInput = ConsoleUtil.CustomReadLine();

            if(!string.IsNullOrEmpty(cashOrNotInput))
            {
                cashOrNotInput = cashOrNotInput.ToLower();

                if (cashOrNotInput == "y" || cashOrNotInput == "yes" || cashOrNotInput == "o" || cashOrNotInput == "oui")
                {
                    return true;
                }
                else if(cashOrNotInput == "n" || cashOrNotInput == "no" || cashOrNotInput == "non")
                {
                    return false;
                }
            }


            ConsoleUtil.ErrorMsg(errorMsg);
            throw new Exception();
        }

        public static double Capital()
        {
            string capitalMsg = "Veuillez saisir votre capital (ex -> 1000) : ";
            string errorMsg = "Erreur : veuillez saisir un nombre";
            return GenericUserInput(capitalMsg, errorMsg);
        }

        public static double QtyPipsLoss()
        {
            string qtyPipsLossMsg = "Veuillez saisir votre quantité de Pips max de pertes (ex -> 50) (SL) : ";
            string errorMsg = "Erreur : veuillez saisir un nombre";
            return GenericUserInput(qtyPipsLossMsg, errorMsg);
        }

        public static double QtyPipsProfit()
        {
            string qtyPipsProfitMsg = "Veuillez saisir votre quantité de Pips visé (ex -> 100) (TP) : ";
            string errorMsg = "Erreur : veuillez saisir un nombre";
            return GenericUserInput(qtyPipsProfitMsg, errorMsg);
        }

        public static double PercentMaxCapitalLoss()
        {
            Console.Write("Veuillez saisir votre pourcentage max de pertes de capital (ex -> 5) : ");

            string? percentLossInput = ConsoleUtil.CustomReadLine();

            if (!double.TryParse(percentLossInput, out double percentLossDouble) || (percentLossDouble > 100 || percentLossDouble < 1))
            {
                ConsoleUtil.ErrorMsg("Erreur : veuillez saisir un pourcentage compris entre 1 et 100");
                throw new Exception();
            }

            return percentLossDouble;
        }

        public static string Symbol()
        {
            Console.Write("Veuillez saisir un symbole (ex -> US30) : ");

            string? symbolInput = ConsoleUtil.CustomReadLine();

            if (string.IsNullOrWhiteSpace(symbolInput))
            {
                ConsoleUtil.ErrorMsg("Erreur : veuillez saisir un symbole existant ex: WTI");
                throw new Exception();
            }

            return symbolInput;
        }
    }
}
