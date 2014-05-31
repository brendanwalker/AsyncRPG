using System;
using System.Linq;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class CharacterDeleteRequestProcessor : RequestProcessor
    {
        // Request Data
        private int m_account_id;
        private int m_character_id;

        // Processor Data
        private CharacterState m_character_state;
        private int[] m_remaining_character_ids;

        public CharacterDeleteRequestProcessor(
            int account_id,
            int character_id)
        {
            m_account_id = account_id;
            m_character_id = character_id;

            m_character_state= null;
            m_remaining_character_ids = null;
        }

        public int[] RemainingCharacterIDs
        {
            get { return m_remaining_character_ids; }
        }        

        protected override bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = true;
            result_code = SuccessMessages.GENERAL_SUCCESS;

            success = LookupCharacterState(out result_code);

            if (success)
            {
                success = ClearPlayerFromCache(requestCache, out result_code);
            }

            if (success)
            {
                PostCharacterLeftGameEvent(requestCache);
                CharacterQueries.DeleteCharacter(requestCache.DatabaseContext, m_character_id);
                CharacterQueries.GetCharacterIDList(requestCache.DatabaseContext, m_account_id, out m_remaining_character_ids);
            }

            return success;
        }

        private bool LookupCharacterState(out string result_code)
        {
            bool success= 
                CharacterQueries.GetFullCharacterState(
                    this.GetCurrentConnectionString(),
                    m_character_id, 
                    out m_character_state, 
                    out result_code);

            result_code = success ? SuccessMessages.GENERAL_SUCCESS : ErrorMessages.DB_ERROR + "(Unable to retrieve character state)";

            return success;
        }

        private bool ClearPlayerFromCache(
            RequestCache requestCache,
            out string result_code)
        {
            bool success;

            if (m_character_state.game_id != -1)
            {
                World world = 
                    WorldCache.GetWorld(
                        requestCache.DatabaseContext,
                        requestCache.SessionCache, 
                        m_character_state.game_id);

                if (world != null)
                {
                    requestCache.RemovePlayer(
                        new RoomKey(
                            m_character_state.game_id,
                            m_character_state.room_x,
                            m_character_state.room_y,
                            m_character_state.room_z), 
                        m_character_id);

                    result_code = SuccessMessages.GENERAL_SUCCESS;
                    success = true;
                }
                else
                {
                    result_code = ErrorMessages.INVALID_WORLD;
                    success = false;
                }
            }
            else
            {
                // Character not bound to game, thus won't be in the cache
                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }

            return success;
        }

        private void PostCharacterLeftGameEvent(
            RequestCache requestCache)
        {
            GameEventParameters gameEvent =
                new GameEvent_CharacterLeftGame()
                {
                    character_state = m_character_state
                };

            // Add a game event if the player is moving to the portal
            GameEventQueries.AddEvent(
                requestCache.DatabaseContext,
                m_character_state.game_id, 
                gameEvent);
        }
    }
}
