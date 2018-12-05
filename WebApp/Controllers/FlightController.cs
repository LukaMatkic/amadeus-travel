using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using WebApp.Codes;

namespace WebApp.Controllers
{
    public class FlightController : Controller
    {
        // MVC

        // GET: /Flight/Index
        #region Index()
        public IActionResult Index()
        {
            return View();
        }
        #endregion


        // JQUERY

        // GET: /Flight/Search
        #region Search(string origin, string destination, DateTime departure, DateTime return_date, int adults, string currency)
        /// <summary>
        /// Funkcija za poslati sve letove prema traženim 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="departure"></param>
        /// <param name="return_date"></param>
        /// <param name="adults"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public async Task<string> Search(string origin, string destination, DateTime departure, DateTime return_date, int adults, string currency)
        {
            // Provjeravamo parametre
            // Parametri su provjereni na frontendu  pa ako dođe neki krivi radi se o bug-u ili korisnik koristi neke API alate za slanje zahtjeva
            // Pošto nije potreban login radimo i mali delay radi zaštite
            if ((string.IsNullOrEmpty(origin) || origin.Length != 3) ||
                (string.IsNullOrEmpty(destination) || destination.Length != 3) ||
                (departure.Date < DateTime.Now.Date || departure.Date >= return_date.Date) ||
                (return_date.Date <= departure.Date) ||
                (adults < 1 || adults > 50) ||
                (currency != "HRK" && currency != "USD" && currency != "EUR"))
            {
                // Ne želimo da korisnik zna gdje je pogriješio pa radimo mali delay u odgovoru i vraćamo prazan odgovor
                Random rand = new Random();
                await Task.Delay(rand.Next(750, 5000));
                return string.Empty;
            }


            // Spajamo se na bazu podataka
            using (SqlConnection connection = new SqlConnection(AppConfig.GetByKey("connectionString")))
            {
                connection.Open();
                string result = string.Empty;

                // Provjeravamo dali u bazi postoji već pretraživanje sa ovim parametrima
                // Ako pretraživanje postoji od tuda uzimamo podatke umjesto sa servisa
                result = LowFareHelper.LoadFromDatabase(connection, origin, destination, departure, return_date, adults, currency);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    // Ako nema u bazi učitavamo sa API servisa
                    result = await LowFareHelper.LoadFromAPI(connection, origin, destination, departure, return_date, adults, currency);
                    if (result != null)
                    {
                        // Spremamo u bazu podatke
                        LowFareHelper.SaveToDatabase(connection, origin, destination, departure, return_date, adults, currency, result);

                        // Ako nema podataka vraćamo praznu listu i takvu
                        if (result == string.Empty)
                        {
                            return string.Empty;
                        }
                        else
                        {
                            return result;
                        }
                    }
                    else
                    {
                        // Dogodila se greška prilikom preuzimanja sa API-a
                        return "Error";
                    }
                }
            }
        }
        #endregion

        // GET: /Flight/GetAirportByKey
        #region GetAirportByKey(int key)
        public string GetAirportByKey(string key)
        {
            // Stvaramo objekt koji ćemo poslati klijentu
            List<dynamic> jsonList = new List<dynamic>();

            // Ako je key prazan vraćamo praznu listu
            if (string.IsNullOrEmpty(key))
            {
                return JsonConvert.SerializeObject(jsonList);
            }

            // Spajamo se na bazu
            using (SqlConnection connection = new SqlConnection(AppConfig.GetByKey("connectionString")))
            {
                connection.Open();

                // Učitavamo sve koji počinju sa traženom riječi
                // Učitavamo samo 25 rezultata kako bi korisnik bio precizniji prema odabiru
                using (SqlCommand command = new SqlCommand("SELECT TOP(25) [iata], [name] FROM [airport] WHERE [iata] LIKE @iata;", connection))
                {
                    // Dodajemo parametar
                    command.Parameters.AddWithValue("iata", key + "%");

                    // Izvršavamo naredbu
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Čitamo podatke i dodajemo ih u listu
                        while (reader.Read())
                        {
                            jsonList.Add(new
                            {
                                iata = (string)reader[0],
                                name = (string)reader[1]
                            });
                        }
                    }
                }
            }

            // Serijaliziramo rezultat u JSON i šaljemo klijentu
            return JsonConvert.SerializeObject(jsonList);
        }
        #endregion
    }
}