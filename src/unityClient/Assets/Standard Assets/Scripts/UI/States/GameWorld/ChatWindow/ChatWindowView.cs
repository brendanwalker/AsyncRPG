using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ChatWindowStyle
{
    public WindowStyle windowStyle;
    public int chatInputHeight= 20;
}


public class ChatWindowView : IWidgetEventListener
{	
	private ChatWindowController _chatWindowController;
    private ChatWindowStyle m_chatWindowStyle;
		
	private WidgetGroup _rootWidgetGroup;
	private WindowWidget _window;
	private ScrollTextWidget _chatText;
	private TextEntryWidget _chatInput;
		
	public ChatWindowView(ChatWindowController chatWindowController) 
	{
		_chatWindowController = chatWindowController;
        m_chatWindowStyle= chatWindowController.ParentController.chatWindowStyle;
	}
		
	public void Start(WidgetGroup parentGroup)
	{
		_rootWidgetGroup = new WidgetGroup(parentGroup, 0, 0, 0.0f, 0.0f);
		_rootWidgetGroup.SetWidgetEventListener(this);
			
		_window = 
            new WindowWidget(
                _rootWidgetGroup, 
                "Chat", 
                m_chatWindowStyle.windowStyle, 
                Screen.width - m_chatWindowStyle.windowStyle.WindowWidth - 3, 0);					
			
		_chatText = 
            new ScrollTextWidget(
                _window, 
                m_chatWindowStyle.windowStyle.WindowWidth, 
                m_chatWindowStyle.windowStyle.WindowHeight 
                - m_chatWindowStyle.chatInputHeight
                - m_chatWindowStyle.windowStyle.TitleBarHeight, 
				0.0f, 0.0f, 
				"");
        _chatText.FontSize= 12;
			
		_chatInput = 
            new TextEntryWidget(
                _window, 
                m_chatWindowStyle.windowStyle.WindowWidth, 
                m_chatWindowStyle.chatInputHeight, 
                0, 
                m_chatWindowStyle.windowStyle.WindowHeight - m_chatWindowStyle.chatInputHeight, 
				"");
        _chatText.FontSize= 12;
		_chatInput.MaxLength = (int)IRCSession.MAX_CHAT_STRING_LENGTH;
		SetChatInputVisible(true);
	}
		
	public void OnDestroy()
	{
		_rootWidgetGroup.OnDestroy();
	}
		
	public void Update()
	{
			
	}
		
	public void AppendChatText(string text)
	{
		_chatText.AppendLine(text);
	}
		
	public void SetChatInputVisible(bool visible)
	{
		_chatInput.Visible = visible;
		_chatInput.EnableReturnSignal= visible; // Want a notification when the return key is hit						
        _chatInput.EnableTabSignal= visible; // Want a notification when the tab key is hit
	}
		
	// Events		
	public void OnWidgetEvent(WidgetEvent widgetEvent)
	{
        bool handeled = false;

		if (widgetEvent.EventSource == _chatInput)
		{
			if (widgetEvent.EventType == WidgetEvent.eEventType.textInputReturn)
			{
				string inputText = _chatInput.Text;
					
				// Attempt to execute the chat string as a debug command first
				// If it works, well get a valid result string back.
				string commandResult = DebugRegistry.ExecuteDebugCommand(inputText);

                _chatInput.Text= "";

				if (commandResult.Length > 0)
				{
					_chatWindowController.OnSystemMessage(commandResult);						
				}
				else
				{
					_chatWindowController.OnChatTextInput(inputText);
				}

                // No need to forward this event on
                handeled = true;
			}
			else if (widgetEvent.EventType == WidgetEvent.eEventType.textInputTab)
			{
				string partialText= _chatInput.Text;
				List<string> completions = DebugRegistry.CompleteDebugCommand(partialText);
					
				if (completions.Count > 1)
				{
					foreach (string completion in completions)
					{
						_chatWindowController.OnSystemMessage(completion);
					}
						
					_chatInput.Text = partialText;
				}
				else if (completions.Count == 1)
				{
					_chatInput.Text = completions[0];

                    // Force the cursor to the end of the line
                    {
                        TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

                        editor.selectPos = _chatInput.Text.Length + 1;
                        editor.pos = _chatInput.Text.Length + 1;
                    }
				}

                // No need to forward this event on
                handeled = true;
			}
		}

        // Forward unhanded events onto our parent
        if (!handeled)
        {
            _rootWidgetGroup.ParentWidgetGroup.OnWidgetEvent(widgetEvent);
        }
	}
}