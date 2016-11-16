using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketCrawler
{
    class SQL
    {
        public void insertInstrumentsInDatabase(List<Instrument> instruments)
        {
            using (SqlConnection sqlConnection = new SqlConnection("Data Source=eurosqlprod.EuroInvestor.com;UID=euroinvestor;PWD=moneymoneymoney;Initial Catalog=IRBackendDB"))
            {
                sqlConnection.Open();

                foreach (Instrument instrument in instruments)
                {
                    using (SqlCommand sqlCommand = new SqlCommand(@"[Instrument_updateCurrentPrice]", sqlConnection) { CommandType = CommandType.StoredProcedure })
                    {
                        sqlCommand.Parameters.AddWithValue("@instrumentid", instrument.instrumentID);
                        sqlCommand.Parameters.AddWithValue("@lastPrice", instrument.lastPrice);
                        sqlCommand.Parameters.AddWithValue("@change", instrument.change);
                        sqlCommand.Parameters.AddWithValue("@pctChange", instrument.pctChange);
                        sqlCommand.Parameters.AddWithValue("@openPrice", instrument.openPrice);
                        sqlCommand.Parameters.AddWithValue("@dayHigh", instrument.dayHigh);
                        sqlCommand.Parameters.AddWithValue("@dayLow", instrument.dayLow);
                        sqlCommand.Parameters.AddWithValue("@prevClose", instrument.prevClose);
                        sqlCommand.Parameters.AddWithValue("@bid", instrument.bid);
                        sqlCommand.Parameters.AddWithValue("@ask", instrument.ask);
                        sqlCommand.Parameters.AddWithValue("@volume", instrument.volume);
                        sqlCommand.Parameters.AddWithValue("@timestamp", instrument.timestamp);
                     //   sqlCommand.Parameters.AddWithValue("@tradeTimestamp", instrument.tradeTimestamp);


                        sqlCommand.ExecuteNonQuery();
                    }

                }
                sqlConnection.Close();
            }
        }
    }
}