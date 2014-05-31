using UnityEngine;
using System.Collections;
using System;
using LitJson;

public class GameEvent 
{
    private static DateTime UNIX_EPOCH= new DateTime(1970, 1, 1);

    public delegate void OnEventCompleteDelegate();

    public enum eEventType
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

        // Server Event Count
        k_game_event_type_count = 17,

        // Client Only Events
        character_updated= 100,
    }

	// Character Event Classes	
    public DateTime Timestamp { get; set; }
    public eEventType EventType { get; set; }
		
	public GameEvent() 
	{
		Timestamp = DateTime.Now;
		EventType = eEventType.none;
	}
			
	private static Type GetEventClassForType(eEventType eventType)
	{
		Type eventClass = null;
			
        switch(eventType)
        {
        case eEventType.character_joined_game:
            eventClass = typeof(GameEvent_CharacterJoinedGame);
            break;
        case eEventType.character_left_game:
            eventClass = typeof(GameEvent_CharacterLeftGame);
            break;
        case eEventType.character_died:
            eventClass = typeof(GameEvent_CharacterDied);
            break;
        case eEventType.character_moved:
            eventClass = typeof(GameEvent_CharacterMoved);
            break;
        case eEventType.character_portaled:
            eventClass = typeof(GameEvent_CharacterPortaled);
            break;				
        case eEventType.character_attacked:
            eventClass = typeof(GameEvent_CharacterAttacked);
            break;
        case eEventType.mob_spawned:
            eventClass = typeof(GameEvent_MobSpawned);
            break;
        case eEventType.mob_died:
            eventClass = typeof(GameEvent_MobDied);
            break;
        case eEventType.mob_moved:
            eventClass = typeof(GameEvent_MobMoved);
            break;
        case eEventType.mob_attacked:
            eventClass = typeof(GameEvent_MobAttacked);
            break;
        case eEventType.mob_dialog:
            eventClass = typeof(GameEvent_MobDialog);
            break;
        case eEventType.mob_player_prop_spotted:
            eventClass = typeof(GameEvent_MobPlayerPropSpotted);
            break;
        case eEventType.mob_player_prop_lost_track:
            eventClass = typeof(GameEvent_MobPlayerPropLostTrack);
            break;
        case eEventType.mob_ai_prop_spotted:
            eventClass = typeof(GameEvent_MobAIPropSpotted);
            break;
        case eEventType.mob_energy_tank_prop_spotted:
            eventClass = typeof(GameEvent_MobEnergyTankPropSpotted);
            break;
        case eEventType.energy_tank_drained:
            eventClass = typeof(GameEvent_EnergyTankDrained);
            break;
        case eEventType.energy_tank_hacked:
            eventClass = typeof(GameEvent_EnergyTankHacked);
            break;				
        case eEventType.character_updated:
            eventClass = typeof(GameEvent_CharacterUpdated);
            break;			
        }
			
		return eventClass;
	}
		
	public JsonData ToObject()
	{
		JsonData jsonObject = new JsonData();
			
		jsonObject["event_type"] = (int)EventType;
        jsonObject["timestamp"] = (Timestamp - UNIX_EPOCH).Milliseconds;
		jsonObject["parameters"] = new JsonData();
			
		AppendParameters(jsonObject["parameters"]);
			
		return jsonObject;
	}
		
	public static GameEvent FromObject(JsonData jsonObject)
	{	
		GameEvent gameEvent = null;			
		eEventType gameEventType= (eEventType)((int)jsonObject["event_type"]);
		Type gameEventClass = GameEvent.GetEventClassForType(gameEventType);
			
		if (gameEventClass != null)
		{
            long epochTicks = JsonUtilities.ParseLong(jsonObject, "timestamp") + UNIX_EPOCH.Ticks;

			gameEvent = (GameEvent)Activator.CreateInstance(gameEventClass);
				
			gameEvent.EventType = gameEventType;
            gameEvent.Timestamp = new DateTime(epochTicks);			
				
			// Parse the child event fields based on type
			gameEvent.ParseParameters( jsonObject["parameters"] as JsonData );
		}
			
		return gameEvent;
	}
		
	protected virtual void ParseParameters(JsonData parameters)
	{
		// Nothing to do in the base class
	}
		
	protected virtual void AppendParameters(JsonData parameters)
	{
		// Nothing to do in the base class
	}		
		
	public virtual string ToChatString(GameWorldController gameWorldController)
	{
		return "[" + Timestamp.ToShortTimeString() + "] ";
	}
		
	public virtual void ApplyEvent(GameWorldController gameWorldController, OnEventCompleteDelegate onComplete)
	{
		// Nothing to do in the base class
	}

	public virtual void UndoEvent(GameWorldController gameWorldController)
	{
		// Nothing to do in the base class
	}		
}
