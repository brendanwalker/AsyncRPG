using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class RoomData
{
    public RoomKey RoomKey { get; set; }
    public Point3d WorldPosition { get; set; }
    public List<RoomPortal> RoomPortals { get; set; }
    public StaticRoomData StaticRoomData { get; set; }

	// Mob data for all mobs in the room
	private Dictionary<int, MobData> m_mobs; // mob_id -> Mob Data
		
	// Energy Tank data for all energy tanks in the room
	private Dictionary<int, EnergyTankData> m_energyTanks; // energy_tank_id -> Energy Tank Data
		
	public RoomData() 
	{
		RoomKey = new RoomKey();
		WorldPosition = new Point3d();
		RoomPortals = new List<RoomPortal>();
		StaticRoomData = new StaticRoomData();
			
		m_mobs = new Dictionary<int, MobData>();
		m_energyTanks = new Dictionary<int, EnergyTankData>();
	}
		
	// Mob State
	public Dictionary<int, MobData> MobMap
	{
		get { return m_mobs; }
	}
		
	public MobData GetMobById(int mob_id)
	{
        MobData result= null;
        
		return m_mobs.TryGetValue(mob_id, out result) ? result : null;
	}
		
	public void SetMobById(int mob_id, MobData mobState)
	{
		m_mobs.Add(mob_id,  mobState);
	}
		
	// Energy Tank State
	public Dictionary<int, EnergyTankData> EnergyTankMap
	{
		get { return m_energyTanks; }
	}
		
	public EnergyTankData GetEnergyTankById(int energy_tank_id)
	{
        EnergyTankData result= null;

		return m_energyTanks.TryGetValue(energy_tank_id, out result) ? result : null;
	}
		
	public void SetEnergyTankById(int energy_tank_id, EnergyTankData energyTank)
	{
		m_energyTanks.Add(energy_tank_id, energyTank);
	}
		
	// Parsing
	public static RoomData FromObject(JsonData jsonData)
	{
		RoomData roomData = new RoomData();

        int gameID = SessionData.GetInstance().GameID;
		int room_x = (int)jsonData["room_x"];
		int room_y = (int)jsonData["room_y"];
		int room_z = (int)jsonData["room_z"];		
						
        float world_x= jsonData["world_x"].IsInt ? (float)((int)jsonData["world_x"]) : (float)((double)jsonData["world_x"]);
        float world_y= jsonData["world_y"].IsInt ? (float)((int)jsonData["world_y"]) : (float)((double)jsonData["world_y"]);
        float world_z= jsonData["world_z"].IsInt ? (float)((int)jsonData["world_z"]) : (float)((double)jsonData["world_z"]);

        roomData.RoomKey.Set(gameID, room_x, room_y, room_z);		
		roomData.WorldPosition.Set(world_x, world_y, world_z);			
		roomData.StaticRoomData = StaticRoomData.FromObject(roomData.RoomKey, jsonData["data"]);
				
		{
			JsonData portalList = jsonData["portals"];
				
			for (int portalIndex= 0; portalIndex < portalList.Count; portalIndex++)
			{
                JsonData portalObject = portalList[portalIndex];
				RoomPortal portal = RoomPortal.FromObject(portalObject);
					
				roomData.RoomPortals.Add(portal);
			}
		}
					
		{
			JsonData mobObjects = jsonData["mobs"];

			roomData.m_mobs = new Dictionary<int, MobData>();
				
			for (int mobIndex= 0; mobIndex < mobObjects.Count; mobIndex++)
			{
                JsonData mobObject = mobObjects[mobIndex];
				MobData mobData = MobData.FromObject(mobObject);
									
				roomData.SetMobById(mobData.mob_id, mobData);
			}
		}

		{
            JsonData energyTankObjects = jsonData["energyTanks"];

			roomData.m_energyTanks = new Dictionary<int, EnergyTankData>();

            for (int energyTankIndex = 0; energyTankIndex < energyTankObjects.Count; energyTankIndex++)
			{
                JsonData energyTankObject = energyTankObjects[energyTankIndex];
				EnergyTankData energyTankData = EnergyTankData.FromObject(energyTankObject);
									
				roomData.SetEnergyTankById(energyTankData.energy_tank_id, energyTankData);
			}
		}
			
		return roomData;
	}
}
