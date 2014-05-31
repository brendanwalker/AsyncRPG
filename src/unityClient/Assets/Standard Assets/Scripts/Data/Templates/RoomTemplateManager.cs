using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;

[Serializable]
public class CompressedRoomTemplate
{
    public string templateName;
    public string xml;
    public byte[] compressedNavCells;
    public byte[] compressedVisibility;
}

public class RoomTemplateManager : MonoBehaviour 
{
    private static bool m_debugging = false;
    private static RoomTemplateManager m_instance;

    public string mapTemplatePath;

    [SerializeField]
    private CompressedRoomTemplate[] m_compressedRoomTemplateList;
    private Dictionary<string, RoomTemplate> m_roomTemplateTable;

	// Use this for initialization
	void Start () 
    {
        m_instance = this;
        m_roomTemplateTable = new Dictionary<string, RoomTemplate>();

        if (m_compressedRoomTemplateList != null && m_compressedRoomTemplateList.Length > 0)
        {
            // Build the room template lookup table from the room template list
            foreach (CompressedRoomTemplate compressedRoomTemplate in m_compressedRoomTemplateList)
            {
                m_roomTemplateTable.Add(
                    compressedRoomTemplate.templateName, 
                    RoomTemplate.ParseCompressedRoomTemplate(compressedRoomTemplate));
            }

            if (m_debugging)
            {
                Debug.LogWarning("RoomTemplateManager: Dumping " + mapTemplatePath + "/ClientRoomTemplates.txt");
                DumpRoomTemplates();
            }
        }
        else
        {
            Debug.LogError("RoomTemplateManager: Room Templates missing!");
        }
	}

    public void OnDestroy()
    {
        m_instance = null;
    }

    public static RoomTemplate GetRoomTemplate(string templateName)
    {
        RoomTemplate roomTemplate = null;

        if (!m_instance.m_roomTemplateTable.TryGetValue(templateName, out roomTemplate))
        {
            Debug.LogError("RoomTemplateManager: Unknown room template: " + templateName);
        }

        return roomTemplate;
    }

    public void CacheCompressedRoomTemplates(CompressedRoomTemplate[] templates)
    {
        m_compressedRoomTemplateList = templates;
    }

    private void DumpRoomTemplates()
    {
        //StringBuilder roomTemplateReport = new StringBuilder();

        //foreach (RoomTemplate roomTemplate in m_roomTemplateTable.Values)
        //{
        //    roomTemplateReport.AppendFormat("[{0}]\n", roomTemplate.TemplateName);
        //    roomTemplate.NavMeshTemplate.ToStringData(roomTemplateReport);
        //}

        //File.WriteAllText(mapTemplatePath + "/ClientRoomTemplates.txt", roomTemplateReport.ToString());
    }
}
