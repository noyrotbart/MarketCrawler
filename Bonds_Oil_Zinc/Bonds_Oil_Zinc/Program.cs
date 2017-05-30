using System;
using System.Data.SqlClient;
using System.Data;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium;
using System.IO;
using System.Net;
using System.Configuration;
using System.Xml.Linq;
using System.Threading;

namespace Bonds_Oil_Zinc
{
    class Program
    {
        public static TimeStamp[] bonds = new TimeStamp[3];
        public static string databaseDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        static void Main(string[] args)
        {



            bonds[0] = new TimeStamp(12976205, "B:CBOTA", "a1hc42-cbo-territoria-anleihe", 6);
            bonds[1] = new TimeStamp(21788437, "B:JZCC", "a1zry7-jz-capital-partners-anleihe", 1);
            bonds[2] = new TimeStamp(33774798, @"B:AGLHA\M3908\5.59", "a1zs43-agl-energy-anleihe", 13);
            while (true)
            {
                Console.WriteLine("*** calling MyMethod *** ");
                runner();
                File.AppendAllText("log.txt", "Success at" + DateTime.Now.ToString() + "\n");
                Thread.Sleep(60 * 60 * 24 * 1000);

            }

        }
        public static void runner()
        {

            try { instrumentUpdate(CrudeOil()); }
            catch (Exception E)
            { sendEmail("noyr@q4inc.com", "Noy Rotbart", "noyr@q4inc.com", "A problem with CrudeOil", E.ToString()); }
            try
            { instrumentUpdate(Zinc()); }
            catch (Exception E)
            {
                sendEmail("noyr@q4inc.com", "Noy Rotbart", "noyr@q4inc.com", "A problem with Zinc", E.ToString());
            }
            try
            {
                for (int i = 0; i < bonds.Length; i++)
                {
                    getItems(ref bonds[i]);
                    instrumentUpdate(bonds[i]);
                }
            }
            catch (Exception E)
            {
                sendEmail("noyr@q4inc.com", "Noy Rotbart", "noyr@q4inc.com", "A problem with Bonds", E.ToString());
            }
            Console.Clear();
            Console.Write("Succesfull wrote " + DateTime.Now.Date);
        }

        public static TimeStamp CrudeOil()
        {
            TimeStamp c = new TimeStamp(2327059, @"C:EBROUSDBR\SP", @"http://marketdata.websol.barchart.com/getQuote.xml?key=2dc64a73f0d7f9dc98df4b45dd5ee56c&symbols=QA*1", 138);
            XDocument doc = XDocument.Load(c.link);
            XContainer item = doc.Root.Element("item");
            c.high = Convert.ToDouble(item.Element("high").Value);
            c.lastprice = Convert.ToDouble(item.Element("lastPrice").Value);
            c.openprice = Convert.ToDouble(item.Element("open").Value);
            c.low = Convert.ToDouble(item.Element("low").Value);
            c.timestamp = DateTime.Now;
            c.change = Convert.ToDouble(item.Element("netChange").Value);
            c.pctChange = Convert.ToDouble(item.Element("percentChange").Value);
            return c;
        }

        public static TimeStamp Zinc()
        {
            TimeStamp c = new TimeStamp(5832888, @"F:ZS\C", @"https://www.quandl.com/api/v3/datasets/LME/PR_ZI.xml?api_key=B2zLLJZn-UUhVFbHqjA2&start_date=2017-05-11", 138);
            XDocument doc = XDocument.Load(c.link);
            var item = doc.Root.Element("dataset").Element("data").Element("datum").Element("datum").NextNode.NextNode as XElement;
            double value = Convert.ToDouble(item.Value);
            c.high = value;
            c.lastprice = value;
            c.openprice = value;
            c.low = value;
            c.timestamp = DateTime.Now;
            c.change = 0;
            c.pctChange = 0;
            return c;
        }

        private static void instrumentUpdate(TimeStamp stamp)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection("Data Source=34.248.126.133;UID=SqlPublic;PWD=Jn51ZR%!IzIS$vbN;Initial Catalog=EuroInvestorStockDB"))
                using (SqlCommand cmd = new SqlCommand("EuroInvestorStockDB..Bonds_Insertion", conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("InstrumentID", stamp.id);
                    cmd.Parameters.AddWithValue("Change", stamp.change);
                    cmd.Parameters.AddWithValue("pctChange", stamp.pctChange);
                    cmd.Parameters.AddWithValue("lastPrice", stamp.lastprice);
                    cmd.Parameters.AddWithValue("high", stamp.high);
                    cmd.Parameters.AddWithValue("low", stamp.low);
                    cmd.Parameters.AddWithValue("openprice", stamp.openprice);
                    cmd.Parameters.AddWithValue("prevclose", stamp.prevclose);
                    cmd.Parameters.AddWithValue("timeStamp", stamp.timestamp);
                    cmd.Parameters.AddWithValue("ExchangeID", stamp.exchangeID);
                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception e)
            {
                Console.Write("DB FAIL " + e.ToString());
                Console.Read();

            }

        }

