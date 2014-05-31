using System;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class PortalRequestProcessor : RequestProcessor
    {
        // Request Data
        private int m_character_id;
        private Point3d m_portal_position;
        private float m_portal_angle = 0;
        private int m_portal_id = -1;

        // Processor Data
        private World m_world;
        private Room m_room;
        private Room m_opposing_room;
        private Portal m_portal;
        private Portal m_opposing_portal;
        private Point3d m_opposing_portal_position;
        private RoomKey m_current_room_key;
        private RoomKey m_opposing_room_key;
        private Point3d m_current_character_position;
        private float m_current_character_angle;
        private int m_game_id;
        private bool m_move_to_target;

        // Result Data
        private GameEvent[] m_result_event_list;

        public PortalRequestProcessor(
            int character_id,
            Point3d portal_position,
            float portal_angle,
            int portal_id)
        {
            m_character_id = character_id;
            m_portal_position = new Point3d(portal_position);
            m_portal_angle = portal_angle;
            m_portal_id = portal_id;

            m_world = null;
            m_room = null;
            m_opposing_room = null;
            m_portal = null;
            m_opposing_portal = null;
            m_opposing_portal_position = new Point3d();
            m_current_room_key = null;
            m_opposing_room_key = null;
            m_current_character_position = new Point3d();
            m_current_character_angle = 0.0f;
            m_game_id = -1;
            m_move_to_target = false;

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

            success= LookupCharacterGameID(requestCache, m_character_id, out m_game_id, out result_code);

            if (success)
            {
                success= LookupWorld(requestCache, m_game_id, out m_world, out result_code);
            }

            if (success)
            {
                success= m_world.GetRoom(requestCache.DatabaseContext, m_current_room_key, out m_room, out result_code);
            }

            if (success)
            {
                success= LookupPortal(out result_code);
            }

            if (success)
            {
                success= VerifyNavMeshConnectivity(out result_code);
            }

            if (success)
            {
                success= LookupOpposingPortal(requestCache, out result_code);
            }

            if (success)
            {
                success= LookupOpposingRoom(requestCache, out result_code);
            }

            if (success)
            {
                PostCharacterMovedEvent(requestCache);
                success= UpdateCharacterPosition(requestCache, out result_code);
            }

            if (success)
            {
                success= SpawnMobs(requestCache, out result_code);
            }

            if (success)
            {
                PostCharacterPortaledEvent(requestCache);
                success= PingCharacter(requestCache, m_character_id, out m_result_event_list, out result_code);
            }

            return success;
        }

        private bool LookupPortal(out string result_code)
        {
            bool success;

            // Make sure the portal actually exists in the room the player is in
            m_portal = m_room.portals.Find(p => p.portal_id == m_portal_id);

            if (m_portal != null)
            {
                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }
            else
            {
                result_code = ErrorMessages.INVALID_REQUEST;
                success = false;
            }

            return success;
        }

        private bool VerifyNavMeshConnectivity(out string result_code)
        {
            bool success;

            if (!m_portal.bounding_box.ContainsPoint(m_portal_position))
            {
                // Target position given not in the portal
                result_code = ErrorMessages.NOT_IN_PORTAL_BOUNDS;
                success= false;
            }
            else if (Point3d.DistanceSquared(m_current_character_position, m_portal_position) > MathConstants.POSITIONAL_EPSILON_SQUARED)
            {
                // Player has to walk to the portal target
                NavRef characterNavRef = m_room.runtime_nav_mesh.ComputeNavRefAtPoint(m_current_character_position);
                NavRef targetNavRef = m_room.runtime_nav_mesh.ComputeNavRefAtPoint(m_portal_position);

                if (m_room.runtime_nav_mesh.AreNavRefsConnected(characterNavRef, targetNavRef))
                {
                    // It's possible to traverse the nav mesh to get to the target
                    m_move_to_target = true;

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

        private bool LookupOpposingPortal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success;

            // If the opposing portal doesn't exist yet,
            // create a new room adjacent to the current room with an opposing portal
            if (m_portal.target_portal_id != -1)
            {
                //If an opposing portal exists, retrieve it from the DB
                m_opposing_portal= 
                    WorldQueries.GetPortal(
                        requestCache.DatabaseContext,
                        m_portal.target_portal_id);
                m_opposing_room_key =
                    new RoomKey(
                        m_game_id, 
                        m_opposing_portal.room_x,
                        m_opposing_portal.room_y, 
                        m_opposing_portal.room_z);

                // Compute our target position in the new room
                // Clamp the players position into the bounding box of the portal in the new room
                m_opposing_portal_position = m_opposing_portal.bounding_box.ClipPoint(m_portal_position);

                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;    
            }
            else
            {
                result_code = ErrorMessages.CACHE_ERROR;
                success = false;
            }

            return success;
        }

        private bool LookupOpposingRoom(
            RequestCache requestCache,
            out string result_code)
        {
            bool success;

            if (m_world.GetRoom(requestCache.DatabaseContext,m_opposing_room_key, out m_opposing_room, out result_code))
            {
                success = true;
            }
            else
            {
                success = false;
            }

            return success;
        }

        private void PostCharacterMovedEvent(
            RequestCache requestCache)
        {
            if (m_move_to_target)
            {
                // Add a game event if the player is moving to the portal
                GameEventQueries.AddEvent(
                    requestCache.DatabaseContext,
                    m_game_id,
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
                        to_x = m_portal_position.x,
                        to_y = m_portal_position.y,
                        to_z = m_portal_position.z,
                        to_angle = m_portal_angle
                    });
            }
        }

        private bool UpdateCharacterPosition(
            RequestCache requestCache,
            out string result_code)
        {
            bool success= true;

            // Update the players position to the new position in the new room
            if (requestCache.MovePlayer(
                    m_current_room_key, 
                    m_opposing_room_key,
                    m_opposing_portal_position,
                    m_portal_angle,
                    m_character_id))
            {
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

        private bool SpawnMobs(
            RequestCache requestCache,
            out string result_code)
        {
            // Spawn mobs in anticipation of our arrival.
            // We do this before the portal event so that the client filters out the mob spawn
            // events because they aren't in the same room yet. Once they arrive in the room,
            // they'll request the room data and get the newly spawned mobs.
            // Other clients who happen to be in the room already will see new mobs spawn
            // and then the other player arrive.
            SpawnMobsRequestProcessor spawnMobsRequestProcessor = new SpawnMobsRequestProcessor(m_world, m_opposing_room);

            return spawnMobsRequestProcessor.ProcessRequest(requestCache, out result_code);
        }

        private void PostCharacterPortaledEvent(
            RequestCache requestCache)
        {
            // Add a game event that the player used a portal 
            GameEventQueries.AddEvent(
                requestCache.DatabaseContext,
                m_game_id,
                new GameEvent_CharacterPortaled()
                {
                    character_id = m_character_id,
                    from_room_x = m_current_room_key.x,
                    from_room_y = m_current_room_key.y,
                    from_room_z = m_current_room_key.z,
                    from_x = m_current_character_position.x,
                    from_y = m_current_character_position.y,
                    from_z = m_current_character_position.z,
                    from_angle = m_current_character_angle,
                    to_room_x = m_opposing_room_key.x,
                    to_room_y = m_opposing_room_key.y,
                    to_room_z = m_opposing_room_key.z,
                    to_x = m_opposing_portal_position.x,
                    to_y = m_opposing_portal_position.y,
                    to_z = m_opposing_portal_position.z,
                    to_angle = m_portal_angle
                });
        }
    }
}
