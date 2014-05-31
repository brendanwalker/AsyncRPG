using System;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Web.Interfaces;

namespace AsyncRPGSharedLib.Environment
{
    public class WorldBuilderCache
    {
        public static WorldBuilder GetWorldBuilder(
            AsyncRPGDataContext db_context,
            ICacheAdapter cache)
        {
            WorldBuilder worldBuilder = (WorldBuilder)cache["world_builder"];

            if (worldBuilder == null)
            {
                worldBuilder = new WorldBuilder();
                worldBuilder.Initialize(db_context);

                cache["world_builder"] = worldBuilder;
            }

            return worldBuilder;
        }

        public static void ClearWorldBuilder(ICacheAdapter cache)
        {
            cache.Remove("world_builder");
        }
    }

    public class WorldCache
    {
        public static World GetWorld(
            AsyncRPGDataContext db_context, 
            ICacheAdapter cache, 
            int game_id)
        {
            string world_identifier = GetWorldCacheIdentifier(game_id);
            World world = (World)cache[world_identifier];

            if (world == null)
            {
                world = WorldBuilderCache.GetWorldBuilder(db_context, cache).LazyLoadWorld(db_context, game_id);

                cache[world_identifier] = world;
            }

            return world;
        }

        public static bool BuildWorld(
            AsyncRPGDataContext db_context, 
            ICacheAdapter cache, 
            int game_id, 
            out string result)
        {
            World world = null;

            bool success =
                WorldBuilderCache.GetWorldBuilder(db_context, cache).BuildWorld(db_context, game_id, out world, out result);

            if (success)
            {
                string world_identifier = GetWorldCacheIdentifier(game_id);

                cache[world_identifier] = world;
            }

            return success;
        }

        public static void ClearWorld(ICacheAdapter cache, int game_id)
        {
            string world_identifier = GetWorldCacheIdentifier(game_id);

            cache.Remove(world_identifier);
        }

        public static string GetWorldCacheIdentifier(int game_id)
        {
            return string.Format("world_{0}", game_id);
        }
    }
}