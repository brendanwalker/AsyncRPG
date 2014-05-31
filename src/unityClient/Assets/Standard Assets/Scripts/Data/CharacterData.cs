using LitJson;
using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class CharacterData
{
    public int character_id;
    public string character_name;
    public GameConstants.eArchetype archetype;
    public GameConstants.eGender gender;
    public int picture_id;
    public uint power_level;
    public int game_id;
    public string game_name;
    public int room_x;
    public int room_y;
    public int room_z;
    public float x;
    public float y;
    public float z;
    public float angle;		
	public int energy;

	public CharacterData() 
	{
		character_id= -1;
		character_name= "";
		archetype= GameConstants.eArchetype.warrior;
		gender= GameConstants.eGender.Female;
		picture_id= -1;
		power_level = 1;
		game_id = -1;
		game_name = "";
		room_x= 0;
		room_y= 0;
		room_z= 0;
		x= 0;
		y= 0;
		z= 0;
		angle = 0;
		energy = 0;
	}

    public RoomKey CurrentRoomKey
    {
        get { return new RoomKey(game_id, room_x, room_y, room_z); }
    }
		
    public Point3d PositionInRoom
    {
        get { return new Point3d(x, y, z); }
    }
		
	public static CharacterData FromObject(JsonData jsonData) 
	{	
		CharacterData result = new CharacterData();
			
		result.character_id= (int)jsonData["character_id"];
		result.character_name= (string)jsonData["character_name"];
		result.archetype= (GameConstants.eArchetype)((int)jsonData["archetype"]);
		result.gender= (GameConstants.eGender)((int)jsonData["gender"]);
		result.picture_id= (int)jsonData["picture_id"];
		result.power_level = (uint)((int)jsonData["power_level"]);
		result.game_id = (int)jsonData["game_id"];
		result.game_name = (string)jsonData["game_name"];
		result.room_x = (int)jsonData["room_x"];
		result.room_y = (int)jsonData["room_y"];
		result.room_z = (int)jsonData["room_z"];
        result.x = jsonData["x"].IsInt ? (float)((int)jsonData["x"]) : (float)((double)jsonData["x"]);
        result.y = jsonData["y"].IsInt ? (float)((int)jsonData["y"]) : (float)((double)jsonData["y"]);
        result.z = jsonData["z"].IsInt ? (float)((int)jsonData["z"]) : (float)((double)jsonData["z"]);
        result.angle = jsonData["angle"].IsInt ? (float)((int)jsonData["angle"]) : (float)((double)jsonData["angle"]);
		result.energy = (int)jsonData["energy"];
						
		return result;
	}
}
