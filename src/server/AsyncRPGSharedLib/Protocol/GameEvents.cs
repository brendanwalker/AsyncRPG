using System;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.Protocol
{
    //NOTE: If you add an event make sure to update the following:
    // * De/SerializeParametersByType() in GameEventQueries.cs
    // * Add a game event structure below
    [Serializable]
    public enum eGameEventType
    {
        none = -1,

        // Character Events
        character_joined_game = 0,
        character_left_game = 1,
        character_died = 2,
        character_moved = 3,
        character_attacked = 4,
        character_portaled = 5,

        // Mob Events
        mob_spawned = 6,
        mob_died = 7,
        mob_moved = 8,
        mob_attacked = 9,
        mob_dialog = 10,
        mob_player_prop_spotted = 11,
        mob_player_prop_lost_track = 12,
        mob_ai_prop_spotted = 13,
        mob_energy_tank_prop_spotted = 14,

        // Energy Tank Events
        energy_tank_drained = 15,
        energy_tank_hacked = 16,

        k_game_event_type_count = 17
    }

    [Serializable]
    public class GameEvent
    {
        public long timestamp = 0;
        public eGameEventType event_type = eGameEventType.none;
        public GameEventParameters parameters = null;
    }

    [Serializable]
    public class GameEventParameters
    {
        public virtual eGameEventType GetEventType()
        {
            return eGameEventType.none;
        }
    }

    // Character Events
    [Serializable]
    public class GameEvent_CharacterJoinedGame : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.character_joined_game;
        }

        public CharacterState character_state;
    }

    [Serializable]
    public class GameEvent_CharacterLeftGame : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.character_left_game;
        }

        public CharacterState character_state;
    }

    [Serializable]
    public class GameEvent_CharacterDied : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.character_died;
        }

        public int character_id = -1;
        public int killer_mob_id = -1;
    }

    [Serializable]
    public class GameEvent_CharacterAttacked : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.character_attacked;
        }

        public int character_id = -1;
        public int mob_id = -1;
    }

    [Serializable]
    public class GameEvent_CharacterMoved : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.character_moved;
        }

        public int character_id = -1;
        public int room_x = 0;
        public int room_y = 0;
        public int room_z = 0;
        public double from_x = 0;
        public double from_y = 0;
        public double from_z = 0;
        public double from_angle = 0;
        public double to_x = 0;
        public double to_y = 0;
        public double to_z = 0;
        public double to_angle = 0;
    }

    [Serializable]
    public class GameEvent_CharacterPortaled : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.character_portaled;
        }

        public int character_id = -1;
        public int from_room_x = 0;
        public int from_room_y = 0;
        public int from_room_z = 0;
        public double from_x = 0;
        public double from_y = 0;
        public double from_z = 0;
        public double from_angle = 0;
        public int to_room_x = 0;
        public int to_room_y = 0;
        public int to_room_z = 0;
        public double to_x = 0;
        public double to_y = 0;
        public double to_z = 0;
        public double to_angle = 0;
    }

    // Mob Events
    [Serializable]
    public class GameEvent_MobSpawned : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.mob_spawned;
        }

        public MobState mob_state;
    }

    [Serializable]
    public class GameEvent_MobDied : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.mob_died;
        }

        public int mob_id = -1;
        public int killer_character_id = -1;
    }

    [Serializable]
    public class GameEvent_MobMoved : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.mob_moved;
        }

        public int mob_id = -1;
        public int room_x = 0;
        public int room_y = 0;
        public int room_z = 0;
        public double from_x = 0;
        public double from_y = 0;
        public double from_z = 0;
        public double from_angle = 0;
        public double to_x = 0;
        public double to_y = 0;
        public double to_z = 0;
        public double to_angle = 0;
    }

    [Serializable]
    public class GameEvent_MobAttacked : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.mob_attacked;
        }

        public int mob_id = -1;
        public int character_id = -1;
    }

    [Serializable]
    public class GameEvent_MobDialog : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.mob_dialog;
        }

        public int mob_id = -1;
        public string dialog = "";
    }

    [Serializable]
    public class GameEvent_MobPlayerPropSpotted : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.mob_player_prop_spotted;
        }

        public int mob_id = -1;
        public int character_id = -1;
        public double x = 0;
        public double y = 0;
        public double z = 0;
    }

    [Serializable]
    public class GameEvent_MobPlayerPropLostTrack : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.mob_player_prop_lost_track;
        }

        public int mob_id = -1;
        public int character_id = -1;
        public double x = 0;
        public double y = 0;
        public double z = 0;
    }

    [Serializable]
    public class GameEvent_MobAIPropSpotted : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.mob_ai_prop_spotted;
        }

        public int mob_id = -1;
        public int spotted_mob_id = -1;
        public double x = 0;
        public double y = 0;
        public double z = 0;
    }

    [Serializable]
    public class GameEvent_MobEnergyTankPropSpotted : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.mob_energy_tank_prop_spotted;
        }

        public int mob_id = -1;
        public int energy_tank_id = -1;
    }

    [Serializable]
    public class GameEvent_EnergyTankDrained : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.energy_tank_drained;
        }

        public int energy_tank_id = -1;
        public int drainer_id = -1;
        public GameConstants.eFaction drainer_faction = GameConstants.eFaction.neutral;
        public int energy_drained = 0;
    }

    [Serializable]
    public class GameEvent_EnergyTankHacked : GameEventParameters
    {
        public override eGameEventType GetEventType()
        {
            return eGameEventType.energy_tank_hacked;
        }

        public int energy_tank_id = -1;
        public GameConstants.eFaction energy_tank_faction = GameConstants.eFaction.neutral;
        public int hacker_id = -1;
        public GameConstants.eFaction hacker_faction = GameConstants.eFaction.neutral;
    }
}
