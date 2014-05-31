using LitJson;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Navigation;

public class GameData 
{
	private const int IRC_MAX_NICK_LENGTH = 16;
		
	// The id of the current character
	private int m_characterID;
		
	// The id of the current game
	private int m_gameID;

	// The name of the current game
	private string m_gameName;
		
	// IRC Settings
	private bool m_ircEnabled;
	private string m_ircServer;
	private uint m_ircPort;
	private string m_ircNick;
	private string m_ircGameChannel;
	private byte[] m_ircEncryptionKey;
	private bool m_ircEncryptionEnabled;
		
    // Character data for all characters in the game
    private Dictionary<int, CharacterData> m_characters; // character_id -> Character Data
		        
    // List of relevant events that have happened to the player since they have been in the game
    public List<GameEvent> m_eventList;
	public uint m_eventCursor;
		
	// Data specific to each room
	private Dictionary<string, RoomData> m_roomDataCache; // room hash -> RoomData
	private RoomKey m_currentRoomKey;
				
	public GameData() 
	{
		reset();
	}
		
	public void reset()
	{
		m_characterID = -1;
		m_gameID = -1;
		m_gameName = "";
			
		m_ircEnabled = false;
		m_ircServer= "";
		m_ircPort = 0;
		m_ircNick = "";
		m_ircGameChannel= "";
		m_ircEncryptionKey = null;
		m_ircEncryptionEnabled = false;
			
		m_characters = new Dictionary<int, CharacterData>();
			
		//m_eventList = new List<GameEvent>();
		m_eventCursor = 0;
			
		m_roomDataCache = new Dictionary<string, RoomData>();
		m_currentRoomKey = new RoomKey();			
	}		
		
	// Game Properties	
	public int GameID
	{
        get { return m_gameID; }
		set { m_gameID = value; }
	}
		
	public string GameName
	{
		get { return m_gameName; }
        set { m_gameName= value; }
	}
			
	// IRC Settings
	public bool IRCEnabled
	{
		get { return m_ircEnabled; }
        set { m_ircEnabled= value; }
	}

    public string IRCServer
    {
        get { return m_ircServer; }
        set { m_ircServer = value; }
    }
				
	public uint IRCPort
	{
		get { return m_ircPort; }
        set { m_ircPort= value; }
	}
		
	public string IRCGameChannel
	{
		get { return m_ircGameChannel; }
        set { m_ircGameChannel= value; }
	}
		
	public string IRCNick
	{
		get { return m_ircNick; }
        set { m_ircNick= value; }
	}
		
	public byte[] IRCEncryptionKey
	{
        get { return m_ircEncryptionEnabled ? m_ircEncryptionKey : null; }
	}
				
	// Room State
    public RoomData CurrentRoom
    {
        get { return GetCachedRoomData(m_currentRoomKey); }
    }		
		
	public RoomKey CurrentRoomKey
	{
		get { return m_currentRoomKey; }
        set { m_currentRoomKey = new RoomKey(value); }
	}

    public bool HasRoomData(RoomKey roomKey)
    {
        return m_roomDataCache.ContainsKey(roomKey.GetHashKey());
    }

    public RoomData GetCachedRoomData(RoomKey roomKey)
    {
        RoomData roomData = null;

        return m_roomDataCache.TryGetValue(roomKey.GetHashKey(), out roomData) ? roomData : null;
    }

    public void SetCachedRoomData(RoomKey roomKey, RoomData roomData)
    {
        string roomKeyString = roomKey.GetHashKey();

        if (m_roomDataCache.ContainsKey(roomKeyString))
        {
            m_roomDataCache[roomKeyString] = roomData;
        }
        else
        {
            m_roomDataCache.Add(roomKeyString, roomData);
        }
    }
			
	// Character State
	public int CharacterID
	{
		get { return m_characterID; }
        set { m_characterID= value; }
	}
			
	public Dictionary<int, CharacterData> CharacterMap
	{
		get { return m_characters; }
	}
		
	public CharacterData GetCharacterById(int character_id)
	{
        CharacterData characterData = null;

		return m_characters.TryGetValue(character_id, out characterData) ? characterData : null;
	}
		
