using System;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class MoveRequestProcessor : RequestProcessor
    {
        // Request Data
        private int m_character_id;
        private Point3d m_target_position;
        private float m_target_angle = 0;

        // Processor Data
        private World m_world;
        private Room m_room;
        private RoomKey m_current_room_key;
        private Point3d m_current_character_position;
        private float m_current_character_angle;
        private int m_game_id;
        private List<GameEventParameters> m_ai_relevant_events;

        // Result Data
        private GameEvent[] m_result_event_list;

        public MoveRequestProcessor(
            int character_id,
            Point3d target_position,
            float target_angle)
        {
            m_character_id = character_id;
            m_target_position = new Point3d(target_position);
            m_target_angle = target_angle;

            m_world = null;
            m_room = null;
            m_current_room_key = null;
            m_current_character_position = new Point3d();
            m_current_character_angle = 0.0f;
            m_game_id = -1;
            m_ai_relevant_events = new List<GameEventParameters>();

            m_result_event_list = null;
        }

        public GameEvent[] ResultEventList
        {
            get { return m_result_event_list; }
        }

        protected override bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = true;

            result_code = SuccessMessages.GENERAL_SUCCESS;

            CharacterQueries.GetCharacterPosition(
                    requestCache.DatabaseContext,
                    m_character_id,
                    out m_current_room_key,
                    out m_current_character_position,
                    out m_current_character_angle);

            success = LookupCharacterGameID(requestCache, m_character_id, out m_game_id, out result_code);

            if (success)
            {
                success = LookupWorld(requestCache, m_game_id, out m_world, out result_code);
            }

            if (success)
            {
                success = m_world.GetRoom(requestCache.DatabaseContext, m_current_room_key, out m_room, out result_code);
            }

            if (success)
            {
                success = VerifyNavMeshConnectivity(out result_code);
            }

            if (success)
            {
                success = UpdateCharacterPosition(requestCache, out result_code);
            }

            if (success)
            {
                PostCharacterMovedEvent(requestCache);
                success = ComputeAIResponse(requestCache, out result_code);
            }

            if (success)
            {
                success = PingCharacter(requestCache, m_character_id, out m_result_event_list, out result_code);

            }

            return success;
        }

        private bool VerifyNavMeshConnectivity(out string result_code)
        {
            bool success;

            if (Point3d.DistanceSquared(m_current_character_position, m_target_position) > MathConstants.POSITIONAL_EPSILON_SQUARED)
            {
                // Player has to walk to the portal target
                NavRef characterNavRef = m_room.runtime_nav_mesh.ComputeNavRefAtPoint(m_current_character_position);
                NavRef targetNavRef = m_room.runtime_nav_mesh.ComputeNavRefAtPoint(m_target_position);

                if (m_room.runtime_nav_mesh.AreNavRefsConnected(characterNavRef, targetNavRef))
                {
                    result_code = SuccessMessages.GENERAL_SUCCESS;
                    success = true;
                }
                else
                {
                    // It isn't possible to reach the target on the nav mesh from where the player is standing
                    result_code = ErrorMessages.TARGET_UNREACHABLE;
                    success = false;
                }
            }
            else
            {
                // Character already on top of the portal
                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }

            return success;
        }

        private void PostCharacterMovedEvent(
            RequestCache requestCache)
        {
            GameEventParameters gameEvent= 
                new GameEvent_CharacterMoved()
                    {
                        character_id = m_character_id,
                        room_x = m_current_room_key.x,
                        room_y = m_current_room_key.y,
                        room_z = m_current_room_key.z,
                        from_x = m_current_character_position.x,
                        from_y = m_current_character_position.y,
                        from_z = m_current_character_position.z,
                        from_angle = m_current_character_angle,
                        to_x = m_target_position.x,
                        to_y = m_target_position.y,
                        to_z = m_target_position.z,
                        to_angle = m_target_angle
                    };

            // Add a game event if the player is moving to the portal
            GameEventQueries.AddEvent(requestCache.DatabaseContext, m_game_id, gameEvent);
            m_ai_relevant_events.Add(gameEvent);
        }

        private bool UpdateCharacterPosition(
            RequestCache requestCache,
            out string result_code)
        {
            bool success;

            // Update the players position to the new position in the new room
            Player player = requestCache.GetPlayer(m_current_room_key, m_character_id);

            if (player != null)
            {
                player.Position= m_target_position;
                player.Angle = m_target_angle;

                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }
            else
            {
                result_code = ErrorMessages.CACHE_ERROR + "(Failed to update character position)";
                success = false;
            }

            return success;
        }

        private bool ComputeAIResponse(
            RequestCache requestCache,
            out string result_code)
        {
            AIMoveRequestProcessor aiMoveRequest = new AIMoveRequestProcessor(m_current_room_key);

            aiMoveRequest.AddRelevantGameEvents(m_ai_relevant_events);

            return aiMoveRequest.ProcessRequest(requestCache, out result_code);
        }
    }
}