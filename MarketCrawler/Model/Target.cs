using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketCrawler
{
    [Serializable]
    public sealed class StockName
    {
        public static readonly StockName NasdaqEurope = new StockName("NasdaqEurope");
        public static readonly StockName EURONEXT = new StockName("EURONEXT");
        public static readonly StockName Nasdaq = new StockName("Nasdaq");
        public static readonly StockName London = new StockName("London");
        public static readonly StockName All = new StockName("All");
        private StockName(string value)
        {
            Value = value;
        }

        public string Value { get;  set; }
    }
    [Serializable]
    public class Target
    {
        public string shortIdentifier;
        public string urlIdentifier;
        public StockName stockExchange;
        public Target(string shortSymbol, string urlName, StockName istock)
        {
            shortIdentifier = shortSymbol;
            urlIdentifier = urlName;
            stockExchange = istock;
        }
    }
}
