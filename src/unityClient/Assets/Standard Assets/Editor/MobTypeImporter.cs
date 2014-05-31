
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml;
using System;
using System.Collections.Generic;
using LitJson;
 
public class MobTypeImporter : AssetPostprocessor 
{
    private static bool m_debugging = false;

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        Log("MobTypeImporter called");

        foreach (string assetPath in importedAssets)
        {
            Log("MobTypeImporter: "+assetPath);
            if (assetPath.EndsWith("MobTypes.prefab"))
            {
                Log("Found MobTypes Asset: " + assetPath);
                GameObject assetGameObject = Resources.Load<GameObject>("MobTypes");
                MobTypeManager mobTypeManager = assetGameObject.GetComponent<MobTypeManager>();

                if (mobTypeManager != null)
                {
                    if (MobTypeImporter.ParseMobTypes(mobTypeManager))
                    {
                        Log("MobTypeImporter: has RoomTemplateManager component!");
                    }
                    else
                    {
                        Debug.LogError("MobTypeImporter: Failed to import room templates");
                    }
                }
                else
                {
                    Debug.LogError("MobTypeImporter: Asset missing RoomTemplateManager component!");
                }
 
                AssetDatabase.Refresh(ImportAssetOptions.Default);
            }
        }
    }

    private static bool ParseMobTypes(MobTypeManager mobTypeManager)
    {
        bool success = true;
        string mob_types_path = mobTypeManager.mobTypePath;
        Dictionary<string, MobType> mobTypes = new Dictionary<string, MobType>();

        try
        {
            string mob_types_json= File.ReadAllText(mob_types_path);
            JsonData jsonObject = JsonMapper.ToObject(mob_types_json);
            JsonData mobTypeSet = jsonObject["mob_types_set"];

            if (mobTypeSet != null && mobTypeSet.Count > 0)
            {
                for (int mobTypeIndex = 0; mobTypeIndex < mobTypeSet.Count; ++mobTypeIndex)
                {
                    JsonData jsonEntry = mobTypeSet[mobTypeIndex];
                    MobType mobType = MobType.FromObject(jsonEntry);

                    if (ValidateMobType(mobType, mobTypes))
                    {
                        mobTypes.Add(mobType.Name, mobType);
                    }
                    else
                    {
                        Debug.LogError("MobTypeImporter: ERROR: Problem(s) validating mob type: " + mobType.Name);
                        success = false;
                    }
                }
            }
            else
            {
                Debug.LogError("MobTypeImporter: ERROR: No mob types found in file:" + mob_types_path);
                success = false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("MobTypeImporter: ERROR: Failed to parse mob types: " + ex.Message);
            Debug.LogException(ex);
            success = false;
        }

        // Store the mob types into the mob type manager
        if (success)
        {
            MobType[] mobTypeArray = new MobType[mobTypes.Count];

            mobTypes.Values.CopyTo(mobTypeArray, 0);
            mobTypeManager.CacheMobTypes(mobTypeArray);
            Log(string.Format("MobTypeImporter: Caching {0} mob types(s) in mob manager.", mobTypes.Count));
        }

        return success;
    }

    private static bool ValidateMobType(MobType mobType, Dictionary<string, MobType> validMobTypes)
    {
        bool valid = true;

        if (mobType.Name.Length == 0)
        {
            Debug.LogError("MobTypeParser: ERROR: Mob Type with missing name");
            valid = false;
        }

        if (mobType.MaxEnergy <= 0)
        {
            Debug.LogError("MobTypeParser: ERROR: Mob Type with non-positive max energy");
            valid = false;
        }

        if (mobType.MaxHealth <= 0)
        {
            Debug.LogError("MobTypeParser: ERROR: Mob Type with non-positive max health");
            valid = false;
        }

        if (mobType.VisionConeAngleDegrees < 0 || mobType.VisionConeAngleDegrees > 360)
        {
            Debug.LogError("MobTypeParser: ERROR: Mob Type vision cone angle must be value in range [0, 360](degrees)");
            valid = false;
        }

        if (mobType.VisionConeDistance < 0 || mobType.VisionConeDistance > 50)
        {
            Debug.LogError("MobTypeParser: ERROR: Mob Type vision cone distance must be value in range [0, 50](meters)");
            valid = false;
        }


        if (validMobTypes.ContainsKey(mobType.Name))
        {
            Debug.LogError("MobTypeParser: ERROR: Duplicate mob type name: "+mobType.Name);
            valid = false;
        }

        return valid;
    }

    private static void Log(string message)
    {
        if (m_debugging)
        {
            Debug.Log(message);
        }
    }
}