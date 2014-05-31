using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

public class ClientGameConstants 
{
	public class Portrait
    {
        public string name;
        public string image;
        public GameConstants.eGender gender;
        public GameConstants.eArchetype archetype;

        public Portrait(string name, string image, GameConstants.eGender gender, GameConstants.eArchetype archetype)
        {
            this.name = name;
            this.image = image;
            this.gender = gender;
            this.archetype = archetype;
        }
    }
			
	private static Portrait[] PORTRAITS = new Portrait[] {
		new Portrait("Ike", "ike", GameConstants.eGender.Male, GameConstants.eArchetype.warrior),
		new Portrait("Mia", "mia", GameConstants.eGender.Female, GameConstants.eArchetype.thief),		
		new Portrait("Nephenee", "nephenee", GameConstants.eGender.Female, GameConstants.eArchetype.archer),
		new Portrait("Rhys", "rhys", GameConstants.eGender.Male, GameConstants.eArchetype.mage)
    };
			
	public static int GetPortraitCount()
	{
		return PORTRAITS.Length;
	}
		
	public static string GetDefaultNameForPicture(uint pictureId)
	{
		return PORTRAITS[pictureId].name;
	}
		
	public static string GetResourceNameForPicture(uint pictureId)
	{
        return "Gfx/Portraits/portrait_" + PORTRAITS[pictureId].image;
	}			

	public static string GetResourceNameForThumbnail(uint pictureId)
	{
        return "Gfx/Portraits/thumb_" + PORTRAITS[pictureId].image;
	}

    public static GameConstants.eGender GetGenderForPicture(uint pictureId)
	{
		return PORTRAITS[pictureId].gender;
	}

    public static GameConstants.eArchetype GetArchetypeForPicture(uint pictureId)
	{
		return PORTRAITS[pictureId].archetype;
	}

    public static Point2d ConvertPixelPositionToScreenPosition(Point2d pixelPosition)
    {
        return new Point2d(
            (pixelPosition.x * (float)Screen.width) / (float)GameConstants.GAME_SCREEN_PIXEL_WIDTH,
            (pixelPosition.y * (float)Screen.height) / (float)GameConstants.GAME_SCREEN_PIXEL_HEIGHT);
    }

    public static Point2d ConvertScreenPositionToPixelPosition(Point2d pixelPosition)
    {
        return new Point2d(
            (pixelPosition.x * (float)GameConstants.GAME_SCREEN_PIXEL_WIDTH) / (float)Screen.width,
            (pixelPosition.y * (float)GameConstants.GAME_SCREEN_PIXEL_HEIGHT) / (float)Screen.height);
    }

    public static Point2d ConvertRoomPositionToScreenPosition(Point3d roomPosition)
    {
        return ConvertPixelPositionToScreenPosition(GameConstants.ConvertRoomPositionToPixelPosition(roomPosition));
    }

    public static Point3d ConvertScreenPositionToRoomPosition(Point2d screenPosition)
    {
        return GameConstants.ConvertPixelPositionToRoomPosition(ConvertScreenPositionToPixelPosition(screenPosition));
    }

    public static Vector3 ConvertPixelPositionToVertexPosition(float pixelX, float pixelY, float depth)
    {
        float vertExtent = Camera.main.camera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        return new Vector3(
            2.0F * horzExtent * ((pixelX / (float)GameConstants.GAME_SCREEN_PIXEL_WIDTH) - 0.5f),
            2.0F * vertExtent * (0.5f - (pixelY / (float)GameConstants.GAME_SCREEN_PIXEL_HEIGHT)),
            depth);
    }

    public static Vector3 ConvertRoomPositionToVertexPosition(Point3d roomPosition)
    {
        Point2d pixelPosition = GameConstants.ConvertRoomPositionToPixelPosition(roomPosition);

        return ConvertPixelPositionToVertexPosition(pixelPosition.x, pixelPosition.y, roomPosition.z);
    }
}
