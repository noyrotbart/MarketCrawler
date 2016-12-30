using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xignite.Sdk.Api.Models.XigniteGlobalQuotes;

namespace MarketCrawler
{


    class GlobalQuoteExtend : GlobalQuote
    {
        public DateTime DBtimestamp { get; set; }
    }
    class Instrument
    {
        public MongoDB.Bson.ObjectId ID;
        public string ticker { get; set; }
        public int instrumentID;
        public double lastPrice;
        public double change;
        public double pctChange;
        public double openPrice;
        public double dayHigh;
        public double dayLow;
        public double prevClose;
        public double bid;
        public double ask;
        public double volume;
        public DateTime timestamp;
        public string comments;
        public bool different;
        public double lastTradePrice;
        public string lastTradeTime { get; set; }
        // public DateTime tradeTimestamp;
        public string url;
        public StockName stockExchangeName { get; set; }


        public Instrument()
        {

        }

        public Instrument compareInstrument(GlobalQuoteExtend input)
        {
           different = false;
            Instrument val = new Instrument();
            if (roundLondon(input.Last) != this.lastPrice && (lastPrice != 0))
            {
                double check = roundLondon(input.Last);
                double check2 = this.lastPrice;

                different = true;
                val.lastPrice = this.lastPrice;
                val.comments += String.Format("Price x={0}, c={1} ", roundLondon(input.Last),this.lastPrice);
            }

            if (roundLondon(input.High)  != dayHigh && (dayHigh != 0))
            {
                different = true;
                val.dayHigh = this.dayHigh;
                val.comments += String.Format("dayHigh x={0}, c={1} ", roundLondon(input.High), this.dayHigh);
            }
            if (roundLondon(input.Low) != dayLow && (dayLow!=0)) 
            {
                different = true;
                val.dayLow = this.dayLow;
                val.comments += String.Format("dayLow x={0}, c={1} ", roundLondon(input.Low), this.dayLow);
            }
            if (input.Volume != volume && (volume!=0)) 
            {
                different = true;
                val.volume = this.volume;
                val.comments += String.Format("volume x={0}, c={1} ", input.Volume, this.volume);
            }
            if (roundLondon(input.Ask) != ask && (ask != 0))
            {
                different = true;
                val.ask = this.ask;
                val.comments += String.Format("ask x={0}, c={1} ", roundLondon(input.Ask), this.ask);
            }
            if (roundLondon(input.ChangeFromPreviousClose) != change && (change != 0)) 
            {
                different = true;
                val.change = this.change;
                val.comments += String.Format("change x={0}, c={1} ", roundLondon(input.ChangeFromPreviousClose), this.change);
            }
            if (roundLondon(input.PreviousClose) != prevClose && (prevClose != 0)) 
            {
                different = true;
                val.prevClose = this.prevClose;
                val.comments += String.Format("prevClose x={0}, c={1} ", roundLondon(input.PreviousClose), this.prevClose);
            }

            Console.WriteLine(this.ticker + " at time (UTC) " + DateTime.Now.ToUniversalTime().ToLongTimeString());
            if (this.stockExchangeName == StockName.London) Console.Write("\n Last Trade: Time x=" + input.Time+" c="+this.lastTradeTime+" Price of last trade in c="+this.lastTradePrice);
            if (different == true)
            {
                val.different = true;
                val.timestamp = this.timestamp;
                val.stockExchangeName = this.stockExchangeName;
                val.ticker = this.ticker;
                Console.WriteLine(val.comments);


            }
            else Console.WriteLine("Is the same");
            double averageOnBidAndAsk = (this.ask + this.bid) / 2;
            if (this.stockExchangeName == StockName.London) Console.WriteLine("Average of offer and bid is " + averageOnBidAndAsk +"\n");
            return val;
        }

        // If you are the London Stock Exchange, then some adjasuments are needed.
        private double roundLondon(double? last)
        {
            if (this.stockExchangeName == StockName.London)
            {
                if (last.HasValue)
                {
                    return Math.Round(last.Value * 100, 2);
                }
                else return 0;
            }
            else return last.Value;
        }

        public void stringConverter(ref string value, bool notationGB = true)
        {
            value = value.Replace("\t", "").Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("&", string.Empty).Replace("#", string.Empty).Replace("%", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("nbsp", string.Empty).Replace("$", string.Empty).Replace(";", string.Empty);
            if (value.Length == 1 && value == "-") value = "0";
            if (value == "N/A") value = "0";
            if (notationGB == false) value.Replace(".", "").Replace(",", ".");
        }
        private void stringConverterDate(ref string value)
        {
            value = value.Replace("\t", "").Replace(" ", "").Replace("\r", "").Replace("\n", "");
        }
        public void LastPrice(double value)
        {
            lastPrice = value;
        }
        public void LastPrice(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            lastPrice = Convert.ToDouble(value);
        }
        public void Change(double value)
        {
            change = value;
        }
        public void Change(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            change = Convert.ToDouble(value);
        }

        public void PctChange(double value)
        {
            pctChange = value;
        }
        public void PctChange(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            pctChange = Convert.ToDouble(value);
        }
        public void OpenPrice(double value)
        {
            openPrice = value;
        }
        public void OpenPrice(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            openPrice = Convert.ToDouble(value);
        }
        public void DayHigh(double value)
        {
            dayHigh = value;
        }
        public void DayHigh(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            dayHigh = Convert.ToDouble(value);
        }
        public void DayLow(double value)
        {
            dayLow = value;
        }
        public void DayLow(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            dayLow = Convert.ToDouble(value);
        }
        public void PrevClose(double value)
        {
            prevClose = value;
        }
        public void PrevClose(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            prevClose = Convert.ToDouble(value);
        }
        public void Bid(double value)
        {
            bid = value;
        }
        public void Bid(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            bid = Convert.ToDouble(value);
        }
        public void LastTradePrice(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            lastTradePrice = Convert.ToDouble(value);
        }
        public void Ask(double value)
        {
            ask = value;
        }
        public void Ask(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            ask = Convert.ToDouble(value);
        }
        public void Volume(int value)
        {
            volume = value;
        }
        public void Volume(string value, bool notationGB = true)
        {
            stringConverter(ref value, notationGB);
            volume = Convert.ToInt32(value.Replace(",",""));
        }
        public void UpdateTimestamp(DateTime value)
        {
            timestamp = value;
        }
        public void UpdateTimestamp(string value, string format)
        {
            stringConverterDate(ref value);
            timestamp = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        }
        //public void TradeTimestamp(DateTime value)
        //{
        //    tradeTimestamp = value;
        //}
        //public void TradeTimestamp(string value, string format)
        //{
        //    stringConverterDate(ref value);
        //    tradeTimestamp = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        //}


        public double getPrevClose()
        {
            return prevClose;
        }
        public double getChange()
        {
            return change;
        }
        public double getLast()
        {
            return lastPrice;
        }

    }
}
