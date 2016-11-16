using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MarketCrawler
{
     class AccessFiles
    {
        static string filename = Program.root + "ListInterpreted.txt";
        public static List<Target> getList(StockName stockExchng)
        {
            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                var bformatter = new BinaryFormatter();

                List<Target> allLists = (List<Target>)bformatter.Deserialize(stream);
                if (stockExchng == StockName.All) return allLists;
                else return allLists.Where(s => s.stockExchange.Value == stockExchng.Value).ToList();

            }
        }


        static public  void ReadData(string StockFile, StockName stockName)
        {
            List<Target> targetList = getList(StockName.All);
            System.IO.StreamReader file = new System.IO.StreamReader(Program.root + StockFile);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                String[] lineSplit = line.Split(',');
                Target toAdd = new Target(lineSplit[1], lineSplit[0], stockName);
                if (!targetList.Contains(toAdd)) targetList.Add(new Target(lineSplit[1], lineSplit[0], stockName));

            }

            using (Stream stream = File.Open(filename, FileMode.Create))
            {
                var bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, targetList);
            }
        }

        // This is a legacy reader
        static public void ReadLondonData()
        {
            List<Target> targetList = new List<Target>();
            List<Instrument> instrumentList = new List<Instrument>();
            String initialPath = "http://www.londonstockexchange.com/exchange/searchengine/all/json/search.html?q=";
            int counter = 0;
            string line;
            string savePathCurrent = Program.root + ".html";
            System.IO.StreamReader file = new System.IO.StreamReader(Program.root + @"StocksToCheck.txt");
            // Going over each of the stocks mentioned,line by line
            while ((line = file.ReadLine()) != null)
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers["User-Agent"] =
                      "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
                     "(compatible; MSIE 6.0; Windows NT 5.1; " +
                     ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                    var jsonfile = client.DownloadString(initialPath + line);

                    JsonTextReader reader = new JsonTextReader(new StringReader(jsonfile));
                    bool continueLoop = true;
                    // In the json file, take the value of symbol to targetList
                    while (continueLoop == true)
                    {
                        reader.Read();
                        if (reader.Value != null)
                        {
                            if (String.Equals(reader.Value.ToString(), "symbol1"))
                            {
                                reader.Read();

                                targetList.Add(new Target(line, reader.Value.ToString(), StockName.London));
                                Console.WriteLine("{0},{1}", reader.Value, line);
                                continueLoop = false;
                            }
                        }
                    }

                }
                counter++;
            }
            //Save to a seralizable file 
            using (Stream stream = File.Open(Program.root + "ListInterpreted.txt", FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                bformatter.Serialize(stream, targetList);
            }
            // At this point targetList contains the keys required to get into the 
            file.Close();
        }

    }
}
