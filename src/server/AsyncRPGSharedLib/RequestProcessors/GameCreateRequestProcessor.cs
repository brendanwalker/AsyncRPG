using System;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class GameCreateRequestProcessor : RequestProcessor
    {
        // Request Data
        private int m_account_id;
        private string m_game_name;
        private GameConstants.eDungeonSize m_dungeon_size;
        private GameConstants.eDungeonDifficulty m_dungeon_difficulty;
        private bool m_irc_enabled;
        private string m_irc_server;
        private int m_irc_port;
        private bool m_irc_encryption_enabled;

        // Processor Data
        private int m_new_game_id;

        public GameCreateRequestProcessor(
            int account_id,
            string game_name,
            GameConstants.eDungeonSize dungeon_size,
            GameConstants.eDungeonDifficulty dungeon_difficulty,
            bool irc_enabled,
            string irc_server,
            int irc_port,
            bool irc_encryption_enabled)
        {
            m_account_id = account_id;
            m_game_name= game_name;
            m_dungeon_size= dungeon_size;
            m_dungeon_difficulty = dungeon_difficulty;
            m_irc_enabled= irc_enabled;
            m_irc_server= irc_server;
            m_irc_port= irc_port;
            m_irc_encryption_enabled= irc_encryption_enabled;

            m_new_game_id = -1;
        }

        protected override bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = false;
            result_code = SuccessMessages.GENERAL_SUCCESS;

            m_new_game_id =
                GameQueries.CreateGame(
                    requestCache.DatabaseContext,
                    m_account_id,
                    m_game_name,
                    m_dungeon_size,
                    m_dungeon_difficulty,
                    m_irc_enabled,
                    m_irc_server,
                    m_irc_port,
                    m_irc_encryption_enabled);

            if (m_new_game_id >= 0)
            {
                success= 
                    WorldCache.BuildWorld(
                        requestCache.DatabaseContext, 
                        requestCache.SessionCache, 
                        m_new_game_id, 
                        out result_code);

                if (!success)
                {
                    GameQueries.DeleteGame(requestCache.DatabaseContext, m_new_game_id);
                }
            }

            return success;
        }
    }
}
