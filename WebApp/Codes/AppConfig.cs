using Microsoft.Extensions.Configuration;
using System;

namespace WebApp.Codes
{
    public class AppConfig
    {
        /// <summary>
        /// Memorija za konfiguraciju
        /// </summary>
        private static IConfiguration config { get; set; }

        
        #region AppConfig(IConfiguration _config)
        /// <summary>
        /// Konstruktor klase za konfiguraciju
        /// </summary>
        /// <param name="_config"></param>
        public AppConfig(IConfiguration _config)
        {
            config = _config;
        }
        #endregion


        #region GetByKey(string key)
        /// <summary>
        /// Funkcija za dobiti vrijednost iz key-a
        /// </summary>
        /// <param name="key"></param>
        /// <param name="allowNull">
        /// Ovo označava dali dopuštamo učitavanje null vrijednosti, u nekim slučajema vrijednost će biti null pa treba dopustiti.
        /// U večini slučajeva ukoliko je vrijednost null radi se o krivom/nepostojećem ključu pa tako stvaramo i funkciju
        /// </param>
        /// <returns></returns>
        public static string GetByKey(string key, bool allowNull = false)
        {
            // Ako je ključ prazan bacamo exception jer nema smisla tražiti
            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Parameter 'key' can not be empty !");
            }
                
            // Učitavamo vrijednost ključa
            string value = config[key];

            // Provjeravamo dali je vrijednost null i ako je dopušteno da vrijednost bude null (objašnjeno iznad u parametru 'allowNull')
            if(value == null && allowNull == false)
            {
                // Bacamo exception jer je ključ nepostojeć
                throw new ArgumentException(
                    String.Format("Key '{0}' does not exist in configuration. Set 'allowNull' to 'true' if 'null' is intended value !", key)
                    );
            }
            else
            {
                // Sve je u redu, vraćamo rezultat
                return value;
            }
        }
        #endregion
    }
}
