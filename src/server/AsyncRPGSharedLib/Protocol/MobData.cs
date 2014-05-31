using System;

namespace AsyncRPGSharedLib.Protocol
{
    [Serializable]
    public class MobState
    {
        public int mob_id;
        public string mob_type_name;
        public int health;
        public int energy;
        public int game_id;
        public int room_x;
        public int room_y;
        public int room_z;
        public double x;
        public double y;
        public double z;
        public double angle;
    }
}
