﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using MarketCrawler.IgniteInstrument;
using HtmlAgilityPack;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using Xignite.Sdk.Api;
using Xignite.Sdk.Api.Models.XigniteGlobalHistorical;


namespace MarketCrawler
{
    class InstrumentContext
    {
        private IMongoDatabase db;
        public InstrumentContext()
        {
            MongoClient client = new MongoClient();
            this.db = client.GetDatabase("clientlist");
            var collection = db.GetCollection<Instrument>("instrument");
            var igniteCollection = db.GetCollection<IgniteRootObject>("igniteInstrument");
        }
        public IMongoCollection<Instrument> Instruments
        {
            get
            {
                return db.GetCollection<Instrument>("Instrument");

            }

        }
        public IMongoCollection<IgniteRootObject> igniteInstruments
        {
            get
            {
                return db.GetCollection<IgniteRootObject>("igniteInstrument");

            }

        }
    }
    class Program
    {
        private static readonly string ConnectionStr = ConfigurationManager.ConnectionStrings["EuroSrv14Prod"].ConnectionString;

        public static string root = @"C:\Users\nr\Desktop\MarketCrawler\MarketCrawler\";
        static void Main(string[] args)
        {
            // This codeblock instantiate our little stock DB from te textlists in the folder
            //AccessFiles.ReadLondonData();
            //AccessFiles.ReadData("NasdaqNordic.txt", StockName.NasdaqEurope);
            //AccessFiles.ReadData("EURONEXT.txt", StockName.EURONEXT);
            //AccessFiles.ReadData("Nasdaq.txt", StockName.Nasdaq);

            Dictionary<string, object> pars = new Dictionary<string, object>();
            pars["ExchName"] = "London";
            pars["Sym"] = "NVA";

            List<List<string>> pathsInput = Sproc.Execute(ConnectionStr, "[dbo].[CF_GetStockFromTickerAndExchange]", pars);
           GlobalHistoricalQuote[] queryX =  AccessXIgnite.getXigniteData("XLON", "EZH", 100);

            while (true)
            {
                AccessEuroNext();
                AccessNasdaqEurope();
                // AccessNasdaq();
                AccessEnglishWeb();
                // wait five minutes
                System.Threading.Thread.Sleep(5 * 60 * 1000);
                }

            }

