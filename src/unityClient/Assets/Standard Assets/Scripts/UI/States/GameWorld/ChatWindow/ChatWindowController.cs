using UnityEngine;
using System.Collections;

public class ChatWindowController
{
	private GameWorldController _gameWorldController;
	private ChatWindowModel _chatWindowModel;
	private ChatWindowView _chatWindowView;

    public ChatWindowModel Model
    {
        get { return _chatWindowModel; }
    }

    public ChatWindowView View
    {
        get { return _chatWindowView; }
    }

    public GameWorldController ParentController
    {
        get { return _gameWorldController; }
    }
	
	public ChatWindowController(GameWorldController gameWorldController) 
	{
		_gameWorldController = gameWorldController;
		_chatWindowModel= new ChatWindowModel(this);
		_chatWindowView = new ChatWindowView(this);
	}
			
	public void Start(WidgetGroup parentWidgetGroup)
	{	
		_chatWindowView.Start(parentWidgetGroup);						
		_chatWindowModel.Start();			
	}
		
	public void OnDestroy()
	{	
		_chatWindowModel.OnDestroy();
		_chatWindowView.OnDestroy();
	}	
		
	public void Update()
	{
		_chatWindowModel.Update();
		_chatWindowView.Update();
	}
		
	// Model Events
	public void OnSystemMessage(string message)
	{
		_chatWindowView.AppendChatText(message);
	}

	public void OnChatMessage(string message)
	{
		_chatWindowView.AppendChatText(message);
	}
		
	public void OnGameEvent(GameEvent gameEvent)
	{
		// Notify the game about the game event we just heard about
		_gameWorldController.OnGameEventPosted(gameEvent);
			
		//TODO: Not sure if I wan't to post this to chat or not
		// I think I actually only want to do this for events received via the WebService
			
		// Display a human readable version of the event to the chat window
		_chatWindowView.AppendChatText(gameEvent.ToChatString(_gameWorldController));			
	}
		
	public void OnIRCStatusChanged()
	{
		if (_chatWindowModel.ChannelOpen)
		{
			_gameWorldController.OnJoinedIRCChannel();
		}
	}
		
	// View Events
	public void OnChatTextInput(string chatInput)
	{
		if (_chatWindowModel.ChannelOpen)
		{
			_chatWindowModel.SendChatText(chatInput);
		}
	}
}
