using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IATAsync
{
    class Program
    {
        /// <summary>
        /// Kod za preuzeti i u bazu spremiti kodove IATA kodove aerodroma sa Wikipedie
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("INITIALIZATION...");

            // Inicijalizacije i definicije
            char[] letters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            string baseUrl = "https://en.wikipedia.org/wiki/List_of_airports_by_IATA_code:_";
            string connectionString = "Data Source=192.168.88.2;Initial Catalog=amadeus-travel;Persist Security Info=True;User ID=amadeus-travel;Password=Lozinka123!#";
            Dictionary<string, string> airports = new Dictionary<string, string>();

            Console.WriteLine("STARTING...");

            // Prolazimo kroz sve URL-e i punimo listu aerodroma iz tablice
            foreach (char letter in letters)
            {
                string fullUrl = baseUrl + letter;
                Console.WriteLine("DOWNLOADING " + fullUrl + "...");

                // Preuzimamo xml sa stranice
                string siteXml = string.Empty;
                using (WebClient webClient = new WebClient())
                {
                    siteXml = webClient.DownloadString(fullUrl);
                }

                // Pretvaramo xml koji smo dobili sa stranice
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(siteXml);

                // Iz xml-a izdvajamo samo tablicu koja ima podatke o aerodromima pomoæu xml query-a
                HtmlNode table = doc.DocumentNode.SelectSingleNode("//table");

                Console.WriteLine("PARSING " + fullUrl + "...");

                // Prolazimo kroz svaki redak u tablici
                foreach (HtmlNode row in table.SelectNodes("//tr"))
                {
                    // Uzimamo prvi i treæi zapis tražeæi "td" zapise
                    // U HTML5 u naslovu tablice (headeru) se koristi "th" pa ovime preskaæemo i naslov
                    HtmlNode iataColumn = row.SelectSingleNode("td[1]");
                    HtmlNode nameColumn = row.SelectSingleNode("td[3]");

                    // Ako prvi i treæi zapis ne postoje radi se o meðuredu u tablici pa preskaæemo
                    if (iataColumn == null || nameColumn == null)
                    {
                        continue;
                    }
                    else
                    {
                        // Ako oba zapisa postoje dodajemo u listu
                        // IATA u tablici zna imati dodatne znakove pa uzimamo samo prva 3
                        airports.Add(iataColumn.InnerText.Substring(0, 3), nameColumn.InnerText.Replace("[1]", string.Empty));
                    }
                }
            }

            Console.WriteLine("SQL SAVING...");

            // Spajamo se na bazu i spremamo nove podatke iz liste
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Brišemo stare podatke iz tablice
                using (SqlCommand command = new SqlCommand("DELETE FROM [airport];", connection))
                {
                    command.ExecuteScalar();
                }

                // Prolazimo kroz sve nove podatke i spremamo ih
                // Trebalo je napisati jedan query za više podataka ali u redu je i ovako
                int count = 0;
                foreach (KeyValuePair<string, string> airport in airports)
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO [airport] ([iata], [name]) VALUES (@iata, @name);", connection))
                    {
                        command.Parameters.AddWithValue("@iata", airport.Key);
                        command.Parameters.AddWithValue("@name", airport.Value);

                        command.ExecuteNonQuery();
                    }

                    count++;

                    // Log svakih 500 redaka znamo da nije stalo
                    if(count%500 == 0)
                    {
                        Console.WriteLine("{0}/{1}", count, airports.Count);
                    }
                }
            }

            Console.WriteLine("ENDED...");
        }

    }
}
