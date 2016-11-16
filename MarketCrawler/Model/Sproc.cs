using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MarketCrawler
{
    public class Sproc
    {
        public static List<List<string>> Execute(string connectionString, string sproc)
        {
            return Execute(connectionString, sproc, new Dictionary<string, object>());
        }

        public static List<List<string>> Execute(string connectionString, string sproc, Dictionary<string, object> pairs)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sproc;
                    foreach (var pair in pairs)
                    {
                        command.Parameters.AddWithValue("@" + pair.Key, CleanObject(pair.Value));

                    }
                    connection.Open();
                    List<List<string>> retrievedData = new List<List<string>>();
                    SqlDataReader reader = command.ExecuteReader();
                    int colCount = reader.FieldCount;
                    while (reader.Read())
                    {
                        List<string> row = new List<string>();
                        for (int i = 0; i < colCount; i++)
                        {
                            row.Add(reader.GetValue(i).ToString());
                        }
                        retrievedData.Add(row);
                    }
                    return retrievedData;
                }
            }
            catch (Exception e)
            {

                //Logger.Log($"{nameof(Execute)} -> {e.Message}");
                //   throw new InvalidSqlRequestException($"{nameof(Execute)} -> \n{e.Message} -> \n{e.StackTrace}");
                throw new Exception("Invalid SQL request Exception"+e.ToString());
            }
        }


        private static object CleanObject(object value)
        {
            return (value as string)?.Trim() ?? ((value == null || value.Equals(new DateTime(9999, 12, 31))) ? DBNull.Value : value);
        }
    }
}
