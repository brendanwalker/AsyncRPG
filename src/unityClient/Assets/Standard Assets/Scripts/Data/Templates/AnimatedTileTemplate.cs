using UnityEngine;
using System.Collections;
using System.Xml;

public class AnimatedTileTemplate 
{
    public uint FrameCount { get; private set; }
    public uint SheetIndex { get; private set; }
    public uint Width { get; private set; }
    public uint Height { get; private set; }
    public string TileSetName { get; private set; }
		
	public AnimatedTileTemplate() 
	{
		FrameCount = 0;
		SheetIndex = 0;
		Width= 0;
		Height= 0;
		TileSetName= "";				
	}
				
	public static AnimatedTileTemplate ParseXML(XmlNode animatedTileXML)
	{
        AnimatedTileTemplate animatedTileTemplate = new AnimatedTileTemplate();
			
		animatedTileTemplate.FrameCount = uint.Parse(animatedTileXML.Attributes["frameCount"].Value);
        animatedTileTemplate.SheetIndex = uint.Parse(animatedTileXML.Attributes["sheetIndex"].Value);
        animatedTileTemplate.Width = uint.Parse(animatedTileXML.Attributes["width"].Value);
        animatedTileTemplate.Height = uint.Parse(animatedTileXML.Attributes["height"].Value);
		animatedTileTemplate.TileSetName = animatedTileXML.Attributes["tileset"].Value;
			
		return animatedTileTemplate;
	}	
}