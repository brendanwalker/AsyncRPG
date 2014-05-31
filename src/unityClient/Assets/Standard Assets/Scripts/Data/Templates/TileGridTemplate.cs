using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System;
using AsyncRPGSharedLib.Common;

public class TileGridTemplate
{
	public string m_tileSetImage;
	private uint m_rows;
	private uint m_colomns;
	private int[] m_grid;
		
	public TileGridTemplate() 
	{
		m_tileSetImage = "";
		m_rows = 0;
		m_colomns = 0;
		m_grid = null;
	}
		
	public string TileSetImageName
	{
		get { return m_tileSetImage; }
	}
		
	public uint RowCount
	{
		get { return m_rows; }
	}
		
	public uint ColomnCount
	{
		get { return m_colomns; }
	}
		
	public int[] TileIndices
	{
		get { return m_grid; }
	}
		
	public float TileWidth 
	{
		get { return GameConstants.TILE_PIXEL_SIZE; }
	}

	public float TileHeight
	{
		get { return GameConstants.TILE_PIXEL_SIZE; }
	}

	public static TileGridTemplate ParseXML(XmlNode tileGridXML)
	{
		TileGridTemplate tileGridTemplate = new TileGridTemplate();
			
		tileGridTemplate.m_tileSetImage = tileGridXML.Attributes["tileset"].Value;

		{
            List<int> gridValues = new List<int>();
			string[] gridRows = tileGridXML.InnerText.Split(new char[] {'\n'});
			
			foreach (string rowString in gridRows)
			{
				string[] rowElements = rowString.Split(new char[] {','});
					
				foreach (string element in rowElements)
				{
					gridValues.Add(int.Parse(element));
				}
			}
			
	        tileGridTemplate.m_grid = gridValues.ToArray();
			tileGridTemplate.m_rows = (uint)gridRows.Length;
			tileGridTemplate.m_colomns = (uint)tileGridTemplate.m_grid.Length / tileGridTemplate.m_rows;
		}
			
		uint expectedSize = GameConstants.ROOM_X_TILES * GameConstants.ROOM_Y_TILES;
		if (tileGridTemplate.m_grid.Length != expectedSize)
		{
			throw new Exception(
                "TileGridTemplate: Invalid tile grid size. Expected "+
                expectedSize.ToString()+
                " entries, got "+
                tileGridTemplate.m_grid.Length.ToString());
		}
			
		return tileGridTemplate;
	}
}
