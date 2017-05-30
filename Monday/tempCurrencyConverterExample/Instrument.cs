using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static CurrenciesCommodities.StateMachine;

namespace CurrenciesCommodities
{
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("InstrumentCollection")]
    public class InstrumentCollection
    {
        [XmlArray("getQuote")]
        [XmlArrayItem("item", typeof(Instrument))]
        public Instrument[] Instruments { get; set; }
    }

    [Serializable()]
    public  class Instrument
    {
        [System.Xml.Serialization.XmlElement("symbol")]
        public string symbol { get; set; }
        [System.Xml.Serialization.XmlElement("exchange")]
        public string exchange { get; set; }
        [System.Xml.Serialization.XmlElement("lastPrice")]
        public double lastPrice { get; set; }
        [System.Xml.Serialization.XmlElement("netChange")]
        public double netchange { get; set; }
        [System.Xml.Serialization.XmlElement("percentChange")]
        public double percentChange { get; set; }
        [System.Xml.Serialization.XmlElement("open")]
        public double open { get; set; }
        [System.Xml.Serialization.XmlElement("high")]
        public double high { get; set; }
        [System.Xml.Serialization.XmlElement("low")]
        public double low { get; set; }
        [System.Xml.Serialization.XmlElement("close")]
        public double close { get; set; } 
        [System.Xml.Serialization.XmlElement("volume")]
        public double volume { get; set; }
        [System.Xml.Serialization.XmlElement("tradeTimestamp")]
        public DateTime tradeTimestamp { get; set; }
        [System.Xml.Serialization.XmlElement("serverTimestamp")]
        public DateTime serverTimeStamp { get; set; }

        public int ID { get; set; }
        public double IntraDayPreviousPrice { get; set; }
        public double ClosePricePreviousPrice { get; set; }


        public Instrument()
        {

        }
        public Instrument(string symbol)
        {
            this.symbol = symbol;
        }
        public Instrument(string symbol,int id)
        {
            this.symbol = symbol;
            this.ID = id;
        }


        public Instrument(string symbol, int ID, double lastPrice, DateTime tradeTimestamp, double netchange, double percentChange, double open, double high, double low, double volume)
        {
            this.serverTimeStamp = DateTime.Now;
            this.symbol = symbol; this.ID = ID; this.lastPrice = lastPrice; this.tradeTimestamp = tradeTimestamp; this.netchange = netchange; this.percentChange = percentChange; this.open = open; this.high = high; this.low = low; this.volume = volume;
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

    }
}
