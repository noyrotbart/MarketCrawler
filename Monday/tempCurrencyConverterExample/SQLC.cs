using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrenciesCommodities
{
    class SQLC
    {
        public static DataTable RunSQL(string command)
        {
            SqlDataReader reader;
            string connection = ConfigurationManager.ConnectionStrings["Production"].ConnectionString;
            using (SqlConnection sqlConnection = new SqlConnection(connection))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(command, sqlConnection))
                {
                    reader = sqlCommand.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
        }

    }
}
