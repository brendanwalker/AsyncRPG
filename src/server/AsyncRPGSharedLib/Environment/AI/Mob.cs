using System;
using System.Xml;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Environment.AI.Behavior;
using AsyncRPGSharedLib.RequestProcessors;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using LitJson;

namespace AsyncRPGSharedLib.Environment.AI
{
    [Serializable]
    public class MobAIState
    {
        public MobAIBehaviorState behavior_data;
        public MobAIPerceptionState perception_data;

        public MobAIState()
        {
            behavior_data = new MobAIBehaviorState();
            perception_data = new MobAIPerceptionState();
        }

        public MobAIState(MobSpawner spawner)
        {
            behavior_data = new MobAIBehaviorState(spawner);
            perception_data = new MobAIPerceptionState();
        }
    }

    public class Mob : CacheableObject, IEnvironmentEntity
    {
        private RoomKey m_room_key;
        private int m_mob_id;
        private MobType m_mob_type;
        private Point3d m_position;
        private float m_angle;
        private int m_health;
        private int m_energy;

        // REVIEW: Performance 
        // Consider storing the ai state sub structures here instead.
        // When they get updated, mark them with a dirty flag.
        // When saving to the DB, only save the dirty fields to the DB.
        private MobAIState m_ai_data;

        public Mob()
        {
            m_room_key = new RoomKey();
            m_mob_id= -1;
            m_position = new Point3d();
            m_angle = 0f;
            m_mob_type = null;
            m_health= 0;
            m_energy= 0;
            m_ai_data = null;
        }

        public int ID
        {
            get { return m_mob_id; }
            set { m_mob_id = value; }
        }

        public GameConstants.eFaction Faction
        {
            get { return GameConstants.eFaction.ai; }
        }

        public MobType MobType
        {
            get { return m_mob_type; }
        }

        public RoomKey RoomKey
        {
            get { return m_room_key; }
        }

        public Point3d Position
        {
            get { return m_position; }
            set { m_position = value; IsDirty = true; }
        }

        public float Angle
        {
            get { return m_angle; }
            set { m_angle = value; IsDirty = true; }
        }

        public int Health
        {
            get { return m_health; }
            set { m_health = value; IsDirty = true; }
        }

        public int Energy
        {
            get { return m_energy; }
            set { m_energy = value; IsDirty = true; }
        }

        public MobAIState AIState
        {
            get { return m_ai_data; }
        }

        public static Mob CreateMob(MobSpawner spawner, MobType mobType)
        {
            Mob mob = new Mob();

            mob.m_room_key = new RoomKey(spawner.RoomKey);
            mob.m_mob_id= -1; // mob ID not set until this gets saved into the DB
            mob.m_mob_type= mobType;
            mob.m_position = new Point3d(spawner.Position);
            mob.m_angle = 0f; //TODO: Add a spawn facing to the mob spawner
            mob.m_health= mobType.MaxHealth;
            mob.m_energy= mobType.MaxEnergy;
            mob.m_ai_data = new MobAIState(spawner);

            return mob;
        }

        public static Mob CreateMob(Mobs dbMob, MobTypeSet mobTypes)
        {
            Mob mob = new Mob();

            mob.m_room_key = new RoomKey(dbMob.GameID, dbMob.RoomX, dbMob.RoomY, dbMob.RoomZ);
            mob.m_mob_id = dbMob.MobID;
            mob.m_mob_type= mobTypes.GetMobTypeById(dbMob.MobTypeID);
            mob.m_position = new Point3d((float)dbMob.X, (float)dbMob.Y, (float)dbMob.Z);
            mob.m_angle = (float)dbMob.Angle;
            mob.m_health = dbMob.Health;
            mob.m_energy = dbMob.Energy;
            mob.m_ai_data = ParseJsonAIData(dbMob.AiData);

            if (mob.m_ai_data == null)
            {
                mob.m_ai_data = new MobAIState();
                //Utilities.LogWarning(string.Format("Mob:CreateMob(id={0}) - Unable to parse AI State. Creating new state.", dbMob.MobID));
            }
            
            return mob;
        }

        public override void WriteDirtyObjectToDatabase(AsyncRPGDataContext db_context)
        {
            if (IsDirty)
            {
                MobQueries.UpdateMob(db_context, this);
                IsDirty = false;
            }
        }

        public static MobAIState ParseJsonAIData(string json_ai_data)
        {
            MobAIState ai_data = null;

            if (json_ai_data.Length > 0)
            {
                ai_data = JsonMapper.ToObject<MobAIState>(json_ai_data);
            }

            return ai_data;
        }

        public static string SerializeAIData(MobAIState ai_state)
        {
            return JsonMapper.ToJson(ai_state);
        }
    }
}
