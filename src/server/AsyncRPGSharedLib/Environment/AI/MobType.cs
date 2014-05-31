using System;
using System.Collections.Generic;
using System.Xml;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using LitJson;

namespace AsyncRPGSharedLib.Environment.AI
{
    [Serializable]
    public class MobTypeVisionCone
    {
        public double distance = 0;
        public double angle = 0; // degrees

        public float GetHalfAngle()
        {
            return (float)angle * MathConstants.DEGREES_TO_RADIANS * 0.5f;
        }
    }

    [Serializable]
    public class MobTypePerception
    {
        public int orphan_turn_timeout = 1;
        public double saliance_distance_weight = 1.0f;
        public double saliance_energy_weight = 1.0f;
        public double saliance_health_weight = 1.0f;
        public double saliance_status_weight = 1.0f;
    }

    [Serializable]
    public class MobTypeAbilities
    {
        public int energy_tank_drain_per_turn;
    }

    [Serializable]
    public class MobTypeData
    {
        public string mob_type_name= "";
        public string label = "";
        public int max_health = 0;
        public int max_energy = 0;
        public double speed = 0.0f;
        public MobTypeVisionCone vision_cone = new MobTypeVisionCone();
        public MobTypePerception perception = new MobTypePerception();
        public MobTypeAbilities abilities = new MobTypeAbilities();
    }

    [Serializable]
    public class MobTypeDataSet
    {
        public MobTypeData[] mob_types_set = new MobTypeData[0];
    }

    public class MobType
    {
        private int m_mob_type_id;
        private MobTypeData m_mob_type_data;

        public MobType(MobTypes db_mob_type)
        {
            m_mob_type_id = db_mob_type.MobTypeID;

            try
            {
                string jsonString= db_mob_type.MobTypeData;

                if (jsonString.Length == 0)
                {
                    throw new ArgumentException();
                }

                m_mob_type_data = JsonMapper.ToObject<MobTypeData>(jsonString);
            }
            catch (System.Exception)
            {
                m_mob_type_data = null;
            }
        }

        public int ID
        {
            get { return m_mob_type_id; }
        }

        public string Name
        {
            get { return m_mob_type_data.mob_type_name; }
        }

        public int MaxHealth
        {
            get { return m_mob_type_data.max_health; }
        }

        public int MaxEnergy
        {
            get { return m_mob_type_data.max_energy; }
        }

        public MobTypeVisionCone VisionCone
        {
            get { return m_mob_type_data.vision_cone; }
        }

        public MobTypePerception Perception
        {
            get { return m_mob_type_data.perception; }
        }

        public MobTypeAbilities Abilities
        {
            get { return m_mob_type_data.abilities; }
        }
    }

    public class MobTypeSet
    {
        private Dictionary<string, MobType> m_mobTypesByName;
        private Dictionary<int, MobType> m_mobTypesById;

        public MobTypeSet()
        {
            m_mobTypesByName = new Dictionary<string, MobType>();
            m_mobTypesById = new Dictionary<int, MobType>();
        }

        public bool Initialize(string connectionString, out string result)
        {
            bool success = MobQueries.LoadMobTypes(connectionString, out m_mobTypesById, out result);

            if (success)
            {
                // Build the name -> type table
                foreach (MobType mob_type in m_mobTypesById.Values)
                {
                    m_mobTypesByName.Add(mob_type.Name, mob_type);
                }
            }

            return success;
        }

        public void Initialize(AsyncRPGDataContext db_context)
        {
            m_mobTypesById= MobQueries.LoadMobTypes(db_context);

            // Build the name -> type table
            foreach (MobType mob_type in m_mobTypesById.Values)
            {
                m_mobTypesByName.Add(mob_type.Name, mob_type);
            }
        }

        public IEnumerator<MobType> MobSpawnTables
        {
            get
            {
                return m_mobTypesById.Values.GetEnumerator();
            }
        }

        public bool HasMobTypeWithName(string type_name)
        {
            return m_mobTypesByName.ContainsKey(type_name);
        }

        public bool HasMobTypeWithId(int type_id)
        {
            return m_mobTypesById.ContainsKey(type_id);
        }

        public MobType GetMobTypeByName(string type_name)
        {
            return m_mobTypesByName[type_name];
        }

        public MobType GetMobTypeById(int type_id)
        {
            return m_mobTypesById[type_id];
        }
    }
}
