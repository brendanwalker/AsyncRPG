using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;

namespace AsyncRPGSharedLib.Queries
{
    public class ConfigQueries
    {
        public class DBTableKeyIterator
        {
            private string m_connection_string;
            private string m_table_name;
            private string m_key_column_name;
            private int m_key_block_size;
            private int m_last_key;
            private List<int> m_keys;
            private bool m_failed;

            public DBTableKeyIterator(
                string connection_string,
                string table_name,
                string key_column_name,
                int key_block_size)
            {
                m_connection_string = connection_string;
                m_table_name = table_name;
                m_key_column_name = key_column_name;
                m_key_block_size = key_block_size;
                m_last_key = -1;
                m_keys = new List<int>();
                m_failed = false;

                Next();
            }

            public List<int> GetKeys()
            {
                return m_keys;
            }

            public bool GetFailed()
            {
                return m_failed;
            }

            public bool Valid()
            {
                return m_keys.Count > 0;
            }

            public void Next()
            {
                SqlConnection dbConnection = new SqlConnection(m_connection_string);

                m_keys.Clear();

                try
                {
                    dbConnection.Open();

                    string commandString =
                        string.Format(
                            "SELECT TOP {0} {1} FROM {2} WHERE {1} > {3} ORDER BY {1} ASC",
                            m_key_block_size,
                            m_key_column_name,
                            m_table_name,
                            m_last_key);

                    SqlCommand dbCommand = new SqlCommand(commandString, dbConnection);
                    SqlDataReader reader = dbCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        m_keys.Add(reader.GetInt32(0));
                    }

                    reader.Close();
                }
                catch (System.Exception)
                {
                    m_failed = true;
                    m_keys.Clear();	
                }
                finally
                {
                    dbConnection.Close();
                }
                
                if (m_keys.Count > 0)
                {
                    m_last_key = m_keys[m_keys.Count - 1];
                }
            }
        }

        // Database Version Table
        //-----------------------
        private delegate bool DBUpgradeCallback(
            SqlConnection dbConnection,
            SqlTransaction dbTransaction,
            out string error_code);

        private class DBVersionEntry
        {
            public int from_version;
            public int to_version;
            public DBUpgradeCallback updateToVersionCallback;

            public DBVersionEntry(int from_version, int to_version, DBUpgradeCallback callback)
            {
                this.from_version = from_version;
                this.to_version = to_version;
                this.updateToVersionCallback= callback;
            }
        }

        // Add new database upgrading functions here
        private static DBVersionEntry[] versioningTable = new DBVersionEntry[] {
        };


        // Database Config Interface
        //--------------------------

        public static void InitializeConfig(
            AsyncRPGDataContext context,
            int version,
            string appUrl,
            string appDebugUrl,
            string clientUrl,
            string emailAddress,
            string emailHost,
            int emailPort,
            string emailUsername,
            string emailPassword,
            string ircServer,
            int ircPort)
        {
            Config config = new Config
            {
                Version = version,
                AppURL = appUrl,
                AppDebugURL = appDebugUrl,
                ClientURL = clientUrl,
                EmailAddress = emailAddress,
                EmailHost = emailHost,
                EmailPort = emailPort,
                EmailUserName = emailUsername,
                EmailPassword = emailPassword,
                IrcServer = ircServer,
                IrcPort = ircPort
            };

            context.Configs.InsertOnSubmit(config);
            context.SubmitChanges();
        }

        public static int GetDatabaseVersion(
            string connection_string)
        {
            int version = -1;

            try
            {
                AsyncRPGDataContext context = new AsyncRPGDataContext(connection_string);

                version = context.Configs.Take(1).Select(c => c.Version).SingleOrDefault();
            }
            catch (System.Exception)
            {
                version = -1;
            }

            return version;
        }

        public static string UpgradeDatabase(
            string connection_string)
        {
            string result_code = "";

            int current_db_version = GetDatabaseVersion(connection_string);

            // If out database is old we need to upgrade it until we reach the most recent version
            if (current_db_version < DatabaseConstants.APPLICATION_VERSION)
            {
                SqlConnection dbConnection = new SqlConnection(connection_string);

                // Walk through the entire version table
                // but only do work when we encounter an entry for upgrading our version
                foreach (DBVersionEntry entry in versioningTable)
                {
                    bool success = true;

                    if (current_db_version == entry.from_version &&
                        entry.to_version <= DatabaseConstants.APPLICATION_VERSION)
                    {
                        SqlTransaction dbTransaction = null;

                        try
                        {
                            // Open and close the connection per version function to close any active data reader
                            dbConnection.Open();

                            // Transaction our upgrades so that if anything breaks we can rollback
                            dbTransaction = dbConnection.BeginTransaction("UpgradeDatabase");

                            // Attempt to upgrade the database
                            if (entry.updateToVersionCallback(dbConnection, dbTransaction, out result_code))
                            {
                                // Upon upgrade success, update the version id in the config table
                                // and commit the changes
                                SetDatabaseVersion(dbConnection, dbTransaction, entry.to_version);
                                current_db_version = entry.to_version;

                                dbTransaction.Commit();
                            }
                            else
                            {
                                dbTransaction.Rollback();
                                success = false;
                            }

                            dbTransaction = null;
                            dbConnection.Close();

                        }
                        catch (System.Exception)
                        {
                            try
                            {
                                // If anything throws an exception in the upgrade
                                // rollback to what we had before the upgrade
                                if (dbTransaction != null)
                                {
                                    dbTransaction.Rollback();
                                }
                            }
                            catch (System.Exception)
                            {
                                //Utilities.LogError(
                                //    string.Format("Unable to rollback failed upgrade ({0}->{1})",
                                //                entry.from_version, entry.to_version));
                            }

                            result_code = ErrorMessages.DB_ERROR;
                            success = false;
                        }
                    }

                    if (!success)
                    {
                        break;
                    }
                }
            }
            // Don't attempt anything if the database is newer than our web-service
            else if (current_db_version > DatabaseConstants.APPLICATION_VERSION)
            {
                result_code = string.Format("Database at newer version:{0} than webservice version:{1}. Old webservice?", 
                    current_db_version,
                    DatabaseConstants.APPLICATION_VERSION);
            }
            // Don't attempt anything if we're already at the latest version
            else
            {
                result_code = string.Format("Database already at latest version {0}", DatabaseConstants.APPLICATION_VERSION); 
            }

            return result_code;
        }

        // Private Helper Functions
        //------------------------
        private static bool SetDatabaseVersion(
            SqlConnection dbConnection,
            SqlTransaction dbTransaction,
            int version)
        {
            SqlParameter dbVersionParameter = new SqlParameter("@version", SqlDbType.Int);
            dbVersionParameter.Direction = ParameterDirection.Input;
            dbVersionParameter.Value = version;

            SqlCommand dbCommand = new SqlCommand(@"UPDATE config SET version=@version", dbConnection, dbTransaction);
            dbCommand.Parameters.Add(dbVersionParameter);

            return dbCommand.ExecuteNonQuery() > 0;
        }

        // DB Versioning Functions
        //------------------------
    }
}