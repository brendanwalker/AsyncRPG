using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using LitJson;

public class IRCSession 
{
    public delegate void OutputStreamDelegate(string message);
    public delegate void GameEventStreamDelegate(GameEvent gameEvent);
    public delegate void LogStreamDelegate(string message);

    public enum eState
    {
        disconnected,
        init_connection,
        verifying_nick,
        joining_game_channel,
        in_game_channel,
        failed
    }

    private const uint MAX_RECONNECTION_ATTEMPTS = 3;

    //From  http://tools.ietf.org/html/rfc2812#section-1.3		
	private const uint MAX_IRC_MESSAGE_LENGTH = 512;
	private const uint MAX_CHANNEL_NAME_LENGTH = 50;
	private const uint MAX_PRIVMSG_PREFIX_LENGTH = 10 + MAX_CHANNEL_NAME_LENGTH; // "PRIVMSG <MAX_CHANNEL_NAME_LENGTH> :"

	// Encrypted chat strings are base64 encoded, which makes them 4/3 longer.
	// The final max chat string length has to take this into account.
	// We add some slop to account for encoding overhead
	private const uint ENCODING_OVERHEAD_MARGIN = 16;		
	public const uint MAX_CHAT_STRING_LENGTH = (3 * (MAX_IRC_MESSAGE_LENGTH - MAX_PRIVMSG_PREFIX_LENGTH) / 4) - ENCODING_OVERHEAD_MARGIN;
		
	// Prefix for game event JSON string that we smuggle through IRC
	private const string GAME_EVENT_PREFIX = "%GameEvent:";

	private string m_ircPrimaryNick;
	private string m_ircNick;
	private string m_ircUser;
	private string m_ircFullIdentity;
	private string m_ircServerAddress;
	private uint m_ircServerPort;
	private string m_ircPrimaryChannelName;
	private string m_ircChannelName;
	private string m_ircChannelKey;
				
	private IRCEventDispatcher m_irc;
		
	private Blowfish m_encryptor;
		
	private List<OutputStreamDelegate> m_outputStreams;
	private List<GameEventStreamDelegate> m_gameEventStreams;
	private List<LogStreamDelegate> m_loggingStreams;
		
	private eState m_state;
	private uint m_reconnectionAttempts;
		
	private Dictionary<string, int> m_nickToCharacterIdMap;

	public IRCSession(
        string serverAddress, 
		uint port, 
		string nick, 
		string user, 
		string ident, 
		string channel, 
		byte[] encryptionKey) 
	{
		m_ircServerAddress = serverAddress;
		m_ircServerPort = port;
		m_ircPrimaryChannelName = channel;
		m_ircChannelName = m_ircPrimaryChannelName;
		m_ircPrimaryNick = nick;
		m_ircNick = m_ircPrimaryNick;
		m_ircUser = user;
		m_ircFullIdentity = ident;
			
		m_irc = new IRCEventDispatcher();

        m_irc.AddEventListener(IRCEvent.EVENT_STATUSMESSAGE, StatusMessageEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_SOCKOPEN, SockOpenEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_ERROR, ErrorEventHandler);

        m_irc.AddEventListener(IRCEvent.EVENT_SOCKERROR, SockErrorEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_ACTIVEMESSAGE, ActiveMessageEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_PING, PingEventHandler);

        m_irc.AddEventListener(IRCEvent.EVENT_WELCOME, WelcomeEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_NOTICE, NoticeEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_JOIN, JoinEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_PRIVMSG, PrivmsgEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_TOPIC, TopicEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_MODE, ModeEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_NICK, NickEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_KICK, KickEventHandler);
        m_irc.AddEventListener(IRCEvent.EVENT_QUIT, QuitEventHandler);		
			
		if (encryptionKey != null)
		{
            // TODO: Generate a real random number for IV, pass it to other clients
            // Blowfish needs an initialization vector.
            // Normally this would be a cryptographically secure random number.
            // This value can be transmitted in public somehow, but I don't feel
            // like doing the plumbing for that yet.
            // So, for now, we make a pseudo-random IV seeded by the IRC port.
            System.Random rng = new System.Random((int)port);
            byte[] initialization_vector= new byte[8];
                
            rng.NextBytes(initialization_vector);
			m_encryptor = new Blowfish(encryptionKey);
            m_encryptor.IV= initialization_vector;

			// The IRC channel key all users use to access the channel is the 
			// channel name encrypted using the IRC encryption key in Base64 encoding
            m_ircChannelKey = EncryptAndBase64Encode(channel);
		}
		else 
		{
			m_ircChannelKey = "";
		}
			
		m_outputStreams = new List<OutputStreamDelegate>();
		m_gameEventStreams = new List<GameEventStreamDelegate>();
		m_loggingStreams = new List<LogStreamDelegate>();
			
		m_state = eState.disconnected;
			
		ClearIrcNickToCharacterIdMap();
	}

