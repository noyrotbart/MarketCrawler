using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using static CurrenciesCommodities.StateMachine;

namespace CurrenciesCommodities
{
  
    class Program
    {
        public static List<Instrument> CurrencyList = new List<Instrument>();
        public static List<Instrument> Others = new List<Instrument>();

        static void Main(string[] args)
        {
            rawData.PopulateCurrencies();
            runner();
        }

        static void runner()
        {
            state = Condition.WorkingC;
           rawData.InstrumentXMLReader(ConfigurationManager.AppSettings["currencypath"]);
            if (state != Condition.WorkingC ) return;
           Console.WriteLine("*** Starting at {0} *** \n *** Currency calling InstrumentInsert *** ", DateTime.Now.ToString("HH:mm:ss"));
            InstrumentInsert(CurrencyList);
            // When the day changes, update closeprice
            if (DateTime.Now.Day != DateTime.Now.AddMinutes(-5).Day)
            {
                    Console.WriteLine("*** calling ClosePriceInsert *** ");
                    ClosePriceInsert(CurrencyList);
            }
            Console.WriteLine("Done with currency");
            //Now moving to working on the Goods
            state = Condition.WorkingG;
            Console.WriteLine("***calling InstrumentInsert for Goods *** ", DateTime.Now.TimeOfDay);
            rawData.InstrumentXMLReader(ConfigurationManager.AppSettings["Commoditypath"]);
            if (state != Condition.WorkingG) return;
            InstrumentInsert(Others);
            if (state != Condition.WorkingG) return;
            Console.WriteLine("*** calling IntradaytInsert for Goods*** ");
            IntradaytInsert(Others);
            // When the day changes, update closeprice
            if (DateTime.Now.Day != DateTime.Now.AddMinutes(-5).Day)
            {
                Console.WriteLine("*** calling ClosePriceInsert for Goods *** ");
                ClosePriceInsert(Others);
            }
            Console.WriteLine("*** Done and resting at " + DateTime.Now.ToString("HH:mm:ss") + "*** ");
            // Now that we are done, we clear the results of this round 

            state =  Condition.Waiting;
        }
        public static void InstrumentInsert(List<Instrument> instruments)
        {
            if (state == Condition.WorkingC)
            {
                foreach (Instrument item in instruments)
                {
                    double ratio = Math.Round(rawData.convertCurrency(item.symbol.Substring(0, 3), item.symbol.Substring(3, 3)), 4);
                    item.lastPrice = ratio;
                    //  item.close = ratio;  item.high = ratio; item.low = ratio; item.netchange = 0;  item.percentChange = 0;  
                    item.tradeTimestamp = DateTime.Now; item.serverTimeStamp = DateTime.Now;
                }
            }
            // write the data 
            StringBuilder sb = new StringBuilder();
            foreach (Instrument item in instruments)
            {
                sb.Append("UPDATE [EuroInvestorStockDB].[dbo].[Instrument] ");
                if (state == Condition.WorkingC)
                {
                    sb.AppendFormat("SET Bid = {0}, Ask ={0}, Mid = {0}, High= {0}, Low = {0}, Last = {0}, PercentageChange= 0, OpenPrice = {0}, Change=0, TimeStamp='{1}'", item.lastPrice, item.serverTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    sb.AppendFormat("SET Bid = {0}, Ask ={0}, Mid = {1}, High= {2}, Low = {3}, Last = {0}, PercentageChange= {5}, OpenPrice = {6}, Change={7}, TimeStamp='{8}'",
                        item.lastPrice, item.lastPrice, Math.Round(0.5*(item.high+item.low),4) , item.high, item.low, item.percentChange,item.open,item.netchange, item.serverTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                sb.AppendFormat(" WHERE id = {0}", item.ID);
            }
            try
            {
                DataTable notused = SQLC.RunSQL(sb.ToString());
            }
            catch
            {
                Console.WriteLine("Failed on select on the DB");
                state=  Condition.DBerror;
                return;
            }
        }

        //This uses a dictionary of currency crosses from rawData to go from any currency XXX to YYY. It makes sure that this can be done with a limited currencyRawDataInput in the form USDZZZ.


        public static void  IntradaytInsert(List<Instrument> instruments)
        {
            StringBuilder allItems = new StringBuilder();
            foreach (Instrument item in instruments)
            {
                allItems.AppendFormat("{0},", item.ID);

            }
            allItems.Remove(allItems.Length - 1, 1);

            // We will need this one later
            rawData.allItems = allItems.ToString();
            string strSQL = string.Format(@"SELECT  TOP {0} InstrumentID,closePrice FROM [EuroInvestorStockDB].[dbo].[Intraday_Price] where  instrumentID in ({1}) order by ID", instruments.Count, allItems);
            DataTable reader;
            try
            {
                reader = SQLC.RunSQL(strSQL);
            }
            catch
            {
                reader = null;
                Console.WriteLine("Failed on select on the DB");
                state= Condition.DBerror;
                return;
            }

            foreach (DataRow dr in reader.Rows)
            {
                try { instruments.Find(x => x.ID == (int)dr["InstrumentID"]).IntraDayPreviousPrice = Convert.ToDouble(dr["closePrice"]); }
                catch { instruments.Find(x => x.ID == (int)dr["InstrumentID"]).IntraDayPreviousPrice = instruments.Find(x => x.ID == (int)dr["InstrumentID"]).lastPrice; }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Insert  into [EuroInvestorStockDB].[dbo].[Intraday_Price] ");
            sb.Append("(InstrumentID, OpenTimeStamp, CloseTimeStamp, OpenPrice, ClosePrice, High, Volume, Low, VWAP, PriceType, totalVol) values");

            foreach (Instrument item in instruments)
            {
                    sb.AppendFormat("({0} ,'{1}' , '{2}',{3} , {4} , {4} , 0 , {4}, 0, null, 0 ), ", item.ID, DateTime.Now, DateTime.Now.AddMinutes(-5), item.IntraDayPreviousPrice, item.lastPrice);
            }

            sb.Remove(sb.Length - 2, 1);
            try
            {
                reader = SQLC.RunSQL(sb.ToString());
            }
            catch
            {
                Console.WriteLine("Failed on select on the DB");
                state = Condition.DBerror;
                return;
            }

            //  If the day changed write to Intraday
        }

        public static void ClosePriceInsert(List<Instrument> instruments)
        {
            string strSQL =  string.Format(@"SELECT  TOP {0} InstrumentID,closePrice FROM [EuroInvestorStockDB].[dbo].[Close_Price] where  instrumentID in ({1}) order by ID", instruments.Count, rawData.allItems);
            DataTable reader;
            try { reader = SQLC.RunSQL(strSQL); }

            catch
            {
                Console.WriteLine("Failed on select on the DB");
                state =  Condition.DBerror;
                return;
            }
            foreach (DataRow dr in reader.Rows)
            {
                try { instruments.Find(x => x.ID == (int)dr["InstrumentID"]).ClosePricePreviousPrice = Convert.ToDouble(dr["closePrice"]); }
                catch { instruments.Find(x => x.ID == (int)dr["InstrumentID"]).ClosePricePreviousPrice = instruments.Find(x => x.ID == (int)dr["InstrumentID"]).lastPrice; }
            }

            StringBuilder stb = new StringBuilder();
            stb.Append("Insert  into [EuroInvestorStockDB].[dbo].[Intraday_Price] ");
            stb.Append("(Date, ExchangeID, InstrumentID, OpenPrice, High, Low, ClosePrice, TotVol, VWAP, OpenInterest,  TradeCoount, PriceType, CloseType ) values");
            foreach (Instrument item in instruments)
            {
                if (state == Condition.WorkingC)
                {
                    stb.AppendFormat("({0} ,164 , {1} ,{2} , {3} , {3},{3},0,NULL,NULL,0,0,0 ), ", DateTime.Now.Date, item.ClosePricePreviousPrice, item.ID, item.lastPrice);
                }
                else
                {
                    stb.AppendFormat("({0} ,87 , {1} ,{2} , {3} , {3},{3},0,NULL,NULL,0,0,0 ), ", DateTime.Now.Date, item.ClosePricePreviousPrice, item.ID, item.lastPrice);
                }
            }
            try { reader = SQLC.RunSQL(stb.ToString()); }

            catch
            {
                Console.WriteLine("Failed on select on the DB");
                state=  Condition.DBerror;
                return;
            }
        }

    }
}

