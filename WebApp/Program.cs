using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using WebApp.Codes;

namespace WebApp
{
    public class Program
    {
        #region Main(string[] args)
        public static void Main(string[] args)
        {
            // Dodajemo log4net konfiguraciju
            ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            ILog logger = LogManager.GetLogger(typeof(Program));
            logger.Info("Application starting...");

            // Dobivamo lokaciju direktorija gdje se nalazi konfiguracija
            string location = Assembly.GetExecutingAssembly().Location;
            string appRoot = Path.GetDirectoryName(location);

            // Učitavamo i build-amo konfiguraciju te ju šaljemo samo jednom u klasu
            // Na izmjeni vrijednosti u konfiguraciji potrebno je restartirati aplikaciju jer učitavamo samo jednom
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(appRoot)
                .AddJsonFile("appsettings.json")
                .Build();
            AppConfig c = new AppConfig(config);

            // Pokrećemo host s postavkama
            var host = new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
            .UseStartup<Startup>()
            .UseUrls("http://*:8030")
            .Build();

            host.Run();
        }
        #endregion
    }
}
