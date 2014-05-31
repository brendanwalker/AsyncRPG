using LitJson;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class MobTypeManager : MonoBehaviour 
{
    private static bool m_debugging = false;
	private static MobTypeManager m_instance;

    public string mobTypePath;

    [SerializeField]
    private MobType[] m_mobTypesList;

	private Dictionary<string, MobType> m_mobTypesDictionary;
		
	public void Start() 
	{
        m_instance = this;
        m_mobTypesDictionary = new Dictionary<string, MobType>();

        if (m_mobTypesList != null && m_mobTypesList.Length > 0)
        {
            // Build the room template lookup table from the room template list
            foreach (MobType mobType in m_mobTypesList)
            {
                m_mobTypesDictionary.Add(mobType.Name, mobType);
            }

            if (m_debugging)
            {
                Debug.LogWarning("MobTypeManager: Dumping " + mobTypePath + ".txt");
                DumpMobTypes();
            }
        }
        else
        {
            Debug.LogError("MobTypeManager: Mob Types missing!");
        }
	}

    public void OnDestroy()
    {
        m_instance = null;
    }
		
	public static MobType GetMobTypeByName(string typeName)
	{
        MobType mobType = null;

        if (!m_instance.m_mobTypesDictionary.TryGetValue(typeName, out mobType))
        {
            Debug.LogError("MobTypeManager: Unknown mob type: " + typeName);
        }

        return mobType;
	}

    public void CacheMobTypes(MobType[] mobTypes)
    {
        m_mobTypesList = mobTypes;
    }

    private void DumpMobTypes()
    {
        //#if UNITY_EDITOR
        //StringBuilder mobReport = new StringBuilder();

        //foreach (MobType mobType in m_mobTypesDictionary.Values)
        //{
        //    mobType.ToStringData(mobReport);
        //}

        //File.WriteAllText(mobTypePath + ".txt", mobReport.ToString());
        //#endif
    }
}
