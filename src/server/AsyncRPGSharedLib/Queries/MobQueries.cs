using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Navigation;

namespace AsyncRPGSharedLib.Queries
{
    public class MobQueries
    {
        public static bool LoadMobTypes(
            string connectionString,
            out Dictionary<int, MobType> mobTypes,
            out string result)
        {
            bool success;

            try
            {
                AsyncRPGDataContext context = new AsyncRPGDataContext(connectionString);

                mobTypes= LoadMobTypes(context);

                success = true;
                result = SuccessMessages.GENERAL_SUCCESS;
            }
            catch (System.Exception ex)
            {
                mobTypes = new Dictionary<int, MobType>();

                success = false;
                result = ex.Message;
            }

            return success;
        }
        
        public static Dictionary<int, MobType> LoadMobTypes(
            AsyncRPGDataContext db_context)
        {
            var db_mob_types = from m in db_context.MobTypes select m;

            Dictionary<int, MobType> mobTypes = new Dictionary<int, MobType>();

            foreach (MobTypes db_mob_type in db_mob_types)
            {
                MobType mob_type = new MobType(db_mob_type);

                mobTypes.Add(mob_type.ID, mob_type);
            }

            return mobTypes;
        }

        public static bool LoadMobSpawnTables(
            string connectionString,
            MobTypeSet mobTypeSet,
            out Dictionary<int, MobSpawnTable> spawnTables,
            out string result)
        {
            bool success;

            try
            {
                AsyncRPGDataContext context = new AsyncRPGDataContext(connectionString);

                spawnTables = LoadMobSpawnTables(context, mobTypeSet);
                success = true;
                result = SuccessMessages.GENERAL_SUCCESS;
            }
            catch (System.Exception ex)
            {
                spawnTables = new Dictionary<int, MobSpawnTable>();
                success = false;
                result = ex.Message;
            }

            return success;
        }

        public static Dictionary<int, MobSpawnTable> LoadMobSpawnTables(
            AsyncRPGDataContext context,
            MobTypeSet mobTypeSet)
        {
            Dictionary<int, MobSpawnTable> spawnTables = new Dictionary<int, MobSpawnTable>();
            var db_spawn_tables = from t in context.MobSpawnTables select t;

            foreach (MobSpawnTables db_spawn_table in db_spawn_tables)
            {
                var db_spawn_table_entries =
                    from e in context.MobSpawnTableEntries
                    where e.MobSpawnTableID == db_spawn_table.MobSpawnTableID
                    select e;

                MobSpawnTable spawnTable =
                    new MobSpawnTable(
                        spawnTables,
                        mobTypeSet,
                        db_spawn_table,
                        db_spawn_table_entries.ToList());

                spawnTables.Add(spawnTable.ID, spawnTable);
            }

            return spawnTables;
        }

        public static List<MobSpawner> GetMobSpawners(
            AsyncRPGDataContext context,
            MobSpawnTableSet mobSpawnTableSet,
            RoomKey roomKey)
        {
            List<MobSpawner> mobSpawners = new List<MobSpawner>();

            var roomMobSpawnerQuery =
                from s in context.MobSpawners
                where s.GameID == roomKey.game_id && s.RoomX == roomKey.x && s.RoomY == roomKey.y && s.RoomZ == roomKey.z
                select s;

            foreach (MobSpawners dbMobSpawner in roomMobSpawnerQuery)
            {
                MobSpawner mobSpawner = MobSpawner.CreateMobSpawner(dbMobSpawner, mobSpawnTableSet);

                mobSpawners.Add(mobSpawner);
            }

            return mobSpawners;
        }

        public static void UpdateMobSpawner(
            AsyncRPGDataContext db_context,
            MobSpawner spawner)
        {
            var roomMobSpawnerQuery =
                from s in db_context.MobSpawners
                where s.MobSpawnerID == spawner.ID
                select s;

            foreach (MobSpawners dbMobSpawner in roomMobSpawnerQuery)
            {
                dbMobSpawner.RandomSeed = spawner.RandomSeed;
                dbMobSpawner.RemainingSpawnCount = spawner.RemainingSpawnCount;
            }

            db_context.SubmitChanges();
        }

        public static void InsertMobs(
            AsyncRPGDataContext db_context,
            RoomKey roomKey,
            List<Mob> mobs)
        {
            foreach (Mob mob in mobs)
            {
                Mobs dbMob = new Mobs
                {
                    MobTypeID = mob.MobType.ID,
                    GameID = mob.RoomKey.game_id,
                    RoomX = mob.RoomKey.x,
                    RoomY = mob.RoomKey.y,
                    RoomZ = mob.RoomKey.z,
                    X = mob.Position.x,
                    Y = mob.Position.y,
                    Z = mob.Position.z,
                    Angle = mob.Angle,
                    Health = mob.Health,
                    Energy = mob.Energy,
                    AiData = Mob.SerializeAIData(mob.AIState)
                };

                db_context.Mobs.InsertOnSubmit(dbMob);
                db_context.SubmitChanges();

                mob.ID = dbMob.MobID;
            }
        }

        public static void UpdateMob(
            AsyncRPGDataContext db_context,
            Mob mob)
        {
            var dbMob =
                (from m in db_context.Mobs
                 where m.MobID == mob.ID
                 select m).Single();

            dbMob.MobTypeID = mob.MobType.ID;
            dbMob.GameID = mob.RoomKey.game_id;
            dbMob.RoomX = mob.RoomKey.x;
            dbMob.RoomY = mob.RoomKey.y;
            dbMob.RoomZ = mob.RoomKey.z;
            dbMob.X = mob.Position.x;
            dbMob.Y = mob.Position.y;
            dbMob.Z = mob.Position.z;
            dbMob.Angle = mob.Angle;
            dbMob.Health = mob.Health;
            dbMob.Energy = mob.Energy;
            dbMob.AiData = Mob.SerializeAIData(mob.AIState);

            db_context.SubmitChanges();
        }

        public static List<Mob> GetMobs(
            AsyncRPGDataContext db_context,
            MobTypeSet mobTypes,
            RoomKey roomKey)
        {
            List<Mob> mobs = new List<Mob>();

            var roomMobQuery =
                from m in db_context.Mobs
                where m.GameID == roomKey.game_id && m.RoomX == roomKey.x && m.RoomY == roomKey.y && m.RoomZ == roomKey.z
                select m;

            foreach (Mobs dbMob in roomMobQuery)
            {
                Mob mob = Mob.CreateMob(dbMob, mobTypes);

                mobs.Add(mob);
            }

            return mobs;
        }

        public static Mob GetMob(
            AsyncRPGDataContext db_context,
            MobTypeSet mobTypes,
            int mobID)
        {
            return
                Mob.CreateMob(
                    (from m in db_context.Mobs
                     where m.MobID == mobID
                     select m).Single(), mobTypes);
        }
    }
}
