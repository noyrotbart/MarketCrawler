using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xignite.Sdk.Api;
using Xignite.Sdk.Api.Models.XigniteGlobalHistorical;
namespace MarketCrawler
{
    class AccessXIgnite
    {
        private static readonly string XigniteKey = "270AE78B8C9349E8912DB09AD5DD1E80";

        public static GlobalHistoricalQuote [] getXigniteData(string exchgangeName, string tickerName, int days)
        {
            var historical = new XigniteGlobalHistorical(XigniteKey);

            var currencies = new XigniteGlobalCurrencies(XigniteKey);
            var bond = new XigniteGlobalQuotes(XigniteKey);
            var ExchList = bond.ListExchanges().ExchangeDescriptions;
            string symbol = String.Format("{0}.{1}", tickerName, exchgangeName);
            string datePattern = @"MM/dd/yyyy";

            var data = historical.GetGlobalHistoricalQuarterlyQuotesRange
                (symbol, IdentifierTypes.Symbol, AdjustmentMethods.None,
                DateTime.Now.AddDays(days * -1).ToString(datePattern),
                DateTime.Now.ToString(datePattern));

            if (data.Outcome == "Success")
            {
                Console.WriteLine("Call took: " + data.Delay);
                return data.GlobalQuotes;
            }
            else
            {
                Console.WriteLine("Call failed: " + data.Message);
                return null;
            }


        }

    }
}
