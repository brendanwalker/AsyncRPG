using System;

namespace AsyncRPGSharedLib.Protocol
{
    [Serializable]
    public class JSONResponse
    {
    }

    [Serializable]
    public class BasicResponse : JSONResponse
    {
        public string result = "";
    }

    [Serializable]
    public class CharacterListResponse : BasicResponse
    {
        public CharacterState[] character_list;
    }

    [Serializable]
    public class CharacterGetPositionResponse : BasicResponse
    {
        public int character_id = -1;
        public int game_id = -1;
        public int room_x = 0;
        public int room_y = 0;
        public int room_z = 0;
        public double x = 0;
        public double y = 0;
        public double z = 0;
        public double angle = 0;
    }

    [Serializable]
    public class CharacterMoveResponse : BasicResponse
    {
        // List of events relevant to the requesting character since they last moved
        public GameEvent[] event_list;
    }

    [Serializable]
    public class CharacterPortalResponse : BasicResponse
    {
        // List of events relevant to the requesting character since they last moved
        public GameEvent[] event_list;
    }

    [Serializable]
    public class CharacterHackEnergyTankResponse : BasicResponse
    {
        // List of events relevant to the requesting character since they hacked the energy tank
        public GameEvent[] event_list;
    }

    [Serializable]
    public class CharacterDrainEnergyTankResponse : BasicResponse
    {
        // List of events relevant to the requesting character since they drained the energy tank
        public GameEvent[] event_list;
    }

    [Serializable]
    public class GameListResponse : BasicResponse
    {
        public GameResponseEntry[] game_list;
    }

    [Serializable]
    public class GamePongResponse : BasicResponse
    {
        public GameEvent[] event_list;
    }

    [Serializable]
    public class WorldGetRoomDataResponse : BasicResponse
    {
        public int room_x;
        public int room_y;
        public int room_z;
        public double world_x;
        public double world_y;
        public double world_z;
        public PortalEntry[] portals;
        public MobState[] mobs;
        public EnergyTankState[] energyTanks;
        public StaticRoomData data;
    }

    [Serializable]
    public class CharacterStateResponse : BasicResponse
    {
        public CharacterState character_state;
    }


    [Serializable]
    public class WorldGetFullGameStateResponse : BasicResponse
    {
        // IRC Details
        public bool irc_enabled;
        public string irc_server;
        public int irc_port;
        public string irc_encryption_key;
        public bool irc_encryption_enabled;

        // Character data for all Characters in the game
        public CharacterState[] characters;

        // List of events relevant to the requesting character since they last logged in
        public GameEvent[] event_list;

        // Room Data for the room that requesting player is in
        public int room_x;
        public int room_y;
        public int room_z;
        public double world_x;
        public double world_y;
        public double world_z;
        public PortalEntry[] portals;
        public MobState[] mobs;
        public EnergyTankState[] energyTanks;
        public StaticRoomData data;
    }
}
