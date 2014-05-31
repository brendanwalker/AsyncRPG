using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using System.Data;
using System.Data.Linq.Mapping;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Utility;
using System.Threading;

namespace AsyncRPGSharedLib.Database
{
    public class DatabaseManagerConfig
    {
        public string CONNECTION_STRING { get; private set; }
        public string MOBS_DIRECTORY { get; private set; }
        public string MAPS_DIRECTORY { get; private set; }

        public int APPLICATION_VERSION { get; private set; }
        public string APPLICATION_WEB_URL { get; private set; }
        public string APPLICATION_DEBUG_WEB_URL { get; private set; }
        public string ACCOUNT_CLIENT_URL { get; private set; }
        public string WEB_SERVICE_EMAIL_ADDRESS { get; private set; }
        public string WEB_SERVICE_EMAIL_HOST { get; private set; }
        public int WEB_SERVICE_EMAIL_PORT { get; private set; }
        public string WEB_SERVICE_EMAIL_USERNAME { get; private set; }
        public string WEB_SERVICE_EMAIL_PASSWORD { get; private set; }
        public string DEFAULT_IRC_SERVER { get; private set; }
        public int DEFAULT_IRC_PORT { get; private set; }

        public DatabaseManagerConfig(
            string connectionString,
            string mobsPath,
            string mapsPath)
        {
            this.CONNECTION_STRING = connectionString;
            this.MOBS_DIRECTORY = mobsPath;
            this.MAPS_DIRECTORY = mapsPath;

            // TODO: Get these from a Configuration Manager
            this.APPLICATION_VERSION= DatabaseConstants.APPLICATION_VERSION;
            this.APPLICATION_WEB_URL= AsyncRPGSharedLib.Common.ApplicationConstants.APPLICATION_WEB_URL;
            this.APPLICATION_DEBUG_WEB_URL= AsyncRPGSharedLib.Common.ApplicationConstants.APPLICATION_DEBUG_WEB_URL;
            this.ACCOUNT_CLIENT_URL= AsyncRPGSharedLib.Common.ApplicationConstants.ACCOUNT_CLIENT_URL;
            this.WEB_SERVICE_EMAIL_ADDRESS= MailConstants.WEB_SERVICE_EMAIL_ADDRESS;
            this.WEB_SERVICE_EMAIL_HOST= MailConstants.WEB_SERVICE_EMAIL_HOST;
            this.WEB_SERVICE_EMAIL_PORT= MailConstants.WEB_SERVICE_EMAIL_PORT;
            this.WEB_SERVICE_EMAIL_USERNAME= MailConstants.WEB_SERVICE_EMAIL_USERNAME;
            this.WEB_SERVICE_EMAIL_PASSWORD= MailConstants.WEB_SERVICE_EMAIL_PASSWORD;
            this.DEFAULT_IRC_SERVER= IrcConstants.DEFAULT_IRC_SERVER;
            this.DEFAULT_IRC_PORT= IrcConstants.DEFAULT_IRC_PORT;
        }
    }

    public class DatabaseManager
    {
        private delegate void DatabaseCommandDelegate(AsyncRPGDataContext context);

        private DatabaseManagerConfig m_config;
        private IDatabaseBuilder m_builder;

        public DatabaseManager(
            DatabaseManagerConfig config)
        {
            string[] keyValuePairs = config.CONNECTION_STRING.Split(new char[] { ';' });
            string vendorType = "SqlServer";

            foreach (string keyValuePair in keyValuePairs)
            {
                string[] tokens = keyValuePair.Split(new char[] { '=' });

                if (tokens.Length == 2)
                {
                    string key = tokens[0];
                    string value = tokens[1];

                    if (key == "DbLinqProvider")
                    {
                        vendorType = value;
                        break;
                    }
                }
            }

            if (vendorType == "Sqlite")
            {
                m_builder = new DatabaseBuilderSQLite();
            }
            else
            {
                // Default to SqlServer if no vendor type is given
                m_builder = new DatabaseBuilderMSSQL();
            }

            // Save off the config data
            m_config = config;
        }

