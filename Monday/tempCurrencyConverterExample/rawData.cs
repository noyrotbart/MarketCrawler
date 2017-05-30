using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static CurrenciesCommodities.StateMachine;

namespace CurrenciesCommodities
{
    public static class rawData
    {
        public static Dictionary<string, double> ConversionDic = new Dictionary<string, double>();
        public static string allItems;
        public static void PopulateCurrencies()
        {
            try
            {
                string strSQL = string.Format(@"SELECT TOP 1000 Symbol,Id  FROM[EuroInvestorStockDB].[dbo].[Instrument] with (nolock) where exchangeid = 164");
                using (DataTable reader = SQLC.RunSQL(strSQL))
                {
                    foreach (DataRow dr in reader.Rows)
                    {
                        // insert instrument ID to the DB
                        Program.CurrencyList.Add(new Instrument(dr["Symbol"].ToString(),(int)dr["id"]));
                    }
                }
            }
            catch
            {
                Console.WriteLine("Failed on select on the DB");
                state = Condition.DBerror;
                return;
            }
        }

        public static double convertCurrency(string from, string to)
        {
            // Some we have and some we don't
            try
            {
                if (from == "USD") { return ConversionDic[from + to]; }
                else if (to == "USD") { return 1 / ConversionDic[to + from]; }
                else { return (1 / rawData.ConversionDic["USD" + from]) * rawData.ConversionDic["USD" + to]; }
            }
            //Those we don't we return 0.
            catch
            { return 0; }
        }

        public static void  InstrumentXMLReader(string path)
        {
            InstrumentCollection instruments = null;
            XmlSerializer serializer = new XmlSerializer(typeof(InstrumentCollection));
            XmlDocument myDoc = new XmlDocument();

            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile(path, "File.xml");
                }
                catch
                {
                    Console.WriteLine("*** Had problem reading the website *** ");
                    state= Condition.FileReadError;
                    return;
                }
                try
                {
                    // adding the parent instrumentCollection
                    myDoc.Load("File.xml");
                    XDocument doc = XDocument.Parse(myDoc.OuterXml);
                    XDocument result = new XDocument(new XElement("InstrumentCollection", doc.Root));
                    result.Save("File.xml");
                    // Now we can use the streamreader to feed the seralized data
                    using (StreamReader reader = new StreamReader("File.xml"))
                    {
                        instruments = (InstrumentCollection)serializer.Deserialize(reader);
                    }
                }
                catch
                {
                    Console.WriteLine("*** Had problem with the XML FILE *** ");
                    state= Condition.DBerror;
                    return;
                }

            }

            foreach (Instrument item in instruments.Instruments)
            {
                item.symbol = item.symbol.Replace("^", String.Empty);
            }

            if (path == ConfigurationManager.AppSettings["currencypath"])
            {
                // Populate the conversion Dictionary 
                foreach (Instrument item in instruments.Instruments)
                {
                    ConversionDic.Add(item.symbol, item.lastPrice);
                }
            }
            else
            {
                // terrible terrible hack :)
                double EUR = convertCurrency("USD", "EUR");
                double GBP = convertCurrency("USD", "GBP");
                Instrument G = instruments.Instruments[0];
                Instrument S = instruments.Instruments[1];
                Program.Others.Add(new Instrument
                    ("XAUUSD", 2327121, G.lastPrice, G.tradeTimestamp, G.netchange, G.percentChange, G.open, G.high, G.low, G.volume));
                Program.Others.Add(new Instrument
                    ("XAGUSD", 2327098, S.lastPrice, S.tradeTimestamp, S.netchange, S.percentChange, S.open, S.high, S.low, S.volume));
                Program.Others.Add(new Instrument
                    ("XAUEUR", 2327107,G.lastPrice*EUR,G.tradeTimestamp,G.netchange*EUR,G.percentChange,G.open*EUR,G.high*EUR,G.low*EUR,G.volume));
                Program.Others.Add(new Instrument
                   ("XAGEUR", 2327093, S.lastPrice * EUR, S.tradeTimestamp, S.netchange * EUR, G.percentChange, S.open * EUR, S.high * EUR, S.low * EUR, G.volume));
                Program.Others.Add(new Instrument
                  ("XAUGBP", 2327110, G.lastPrice * GBP, G.tradeTimestamp, G.netchange * GBP, G.percentChange, G.open * GBP, G.high * GBP, G.low * GBP, G.volume));
                Program.Others.Add(new Instrument
                   ("XAGGBP", 2327095, S.lastPrice * GBP, S.tradeTimestamp, S.netchange * GBP, G.percentChange, S.open * GBP, S.high * GBP, S.low * GBP, G.volume));
            }
        }
    }
}
