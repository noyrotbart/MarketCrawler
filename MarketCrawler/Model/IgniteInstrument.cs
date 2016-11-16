using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketCrawler.IgniteInstrument
{
    public class Security
    {
        public object CIK { get; set; }
        public object CUSIP { get; set; }
        public string Symbol { get; set; }
        public object ISIN { get; set; }
        public string Valoren { get; set; }
        public string Name { get; set; }
        public string Market { get; set; }
        public string MarketIdentificationCode { get; set; }
        public bool MostLiquidExchange { get; set; }
        public string CategoryOrIndustry { get; set; }
    }

    public class IgniteRootObject
    {
        public string Outcome { get; set; }
        public string Message { get; set; }
        public string Identity { get; set; }
        public double Delay { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public double UTCOffset { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Last { get; set; }
        public double LastSize { get; set; }
        public double Volume { get; set; }
        public double PreviousClose { get; set; }
        public string PreviousCloseDate { get; set; }
        public double ChangeFromPreviousClose { get; set; }
        public double PercentChangeFromPreviousClose { get; set; }
        public double Bid { get; set; }
        public double BidSize { get; set; }
        public string BidDate { get; set; }
        public string BidTime { get; set; }
        public double Ask { get; set; }
        public double AskSize { get; set; }
        public string AskDate { get; set; }
        public string AskTime { get; set; }
        public double High52Weeks { get; set; }
        public double Low52Weeks { get; set; }
        public string Currency { get; set; }
        public bool TradingHalted { get; set; }
        public Security Security { get; set; }
    }
}