        static public void getItems(ref TimeStamp cTimestep)
        {
            var service = PhantomJSDriverService.CreateDefaultService();
            var phantomJSDriverService = PhantomJSDriverService.CreateDefaultService(@"C:\phantomjs-2.1.1-windows\bin");
            using (PhantomJSDriver driver = new PhantomJSDriver(phantomJSDriverService))
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                driver.Url = @"http://www.finanzen.net/anleihen/" + cTimestep.link;
                driver.Navigate();
                //Console.WriteLine(driver.PageSource);
                try
                {
                    IWebElement element = driver.FindElement(By.ClassName("col-xs-5"));
                    cTimestep.lastprice = Convert.ToDouble(element.Text.Replace(",", ".").Replace("-", "0").Replace("%", null).Replace("±", null));
                    element = driver.FindElement(By.ClassName("col-xs-4"));
                    cTimestep.change = Convert.ToDouble(element.Text.Replace(",", String.Empty).Replace("-", "0").Replace("+", null).Replace("±", null));
                    element = driver.FindElement(By.ClassName("col-xs-3"));
                    cTimestep.pctChange = Convert.ToDouble(element.Text.Replace(",", ".").Replace("-", "0").Replace("%", null).Replace("+", null).Replace("±", null));
                    cTimestep.high = cTimestep.lastprice;
                    cTimestep.low = cTimestep.lastprice;
                    cTimestep.timestamp = DateTime.Now;
                }
                catch (Exception e)
                {
                    throw new Exception("Problem getting the input" + e + " \n WITH"+ cTimestep.id);
                }
            }
        }
        private static int sendEmail(string senderAddress, string senderName, string recipientEmail, string emailSubject, string emailText)
        {
            using (SqlConnection conn = new SqlConnection("Data Source=eurosrv14.EuroInvestor.com;UID=euroinvestor;PWD=moneymoneymoney;Initial Catalog=euroinvestorDB"))
            {
                conn.Open();
                emailText = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">
                <head>
                <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
                <title>Euroinvestor IR Solutions</title>
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
                </head>
                <body>" + emailText + @"
                </body>
                </html>";
                SqlCommand sqlCommand = new SqlCommand(@"PekunuSite.dbo.Mail_SendEmail", conn);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add("@From", SqlDbType.VarChar);
                sqlCommand.Parameters["@From"].Value = senderName + " <" + senderAddress + ">";
                sqlCommand.Parameters.Add("@MailTo", SqlDbType.VarChar);
                sqlCommand.Parameters["@MailTo"].Value = "" + recipientEmail + "";//sqlCommand.Parameters["@MailTo"].Value = "<"+recipientEmail+">";
                sqlCommand.Parameters.Add("@Subject", SqlDbType.VarChar);
                sqlCommand.Parameters["@Subject"].Value = emailSubject;
                sqlCommand.Parameters.Add("@ContentHTML", SqlDbType.VarChar);
                sqlCommand.Parameters["@ContentHTML"].Value = emailText;
                sqlCommand.Parameters.Add("@SMTPConfigID", SqlDbType.Int);
                sqlCommand.Parameters["@SMTPConfigID"].Value = 1;
                sqlCommand.Parameters.Add("@MailReplyTo", SqlDbType.VarChar);
                sqlCommand.Parameters["@MailReplyTo"].Value = "irsales@euroinvestor.com";

                sqlCommand.Parameters.Add("@MailCC", SqlDbType.VarChar);
                sqlCommand.Parameters["@MailCC"].Value = DBNull.Value;
                sqlCommand.Parameters.Add("@MailBcc", SqlDbType.VarChar);
                sqlCommand.Parameters["@MailBcc"].Value = DBNull.Value;
                sqlCommand.Parameters.Add("@ContentText", SqlDbType.VarChar);
                sqlCommand.Parameters["@ContentText"].Value = DBNull.Value;

                string sqlOutput = (string)sqlCommand.ExecuteScalar();
                return Convert.ToInt32(sqlOutput);
            }
        }

    }
}
