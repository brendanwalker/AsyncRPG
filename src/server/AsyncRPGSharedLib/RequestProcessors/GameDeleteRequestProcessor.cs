using System;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class GameDeleteRequestProcessor : RequestProcessor
    {
        // Request Data
        private int m_account_id;
        private int m_game_id;

        public GameDeleteRequestProcessor(
            int account_id,
            int game_id)
        {
            m_account_id = account_id;
            m_game_id = game_id;
        }

        protected override bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = false;
            
            result_code = SuccessMessages.GENERAL_SUCCESS;

            if (GameQueries.VerifyAccountOwnsGame(requestCache.DatabaseContext, m_account_id, m_game_id))
            {
                GameQueries.DeleteGame(requestCache.DatabaseContext, m_game_id);
                WorldCache.ClearWorld(requestCache.SessionCache, m_game_id);
                success = true;
            }
            else
            {
                result_code = ErrorMessages.NOT_GAME_OWNER;
            }

            return success;
        }
    }
}