        public bool IsDatabaseValid()
        {
            int dbVersion = ConfigQueries.GetDatabaseVersion(m_config.CONNECTION_STRING);
            bool databaseExists = (dbVersion == m_config.APPLICATION_VERSION);

            return databaseExists;
        }

        public bool CreateDatabase(
            Logger logger,
            out string result_code)
        {
            // Run the command on the database
            return ExecuteSQLCommand(
                new AsyncRPGDataContext(m_config.CONNECTION_STRING),
                (AsyncRPGDataContext context) =>
                {
                    // Create the database tables
                    string createSQL = CreateDatabaseSQL(context.Mapping);
                    context.ExecuteCommand(createSQL);

                    // Fill in the tables with some initial values
                    InitalizeTables(context, logger);
                },
                out result_code);
        }

        public bool DeleteDatabase(
            out string result_code)
        {
            return ExecuteSQLCommand(
                new AsyncRPGDataContext(m_config.CONNECTION_STRING),
                (AsyncRPGDataContext context) => 
                {
                    // Delete all of the tables
                    string deleteSQL= DeleteDatabaseSQL(context.Mapping);
                    context.ExecuteCommand(deleteSQL);
                },
                out result_code);
        }

        public bool ReCreateDatabase(
            Logger logger,
            out string result_code)
        {
            return ExecuteSQLCommand(
                new AsyncRPGDataContext(m_config.CONNECTION_STRING),
                (AsyncRPGDataContext context) => 
                {
                    // Delete and Re-create all tables
                    StringBuilder databaseSQL = new StringBuilder();

                    databaseSQL.Append(DeleteDatabaseSQL(context.Mapping));
                    databaseSQL.Append(CreateDatabaseSQL(context.Mapping));

                    context.ExecuteCommand(databaseSQL.ToString());

                    // Fill in the tables with some initial values
                    InitalizeTables(context, logger);
                },
                out result_code);
        }

        public void InitalizeTables(
            AsyncRPGDataContext dbContext,
            Logger logger)
        {
            // Save the server constants into the DB
            ConfigQueries.InitializeConfig(
                dbContext,
                m_config.APPLICATION_VERSION,
                m_config.APPLICATION_WEB_URL,
                m_config.APPLICATION_DEBUG_WEB_URL,
                m_config.ACCOUNT_CLIENT_URL,
                m_config.WEB_SERVICE_EMAIL_ADDRESS,
                m_config.WEB_SERVICE_EMAIL_HOST,
                m_config.WEB_SERVICE_EMAIL_PORT,
                m_config.WEB_SERVICE_EMAIL_USERNAME,
                m_config.WEB_SERVICE_EMAIL_PASSWORD,
                m_config.DEFAULT_IRC_SERVER,
                m_config.DEFAULT_IRC_PORT);

            // Create test accounts
            AccountQueries.CreateAccountNoEmailVerify(
                dbContext, 
                "test", 
                AccountQueries.ClientPasswordHash("password"), 
                "test@test.com", 
                DatabaseConstants.OpsLevel.player);
            AccountQueries.CreateAccountNoEmailVerify(
                dbContext, 
                "test2",
                AccountQueries.ClientPasswordHash("password"), 
                "test@test.com", 
                DatabaseConstants.OpsLevel.player);

            // Re-import the mob type JSON data into the DB
            {
                MobTypeImporter importer = new MobTypeImporter(logger);

                importer.ParseMobTypes(dbContext, m_config.MOBS_DIRECTORY + "/mob_types.json");
            }

            // Re-import the mob spawn tables JSON data into the DB (dependent on mob types)
            {
                MobSpawnTableImporter importer = new MobSpawnTableImporter(logger);

                importer.ParseMobSpawnTables(dbContext, m_config.MOBS_DIRECTORY + "/mob_spawn_tables.json");
            }

            // Re-import the room template XML into the DB (dependent on mob types and spawn tables)
            // This also does some import processing on the room templates (visibility, nav-mesh, etc).
            {
                RoomTemplateImporter importer = new RoomTemplateImporter(logger);

                importer.ParseRoomTemplates(dbContext, m_config.MAPS_DIRECTORY);
            }
        }

