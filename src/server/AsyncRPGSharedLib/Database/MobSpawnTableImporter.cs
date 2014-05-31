using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Utility;
using LitJson;

namespace AsyncRPGSharedLib.Database
{
    [Serializable]
    class JSONMobSpawnTableEntry
    {
        public string mob_type_name = "";
        public string mob_spawn_table_name = "";
        public int weight = 0;
    }

    [Serializable]
    class JSONMobSpawnTable
    {
        public string mob_spawn_table_name = "";
        public JSONMobSpawnTableEntry[] entries = new JSONMobSpawnTableEntry[0];
    }

    [Serializable]
    class JSONMobSpawnTableSet
    {
        public JSONMobSpawnTable[] mob_spawn_table_set = new JSONMobSpawnTable[0];
    }

    public class MobSpawnTableImporter
    {
        private Logger _logger;

        public MobSpawnTableImporter(Logger logger)
        {
            _logger = logger;
        }

        public void ParseMobSpawnTables(
            AsyncRPGDataContext db_context,
            string mob_spawn_tables_file_path)
        {
            MobTypeSet mobTypeSet = new MobTypeSet();

            // Load the mob spawn types from the DB
            mobTypeSet.Initialize(db_context);

            // Clear out any existing room templates
            db_context.ExecuteCommand("DELETE FROM mob_spawn_table_entries");
            db_context.ExecuteCommand("DELETE FROM mob_spawn_tables");                    

            // Read in the given JSON file and save it into the mob spawn table db table
            string jsonString = "";
            JSONMobSpawnTableSet jsonMobSpawnTableSet = null;

            // Read the spawn table file into a string
            using (StreamReader streamReader = new StreamReader(mob_spawn_tables_file_path))
            {
                jsonString = streamReader.ReadToEnd();

                if (jsonString.Length == 0)
                {
                    throw new Exception("MobSpawnTableParser: Mob spawn table file empty:" + mob_spawn_tables_file_path);
                }
            }

            // Parse the JSON into an object
            jsonMobSpawnTableSet = JsonMapper.ToObject<JSONMobSpawnTableSet>(jsonString);

            // Parse and verify each spawn table given
            {
                Dictionary<string, JSONMobSpawnTable> validMobSpawnTables = new Dictionary<string, JSONMobSpawnTable>();
                Dictionary<string, int> spawnTableNameToId = new Dictionary<string, int>();

                foreach (JSONMobSpawnTable jsonMobSpawnTable in jsonMobSpawnTableSet.mob_spawn_table_set)
                {
                    if (ValidateMobSpawnTable(jsonMobSpawnTable, validMobSpawnTables))
                    {
                        bool validTable = true;

                        // Verify each entry references either a valid spawn table or mob type
                        // and has a valid weight
                        foreach (JSONMobSpawnTableEntry jsonSpawnTableEntry in jsonMobSpawnTable.entries)
                        {
                            if (!ValidateMobSpawnTableEntry(
                                    jsonSpawnTableEntry,
                                    mobTypeSet,
                                    validMobSpawnTables))
                            {
                                validTable = false;
                            }
                        }

                        // If all the table entries look good, add the spawn table to the DB
                        if (validTable)
                        {
                            // Insert the spawn table first
                            MobSpawnTables dbMobSpawnTable = new MobSpawnTables
                            {
                                MobSpawnTableName = jsonMobSpawnTable.mob_spawn_table_name,
                            };

                            db_context.MobSpawnTables.InsertOnSubmit(dbMobSpawnTable);
                            db_context.SubmitChanges();

                            // Then insert all the entries associated with the table
                            foreach (JSONMobSpawnTableEntry jsonEntry in jsonMobSpawnTable.entries)
                            {
                                MobSpawnTableEntries dbMobSpawnTableEntry = null;

                                if (jsonEntry.mob_spawn_table_name.Length > 0)
                                {
                                    dbMobSpawnTableEntry = new MobSpawnTableEntries
                                    {
                                        MobSpawnTableID = dbMobSpawnTable.MobSpawnTableID,
                                        ReferenceID = spawnTableNameToId[jsonEntry.mob_spawn_table_name],
                                        ReferenceIsMobType = false,
                                        Weight = jsonEntry.weight
                                    };
                                }
                                else
                                {
                                    dbMobSpawnTableEntry = new MobSpawnTableEntries
                                    {
                                        MobSpawnTableID = dbMobSpawnTable.MobSpawnTableID,
                                        ReferenceID = mobTypeSet.GetMobTypeByName(jsonEntry.mob_type_name).ID,
                                        ReferenceIsMobType = true,
                                        Weight = jsonEntry.weight
                                    };
                                }

                                db_context.MobSpawnTableEntries.InsertOnSubmit(dbMobSpawnTableEntry);
                                db_context.SubmitChanges();
                            }

                            _logger.LogInfo("MobSpawnTableParser: Added Mob Spawn Table:");
                            _logger.LogInfo(jsonMobSpawnTable.mob_spawn_table_name);

                            // Keep track of each valid table added and what it's DB id is
                            validMobSpawnTables.Add(
                                jsonMobSpawnTable.mob_spawn_table_name,
                                jsonMobSpawnTable);
                            spawnTableNameToId.Add(
                                jsonMobSpawnTable.mob_spawn_table_name,
                                dbMobSpawnTable.MobSpawnTableID);
                        }
                        else
                        {
                            throw new Exception("MobSpawnTableParser: Problem(s) validating mob spawn table entries: " + jsonMobSpawnTable.mob_spawn_table_name);
                        }
                    }
                    else
                    {
                        throw new Exception("MobSpawnTableParser: Problem(s) validating mob spawn table: " + jsonMobSpawnTable.mob_spawn_table_name);
                    }
                }
            }
        }

