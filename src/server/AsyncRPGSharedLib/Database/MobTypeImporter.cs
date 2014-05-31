using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Utility;
using LitJson;

namespace AsyncRPGSharedLib.Database
{
    public class MobTypeImporter
    {
        private Logger _logger;

        public MobTypeImporter(Logger logger)
        {
            _logger = logger;
        }

        public void ParseMobTypes(
            AsyncRPGDataContext db_context,
            string mob_types_file_path)
        {
            // Clear out any existing room templates
            db_context.ExecuteCommand("DELETE FROM mob_types");

            // Read in the given JSON file and save it into the mob types table
            string jsonString = "";
            MobTypeDataSet jsonMobTypeSet = null;

            using (StreamReader streamReader = new StreamReader(mob_types_file_path))
            {
                jsonString = streamReader.ReadToEnd();

                if (jsonString.Length == 0)
                {
                    throw new Exception("MobTypeParser: Mob type file empty: " + mob_types_file_path);
                }
            }

            jsonMobTypeSet = JsonMapper.ToObject<MobTypeDataSet>(jsonString);

            {
                Dictionary<string, MobTypeData> validMobTypes = new Dictionary<string, MobTypeData>();

                foreach (MobTypeData jsonMobType in jsonMobTypeSet.mob_types_set)
                {
                    if (ValidateMobType(jsonMobType, validMobTypes))
                    {
                        string mobTypeJsonString = JsonMapper.ToJson(jsonMobType);

                        MobTypes dbMobType = new MobTypes
                        {
                            MobTypeData = mobTypeJsonString
                        };

                        db_context.MobTypes.InsertOnSubmit(dbMobType);
                        db_context.SubmitChanges();

                        _logger.LogInfo("MobTypeParser: Added MobType:");
                        _logger.LogInfo(jsonMobType.mob_type_name);

                        validMobTypes.Add(jsonMobType.mob_type_name, jsonMobType);
                    }
                    else
                    {
                        throw new Exception("MobTypeParser: Problem(s) validating mob type: " + jsonMobType.mob_type_name);
                    }
                }
            }
        }

        private bool ValidateMobType(MobTypeData jsonMobType, Dictionary<string, MobTypeData> validMobTypes)
        {
            bool valid= true;

            if (jsonMobType.mob_type_name.Length == 0)
            {
                _logger.LogError("MobTypeParser: Mob Type with missing name");
                valid = false;
            }

            if (jsonMobType.max_energy <= 0)
            {
                _logger.LogError("MobTypeParser: Mob Type with non-positive max energy");
                valid = false;
            }

            if (jsonMobType.max_health <= 0)
            {
                _logger.LogError("MobTypeParser: Mob Type with non-positive max health");
                valid = false;
            }

            if (jsonMobType.vision_cone.angle < 0 || jsonMobType.vision_cone.angle > 360)
            {
                _logger.LogError("MobTypeParser: Mob Type vision cone angle must be value in range [0, 360](degrees)");
                valid = false;
            }

            if (jsonMobType.vision_cone.distance < 0 || jsonMobType.vision_cone.distance > 50)
            {
                _logger.LogError("MobTypeParser: Mob Type vision cone distance must be value in range [0, 50](meters)");
                valid = false;
            }

            if (jsonMobType.perception.orphan_turn_timeout < 0)
            {
                _logger.LogError("MobTypeParser: Mob Type Orphan turn timeout must be >= 0");
                valid = false;
            }

            if (jsonMobType.perception.saliance_distance_weight < 0.0f || jsonMobType.perception.saliance_distance_weight > 1.0f)
            {
                _logger.LogError("MobTypeParser: Mob Type salience distance weight must be value in range [0, 1]");
                valid = false;
            }

            if (jsonMobType.perception.saliance_energy_weight < 0.0f || jsonMobType.perception.saliance_energy_weight > 1.0f)
            {
                _logger.LogError("MobTypeParser: Mob Type salience energy weight must be value in range [0, 1]");
                valid = false;
            }

            if (jsonMobType.perception.saliance_health_weight < 0.0f || jsonMobType.perception.saliance_health_weight > 1.0f)
            {
                _logger.LogError("MobTypeParser: Mob Type salience health weight must be value in range [0, 1]");
                valid = false;
            }

            if (jsonMobType.perception.saliance_status_weight < 0.0f || jsonMobType.perception.saliance_status_weight > 1.0f)
            {
                _logger.LogError("MobTypeParser: Mob Type salience health weight must be value in range [0, 1]");
                valid = false;
            }

            if (jsonMobType.abilities.energy_tank_drain_per_turn < 0)
            {
                _logger.LogError("MobTypeParser: Mob Type Energy Tank Drain Per Turn >= 0");
                valid = false;
            }

            if (validMobTypes.ContainsKey(jsonMobType.mob_type_name))
            {
                _logger.LogError("MobTypeParser: Duplicate mob type name:");
                _logger.LogError(jsonMobType.mob_type_name);
                valid = false;
            }

            return valid;
        }
    }
}
