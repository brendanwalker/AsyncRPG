using LitJson;
using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class MobData 
{
    public int mob_id;
    public string mob_type_name;
    public int health;
    public int energy;
    public int game_id;
    public int room_x;
    public int room_y;
    public int room_z;
    public float x;
    public float y;
    public float z;
    public float angle;
		
	public MobData() 
	{
		mob_id= -1;
		mob_type_name= "";
		health= 0;
		energy= 0;
		game_id= -1;
		room_x= 0;
		room_y= 0;
		room_z= 0;
		x= 0;
		y= 0;
		z= 0;
		angle= 0;			
	}
	
	public RoomKey CurrentRoomKey
	{
		get { return new RoomKey(game_id, room_x, room_y, room_z); }
	}
		
	public Point3d PositionInRoom
	{
		get { return new Point3d(x, y, z); }
	}
		
	public static MobData FromObject(JsonData jsonData)
	{	
		MobData result = new MobData();
			
		result.mob_id = (int)jsonData["mob_id"];
        result.mob_type_name = (string)jsonData["mob_type_name"];
        result.health = (int)jsonData["health"];
        result.energy = (int)jsonData["energy"];
        result.game_id = (int)jsonData["game_id"];
        result.room_x = (int)jsonData["room_x"];
        result.room_y = (int)jsonData["room_y"];
        result.room_z = (int)jsonData["room_z"];
        result.x = jsonData["x"].IsInt ? (float)((int)jsonData["x"]) : (float)((double)jsonData["x"]);
        result.y = jsonData["y"].IsInt ? (float)((int)jsonData["y"]) : (float)((double)jsonData["y"]);
        result.z = jsonData["z"].IsInt ? (float)((int)jsonData["z"]) : (float)((double)jsonData["z"]);
        result.angle = jsonData["angle"].IsInt ? (float)((int)jsonData["angle"]) : (float)((double)jsonData["angle"]);
			
		return result;
	}		
}