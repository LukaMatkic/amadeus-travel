using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace WebApp.Codes
{
    public class LowFareHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region LoadFromDatabase(SqlConnection connection, string origin, string destination, DateTime departure, DateTime return_date, int adults, string currency)
        /// <summary>
        /// Funkcija za učitati sve letove iz baze ako postoje
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="departure"></param>
        /// <param name="return_date"></param>
        /// <param name="adults"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static string LoadFromDatabase(SqlConnection connection, string origin, string destination, DateTime departure, DateTime return_date, int adults, string currency)
        {
            // Stvaramo naredbu
            string sqlQuery =
                "SELECT TOP(1) [json_result] FROM [result] WHERE " +
                "[departure]=@departure AND " +
                "[arrival]=@arrival AND " +
                "[dep_date]=@dep_date AND " +
                "[arr_date]=@arr_date AND " +
                "[passangers]=@passangers AND " +
                "[currency]=@currency;";

            // Dodajemo naredbu
            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
            {
                // Dodajemo sve potrebne parametre
                command.Parameters.AddWithValue("departure", origin);
                command.Parameters.AddWithValue("arrival", destination);
                command.Parameters.AddWithValue("dep_date", departure);
                command.Parameters.AddWithValue("arr_date", return_date);
                command.Parameters.AddWithValue("passangers", adults);
                command.Parameters.AddWithValue("currency", currency);

                // Izvršavamo naredbu
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // Log
                    log.Info(String.Format("LoadFromDatabase RESULT ['{0}'] PARAMS ['{1}','{2}','{3}','{4}','{5}','{6}']", reader.HasRows, origin, destination, departure.ToString(), return_date.ToString(), adults, currency));

                    // Ako ima redaka dodajemo ih
                    if (reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            return (string)reader[0];
                        }

                        return null;
                    }
                    else
                    {
                        // Ako nema redaka vraćamo null
                        return null;
                    }
                }
            }
        }
        #endregion

        #region LoadFromAPI(SqlConnection connection, string origin, string destination, DateTime departure, DateTime return_date, int adults, string currency)
        /// <summary>
        /// Funkcija za učitati sve letove sa API-a
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="departure"></param>
        /// <param name="return_date"></param>
        /// <param name="adults"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static async Task<string> LoadFromAPI(SqlConnection connection, string origin, string destination, DateTime departure, DateTime return_date, int adults, string currency)
        {
            // Stvaramo zahtjev za slanje
            UriBuilder builder = new UriBuilder
            {
                Scheme = Uri.UriSchemeHttps,
                Port = -1,
                Host = "api.sandbox.amadeus.com",
                Path = "v1.2/flights/low-fare-search"
            };

            // Dodajemo sve potrebne parametre
            NameValueCollection query = HttpUtility.ParseQueryString(builder.Query);
            query["apikey"] = "LzMGUiqaJVTEGzfkhJhgvAGcZ7hRLV5I";
            query["origin"] = origin;
            query["destination"] = destination;
            query["departure_date"] = departure.ToString("yyyy-MM-dd");
            query["return_date"] = return_date.ToString("yyyy-MM-dd");
            query["adults"] = adults.ToString();
            query["currency"] = currency;
            builder.Query = query.ToString();

            // Stvaramo klijenta i šaljemo zahtjev
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(builder.ToString());

            // Log
            log.Info(String.Format("LoadFromAPI RESULT ['{0}'] PARAMS ['{1}','{2}','{3}','{4}','{5}','{6}']", response.StatusCode, origin, destination, departure.ToString(), return_date.ToString(), adults, currency));

            // Ukoliko je odgovor zahtjeva 404 (NOT FOUND) vraćamo prazan string
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return string.Empty;
            }
            else if (response.IsSuccessStatusCode) // Ukoliko je 200 (OK) vraćamo odgovor
            {
                // Ovdje je bitno da pretvaramo u LowFare objekt u kojem smo uklonili nepotrebne podatke koje ne koristimo
                LowFare fare = await response.Content.ReadAsAsync<LowFare>();

                // Šaljemo serijaliziran sa manje podataka
                return JsonConvert.SerializeObject(fare);
            }
            else // U svakom drugom slučaju vrati null
            {
                return null;
            }
        }
        #endregion

        #region SaveToDatabase(SqlConnection connection, string origin, string destination, DateTime departure, DateTime return_date, int adults, string currency, string fare)
        /// <summary>
        /// Funkcija za spremanje rezultata u bazu podataka
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="departure"></param>
        /// <param name="return_date"></param>
        /// <param name="adults"></param>
        /// <param name="currency"></param>
        /// <param name="fare"></param>
        public static void SaveToDatabase(SqlConnection connection, string origin, string destination, DateTime departure, DateTime return_date, int adults, string currency, string fare)
        {
            // Stvaramo naredbu
            string sqlQuery =
                "INSERT INTO [result] " +
                "([departure], [arrival], [dep_date], [arr_date], [passangers], [currency], [json_result]) " +
                "VALUES " +
                "(@departure, @arrival, @dep_date, @arr_date, @passangers, @currency, @json_result);";

            // Dodajemo naredbu
            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
            {
                // Dodajemo sve potrebne parametre
                command.Parameters.AddWithValue("departure", origin);
                command.Parameters.AddWithValue("arrival", destination);
                command.Parameters.AddWithValue("dep_date", departure);
                command.Parameters.AddWithValue("arr_date", return_date);
                command.Parameters.AddWithValue("passangers", adults);
                command.Parameters.AddWithValue("currency", currency);
                command.Parameters.AddWithValue("json_result", fare);

                int result = command.ExecuteNonQuery();

                // Log
                log.Info(String.Format("SaveToDatabase RESULT ['{0}'] PARAMS ['{1}','{2}','{3}','{4}','{5}','{6}']", (result == 1), origin, destination, departure.ToString(), return_date.ToString(), adults, currency));
            }
        }
        #endregion

    }
}
