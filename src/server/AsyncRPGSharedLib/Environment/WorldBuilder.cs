using System;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;

namespace AsyncRPGSharedLib.Environment
{
    public enum eWorldSize
    {
        small,
        medium,
        large
    }

    public class WorldBuilder
    {
        private RoomTemplateSet m_roomTemplateSet;
        private MobTypeSet m_mobTypeSet;
        private MobSpawnTableSet m_mobSpawnTableSet;

        public WorldBuilder()
        {
            m_roomTemplateSet = new RoomTemplateSet();
            m_mobTypeSet = new MobTypeSet();
            m_mobSpawnTableSet = new MobSpawnTableSet();
        }

        public RoomTemplateSet RoomTemplates
        {
            get { return m_roomTemplateSet; }
        }

        public MobTypeSet MobTypes
        {
            get { return m_mobTypeSet; }
        }

        public MobSpawnTableSet MobSpawnTables
        {
            get { return m_mobSpawnTableSet; }
        }

        public void Initialize(AsyncRPGDataContext db_context)
        {
            m_roomTemplateSet.Initialize(db_context);
            m_mobTypeSet.Initialize(db_context);
            m_mobSpawnTableSet.Initialize(db_context, m_mobTypeSet);
        }

        public bool BuildWorld(
            AsyncRPGDataContext db_context,
            int game_id, 
            out World world, 
            out string result)
        {
            bool success = true;
            WorldTemplate worldTemplate = null;
            DungeonLayout layout = null;

            world = null;
            result = SuccessMessages.GENERAL_SUCCESS;

            // Get the world generation parameters from the DB
            worldTemplate =
                WorldQueries.GetWorldGenerationParameters(
                    db_context,
                    game_id);

            // Create the initial set of rooms for the world
            if (success)
            {
                layout = new DungeonLayout(game_id, worldTemplate, m_roomTemplateSet, m_mobSpawnTableSet);

                if (!layout.BuildRoomLayout(out result))
                {
                    success = false;
                }
            }

            // Copy the rooms and portals from the layout into the world
            if (success)
            {
                world = new World(this, layout.LayoutWorldTemplate, game_id);
                world.ApplyLayout(layout);
            }

            // TODO: WorldBuilder: Generate the environment objects, etc

            // Save the cached world into the database
            if (success)
            {
                WorldQueries.InsertWorld(db_context, world);
            }

            return success;
        }

        public World LazyLoadWorld(
            AsyncRPGDataContext db_context,
            int game_id)
        {
            World world = 
                new World(
                    this, 
                    WorldQueries.GetWorldGenerationParameters(db_context, game_id), 
                    game_id);

            return world;
        }
    }
}