        private bool ValidateMobSpawnTable(
            JSONMobSpawnTable jsonMobSpawnTable, 
            Dictionary<string, JSONMobSpawnTable> validMobTypes)
        {
            bool valid = true;

            if (jsonMobSpawnTable.mob_spawn_table_name.Length == 0)
            {
                _logger.LogError("MobSpawnTableParser: Mob Spawn Table with missing name");
                valid = false;
            }

            if (validMobTypes.ContainsKey(jsonMobSpawnTable.mob_spawn_table_name))
            {
                _logger.LogError("MobSpawnTableParser: Duplicate mob spawn table name:");
                _logger.LogError(jsonMobSpawnTable.mob_spawn_table_name);
                valid = false;
            }

            return valid;
        }

        private bool ValidateMobSpawnTableEntry(
            JSONMobSpawnTableEntry jsonSpawnTableEntry, 
            MobTypeSet mobTypeSet,
            Dictionary<string, JSONMobSpawnTable> validMobSpawnTables)
        {
            bool valid= true;

            if (jsonSpawnTableEntry.mob_spawn_table_name.Length > 0)
            {
                if (!validMobSpawnTables.ContainsKey(jsonSpawnTableEntry.mob_spawn_table_name))
                {
                    _logger.LogError("MobSpawnTableParser: Invalid mob spawn table reference:");
                    _logger.LogError(jsonSpawnTableEntry.mob_spawn_table_name);
                    valid = false;
                }
            }
            else if (jsonSpawnTableEntry.mob_type_name.Length > 0)
            {
                if (!mobTypeSet.HasMobTypeWithName(jsonSpawnTableEntry.mob_type_name))
                {
                    _logger.LogError("MobSpawnTableParser: Invalid mob type reference:");
                    _logger.LogError(jsonSpawnTableEntry.mob_type_name);
                    valid = false;
                }
            }
            else
            {
                _logger.LogError("MobSpawnTableParser: No spawn table or mob type references:");
                valid = false;
            }

            if (jsonSpawnTableEntry.weight <= 0)
            {
                _logger.LogError("MobSpawnTableParser: Non-positive weight given");
                valid = false;
            }

            return valid;
        }
    }
}