	public void Connect()
	{
		if (m_state == eState.disconnected || m_state == eState.failed)
		{
			m_reconnectionAttempts = 0;
			SetState(eState.init_connection);				
		}
	}
		
	public void Disconnect()
	{
		if (m_irc.IsConnected)
		{
			m_irc.Disconnect(m_ircUser + " left the game");
			SetState(eState.disconnected);
		}
	}

	public void Update()
	{	
		if (m_state == eState.failed)
		{
			if (m_reconnectionAttempts < MAX_RECONNECTION_ATTEMPTS)
			{
				SetState(eState.init_connection);
			}
			else 
			{
				SetState(eState.disconnected);
			}	
		}

        // Update the IRC socket
        m_irc.Update();
	}
		
	public bool ChannelOpen
	{
        get { return m_state == eState.in_game_channel;  }
	}

	private void SetState(eState newState)
	{
        m_state = newState;

		switch(newState)
		{
		case eState.disconnected:
			{
				m_reconnectionAttempts = 0;
				m_ircNick = m_ircPrimaryNick; // Reset back to our primary nick
				m_ircChannelName = m_ircPrimaryChannelName; // Reset back to our primary room name

				ClearIrcNickToCharacterIdMap();
			}
			break;
		case eState.init_connection:
			{
				m_reconnectionAttempts++;
					
				LogText("[Connecting to "+m_ircServerAddress+":"+m_ircServerPort+", Attempt #"+m_reconnectionAttempts+"]");	
				OutputText("[Connecting to " + m_ircServerAddress + ":" + m_ircServerPort + ", Attempt #" + m_reconnectionAttempts + "]");
					
				m_irc.Connect(m_ircServerAddress, m_ircServerPort, m_ircNick, m_ircUser, m_ircFullIdentity, true);
			}
			break;
		case eState.verifying_nick:
			{
				LogText("[Verifying nickname " + m_ircNick + " available]");
				OutputText("[Verifying nickname " + m_ircNick + " available]");
			}
			break;
		case eState.joining_game_channel:
			{
				LogText("[joining game chat room " + m_ircChannelName + "]");
				OutputText("[joining game chat room " + m_ircChannelName + "]");
					
				// Auto-join the channel for the game and leave any other channels
				if (m_ircChannelKey.Length > 0)
				{
					SendCommand("/join #" + m_ircChannelName +" "+m_ircChannelKey+ " 0");
				}
				else 
				{
					SendCommand("/join #" + m_ircChannelName + " 0");
				}					
			}
			break;
		case eState.in_game_channel:
			{				
				if (m_ircChannelKey.Length > 0)
				{
					// Set the channel key in case it isn't already set
					SendCommand("/MODE #" + m_ircChannelName + " +k "+m_ircChannelKey);
				}
					
				// Mark the channel as secret so that it's not advertised to the rest of the server
				SendCommand("/MODE #" + m_ircChannelName + " +s");		
			}
			break;				
		case eState.failed:
			{
				LogText("[Connection Failed]");
				OutputText("[Connection Failed]");
					
				if (m_irc.IsConnected)
				{
					m_irc.Disconnect(m_ircUser + " left the game");
				}				
			}
			break;								
		}			
	}

    public void SendChat(String message)
	{
        if (m_irc.IsConnected && ChannelOpen)
        {
            IRCPrivmsgEvent privmsgEvent = new IRCPrivmsgEvent();
				
            // Nuder any commands by stripping off the "/"
            if (message.IndexOf("/") == 0)
            {
                message = message.Substring(1);
            }
				
            privmsgEvent.Channel = "#" + m_ircChannelName;				
            privmsgEvent.Message = message;
				
            m_irc.OnInput(privmsgEvent, EncryptAndBase64Encode);
        }
        else 
        {
            LogText("[failed to send message]: "+message);
            OutputText("[failed to send message]: "+message);				
        }
	}
		
