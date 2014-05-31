using LitJson;
using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class EnergyTankData
{
    public const float ENERGY_TANK_WORLD_UNIT_SIZE = 2 * GameConstants.TILE_WORLD_UNITS_SIZE;

    public int energy_tank_id;
    public uint energy;
    public GameConstants.eFaction ownership;
    public Point3d position;
    public AABB3d boundingBox;
    public RoomKey room_key;

    public EnergyTankData()
    {
        energy_tank_id = -1;
        energy = 0;
        ownership = GameConstants.eFaction.neutral;
        position = new Point3d();
        boundingBox = new AABB3d();
        room_key = new RoomKey();
    }

    public static EnergyTankData FromObject(JsonData jsonData)
    {
        EnergyTankData energyTank = new EnergyTankData();

        // Upper left hand corner of the energy tank
        float x = jsonData["x"].IsInt ? (float)((int)jsonData["x"]) : (float)((double)jsonData["x"]);
        float y = jsonData["y"].IsInt ? (float)((int)jsonData["y"]) : (float)((double)jsonData["y"]);

        int game_id = SessionData.GetInstance().GameID;
        int room_x = (int)jsonData["room_x"];
        int room_y = (int)jsonData["room_y"];
        int room_z = (int)jsonData["room_z"];

        energyTank.room_key = new RoomKey(game_id, room_x, room_y, room_z);
        energyTank.energy_tank_id = (int)jsonData["energy_tank_id"];
        energyTank.energy = (uint)((int)jsonData["energy"]);
        energyTank.ownership = (GameConstants.eFaction)((int)jsonData["ownership"]);
        energyTank.position.Set(x, y, 0.0f);
        energyTank.boundingBox.SetBounds2d(
            x, y - ENERGY_TANK_WORLD_UNIT_SIZE,
            x + ENERGY_TANK_WORLD_UNIT_SIZE, y);

        return energyTank;
    }
}