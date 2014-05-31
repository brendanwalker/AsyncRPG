using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System;

public class RoomTemplate
{
	private string m_templateName;
	private TileGridTemplate m_floor;
	private TileGridTemplate m_walls;
	private TileGridTemplate m_backgroundObjects;
	private TileGridTemplate m_forgroundObjects;
    private AsyncRPGSharedLib.Navigation.NavMesh m_navMeshTemplate;

    public RoomTemplate() 
	{
		m_templateName = "";
		m_navMeshTemplate = null;
		m_floor = null;
		m_walls = null;
		m_backgroundObjects = null;
		m_forgroundObjects = null;
	}
		
	public string TemplateName
	{
		get { return m_templateName; }
	}
		
	public TileGridTemplate FloorGridTemplate
	{
		get { return m_floor; }
	}
		
	public TileGridTemplate WallGridTemplate
	{
		get { return m_walls; }
	}
		
	public TileGridTemplate BackgroundObjectsTemplate
	{
		get { return m_backgroundObjects; }
	}
		
	public TileGridTemplate ForegroundObjectsTemplate
	{
		get { return m_forgroundObjects; }
	}

    public AsyncRPGSharedLib.Navigation.NavMesh NavMeshTemplate
	{
		get { return m_navMeshTemplate; }
	}
					
	public static RoomTemplate ParseXML(string templateName, XmlNode roomTemplateXML)
	{
        RoomTemplate roomTemplate = new RoomTemplate();
			
		roomTemplate.m_templateName = templateName;			
		roomTemplate.m_floor = TileGridTemplate.ParseXML(roomTemplateXML.SelectSingleNode(".//Floor"));
        roomTemplate.m_walls = TileGridTemplate.ParseXML(roomTemplateXML.SelectSingleNode(".//Walls"));
        roomTemplate.m_backgroundObjects = TileGridTemplate.ParseXML(roomTemplateXML.SelectSingleNode(".//BackgroundObjects"));
        roomTemplate.m_navMeshTemplate = 
            AsyncRPGSharedLib.Navigation.NavMesh.FromNavMeshXML(roomTemplateXML.SelectSingleNode(".//NavMesh"));
        roomTemplate.m_forgroundObjects = TileGridTemplate.ParseXML(roomTemplateXML.SelectSingleNode(".//ForegroundObjects"));
					
		return roomTemplate;
	}

    public static RoomTemplate ParseCompressedRoomTemplate(CompressedRoomTemplate compressedRoomTemplate)
    {
        RoomTemplate roomTemplate = new RoomTemplate();

        XmlDocument roomTemplateXML = new XmlDocument();
        roomTemplateXML.LoadXml(compressedRoomTemplate.xml);

        roomTemplate.m_templateName = compressedRoomTemplate.templateName;
        roomTemplate.m_floor = TileGridTemplate.ParseXML(roomTemplateXML.SelectSingleNode(".//Floor"));
        roomTemplate.m_walls = TileGridTemplate.ParseXML(roomTemplateXML.SelectSingleNode(".//Walls"));
        roomTemplate.m_backgroundObjects = TileGridTemplate.ParseXML(roomTemplateXML.SelectSingleNode(".//BackgroundObjects"));
        roomTemplate.m_navMeshTemplate =
            AsyncRPGSharedLib.Navigation.NavMesh.FromCompressedNavMeshData(
                compressedRoomTemplate.compressedNavCells, 
                compressedRoomTemplate.compressedVisibility);
        roomTemplate.m_forgroundObjects = TileGridTemplate.ParseXML(roomTemplateXML.SelectSingleNode(".//ForegroundObjects"));

        return roomTemplate;
    }
}
