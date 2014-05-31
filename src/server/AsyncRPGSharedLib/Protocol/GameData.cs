using System;

namespace AsyncRPGSharedLib.Protocol
{
    [Serializable]
    public class GameResponseEntry
    {
        public int game_id;
        public string game_name;
        public int owner_account_id;
        public string owner_account_name;
        public string[] character_names;
    }
}
