using System;

namespace AsyncRPGSharedLib.Protocol
{
    [Serializable]
    public class CharacterState
    {
        public int game_id;
        public string game_name;
        public int character_id;
        public string character_name;
        public int archetype;
        public int gender;
        public int picture_id;
        public int power_level;
        public int energy;
        public int room_x;
        public int room_y;
        public int room_z;
        public double x;
        public double y;
        public double z;
        public double angle;
    }
}