	public void SendGameEvent(GameEvent gameEvent)
	{
        string eventString = gameEvent.ToObject().ToJson();
			
        if (m_irc.IsConnected && ChannelOpen)
        {
            IRCPrivmsgEvent privmsgEvent = new IRCPrivmsgEvent();
								
            privmsgEvent.Channel = "#" + m_ircChannelName;				
            privmsgEvent.Message = GAME_EVENT_PREFIX + eventString;

            m_irc.OnInput(privmsgEvent, EncryptAndBase64Encode);
        }
        else 
        {
            LogText("[failed to game event message]: "+eventString);
            OutputText("[failed to game event message]: "+eventString);				
        }				
	}
		
	public void SendCommand(string command) 
	{
		if (m_irc.IsConnected)
		{
			m_irc.ProcessInput(command);
		}
	}
		
	public void AddOutputStream(OutputStreamDelegate stream)
	{
		m_outputStreams.Add(stream);
	}
		
	private void OutputText(string text)
	{
		foreach(OutputStreamDelegate stream in m_outputStreams)
		{
			stream(text);
		}
	}

	public void AddGameEventStream(GameEventStreamDelegate stream)
	{
		m_gameEventStreams.Add(stream);
	}
		
	public void OutputGameEvent(GameEvent gameEvent)
	{
		foreach (GameEventStreamDelegate stream in m_gameEventStreams)
		{
			stream(gameEvent);
		}			
	}
		
	public void AddLoggingStream(LogStreamDelegate stream)
	{
		m_loggingStreams.Add(stream);
	}
		
	private void LogText(string text)
	{
		foreach (LogStreamDelegate stream in m_loggingStreams)
		{
			stream(text);
		}
	}
		
	private void ClearIrcNickToCharacterIdMap()
	{
		m_nickToCharacterIdMap = new Dictionary<string, int>();
	}
		
	private void AddIrcNickToCharacterIdMap(string nick, int characterId)
	{
		if (nick.Length > 0)
		{
            if (m_nickToCharacterIdMap.ContainsKey(nick))
            {
                m_nickToCharacterIdMap[nick]= characterId;
            }
            else
            {
                m_nickToCharacterIdMap.Add(nick, characterId);
            }
		}
	}
		
	private void RemoveIrcNickFromCharacterIdMap(string nick)
	{
		if (m_nickToCharacterIdMap.ContainsKey(nick))
		{
			m_nickToCharacterIdMap.Remove(nick);
		}
	}
		
	private int GetCharacterIdForIrcNick(string nick)
	{
        int characterId;
            
		return m_nickToCharacterIdMap.TryGetValue(nick, out characterId) ? characterId : -1;
	}

    private string EncryptAndBase64Encode(string plainText)
    {
        if (m_encryptor != null)
        {
            byte[] plaintextBytes = Encoding.ASCII.GetBytes(plainText);
            byte[] encryptedBytes = m_encryptor.Encrypt_CBC(plaintextBytes);

            return Convert.ToBase64String(encryptedBytes);
        }
        else
        {
            return plainText;
        }
    }

    private string Base64DecodeAndDecrypt(string base64CypherText)
    {
        if (m_encryptor != null)
        {
            byte[] cypherTextBytes= Convert.FromBase64String(base64CypherText);
            byte[] plainTextBytes= m_encryptor.Decrypt_CBC(cypherTextBytes);

            // Trim any null terminators off the end
            return Encoding.ASCII.GetString(plainTextBytes).TrimEnd(new char[] { '\0' });
        }
        else
        {
            return base64CypherText;
        }
    }

	// Game Specific Events
	private void PostThisPlayerJoinedGameChannel()
	{
		int currentCharacterID = SessionData.GetInstance().CharacterID;
		GameEvent_CharacterUpdated characterUpdateEvent = new GameEvent_CharacterUpdated();
			
		// Add ourselves to the username -> character id mapping table
		AddIrcNickToCharacterIdMap(m_ircNick, currentCharacterID);
			
		// Send the event
		characterUpdateEvent.CharacterID = currentCharacterID;						
		SendGameEvent(characterUpdateEvent);
	}			
		
	private void PostOtherPlayerLeftGameChannel(string nick)
	{
		int characterId = GetCharacterIdForIrcNick(nick);
			
		if (characterId != -1)
		{
			// Locally force the character to update
			GameEvent_CharacterUpdated characterUpdatedEvent = new GameEvent_CharacterUpdated();
				
			characterUpdatedEvent.CharacterID = characterId;
			OutputGameEvent(characterUpdatedEvent);
				
			// Remove the kicked individual from the nick -> character id mapping
			RemoveIrcNickFromCharacterIdMap(nick);				
		}			
	}
		
