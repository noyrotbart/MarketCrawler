using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xignite.Sdk.Api;
using Xignite.Sdk.Api.Models.XigniteGlobalQuotes;
namespace MarketCrawler
{
    class AccessXIgnite
    {
        private static readonly string XigniteKey = "270AE78B8C9349E8912DB09AD5DD1E80";

        public static GlobalQuote  getXigniteData(string exchgangeName, string tickerName)
        {
            var historical = new XigniteGlobalHistorical(XigniteKey);

            var currencies = new XigniteGlobalCurrencies(XigniteKey);
            var bond = new XigniteGlobalQuotes(XigniteKey);
            var ExchList = bond.ListExchanges().ExchangeDescriptions;
            string symbol;
            var a = historical.ListExchanges().ExchangesDescriptions;
            int i = 0;
            foreach (var b in a)
            {
                Console.WriteLine(b.Market+i+" "+b.MarketIdentificationCode);
                i++;
            }
            if (exchgangeName != "XNAS")
            {
                symbol = String.Format("{0}.{1}", tickerName, exchgangeName);
            }
            else symbol = tickerName;
            //string datePattern = @"MM/dd/yyyy";

            GlobalQuote data = bond.GetGlobalDelayedQuote (symbol, IdentifierTypes.Symbol);

            if (data.Message.Contains("Delay times are"))
            {
                //Console.WriteLine("Call took: " + data.Delay);
                return data;
            }
            else
            {
                Console.WriteLine("Call failed: " + data.Message);
                return null;
            }


        }

    }
}
