using System;
using System.Text;
using System.Configuration;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Web;
using AsyncRPGSharedLib.Web.Modules;
using AsyncRPGSharedLib.Database;
using System.Threading;

namespace AsyncRPGWebServer
{
    class Program
    {
        private static bool haltSignal = false;

        static void Main(string[] args)
        {
            Logger logger = new Logger(Console.WriteLine);

            InitializeConstants();

            // Only start up the server if the database is setup correctly
            if (InitializeDatabase(logger))
            {
                // Spin waiting for a termination signal
                RunWebServer(logger);
            }
        }

        private static void InitializeConstants()
        {
            bool isDebugging= false;

            if (bool.TryParse(ConfigurationManager.AppSettings["Debug"], out isDebugging))
            {
                ApplicationConstants.IsDebuggingEnabled = isDebugging;
            }

            // Set the optional e-mail account constants
            {
                int emailPort= 0;

                if (ConfigurationManager.AppSettings["emailAddress"] != null)
                {
                    MailConstants.WEB_SERVICE_EMAIL_ADDRESS =
                        ConfigurationManager.AppSettings["emailAddress"];
                }

                if (ConfigurationManager.AppSettings["emailHost"] != null)
                {
                    MailConstants.WEB_SERVICE_EMAIL_HOST =
                        ConfigurationManager.AppSettings["emailHost"];
                }

                if (ConfigurationManager.AppSettings["emailPort"] != null &&
                    int.TryParse(ConfigurationManager.AppSettings["emailPort"], out emailPort))
                {
                    MailConstants.WEB_SERVICE_EMAIL_PORT = emailPort;
                }

                if (ConfigurationManager.AppSettings["emailUsername"] != null)
                {
                    MailConstants.WEB_SERVICE_EMAIL_USERNAME =
                        ConfigurationManager.AppSettings["emailUsername"];
                }

                if (ConfigurationManager.AppSettings["emailPassword"] != null)
                {
                    MailConstants.WEB_SERVICE_EMAIL_PASSWORD =
                        ConfigurationManager.AppSettings["emailPassword"];
                }
            }

            // Set the relevant source data directories
            ApplicationConstants.MAPS_DIRECTORY = ConfigurationManager.AppSettings["MapDataDirectory"];
            ApplicationConstants.MOBS_DIRECTORY = ConfigurationManager.AppSettings["MobDataDirectory"];

            // Set the DB connection string based on the build configuration
            string connectionName = ConfigurationManager.AppSettings["DBConnectionString"];
            ApplicationConstants.CONNECTION_STRING=
                ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;

            // Set |DataDirectory| value
            AppDomain.CurrentDomain.SetData(
                "DataDirectory",
                AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["DatabaseDataDirectory"]);
        }

        private static bool InitializeDatabase(
            Logger logger)
        {
            DatabaseManagerConfig dbConfig =
                new DatabaseManagerConfig(
                    ApplicationConstants.CONNECTION_STRING,
                    ApplicationConstants.MOBS_DIRECTORY,
                    ApplicationConstants.MAPS_DIRECTORY);
            DatabaseManager dbManager = new DatabaseManager(dbConfig);
            bool databaseValid = false;

            if (!dbManager.IsDatabaseValid())
            {
                string constructionResult = "";

                logger.LogInfo("Database invalid. Re-initializing...");

                if (dbManager.ReCreateDatabase(logger, out constructionResult))
                {
                    databaseValid = true;
                }
                else
                {
                    logger.LogError(constructionResult);
                }
            }
            else
            {
                databaseValid = true;
            }

            return databaseValid;
        }

        private static void RunWebServer(
            Logger logger)
        {
            // Initialize the web server
            WebServer server =
                new WebServer(
                    ConfigurationManager.AppSettings["UriAddress"],
                    logger,
                    new Type[] {
                        typeof(AccountModule),
                        typeof(AdminModule),
                        typeof(CharacterModule),
                        typeof(EnergyTankModule),
                        typeof(GameModule),
                        typeof(WorldModule)
                    });

            // Start listening
            server.Start();

            // Register a handler 
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs cancelEventArgs)
            {
                cancelEventArgs.Cancel = true;
                Program.haltSignal = true;
            };

            // Spin waiting for a signal to terminate the server
            while (!Program.haltSignal) 
            {
                Thread.Sleep(500);
            }

            // Clean up the web server
            server.Stop();
        }
    }
}
