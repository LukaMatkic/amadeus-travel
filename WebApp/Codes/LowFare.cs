using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Codes
{
    /// <summary>
    /// Glavna klasa u kojoj se nalaze letovi
    /// </summary>
    public class LowFare
    {
        /// <summary>
        /// Valuta
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// Lista rezultata
        /// </summary>
        public List<Result> results { get; set; }
    }

    /// <summary>
    /// Rezultati odgovora
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Letovi
        /// </summary>
        public List<Itinerary> itineraries { get; set; }
        /// <summary>
        /// Informacije o cijeni leta
        /// </summary>
        public Fare fare { get; set; }
    }

    /// <summary>
    /// Informacije 
    /// </summary>
    public class Fare
    {
        /// <summary>
        /// Ukupna cijena leta
        /// </summary>
        public string total_price { get; set; }
    }

    /// <summary>
    /// Letovi
    /// </summary>
    public class Itinerary
    {
        /// <summary>
        /// Informacije odlaznog leta
        /// </summary>
        public FlightData outbound { get; set; }
        /// <summary>
        /// Informacije dolaznog leta
        /// </summary>
        public FlightData inbound { get; set; }
    }

    /// <summary>
    /// Klasa za inforormacije o letovima u odlaznom ili dolaznom smjeru
    /// </summary>
    public class FlightData
    {
        /// <summary>
        /// Lista letova
        /// </summary>
        public List<Flight> flights { get; set; }
    }

    /// <summary>
    /// Informacije o letu
    /// </summary>
    public class Flight
    {
        /// <summary>
        /// Vrijeme polaska
        /// </summary>
        public string departs_at { get; set; }
        /// <summary>
        /// Vrijeme dolaska
        /// </summary>
        public string arrives_at { get; set; }
        /// <summary>
        /// Polazišni aerodrom
        /// </summary>
        public Airport origin { get; set; }
        /// <summary>
        /// Destinacijski aerodrom
        /// </summary>
        public Airport destination { get; set; }
        /// <summary>
        /// Informacije o bookingu
        /// </summary>
        public BookingInfo booking_info { get; set; }
    }

    /// <summary>
    /// Informacije o aerodromu
    /// </summary>
    public class Airport
    {
        /// <summary>
        /// IATA šifra aerodroma
        /// </summary>
        public string airport { get; set; }
    }

    /// <summary>
    /// Informacije o bookingu
    /// </summary>
    public class BookingInfo
    {
        /// <summary>
        /// Broj preostalih sjedala u letu
        /// </summary>
        public int seats_remaining { get; set; }
    }
}