	private void GameEventHandler(string fromNick, GameEvent gameEvent) 
	{
		// Use the CharacterUpdated events to keep track of nick -> character ID mappings
		if (gameEvent is GameEvent_CharacterUpdated)
		{
            GameEvent_CharacterUpdated characterUpdatedEvent = gameEvent as GameEvent_CharacterUpdated;
				
			if (GetCharacterIdForIrcNick(fromNick) == -1)
			{
				AddIrcNickToCharacterIdMap(fromNick, characterUpdatedEvent.CharacterID);
			}
		}
			
		OutputGameEvent(gameEvent);
	}

	// IRC Events
	private void WelcomeEventHandler(IRCEvent e)		
	{
		LogText("[Welcome]: " + m_irc.GetLastStatusMessage());	
			
		if (m_state == eState.verifying_nick)
		{
			SetState(eState.joining_game_channel);
		}			
	}

	private void NoticeEventHandler(IRCEvent e)
	{
        IRCNoticeEvent noticeEvent = (IRCNoticeEvent)e;

        LogText("[notice] " + noticeEvent.User.Nick + ": " + noticeEvent.Text);
	}

	private void JoinEventHandler(IRCEvent e)
	{
        IRCJoinEvent joinEvent = (IRCJoinEvent)e;
		string properChannelName = "#" + m_ircChannelName;

        LogText("[" + joinEvent.User.Nick + " joined channel" + joinEvent.Channel + "]");
			
		// Ignore the redundant ops user join
        if (joinEvent.User.Nick[0] != '@')
		{
            if (joinEvent.User.Nick == m_ircNick)
			{
				if (m_state == eState.joining_game_channel && joinEvent.Channel == properChannelName)
				{
					SetState(eState.in_game_channel);						
						
					// Notify other players with a game event to ping the server state since we joined
					PostThisPlayerJoinedGameChannel();
				}
			}
				
			if (joinEvent.Channel == properChannelName)
			{
				OutputText(joinEvent.User.Nick + " joined the game");
			}
		}
	}

	private void PrivmsgEventHandler(IRCEvent e) 
	{
        IRCPrivmsgEvent privmsgEvent= (IRCPrivmsgEvent)e;
		string resultMessage = privmsgEvent.Message;	
		string properChannelName = "#" + m_ircChannelName;		
			
		if (privmsgEvent.Channel == properChannelName)
		{
			// If the encryptor is active, assume that the incoming chat is encrypted 
			if (m_encryptor != null)
			{
				try
				{
                    resultMessage= Base64DecodeAndDecrypt(privmsgEvent.Message);
				}
				catch (Exception)
				{
					resultMessage = "(UNENCRYPTED)" + privmsgEvent.Message;
				}
			}		
				
			// Decode game events smuggled through IRC
			if (resultMessage.IndexOf(GAME_EVENT_PREFIX) == 0)
			{
				string gameEventString = resultMessage.Substring(GAME_EVENT_PREFIX.Length);
				JsonData gameEventObject = null;
					
				try
				{
					gameEventObject = JsonMapper.ToObject(gameEventString);
				}
				catch (Exception ex)
				{
                    Debug.LogException(ex);
					gameEventObject = null;
				}
					
				if (gameEventObject != null)
				{
					GameEvent gameEvent = GameEvent.FromObject(gameEventObject);
						
					if (gameEvent != null)
					{
						GameEventHandler(privmsgEvent.User.Nick, gameEvent);
					}
					else 
					{
						LogText("Error parsing game event object: " + gameEventString);
					}
				}
				else
				{
					LogText("Malformed game event JSON string: " + gameEventString);
				}
			}
			// All other text gets emitted as chat from some user
			else
			{
				OutputText("[" + privmsgEvent.User.Nick + "] " + resultMessage);
			}
		}
		else 
		{
            LogText("[private mesg] " + privmsgEvent.User.Nick + ": " + privmsgEvent.Message);
		}
	}

	private void TopicEventHandler(IRCEvent e)
	{
        IRCTopicEvent topicEvent = (IRCTopicEvent)e;

        switch (topicEvent.Mode)
		{
            case IRCTopicEvent.eModeType.set_by:
                LogText("[mod set, channel=" + topicEvent.Channel + "] " + topicEvent.Message);
				break;
            case IRCTopicEvent.eModeType.no_topic:
                LogText("[no topic set, channel=" + topicEvent.Channel + "] " + topicEvent.Message);
				break;
            case IRCTopicEvent.eModeType.topic:
                LogText("[topic set, channel=" + topicEvent.Channel + "] " + topicEvent.Message);
				break;					
		}			
	}

