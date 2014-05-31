using System;
using System.Xml;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Utility;

namespace AsyncRPGSharedLib.Environment.AI
{
    public class MobSpawnerTemplate
    {
        private int m_id;
        private Point3d m_position;
        private string m_mob_spawn_table;
        private int m_max_spawn_count;

        public MobSpawnerTemplate(XmlNode xmlNode)
        {
            int pixel_x = Int32.Parse(xmlNode.Attributes["x"].Value);
            int pixel_y = Int32.Parse(xmlNode.Attributes["y"].Value);

            m_id = Int32.Parse(xmlNode.Attributes["id"].Value);
            m_mob_spawn_table = xmlNode.Attributes["mob_spawn_table"].Value;
            m_max_spawn_count = Int32.Parse(xmlNode.Attributes["spawn_count"].Value);

            m_position = GameConstants.ConvertPixelPositionToRoomPosition(pixel_x, pixel_y);
        }

        public string SpawnTableName
        {
            get { return m_mob_spawn_table; }
        }

        public int MaxSpawnCount
        {
            get { return m_max_spawn_count; }
        }

        public Point3d Position
        {
            get { return m_position; }
        }
    }

    public class MobSpawner
    {
        private RoomKey m_room_key;
        private int m_mob_spawner_id;
        private Point3d m_position;
        private MobSpawnTable m_spawn_table;
        private int m_remaining_spawn_count;
        private int m_random_seed;

        public MobSpawner()
        {
            m_room_key = new RoomKey();
            m_mob_spawner_id = -1;
            m_position = new Point3d();
            m_spawn_table = null;
            m_remaining_spawn_count = 0;
            m_random_seed = 0;
        }

        public static MobSpawner CreateMobSpawner(RoomKey roomKey, MobSpawnerTemplate template, MobSpawnTableSet spawnTableSet, Random rng)
        {
            MobSpawner newMobSpawner = new MobSpawner();

            newMobSpawner.m_room_key = new RoomKey(roomKey);
            newMobSpawner.m_mob_spawner_id = -1; // spawner ID not set until this gets saved into the DB
            newMobSpawner.m_position = new Point3d(template.Position);
            newMobSpawner.m_remaining_spawn_count = RNGUtilities.RandomInt(rng, 0, template.MaxSpawnCount);
            newMobSpawner.m_random_seed = rng.Next();
            newMobSpawner.m_spawn_table = spawnTableSet.GetMobSpawnTableByName(template.SpawnTableName);

            return newMobSpawner;
        }

        public static MobSpawner CreateMobSpawner(MobSpawners dbMobSpawner, MobSpawnTableSet spawnTableSet)
        {
            MobSpawner newMobSpawner = new MobSpawner();

            newMobSpawner.m_room_key = new RoomKey(dbMobSpawner.GameID, dbMobSpawner.RoomX, dbMobSpawner.RoomY, dbMobSpawner.RoomZ);
            newMobSpawner.m_mob_spawner_id = dbMobSpawner.MobSpawnerID;
            newMobSpawner.m_position = new Point3d((float)dbMobSpawner.X, (float)dbMobSpawner.Y, (float)dbMobSpawner.Z);
            newMobSpawner.m_remaining_spawn_count = dbMobSpawner.RemainingSpawnCount;
            newMobSpawner.m_random_seed = dbMobSpawner.RandomSeed;
            newMobSpawner.m_spawn_table = spawnTableSet.GetMobSpawnTableByID(dbMobSpawner.MobSpawnerTableID);

            return newMobSpawner;
        }

        public int ID
        {
            get { return m_mob_spawner_id; }
            set { m_mob_spawner_id = value; }
        }

        public RoomKey RoomKey
        {
            get { return m_room_key; }
        }

        public Point3d Position
        {
            get { return m_position; }
        }

        public int RemainingSpawnCount
        {
            get { return m_remaining_spawn_count; }
            set { m_remaining_spawn_count = value; }
        }

        public MobSpawnTable SpawnTable
        {
            get { return m_spawn_table; }
        }

        public int RandomSeed
        {
            get { return m_random_seed; }
            set { m_random_seed = value; }
        }

        public Mob SpawnRandomMob()
        {
            Mob result = null;

            if (RemainingSpawnCount > 0)
            {
                Random rng = new Random(m_random_seed);
                MobType mobType = m_spawn_table.PickRandomMobType(rng);

                m_random_seed = rng.Next();
                m_remaining_spawn_count = Math.Max(m_remaining_spawn_count - 1, 0);

                result = Mob.CreateMob(this, mobType);
            }

            return result;
        }
    }
}
