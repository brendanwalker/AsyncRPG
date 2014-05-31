using UnityEngine;
using System.Collections;

public class ChatWindowModel 
{
	private ChatWindowController _chatWindowController;
	private IRCSession _ircSession;

	public bool ChannelOpen { get; private set; }
				
	public ChatWindowModel(ChatWindowController chatWindowController) 
	{
		_chatWindowController = chatWindowController;
		_ircSession = null;


		ChannelOpen = false;
	}
		
	public void Start()
	{
		ConnectToChatServer();
	}
		
	public void OnDestroy()
	{
		DisconnectFromChatServer();
	}
		
	public void Update()
	{			
		bool wasChannelOpen= ChannelOpen;
		bool isChannelOpen= ChannelOpen;
			
		if (_ircSession != null)
		{
			_ircSession.Update();
			isChannelOpen = _ircSession.ChannelOpen;
		}
			
		if (wasChannelOpen != isChannelOpen)
		{
			_chatWindowController.OnIRCStatusChanged();
			ChannelOpen = isChannelOpen;
		}
	}
		
	public void SendChatText(string chatInput)
	{
		if (_ircSession != null && ChannelOpen)
		{
			_ircSession.SendChat(chatInput);
		}
	}
		
	public void SendGameEvent(GameEvent gameEvent)
	{
		if (_ircSession != null && ChannelOpen)
		{
			_ircSession.SendGameEvent(gameEvent);
		}
	}		
		
	public void SendCharacterUpdatedGameEvent()
	{
		SessionData sessionData= SessionData.GetInstance();
		GameData gameData= sessionData.CurrentGameData;
			
		GameEvent_CharacterUpdated gameEvent = new GameEvent_CharacterUpdated();
		gameEvent.CharacterID = gameData.CharacterID;
			
		SendGameEvent(gameEvent);
	}
		
	private void ConnectToChatServer()
	{	
		if (_ircSession == null)
		{
			SessionData sessionData= SessionData.GetInstance();
			GameData gameData= sessionData.CurrentGameData;
			CharacterData character= gameData.GetCharacterById(gameData.CharacterID);

			if (gameData.IRCEnabled)
			{
				_ircSession = new IRCSession(
					gameData.IRCServer, 
					gameData.IRCPort, 
					gameData.IRCNick, // Nickname
					gameData.IRCNick, // User,
					"Async RPG Character "+character.character_name,
					gameData.IRCGameChannel, 
					gameData.IRCEncryptionKey);					
						
				_ircSession.AddOutputStream(_chatWindowController.OnChatMessage);
				_ircSession.AddGameEventStream(_chatWindowController.OnGameEvent);
				_ircSession.AddLoggingStream((string mesg) => { Debug.Log(mesg); });
				_ircSession.Connect();
			}
		}
	}
		
	private void DisconnectFromChatServer()
	{
		if (_ircSession != null)
		{
			_ircSession.Disconnect();
			_ircSession = null;
		}
	}	
}
