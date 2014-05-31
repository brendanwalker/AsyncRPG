using System;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Protocol;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class BindCharacterToGameRequestProcessor : RequestProcessor
    {
        private enum eBindGameAction
        {
            leaveGame,
            joinGame,
        }

        // Request Data
        private int m_character_id;
        private int m_game_id;

        // Processor Data
        private CharacterState m_character_state;
        private int m_previous_game_id;

        // Result Data

        public BindCharacterToGameRequestProcessor(
            int character_id,
            int game_id)
        {
            m_character_id = character_id;
            m_game_id = game_id;

            m_character_state = null;
            m_previous_game_id = -1;
        }

        override protected bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = true;

            result_code = SuccessMessages.GENERAL_SUCCESS;
            success= VerifyGameExists(requestCache, m_game_id, out result_code);

            if (success)
            {
                TypedFlags<eBindGameAction> actionFlags = LookupCharacterData(requestCache);

                if (actionFlags.Test(eBindGameAction.leaveGame))
                {
                    PostCharacterLeftGameEvent(requestCache);
                    LeaveGame(requestCache);
                }

                if (actionFlags.Test(eBindGameAction.joinGame))
                {
                    PostCharacterJoinedGameEvent(requestCache);
                    JoinGame(requestCache);
                }
            }

            return success;
        }

        private TypedFlags<eBindGameAction> LookupCharacterData(
            RequestCache requestCache)
        {
            TypedFlags<eBindGameAction> actionFlags = new TypedFlags<eBindGameAction>();

            actionFlags.Clear();

            // Get the current state of the character before moving them to another game
            m_character_state = CharacterQueries.GetFullCharacterState(requestCache.DatabaseContext, m_character_id);
                
            // Get the previous game this character was a part of, if any
            m_previous_game_id = m_character_state.game_id;

            // Only bother doing anything if the character isn't already bound to this game
            if (m_previous_game_id != m_game_id)
            {
                if (m_previous_game_id != -1)
                {
                    actionFlags.Set(eBindGameAction.leaveGame, true);
                }

                actionFlags.Set(eBindGameAction.joinGame, true);
            }

            return actionFlags;
        }

        private void PostCharacterLeftGameEvent(
            RequestCache requestCache)
        {
            GameEventQueries.AddEvent(
                requestCache.DatabaseContext,
                m_previous_game_id,
                new GameEvent_CharacterLeftGame()
                {
                    character_state = m_character_state
                });
        }

        private void LeaveGame(
            RequestCache requestCache)
        {
            GameQueries.UnBindCharacterFromGame(requestCache.DatabaseContext, m_character_id);
        }

        private void PostCharacterJoinedGameEvent(
            RequestCache requestCache)
        {

            // Update the character state to match the new game
            m_character_state.game_id = m_game_id;
            m_character_state.game_name = GameQueries.GetGameName(requestCache.DatabaseContext, m_game_id);

            // Add a note to the game event log that this new character joined the new game
            GameEventQueries.AddEvent(
                requestCache.DatabaseContext,
                m_game_id,
                new GameEvent_CharacterJoinedGame()
                {
                    character_state = m_character_state
                });
        }

        private void JoinGame(
            RequestCache requestCache)
        {
            GameQueries.BindCharacterToGame(requestCache.DatabaseContext, m_character_id, m_game_id);
        }
    }
}
