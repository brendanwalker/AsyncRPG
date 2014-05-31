using System;
using System.Collections.Generic;
using System.Linq;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class SpawnMobsRequestProcessor : RequestProcessor
    {
        // Processor Data
        private World m_world;
        private Room m_room;
        private int m_max_spawn_count;
        private int m_current_spawn_count;
        private List<MobSpawner> m_chosenMobSpawners;
        private List<Mob> m_newMobs;

        public SpawnMobsRequestProcessor(
            World world,
            Room room)
        {
            m_world = world;
            m_room = room;
            m_chosenMobSpawners = new List<MobSpawner>();
            m_newMobs = new List<Mob>();
        }

        protected override bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = true;

            result_code = SuccessMessages.GENERAL_SUCCESS;

            if (DetermineSpawnCount(requestCache))
            {
                success = ChooseMobSpawners(requestCache, out result_code);

                if (success)
                {
                    UpdateRoomRandomSeed(requestCache);
                    SpawnMobs(requestCache);
                    UpdateMobSpawners(requestCache);
                    PostSpawnedMobEvents(requestCache);
                }
            }

            return success;
        }

        private bool DetermineSpawnCount(
            RequestCache requestCache)
        {
            m_max_spawn_count = m_world.WorldTemplate.MaxSpawnsPerRoomEntry;
            m_current_spawn_count = requestCache.GetMobs(m_room.room_key).Count();

            return m_current_spawn_count < m_max_spawn_count;
        }

        private bool ChooseMobSpawners(
            RequestCache requestCache,
            out string result_code)
        {
            bool success= true;
            int desiredSpawnCount = m_max_spawn_count - m_current_spawn_count;

            result_code = SuccessMessages.GENERAL_SUCCESS;

            // Compute a list of all the occupied nav cells
            List<int> occupiedNavCells =
                requestCache.GetMobs(m_room.room_key).Select(m => m_room.runtime_nav_mesh.ComputeNavRefAtPoint(m.Position).NavCellIndex).ToList();

            // Only use mob spawners that have a non zero remaining spawn count
            // and don't have a mob currently standing over stop of it.
            List<MobSpawner> availableMobSpawners = 
                (from s in m_room.mobSpawners
                where s.RemainingSpawnCount > 0 && 
                        !occupiedNavCells.Contains(m_room.runtime_nav_mesh.ComputeNavRefAtPoint(s.Position).NavCellIndex)
                select s).ToList<MobSpawner>();

            // Shuffle the list and pick the top N spawners (where N is based on difficulty)
            if (availableMobSpawners.Count > 0)
            {
                RNGUtilities.DeterministicKnuthShuffle(m_room.random_seed, availableMobSpawners);

                for (int spawnerIndex = 0; 
                    spawnerIndex < availableMobSpawners.Count && spawnerIndex < desiredSpawnCount; 
                    spawnerIndex++)
                {
                    m_chosenMobSpawners.Add(availableMobSpawners[spawnerIndex]);
                }
            }

            return success;
        }

        private void UpdateRoomRandomSeed(
            RequestCache requestCache)
        {
            Random random = new Random(m_room.random_seed);

            m_room.random_seed = random.Next();

            // Save the random seed back to the room since we just burned a seed
            // REVIEW: UpdateRoomRandomSeed - Not sure if this will be determinism issues if another user enters the room
            // around the same time since this isn't updated as a transaction
            WorldQueries.UpdateRoomRandomSeed(
                    requestCache.DatabaseContext,
                    m_room.room_key,
                    m_room.random_seed);
        }

        private void SpawnMobs(
            RequestCache requestCache)
        {
            foreach (MobSpawner spawner in m_chosenMobSpawners)
            {
                Mob newMob= spawner.SpawnRandomMob();

                // Locally keep track of all the mobs we created for the next steps
                m_newMobs.Add(newMob);
            }

            // Save the mobs into the DB
            MobQueries.InsertMobs(requestCache.DatabaseContext, m_room.room_key, m_newMobs);

            // Add the mob to the cached room data now that is has a valid mob id
            foreach (Mob newMob in m_newMobs)
            {
                requestCache.AddMob(newMob);
            }
        }

        private void UpdateMobSpawners(
            RequestCache requestCache)
        {
            foreach (MobSpawner spawner in m_chosenMobSpawners)
            {
                MobQueries.UpdateMobSpawner(requestCache.DatabaseContext, spawner);
            }
        }

        private void PostSpawnedMobEvents(
            RequestCache requestCache)
        {
            // Add a game event for every new mob
            foreach (Mob mob in m_newMobs)
            {
                GameEventQueries.AddEvent(
                    requestCache.DatabaseContext,
                    m_room.room_key.game_id,
                    new GameEvent_MobSpawned()
                    {
                        mob_state = new MobState()
                        {
                            mob_id = mob.ID,
                            mob_type_name = mob.MobType.Name,
                            health = mob.Health,
                            energy = mob.Energy,
                            game_id = mob.RoomKey.game_id,
                            room_x = mob.RoomKey.x,
                            room_y = mob.RoomKey.y,
                            room_z = mob.RoomKey.z,
                            x = mob.Position.x,
                            y = mob.Position.y,
                            z = mob.Position.z,
                            angle = mob.Angle
                        }
                    });
            }
        }
    }
}