	private void ModeEventHandler(IRCEvent e)
	{
        IRCModeEvent modeEvent = (IRCModeEvent)e;

        if (modeEvent.User != null)
		{
            LogText("[Mode " + modeEvent.Mode + " set by " + modeEvent.User.Nick + " in channel " + modeEvent.Channel);
		}
		else 
		{
            LogText("[Mode " + modeEvent.Mode + "set in channel " + modeEvent.Channel);
		}
	}

	private void NickEventHandler(IRCEvent e)
	{
        IRCNickEvent nickEvent = (IRCNickEvent)e;

        LogText("[user " + nickEvent.OldUser.Nick + " changed nick to " + nickEvent.NewUser.Nick);
	}

	private void KickEventHandler(IRCEvent e)
	{
        IRCKickEvent kickEvent = (IRCKickEvent)e;
		string victimNick = kickEvent.Victim.Nick;
		string properChannelName = "#" + m_ircChannelName;

        LogText("[" + kickEvent.Agressor.Nick + " kicked " + victimNick + " from " + kickEvent.Channel + "] " + kickEvent.Reason);

        if (kickEvent.Channel == properChannelName)
		{			
			if (victimNick == m_ircNick)
			{
				// We got kicked.
				// Try to re-connect to the game channel
				m_reconnectionAttempts = 0;
				SetState(eState.failed);
			}
			else 
			{
				// Someone else got kicked
				PostOtherPlayerLeftGameChannel(victimNick);
			}

            OutputText("[" + kickEvent.Agressor.Nick + " kicked " + m_ircNick + " from game] " + kickEvent.Reason);				
		}			
	}

	private void QuitEventHandler(IRCEvent e)
	{
        IRCQuitEvent quitEvent= (IRCQuitEvent)e;
		string nick = quitEvent.User.Nick;
			
		PostOtherPlayerLeftGameChannel(nick);			
		LogText("[" + nick + " quit] " + quitEvent.Message);
		OutputText("[" + nick + " quit] " + quitEvent.Message);			
	}

	private void PingEventHandler(IRCEvent e)
	{
		LogText("[ping]");
	}

	// Message Events
	public void StatusMessageEventHandler(IRCEvent e)
	{
		LogText("[Status]: "+m_irc.GetLastStatusMessage());	
	}
		
	public void ActiveMessageEventHandler(IRCEvent e)
	{
		LogText("[Active Message]: " + m_irc.GetLastActiveMessage());
			
		switch(m_state)
		{
		case eState.verifying_nick:
			OutputText("[Can't Verify Nick] " + m_irc.GetLastActiveMessage());
			m_ircNick = m_ircNick+"_"; // Try changing the nick we use in case some jerk already took it
			SetState(eState.failed);
			break;				
		case eState.joining_game_channel:
			OutputText("[Can't Join Channel] " + m_irc.GetLastActiveMessage());
			m_ircChannelName = m_ircChannelName + "_"; // Try changing the channel name in case some jerk already took it.
			SetState(eState.failed);
			break;
		}			
	}

	// Socket Events
	private void SockErrorEventHandler(IRCEvent e)
	{
		LogText("[Socket Error] "+m_irc.GetLastError());
		OutputText("[Socket Error]");
		SetState(eState.failed);
	}
				
	public void ErrorEventHandler(IRCEvent e)
	{
        IRCErrorEvent errorEvent= (IRCErrorEvent)e;
		LogText("[General Error]" + errorEvent.Message);
			
		switch(m_state)
		{
		case eState.verifying_nick:
			OutputText("[Can't Verify Nick] " + errorEvent.Message);
			m_ircNick = m_ircNick+"_"; // Try changing the nick we use in case some jerk already took it
			SetState(eState.failed);
			break;				
		case eState.joining_game_channel:
			OutputText("[Can't Join Channel] " + errorEvent.Message);
			m_ircChannelName = m_ircChannelName + "_"; // Try changing the channel name in case some jerk already took it.
			SetState(eState.failed);
			break;
		}
	}

    public void SockOpenEventHandler(IRCEvent e)
	{
		LogText("[Connected!]");	
		OutputText("[Connected!]");
			
		// Wait for nickname verification 
		SetState(eState.verifying_nick);
	}	
}
