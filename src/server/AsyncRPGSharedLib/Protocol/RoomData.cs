using System;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.Protocol
{
    [Serializable]
    public class RoomObjectEntry
    {
        public int id = -1;
        public double x = 0;
        public double y = 0;
    }

    [Serializable]
    public class StaticRoomData
    {
        public string room_template_name;
        public RoomObjectEntry[] objects;
    }

    [Serializable]
    public class PortalEntry
    {
        public int id = -1; // Portal ID
        public int target_portal_id = -1;
        public double x0 = 0; // Room relative bounding box
        public double y0 = 0;
        public double x1 = 0;
        public double y1 = 0;
    }

    [Serializable]
    public class EnergyTankState
    {
        public int energy_tank_id = -1;
        public int energy = 0;
        public int ownership = (int)GameConstants.eFaction.neutral;
        public int room_x = 0;
        public int room_y = 0;
        public int room_z = 0;
        public double x = 0;
        public double y = 0;
    }
}
