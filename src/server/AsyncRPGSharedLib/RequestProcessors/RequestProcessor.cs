using System;
using System.Data;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Web.Cache;
using AsyncRPGSharedLib.Web.Interfaces;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.RequestProcessors
{
    abstract public class RequestProcessor
    {
        private string m_connectionString;
        private int m_retryCount;

        public RequestProcessor()
        {
            m_connectionString = "";
            m_retryCount = 1;
        }

		public RequestProcessor(
			int retryCount)
		{
			m_connectionString = "";
			m_retryCount = retryCount+1;
		}

        public bool ProcessRequest(
            RequestCache requestCache,
            out string result_code)
        {
            return ProcessRequestInternal(requestCache, out result_code);
        }

        public bool ProcessRequest(
            string connection_string,
            ICacheAdapter sessionCache,
            out string result_code)
        {
            bool success = false;

            result_code = ErrorMessages.GENERAL_ERROR;

            // Remember the connection string we're using
            // so that request processors internal to this one
            // can use it as well
            m_connectionString = connection_string;

            while (!success && m_retryCount > 0)
            {
                System.Data.Common.DbTransaction dbTransaction = null;
                AsyncRPGDataContext dbContext = new AsyncRPGDataContext(connection_string);
                RequestCache requestCache = new RequestCache(sessionCache, dbContext);

                m_retryCount--;

                try
                {
                    // Open the db connection immediately
                    dbContext.Connection.Open();

                    // Create a transaction for the request processor.
                    // If anything fails along the way, roll back everything.
                    dbTransaction = dbContext.Connection.BeginTransaction(IsolationLevel.ReadCommitted);

                    // Tell the context about the transaction so that it doesn't create one of it's own
                    dbContext.Transaction = dbTransaction;

                    // Start off assuming success
                    result_code = SuccessMessages.GENERAL_SUCCESS;

                    // Attempt to process the request
                    if (ProcessRequestInternal(
                            requestCache,
                            out result_code))
                    {
                        // Save any modified objects back into the DB
                        requestCache.WriteDirtyObjectsToDatabase();

                        // Commit the transaction to the DB up success
                        dbTransaction.Commit();
                        dbTransaction = null;

                        success = true;
                    }
                    else
                    {
                        // Something failed (in a controlled manner)
                        dbTransaction.Rollback();
                        dbTransaction = null;
                    }
                }
                catch (System.Transactions.TransactionAbortedException ex)
                {
                    // Our attempt to rollback a failed transaction failed.
                    // Possible data corruption :(
                    result_code = ErrorMessages.DB_ERROR + "(Transaction Aborted:" + ex.Message + ")";
                    success = false;

                    // Don't bother retrying in this case
                    m_retryCount = 0; 
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

                    // Don't bother retrying in this case
                    m_retryCount = 0; 
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
            }

            return success;
        }

        abstract protected bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code);

        // Common Helpers
        protected string GetCurrentConnectionString()
        {
            return m_connectionString;
        }

        protected static bool LookupWorld(
            RequestCache requestCache,
            int game_id,
            out World world,
            out string result_code)
        {
            bool success;

            // Get the room data for the room that the player is currently in
            world = requestCache.GetWorld(game_id);

            if (world != null)
            {
                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }
            else
            {
                result_code = ErrorMessages.INVALID_WORLD;
                success = false;
            }

            return success;
        }

        protected bool LookupCharacterGameID(
            RequestCache requestCache,
            int character_id,
            out int game_id,
            out string result_code)
        {
            bool success;

            // Get the game that the character currently belongs to
            game_id = CharacterQueries.GetCharacterGameId(requestCache.DatabaseContext, character_id);

            if (game_id >= 0)
            {
                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }
            else
            {
                result_code = ErrorMessages.NOT_BOUND;
                success = false;
            }

            return success;
        }

        protected static bool PingCharacter(
            RequestCache requestCache,
            int character_id,
            out GameEvent[] event_list,
            out string result_code)
        {
            PingCharacterRequestProcessor pingProcessor = new PingCharacterRequestProcessor(character_id);
            bool success = pingProcessor.ProcessRequest(requestCache, out result_code);

            if (success)
            {
                result_code = SuccessMessages.GENERAL_SUCCESS;
                event_list = pingProcessor.ResultGameEvents;
            }
            else
            {
                event_list = new GameEvent[] { };
            }

            return success;
        }

        protected bool VerifyGameExists(
            RequestCache requestCache,
            int current_game_id,
            out string result_code)
        {
            bool success;

            if (GameQueries.VerifyGameExists(requestCache.DatabaseContext, current_game_id))
            {
                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }
            else
            {
                result_code = ErrorMessages.INVALID_GAME;
                success = false;
            }

            return success;
        }

    }
}
