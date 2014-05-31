using System;
using System.Web;
using AsyncRPGSharedLib.Common;
using System.Configuration;

namespace AsyncRPGWebService
{
    public class WebUtilities
    {
        public static void InitializeConstants()
        {
            // Set the optional e-mail account constants
            {
                int emailPort = 0;

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
            ApplicationConstants.IsDebuggingEnabled = HttpContext.Current.IsDebuggingEnabled;
        }
    }
}