	public void SetCharacterById(int character_id, CharacterData characterState)
	{
        if (m_characters.ContainsKey(character_id))
        {
            m_characters[character_id] = characterState;
        }
        else
        {
            m_characters.Add(character_id, characterState);
        }
	}
		
	// Game Event Handling
    public int EventCount
    {
        get { return m_eventList.Count; }
    }

	public uint EventCursor
	{
		get { return m_eventCursor; }
	}
		
	public bool IsEventCursorAtFistEvent
	{
		get { return m_eventCursor == 0; }
	}

    public bool IsEventCursorAtLastEvent
    {
        get { return m_eventCursor >= m_eventList.Count; }
    }

    public bool ReverseEventCursor(GameWorldController gameWorldController)
    {
        bool success = false;

        if (!IsEventCursorAtFistEvent)
        {
            m_eventCursor = m_eventCursor - 1;

            m_eventList[(int)m_eventCursor].UndoEvent(gameWorldController);

            success = true;
        }

        return success;
    }

    public bool AdvanceEventCursor(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
    {
        bool success = false;

        if (!IsEventCursorAtLastEvent)
        {
            GameEvent gameEvent= m_eventList[(int)m_eventCursor];
            string chatString = gameEvent.ToChatString(gameWorldController);

            gameEvent.ApplyEvent(gameWorldController, onComplete);

            if (chatString.Length > 0)
            {
                gameWorldController.SendChatWindowText(chatString);
            }

            m_eventCursor = m_eventCursor + 1;

            success = true;
        }

        return success;
    }
		
	// Server Response Parsers
	public bool ParseFullGameStateResponse(JsonData response)
	{
		CharacterData character = null;
		bool success = true;
			
		// Parse the character data
		{
			m_characters = new Dictionary<int, CharacterData>();
				
			JsonData characterObjects = response["characters"];
			for (int characterListIndex= 0; characterListIndex < characterObjects.Count; characterListIndex++)
			{
                JsonData characterObject= characterObjects[characterListIndex];
				CharacterData characterData = CharacterData.FromObject(characterObject);
					
				// Use the existing game name and ID store on the game state
				// This should have already been set by the time we made the request for the data.
				characterData.game_id = this.m_gameID;
				characterData.game_name = this.m_gameName;
					
				SetCharacterById(characterData.character_id, characterData);
			}
				
			character = GetCharacterById(this.CharacterID);
			success = character != null;
		}		
					
		// Parse the IRC data
		{
			m_ircEnabled = (bool)response["irc_enabled"];
			m_ircServer= (string)response["irc_server"];
			m_ircPort = (uint)((int)response["irc_port"]);				
			m_ircGameChannel= "ARPG_" + this.GameID.ToString();
			m_ircEncryptionKey = System.Convert.FromBase64String((string)response["irc_encryption_key"]);		
			m_ircEncryptionEnabled = (bool)response["irc_encryption_enabled"];
				
			m_ircNick = character.character_name+"_"+character.character_id.ToString()+"_"+m_gameID.ToString();
			m_ircNick = m_ircNick.Substring(0, Math.Min(IRC_MAX_NICK_LENGTH, m_ircNick.Length));
		}
					
		// Parse the room data for the current character and cache it
        if (success)
        {
            success = ParseRoomDataResponse(response);
        }
			
		// Parse the game event list
        if (success)
        {
            m_eventList = new List<GameEvent>();

            // Append all of the events contained in the response
            ParseEventResponse(response);

            // Set the event cursor to the end of the event list
            m_eventCursor = (uint)m_eventList.Count;
        }			
			
		return success;
	}

    public bool ParseRoomDataResponse(JsonData response)
    {
        RoomData roomData = RoomData.FromObject(response);
        bool success = false;

        if (roomData != null)
        {
            m_currentRoomKey = roomData.RoomKey;
            SetCachedRoomData(roomData.RoomKey, roomData);
            success = true;
        }

        return success;
    }

    public void ParseEventResponse(JsonData response)
    {
        JsonData eventObjects = response["event_list"];

        if (eventObjects != null)
        {
            for (int list_index = 0; list_index < eventObjects.Count; list_index++)
            {
                JsonData eventObject = eventObjects[list_index];
                GameEvent gameEvent = GameEvent.FromObject(eventObject);

                m_eventList.Add(gameEvent);
            }
        }
    }		
}