        private bool ExecuteSQLCommand(
            AsyncRPGDataContext dbContext,
            DatabaseCommandDelegate commandDelegate,
            out string result_code)
        {
            System.Data.Common.DbTransaction dbTransaction = null;
            bool success = false;

            try
            {
                // Open the db connection immediately
                dbContext.Connection.Open();

                // Create a transaction for the request processor.
                // If anything fails along the way, roll back everything.
                dbTransaction = dbContext.Connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

                // Tell the context about the transaction so that it doesn't create one of it's own
                dbContext.Transaction = dbTransaction;

                // Run the command on the database
                commandDelegate(dbContext);

                // Commit the transaction to the DB up success
                dbTransaction.Commit();
                dbTransaction = null;

                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }
            catch (System.Transactions.TransactionAbortedException ex)
            {
                // Our attempt to rollback a failed transaction failed.
                // Possible data corruption :(
                result_code = ErrorMessages.DB_ERROR + "(Transaction Aborted:" + ex.Message + ")";
                success = false;

            }
            catch (System.Exception ex)
            {
                // An unexpected error occurred
                // Attempt to rollback any db changes if any were made
                if (dbTransaction != null)
                {
                    dbTransaction.Rollback();
                    dbTransaction = null;
                }

                // Don't bother retrying in this case
                result_code = ex.Message;
                success = false;
            }
            finally
            {
                // In all cases, make sure to close the connection to the DB
                if (dbContext.Connection.State == ConnectionState.Open)
                {
                    dbContext.Connection.Close();
                }

                dbContext = null;
            }

            return success;
        }

        private string CreateDatabaseSQL(
            MetaModel model)
        {
            StringBuilder commandBuilder = new StringBuilder();

            commandBuilder.Append(m_builder.CreateDatabasePreambleSQL());

            // Append the SQL for deleting the tables
            foreach (MetaTable table in model.GetTables())
            {
                commandBuilder.Append(m_builder.CreateTableSQL(table));
            }

            return commandBuilder.ToString();
        }

        private string DeleteDatabaseSQL(
            MetaModel model)
        {
            StringBuilder commandBuilder = new StringBuilder();

            // Append the SQL for deleting the tables
            foreach (MetaTable table in model.GetTables())
            {
                commandBuilder.Append(m_builder.DeleteTableSQL(table));
                commandBuilder.Append("\r\n");
            }

            return commandBuilder.ToString();
        }
    }

    public class AsyncDatabaseInitializeRequest
    {
        private Logger m_logger;
        private DatabaseManager m_dbManager;
        private Thread m_thread;
        private ManualResetEvent m_exitedSignal;
        private bool m_succeeded;

        public AsyncDatabaseInitializeRequest(
            DatabaseManagerConfig config,
            Logger logger)
        {
            m_dbManager = new DatabaseManager(config);
            m_logger = logger;

            m_thread = null;
            m_exitedSignal = null;
            m_succeeded = false;
        }

        public void Execute()
        {
            if (m_thread == null)
            {
                m_succeeded = false;
                m_exitedSignal = new ManualResetEvent(false);

                m_thread = new Thread(() => { this.ProcessRequest(); });
                m_thread.Start();
            }
        }

        public bool IsStarted()
        {
            return m_thread != null;
        }

        public bool IsComplete()
        {
            bool completed = false;

            if (m_thread != null && m_exitedSignal == null)
            {
                completed = true;
            }
            else if (m_exitedSignal != null && m_exitedSignal.WaitOne(0))
            {
                m_exitedSignal = null;

                completed = true;
            }

            return completed;
        }

        public bool HasSucceeded()
        {
            return m_succeeded;
        }

        private void ProcessRequest()
        {
            try
            {
                if (!m_dbManager.IsDatabaseValid())
                {
                    string constructionResult = "";

                    m_logger.LogInfo("Database invalid. Re-initializing...");

                    if (m_dbManager.ReCreateDatabase(m_logger, out constructionResult))
                    {
                        // Recreated the DB
                        m_succeeded = true;
                    }
                    else
                    {
                        m_logger.LogError(constructionResult);
                    }
                }
                else
                {
                    // DB is already valid
                    m_succeeded = true;
                }
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex.Message);
                m_logger.LogError(ex.StackTrace);
            }
            finally
            {
                m_exitedSignal.Set();
            }
        }
    }
}