        // This function gets  a url path and an xquery and returns an array of the values in the xquery taken from the path
        static HtmlNode[] GetTable(string path, string xquery)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers["User-Agent"] =
                           "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36";
                client.UseDefaultCredentials = true;
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(client.DownloadString(path));
                htmlDoc.Save(root + "websiteDump.txt");
                HtmlNodeCollection currentData = htmlDoc.DocumentNode.SelectNodes(xquery);
                return currentData.Cast<HtmlNode>().ToArray();
            }

        }

       
        static void AccessNasdaqEurope()
        {
            List<Target> stockList = AccessFiles.getList(StockName.NasdaqEurope);
            using (PhantomJSDriver driver = new PhantomJSDriver(@"C:\phantomjs-2.1.1-windows\bin"))
            {
                foreach (Target stock in stockList)
                {
                    driver.Url = stock.urlIdentifier;
                    driver.Navigate();
                    var table = driver.FindElementByCssSelector("table");
                    string[] valueChange = driver.FindElementByClassName("valueChange").Text.Split('(');
                    //In case of no change, the website has an empty string rather than 0, which we fix in the line below
                    if (valueChange[0].Trim() == String.Empty)
                    {
                        valueChange[0] = "0";
                        
                     }

                    Instrument myInstrument = new Instrument();
                    {
                        myInstrument.PctChange(valueChange[1].Replace("%", string.Empty).Replace(")", string.Empty));
                        myInstrument.Change(valueChange[0]);
                        myInstrument.LastPrice(driver.FindElementByClassName("valueLatest").Text);
                        myInstrument.DayHigh(table.FindElement(By.ClassName("hp")).Text);
                        myInstrument.DayLow(table.FindElement(By.ClassName("lp")).Text);
                        myInstrument.Volume(table.FindElement(By.ClassName("tv")).Text);
                        myInstrument.PrevClose(table.FindElement(By.ClassName("op")).Text);
                        //    myInstrument.Bid(innerText[7]);
                        //    myInstrument.Ask(innerText[23]);
                        myInstrument.ticker = stock.shortIdentifier;
                        myInstrument.timestamp = System.DateTime.Now;
                        myInstrument.stockExchangeName = StockName.NasdaqEurope;
                        myInstrument.url = stock.urlIdentifier;
                    }
                    InstrumentContext ctx = new InstrumentContext();
                    ctx.Instruments.InsertOne(myInstrument);
                }
            }

        }

        static void AccessEuroNext()
        {
            List<Target> stockList = AccessFiles.getList(StockName.EURONEXT);
            using (PhantomJSDriver driver = new PhantomJSDriver(@"C:\phantomjs-2.1.1-windows\bin"))
            {
                foreach (Target stock in stockList)
                {
                    driver.Url = stock.urlIdentifier;
                    driver.Navigate();
                    System.Threading.Thread.Sleep(3 * 1000);
                    Instrument myInstrument = new Instrument();
                    {
                        try
                        {
                            myInstrument.PctChange(driver.FindElementById("cnDiffRelvalue").Text.Replace("(", string.Empty).Replace(")", string.Empty));
                            myInstrument.Change(driver.FindElementById("cnDiffAbsvalue").Text);
                            myInstrument.LastPrice(driver.FindElementById("lastPriceint").Text + driver.FindElementById("lastPricefract").Text);
                            myInstrument.DayHigh(driver.FindElementById("highPricevalue").Text.Split('[')[0]);
                            myInstrument.DayLow(driver.FindElementById("lowPricevalue").Text.Split('[')[0]);
                            string b = driver.FindElementById("todayVolumevalue").Text.Split('[')[0];
                            myInstrument.Volume(driver.FindElementById("todayVolumevalue").Text.Split('[')[0]);
                            //myInstrument.PrevClose(table.FindElement(By.ClassName("op")).Text);

                            myInstrument.Bid(driver.FindElementById("bidPricevalue").Text);
                            myInstrument.Ask(driver.FindElementById("askPricevalue").Text);


                            myInstrument.ticker = stock.shortIdentifier;
                            myInstrument.timestamp = System.DateTime.Now;
                            myInstrument.stockExchangeName = StockName.EURONEXT;
                            myInstrument.url = stock.urlIdentifier;
                        }

                        catch

                        {

                        }
                    }
                    InstrumentContext ctx = new InstrumentContext();
                    ctx.Instruments.InsertOne(myInstrument);
                }
            }
        }

        static void AccessNasdaq()
        {
            List<Target> stockList = AccessFiles.getList(StockName.Nasdaq);
                foreach (Target stock in stockList)
                {
                    string xquery = ".//div[@class='genTable thin']//td";
                    HtmlNode[] myArray = GetTable(stock.urlIdentifier, xquery);
                    String[] m = Regex.Split(myArray[17].InnerText, ";");
                    string[] innerText = new string[myArray.Length];
                    for (int i = 0; i < myArray.Length; i++)
                    {
                        innerText[i] = myArray[i].InnerText.Replace("&", string.Empty).Replace("#", string.Empty).Replace("%", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("nbsp", string.Empty).Replace("$", string.Empty).Replace(";", string.Empty);
                    }
                    for (int i = 0; i < m.Length; i++)
                    {
                        m[i].Replace("&", string.Empty).Replace("#", string.Empty).Replace("%", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("nbsp", string.Empty).Replace("$", string.Empty).Replace(";", string.Empty);
                    }

                    Instrument myInstrument = new Instrument();
                    {
                        //  This is a combination of change and pct change and we thus separate them
                        if (m[1] == "9660")
                        {
                            myInstrument.PctChange("-" + m[3]);
                            myInstrument.Change("-" + m[0]);
                        }

                        else {
                            myInstrument.PctChange(m[3]);
                            myInstrument.Change(m[0]);
                        }
                        myInstrument.LastPrice(innerText[1]);
                        myInstrument.DayHigh(innerText[5]);
                        myInstrument.DayLow(innerText[21]);
                        myInstrument.Volume(innerText[3]);
                        myInstrument.PrevClose(innerText[19]);
                        myInstrument.Bid(innerText[7]);
                        myInstrument.Ask(innerText[23]);
                        myInstrument.ticker = stock.shortIdentifier;
                        myInstrument.timestamp = System.DateTime.Now;
                        myInstrument.stockExchangeName = StockName.Nasdaq;
                        myInstrument.url = stock.urlIdentifier;
                    }
                    InstrumentContext ctx = new InstrumentContext();
                    ctx.Instruments.InsertOne(myInstrument);
                }

        }

       static void AccessEnglishWeb()
        {
            InstrumentContext ctx = new InstrumentContext();
            String queryPath = @"http://www.londonstockexchange.com/exchange/prices-and-markets/stocks/summary/company-summary/";
            List<Target> londonStocks =  AccessFiles.getList(StockName.London);
                foreach (Target item in londonStocks)
                {
                    string xquery = ".//div[@class='commonTable table-responsive']/table//td";
                    string path = queryPath + item.urlIdentifier + ".html";
                    Console.WriteLine("Getting {0}", path);
                    var myArray = GetTable(path, xquery);
                    Instrument myInstrument = new Instrument();
                    {
                        string changes = myArray[3].InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(")", string.Empty).Replace("(", string.Empty);
                        String[] m = Regex.Split(changes, "%");
                        myInstrument.PctChange(m[0]);
                        myInstrument.Change(m[1]);
                        myInstrument.LastPrice(myArray[1].InnerText);
                        myInstrument.DayHigh(myArray[5].InnerText);
                        myInstrument.DayLow(myArray[7].InnerText);
                        myInstrument.Volume(myArray[9].InnerText);
                        string isolatePrevClose = myArray[11].InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty);
                        m = Regex.Split(isolatePrevClose, "on");
                        myInstrument.PrevClose(m[0]);
                        myInstrument.Bid(myArray[13].InnerText);
                        myInstrument.Ask(myArray[15].InnerText);
                        myInstrument.ticker = item.shortIdentifier;
                        myInstrument.timestamp = System.DateTime.Now;
                        myInstrument.stockExchangeName = StockName.London;
                        myInstrument.url = path;
                    }
                    // accessing the mongoDB 

                    ctx.Instruments.InsertOne(myInstrument);
                    // In order to not get banned we delay individul request from the london website
                    System.Threading.Thread.Sleep(1000);

                }
            }
        }
}
