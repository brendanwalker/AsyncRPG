using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[System.Serializable]
public class SelectCharacterView : IWidgetEventListener
{
    public CharacterPanelStyle characterPanelStyle;
    public ScrollListStyle scrollListStyle;
    public ButtonStyle buttonStyle;
    public CharacterThumbnailStyle characterThumbnailStyle;

    public SelectCharacterController SelectCharacterController { get; set; }

    private const float SCROLL_LIST_X = 4;
    private const float SCROLL_LIST_Y = 4;

    private const float CHARACTER_PANEL_X = 144;
    private const float CHARACTER_PANEL_Y = 50;

    private const float STATS_LABEL_WIDTH = 70;
	private const float STATS_LABEL_HEIGHT = 20;
	private const float BORDER_WIDTH = 6;

	private WidgetGroup m_rootWidgetGroup;
		
	// Character Panel
	private CharacterPanelWidget m_characterPanel;
    private ButtonWidget m_characterCreateButton;
    private ButtonWidget m_characterSelectButton;
    private ButtonWidget m_characterDeleteButton;
		
	// Scroll Bar
    private ScrollListWidget m_characterScrollList;

    public void Start()
    {
        float viewWidth = CHARACTER_PANEL_X + characterPanelStyle.BackgroundWidth;
        float viewHeight = Math.Max(SCROLL_LIST_Y + scrollListStyle.Width, CHARACTER_PANEL_Y + characterPanelStyle.BackgroundHeight);

		// Create the root widget group
		m_rootWidgetGroup = new WidgetGroup(null, viewWidth, viewHeight, 0.0f, 0.0f);
		m_rootWidgetGroup.SetWidgetEventListener(this);
			
		// Game list
        m_characterScrollList = 
            new ScrollListWidget(
                m_rootWidgetGroup, 
			    (ScrollListWidget parentGroup, object parameters) => 
			    {
				    return new CharacterThumbnailWidget(
                        parentGroup,
                        characterThumbnailStyle,
                        parameters as CharacterData);
			    }, 
                scrollListStyle,
                SCROLL_LIST_X, SCROLL_LIST_Y);
			
		// Character panel
        m_characterPanel = new CharacterPanelWidget(m_rootWidgetGroup, characterPanelStyle, CHARACTER_PANEL_X, CHARACTER_PANEL_Y);
        float panelWidth = m_characterPanel.Width - 2.0f * BORDER_WIDTH;
			
		// Create game button
        m_characterCreateButton = new ButtonWidget(m_characterPanel, buttonStyle, 0, 0, "Create");
		m_characterCreateButton.SetLocalPosition(
            BORDER_WIDTH + panelWidth / 3 - m_characterCreateButton.Width,
            m_characterPanel.Height - m_characterCreateButton.Height - 5);
			
		// Select game button
        m_characterSelectButton = new ButtonWidget(m_characterPanel, buttonStyle, 0, 0, "Select");
        m_characterSelectButton.SetLocalPosition(
            BORDER_WIDTH + (2 * panelWidth) / 3 - m_characterSelectButton.Width,
            m_characterPanel.Height - m_characterSelectButton.Height - 5);
        m_characterSelectButton.Visible = false;

		// Delete game button
        m_characterDeleteButton = new ButtonWidget(m_characterPanel, buttonStyle, 0, 0, "Delete");
        m_characterDeleteButton.SetLocalPosition(
            BORDER_WIDTH + (3 * panelWidth) / 3 - m_characterDeleteButton.Width,
            m_characterPanel.Height - m_characterDeleteButton.Height - 5);
        m_characterDeleteButton.Visible = false;			
			
		// Initially hide all game data
		m_characterPanel.HideCharacterData();
    }

    public void OnGUI()
    {
        m_rootWidgetGroup.OnGUI();
    }

    public void OnDestroy()
    {
        m_rootWidgetGroup.OnDestroy();
    }

	public void RebuildCharacterList(List<object> gameList)
	{
        m_characterScrollList.SetListData(gameList);

        if (m_characterScrollList.Length > 0)
		{
			m_characterSelectButton.Visible = true;
			m_characterDeleteButton.Visible = true; 
		}
		else
		{
            m_characterPanel.HideCharacterData();
            m_characterSelectButton.Visible = false;
            m_characterDeleteButton.Visible = false;
		}			
	}
				
	public void OnSelectedCharacterChanged(int scrollIndex)
	{
        CharacterData characterEntry = SelectCharacterController.Model.GetCharacterEntry(scrollIndex);

		// Update the data in the game info panel
        m_characterPanel.ShowCharacterData(characterEntry);
			
		// Notify the controller
        SelectCharacterController.OnSelectedCharacterChanged(scrollIndex);
	}
		
	// IWidgetEventListener
	public void OnWidgetEvent(WidgetEvent widgetEvent)
	{
		if (widgetEvent.EventSource == m_characterCreateButton)
		{
            SelectCharacterController.OnCharacterCreateClicked();
		}
        else if (widgetEvent.EventSource == m_characterDeleteButton)
		{
            SelectCharacterController.OnCharacterDeleteClicked();
		}
        else if (widgetEvent.EventSource == m_characterSelectButton)
		{
            SelectCharacterController.OnCharacterSelectClicked();
		}
        else if (widgetEvent.EventSource == m_characterScrollList && 
                 widgetEvent.EventType == WidgetEvent.eEventType.listSelectionChanged)
		{
            WidgetEvent.ListSelectionChangedEventParameters eventParameters= 
                (WidgetEvent.ListSelectionChangedEventParameters)widgetEvent.EventParameters;

            OnSelectedCharacterChanged(eventParameters.ListIndex);
		}
	}
}
