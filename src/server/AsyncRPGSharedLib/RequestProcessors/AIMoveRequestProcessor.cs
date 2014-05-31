using System;
using System.Collections.Generic;
using System.Linq;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class EntityPath
    {
        public int entity_id;
        public List<PathStep> path;
    }

    public class AIMoveRequestProcessor : RequestProcessor
    {
        // Processor Data
        private RoomKey m_roomKey;
        private World m_world;
        private Room m_room;
        private List<EnergyTank> m_energyTanks;
        private List<Player> m_players;
        private List<GameEventParameters> m_relevantGameEvents;
        private List<MobUpdateContext> m_mobContexts;
        private List<EntityPath> m_playerPaths;

        public AIMoveRequestProcessor(
            RoomKey roomKey)
        {
            m_roomKey = new RoomKey(roomKey);
            m_world = null;
            m_room = null;
            m_mobContexts = new List<MobUpdateContext>();
            m_relevantGameEvents = new List<GameEventParameters>();
            m_playerPaths = new List<EntityPath>();
        }

        public World World
        {
            get { return m_world; }
        }

        public Room Room
        {
            get { return m_room; }
        }

        public List<Player> Players
        {
            get { return m_players; }
        }

        public List<EnergyTank> EnergyTanks
        {
            get { return m_energyTanks; }
        }

        public List<GameEventParameters> AIRelevantEvents
        {
            get { return m_relevantGameEvents; }
        }

        public List<EntityPath> PlayerPaths
        {
            get { return m_playerPaths; }
        }

        public List<MobUpdateContext> MobContexts
        {
            get { return m_mobContexts; }
        }

        public void AddRelevantGameEvent(GameEventParameters gameEvent)
        {
            m_relevantGameEvents.Add(gameEvent);
        }

        public void AddRelevantGameEvents(List<GameEventParameters> gameEvents)
        {
            m_relevantGameEvents.AddRange(gameEvents);
        }

        override protected bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = true;
            result_code = SuccessMessages.GENERAL_SUCCESS;

            if (LookupEntities(requestCache) > 0)
            {
                success = LookupWorld(requestCache, m_roomKey.game_id, out m_world, out result_code);

                if (success)
                {
                    success = LookupRoom(requestCache, out result_code);
                }

                if (success)
                {
                    success = ComputePlayerPaths(out result_code);
                }

                if (success)
                {
                    ComputeMovesForMobs(requestCache);
                }
            }

            return success;
        }

        private int LookupEntities(
            RequestCache requestCache)
        {
            foreach (Mob mob in requestCache.GetMobs(m_roomKey))
            {
                m_mobContexts.Add(new MobUpdateContext(this, mob));
            }

            // Don't bother looking up anything else if there are no mobs
            if (m_mobContexts.Count > 0)
            {
                m_energyTanks = requestCache.GetEnergyTanks(m_roomKey).ToList();
                m_players = requestCache.GetPlayers(m_roomKey).ToList();
            }

            return m_mobContexts.Count;
        }

        private bool LookupRoom(
            RequestCache requestCache,
            out string result_code)
        {

            return m_world.GetRoom(requestCache.DatabaseContext, m_roomKey, out m_room, out result_code);
        }

        private bool ComputePlayerPaths(
            out string result_code)
        {
            PathComputer pathComputer = new PathComputer();
            bool success = true;

            result_code = SuccessMessages.GENERAL_SUCCESS;

            foreach (GameEventParameters gameEvent in m_relevantGameEvents)
            {
                if (gameEvent.GetEventType() == eGameEventType.character_moved)
                {
                    GameEvent_CharacterMoved movedEvent = (GameEvent_CharacterMoved)gameEvent;

                    success =
                        pathComputer.BlockingPathRequest(
                            m_room.runtime_nav_mesh,
                            m_room.room_key,
                            new Point3d((float)movedEvent.from_x, (float)movedEvent.from_y, (float)movedEvent.from_z),
                            new Point3d((float)movedEvent.to_x, (float)movedEvent.to_y, (float)movedEvent.to_z));

                    if (success)
                    {
                        m_playerPaths.Add(
                            new EntityPath()
                            {
                                entity_id = movedEvent.character_id,
                                path = pathComputer.FinalPath
                            });
                    }
                    else
                    {
                        result_code = ErrorMessages.CANT_COMPUTE_PATH;
                        break;
                    }
                }
            }

            return success;
        }

        private void ComputeMovesForMobs(
            RequestCache requestCache)
        {
            foreach (MobUpdateContext context in m_mobContexts)
            {
                // Compute the move for each mob
                context.ComputeMove();

                // Post events that occurred during the move 
                foreach (GameEventParameters game_event in context.output_game_events)
                {
                    GameEventQueries.AddEvent(requestCache.DatabaseContext, m_world.GameID, game_event);
                }
            }
        }
    }
}
