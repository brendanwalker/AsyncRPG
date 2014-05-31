using System;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class PingCharacterRequestProcessor : RequestProcessor
    {
        // Request Data        
        private int m_character_id;

        // Processor Data
        private int m_current_game_id;
        private bool m_has_new_events;
        private GameEvent[] m_game_events;

        public PingCharacterRequestProcessor(
            int character_id)
        {
            m_character_id = character_id;

            m_current_game_id= -1;
            m_has_new_events= false;
            m_game_events= null;
        }

        public GameEvent[] ResultGameEvents
        {
            get { return m_game_events; }
        }

        protected override bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = true;

            result_code = SuccessMessages.GENERAL_SUCCESS;

            CharacterQueries.UpdateCharacterLastPingTime(requestCache.DatabaseContext, m_character_id);

            m_has_new_events = CharacterQueries.GetCharacterNewEventFlag(requestCache.DatabaseContext, m_character_id);

            if (m_has_new_events)
            {
                success = LookupCharacterGameId(requestCache, out result_code);

                if (success)
                {
                    success = VerifyGameExists(requestCache, m_current_game_id, out result_code);
                }

                if (success)
                {
                    LookupCharacterRecentEvents(requestCache);
                }
            }

            return success;
        }

        private bool LookupCharacterGameId(
            RequestCache requestCache,
            out string result_code)
        {
            bool success;

            m_current_game_id= CharacterQueries.GetCharacterGameId(requestCache.DatabaseContext, m_character_id);

            if (m_current_game_id != -1)
            {
                result_code= SuccessMessages.GENERAL_SUCCESS;
                success= true;
            }
            else
            {
                result_code= ErrorMessages.INVALID_GAME;
                success= false;
            }

            return success;
        }

        private void LookupCharacterRecentEvents(
            RequestCache requestCache)
        {
            int new_last_game_event_id = -1;
            int last_game_event_id = 
                CharacterQueries.GetCharacterLastEventId(
                    requestCache.DatabaseContext, m_character_id);

            CharacterQueries.ClearCharacterNewEventFlag(
                requestCache.DatabaseContext, m_character_id);

            GameEventQueries.GetGameEventsAfterId(
                        requestCache.DatabaseContext,
                        m_current_game_id,
                        last_game_event_id,
                        out m_game_events,
                        out new_last_game_event_id);

            if (m_has_new_events && new_last_game_event_id != last_game_event_id)
            {
                CharacterQueries.UpdateCharacterLastEventId(
                    requestCache.DatabaseContext, m_character_id, new_last_game_event_id);
            }
        }
    }
}
