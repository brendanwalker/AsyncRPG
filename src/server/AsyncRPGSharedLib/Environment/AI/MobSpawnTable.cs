using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Database;

namespace AsyncRPGSharedLib.Environment.AI
{
    public class MobSpawnTable
    {
        private class MobSpawnTableEntry
        {
            public MobType mob_type;
            public MobSpawnTable mob_spawn_table;
            public float cumulative_weight;
        }

        private MobSpawnTableEntry[] m_entries;
        private string m_table_name;
        private int m_table_id;

        public int ID
        {
            get { return m_table_id; }
        }

        public string Name
        {
            get { return m_table_name; }
        }

        public MobSpawnTable(
            Dictionary<int, MobSpawnTable> spawnTables,
            MobTypeSet mob_types,
            MobSpawnTables db_table,
            List<MobSpawnTableEntries> db_entries)
        {
            m_table_id = db_table.MobSpawnTableID;
            m_table_name = db_table.MobSpawnTableName;

            if (db_entries.Count > 0)
            {
                // Compute the total non normalized weight
                float weight_sum = (float)db_entries.Sum(e => e.Weight);

                // Sort the source list by weight and then store in the internal entry array
                m_entries = new MobSpawnTableEntry[db_entries.Count];

                // Copy over the source entries into the table
                // Normalize the weights
                for (int entry_index = 0; entry_index < db_entries.Count; entry_index++)
                {
                    MobSpawnTableEntries db_entry = db_entries[entry_index];
                    MobSpawnTableEntry entry = new MobSpawnTableEntry();

                    entry.cumulative_weight = (float)db_entry.Weight / weight_sum;

                    if (db_entry.ReferenceIsMobType)
                    {
                        entry.mob_type = mob_types.GetMobTypeById(db_entry.ReferenceID);
                        entry.mob_spawn_table = null;
                    }
                    else
                    {
                        entry.mob_type = null;
                        entry.mob_spawn_table = spawnTables[db_entry.ReferenceID];
                    }

                    m_entries[entry_index] = entry;
                }

                // Sort the weights. Largest weight first.
                // We do this so that when we search through the CDF
                // we're more likely to find the random entry we're looking for earlier in the array.
                m_entries.OrderByDescending(e => e.cumulative_weight);

                // Convert the probability density function (PDF) into a cumulative distribution function (CDF)
                float accumulator = 0f;
                for (int entry_index = 0; entry_index < m_entries.Length; entry_index++)
                {
                    MobSpawnTableEntry entry = m_entries[entry_index];
                    float weight = entry.cumulative_weight;

                    entry.cumulative_weight = Math.Min(accumulator + entry.cumulative_weight, 1.0f);
                    accumulator = accumulator + weight;
                }
            }
        }

        public MobType PickRandomMobType(Random rng)
        {
            MobType result = null;
            float random_variable = (float)rng.NextDouble();

            for (int entry_index = 0; entry_index < m_entries.Length; entry_index++)
            {
                MobSpawnTableEntry entry = m_entries[entry_index];

                if (random_variable <= entry.cumulative_weight)
                {
                    if (entry.mob_spawn_table != null)
                    {
                        result = entry.mob_spawn_table.PickRandomMobType(rng);
                    }
                    else
                    {
                        result = entry.mob_type;
                    }

                    break;
                }
            }

            return result;
        }
    }

    public class MobSpawnTableSet
    {
        private Dictionary<string, MobSpawnTable> m_mobSpawnTablesByName;
        private Dictionary<int, MobSpawnTable> m_mobSpawnTablesById;

        public MobSpawnTableSet()
        {
            m_mobSpawnTablesByName = new Dictionary<string, MobSpawnTable>();
            m_mobSpawnTablesById = new Dictionary<int, MobSpawnTable>();
        }

        public bool Initialize(
            string connection_string,
            MobTypeSet mobTypeSet, 
            out string result)
        {
            bool success = 
                MobQueries.LoadMobSpawnTables(
                    connection_string, 
                    mobTypeSet, 
                    out m_mobSpawnTablesById, 
                    out result);

            if (success)
            {
                foreach (MobSpawnTable mobSpawnTable in m_mobSpawnTablesById.Values)
                {
                    m_mobSpawnTablesByName.Add(mobSpawnTable.Name, mobSpawnTable);
                }
            }

            return success;
        }

        public void Initialize(
            AsyncRPGDataContext db_context,
            MobTypeSet mobTypeSet)
        {
            m_mobSpawnTablesById = MobQueries.LoadMobSpawnTables(db_context, mobTypeSet);

            foreach (MobSpawnTable mobSpawnTable in m_mobSpawnTablesById.Values)
            {
                m_mobSpawnTablesByName.Add(mobSpawnTable.Name, mobSpawnTable);
            }
        }

        public IEnumerator<MobSpawnTable> MobSpawnTables
        {
            get
            {
                return m_mobSpawnTablesById.Values.GetEnumerator();
            }
        }

        public MobSpawnTable GetMobSpawnTableByName(string mobSpawnTableName)
        {
            return m_mobSpawnTablesByName[mobSpawnTableName];
        }

        public MobSpawnTable GetMobSpawnTableByID(int mobSpawnTableId)
        {
            return m_mobSpawnTablesById[mobSpawnTableId];
        }
    }
}
