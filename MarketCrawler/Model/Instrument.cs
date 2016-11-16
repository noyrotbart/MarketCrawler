using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketCrawler
{
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
      // public DateTime tradeTimestamp;
        public string url;
        public StockName stockExchangeName { get; set; }

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
