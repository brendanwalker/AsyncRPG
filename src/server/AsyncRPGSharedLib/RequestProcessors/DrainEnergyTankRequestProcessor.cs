using System;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class DrainEnergyTankRequestProcessor : RequestProcessor
    {
        // Request Data
        private int m_character_id;
        private int m_energy_tank_id = -1;

        // Processor Data
        private World m_world;
        private Room m_room;
        private EnergyTank m_energy_tank;
        private int m_energy_tank_old_energy;
        private RoomKey m_current_room_key;
        private Point3d m_current_character_position;
        private int m_current_character_energy;
        private float m_current_character_angle;
        private int m_game_id;
        private bool m_move_to_target;
        private List<GameEventParameters> m_ai_relevant_events;

        // Result Data
        private GameEvent[] m_result_event_list;

        public DrainEnergyTankRequestProcessor(
            int character_id,
            int energy_tank_id)
        {
            m_character_id = character_id;
            m_energy_tank_id = energy_tank_id;

            m_world = null;
            m_room = null;
            m_energy_tank = null;
            m_energy_tank_old_energy = 0;
            m_current_room_key = null;
            m_current_character_position = new Point3d();
            m_current_character_energy = 0;
            m_current_character_angle = 0.0f;
            m_game_id = -1;
            m_move_to_target = false;
            m_ai_relevant_events = new List<GameEventParameters>();

            m_result_event_list = null;
        }

        public GameEvent[] ResultEventList
        {
            get { return m_result_event_list; }
        }

        override protected bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = true;
            bool is_touching_energy_tank = false;

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
                m_current_character_energy = CharacterQueries.GetCharacterEnergy(requestCache.DatabaseContext, m_character_id);
                success = LookupWorld(requestCache, m_game_id, out m_world, out result_code);
            }

            if (success)
            {
                success = m_world.GetRoom(requestCache.DatabaseContext, m_current_room_key, out m_room, out result_code);
            }

            if (success)
            {
                // Goes to verifyNavMeshConnectivity if the character needs to move to reach the energy tank,
                // Otherwise skips to updateEnergyTankState
                // (Or failure if the energy tank can't be hacked)
                success = VerifyEnergyTank(requestCache, out is_touching_energy_tank, out result_code);
            }

            if (success)
            {
                success = VerifyNavMeshConnectivity(out result_code);

                if (success)
                {
                    UpdateCharacterState(requestCache, out result_code);
                }

                if (success)
                {
                    PostCharacterMovedEvent(requestCache);
                }
            }

            if (success)
            {
                success = UpdateEnergyTankState(requestCache, out result_code);
            }

            if (success)
            {
                PostEnergyTankDrainedEvent(requestCache);
                success = ComputeAIResponse(requestCache, out result_code);
            }

            if (success)
            {
                success = PingCharacter(requestCache, m_character_id, out m_result_event_list, out result_code);
            }

            return success;
        }

        private bool VerifyEnergyTank(
            RequestCache requestCache,
            out bool out_is_touching,
            out string result_code)
        {
            bool success = false;

            out_is_touching = false;

            // Make sure the portal actually exists in the room the player is in
            m_energy_tank = requestCache.GetEnergyTank(m_current_room_key, m_energy_tank_id);

            if (m_energy_tank != null)
            {
                // Remember how much energy the tank had before we drained it
                m_energy_tank_old_energy = m_energy_tank.Energy;

                switch (m_energy_tank.Faction)
                {
                    case GameConstants.eFaction.player:
                        result_code = SuccessMessages.GENERAL_SUCCESS;
                        success = true;
                        break;
                    case GameConstants.eFaction.ai:
                    case GameConstants.eFaction.neutral:
                        result_code = ErrorMessages.ENERGY_TANK_NOT_PLAYER_OWNED;
                        success = false;
                        break;
                    default:
                        result_code = ErrorMessages.DB_ERROR+"(Invalid energy tank faction)";
                        success = false;
                        break;
                }

                if (success && m_energy_tank.Energy <= 0)
                {
                    result_code = ErrorMessages.ENERGY_TANK_EMPTY;
                    success = false;
                }

                if (success)
                {
                    float min_distance_squared= WorldConstants.ROOM_TILE_SIZE*WorldConstants.ROOM_TILE_SIZE;
                    float distance = Point3d.DistanceSquared(m_current_character_position, m_energy_tank.Position);

                    out_is_touching = distance <= min_distance_squared;
                }
            }
            else
            {
                result_code = ErrorMessages.INVALID_REQUEST;
            }

            return success;
        }

        private bool VerifyNavMeshConnectivity(out string result_code)
        {
            bool success;

            // Player has to walk to the portal target
            NavRef characterNavRef = m_room.runtime_nav_mesh.ComputeNavRefAtPoint(m_current_character_position);
            NavRef targetNavRef = m_room.runtime_nav_mesh.ComputeNavRefAtPoint(m_energy_tank.Position);

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

            return success;
        }

        private bool UpdateCharacterState(
            RequestCache requestCache,
            out string result_code)
        {
            bool success;

            // Update the players position to the new position in the room
            Player player = requestCache.GetPlayer(m_room.room_key, m_character_id);

            if (player != null)
            {
                player.Position.Set(m_energy_tank.Position);
                player.Angle = MathConstants.GetAngleForDirection(MathConstants.eDirection.down);
                player.Energy = m_current_character_energy + m_energy_tank_old_energy;

                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }
            else
            {
                result_code = ErrorMessages.CACHE_ERROR + "(Failed to update character position and energy)";
                success = false;
            }

            return success;
        }

        private void PostCharacterMovedEvent(
            RequestCache requestCache)
        {
            if (m_move_to_target)
            {
                GameEventParameters gameEvent =
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
                            to_x = m_energy_tank.Position.x,
                            to_y = m_energy_tank.Position.y,
                            to_z = m_energy_tank.Position.z,
                            to_angle = MathConstants.GetAngleForDirection(MathConstants.eDirection.down)
                        };

                // Add a game event if the player is moving to the portal
                GameEventQueries.AddEvent(requestCache.DatabaseContext, m_game_id, gameEvent);
                m_ai_relevant_events.Add(gameEvent);
            }
        }

        private bool UpdateEnergyTankState(
            RequestCache requestCache,
            out string result_code)
        {
            bool success= false;

            EnergyTank energyTank = requestCache.GetEnergyTank(m_current_room_key, m_energy_tank_id);

            if (energyTank != null)
            {
                energyTank.Energy = 0;

                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }
            else
            {
                result_code = ErrorMessages.CACHE_ERROR+"(Unable to update energy tank state)";
                success = false;
            }  

            return success;
        }

        private void PostEnergyTankDrainedEvent(
            RequestCache requestCache)
        {
            GameEventParameters gameEvent=
                new GameEvent_EnergyTankDrained()
                    {
                        energy_tank_id = m_energy_tank_id,
                        drainer_id = m_character_id,
                        drainer_faction = GameConstants.eFaction.player,
                        energy_drained = m_energy_tank_old_energy
                    };

            // Add a game event if the player is moving to the portal
            GameEventQueries.AddEvent(requestCache.DatabaseContext, m_game_id, gameEvent);
            m_ai_relevant_events.Add(gameEvent);
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
