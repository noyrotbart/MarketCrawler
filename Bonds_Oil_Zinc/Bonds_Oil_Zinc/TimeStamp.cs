using System;
using System.Collections.Generic;
using System.Linq;


namespace Bonds_Oil_Zinc
{
    class TimeList
    {
        protected static int maxItems = 30;
        // A collection of timestamps, at most 30, ordered by their time
        protected static SortedList<DateTime, TimeStamp> MyBuffer = new SortedList<DateTime, TimeStamp>();
        public void InsertValue(DateTime time, TimeStamp stamp)
        {
            //Make space
            while (MyBuffer.Count > maxItems - 1)
            {
                MyBuffer.Remove(MyBuffer.Keys.Min());
            }
            MyBuffer.Add(time, stamp);
        }
        public TimeStamp getPostponedValue(DateTime time, int minuteDelay)
        {
            // Get the closest value of  minuteDelay prior, from below
            DateTime updatedTime = time.Subtract(new TimeSpan(0, minuteDelay, 0));
            try
            {
                return MyBuffer.Last(x => x.Key <= updatedTime).Value;

            }
            catch
            {
                return MyBuffer.Last().Value;
            }
        }
    }
    class TimeStamp

    // A struct containing the  requested items
    {
        public string symbol { get; set; }
        public int id { get; set; }
        public int exchangeID { get; set; }
        public string link { get; set; }
        public double pctChange { get; set; }
        public double lastprice { get; set; }
        public double change { get; set; }
        public int volume { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double openprice { get; set; }
        public double prevclose { get; set; }
        public DateTime timestamp { get; set; }

        public TimeStamp()
        {

        }
        public TimeStamp(int id, string symbol, string link, int exchangeID)
        {
            this.id = id; this.symbol = symbol; this.link = link; this.exchangeID = exchangeID;
        }

        public override string ToString()
        {
            return (string.Format("change:{0},lastprice:{1},pctChange:{2},volume:{3},high:{4},low:{5},openprice:{6},prevclose:{7},time:{8}",
                this.change, this.lastprice, this.pctChange, this.volume, this.high, this.low,
                this.openprice, this.prevclose, this.timestamp));

        }

    }
}
