using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[System.Serializable]
public class SelectGameView : IWidgetEventListener
{
    public GamePanelStyle gamePanelStyle;
    public ScrollListStyle scrollListStyle;
    public ButtonStyle buttonStyle;
    public GameThumbnailStyle gameThumbnailStyle;

    public SelectGameController SelectGameController { get; set; }

    private const float SCROLL_LIST_X = 4;
    private const float SCROLL_LIST_Y = 4;

    private const float GAME_PANEL_X = 144;
    private const float GAME_PANEL_Y = 50;

    private const float STATS_LABEL_WIDTH = 70;
	private const float STATS_LABEL_HEIGHT = 20;
	private const float BORDER_WIDTH = 6;

	private WidgetGroup m_rootWidgetGroup;
		
	// Character Panel
	private GamePanelWidget m_gamePanel;		
	private ButtonWidget m_gameCreateButton;
	private ButtonWidget m_gameSelectButton;
	private ButtonWidget m_gameDeleteButton;
		
	// Scroll Bar
    private ScrollListWidget m_gameScrollList;

    public void Start()
    {
        float viewWidth = GAME_PANEL_X + gamePanelStyle.Width;
        float viewHeight = Math.Max(SCROLL_LIST_Y + scrollListStyle.Width, GAME_PANEL_Y + gamePanelStyle.Height);

		// Create the root widget group
		m_rootWidgetGroup = new WidgetGroup(null, viewWidth, viewHeight, 0.0f, 0.0f);
		m_rootWidgetGroup.SetWidgetEventListener(this);
			
		// Game list
		m_gameScrollList = 
            new ScrollListWidget(
                m_rootWidgetGroup, 
			    (ScrollListWidget parentGroup, object parameters) => 
			    {
				    return new GameThumbnailWidget(
                        parentGroup,
                        gameThumbnailStyle,
                        parameters as GameResponseEntry, 
						0.0f, 0.0f);
			    }, 
                scrollListStyle,
                SCROLL_LIST_X, SCROLL_LIST_Y);
			
		// Game panel
        m_gamePanel = new GamePanelWidget(m_rootWidgetGroup, gamePanelStyle, GAME_PANEL_X, GAME_PANEL_Y);
		float panelWidth= m_gamePanel.Width - 2.0f * BORDER_WIDTH;
			
		// Create game button
		m_gameCreateButton = new ButtonWidget(m_gamePanel, buttonStyle, 0, 0, "Create");
		m_gameCreateButton.SetLocalPosition(
            BORDER_WIDTH + panelWidth/3 - m_gameCreateButton.Width,
			m_gamePanel.Height - m_gameCreateButton.Height - 5);
			
		// Select game button
		m_gameSelectButton = new ButtonWidget(m_gamePanel, buttonStyle, 0, 0, "Select");
		m_gameSelectButton.SetLocalPosition(
            BORDER_WIDTH + (2*panelWidth)/3 - m_gameSelectButton.Width,
			m_gamePanel.Height - m_gameSelectButton.Height - 5);
		m_gameSelectButton.Visible = false;

		// Delete game button
		m_gameDeleteButton = new ButtonWidget(m_gamePanel, buttonStyle, 0, 0, "Delete");
		m_gameDeleteButton.SetLocalPosition(
            BORDER_WIDTH + (3*panelWidth)/3 - m_gameDeleteButton.Width,
			m_gamePanel.Height - m_gameSelectButton.Height - 5);
		m_gameDeleteButton.Visible = false;			
			
		// Initially hide all game data
		m_gamePanel.HideGameData();
    }

    public void OnGUI()
    {
        m_rootWidgetGroup.OnGUI();
    }

    public void OnDestroy()
    {
        m_rootWidgetGroup.OnDestroy();
    }

	public void RebuildGameList(List<object> gameList)
	{				
		m_gameScrollList.SetListData(gameList);
			
		if (m_gameScrollList.Length > 0)
		{
			m_gameSelectButton.Visible = true;
			m_gameDeleteButton.Visible = true; 
		}
		else
		{
			m_gamePanel.HideGameData();
			m_gameSelectButton.Visible = false;
			m_gameDeleteButton.Visible = false;
		}			
	}
				
	public void OnSelectedGameChanged(int scrollIndex)
	{
        GameResponseEntry gameEntry = SelectGameController.Model.GetGameEntry(scrollIndex);

		// Update the data in the game info panel
		m_gamePanel.ShowGameData(gameEntry);
			
		// Notify the controller
        SelectGameController.OnSelectedGameChanged(scrollIndex);
	}
		
	// IWidgetEventListener
	public void OnWidgetEvent(WidgetEvent widgetEvent)
	{
		if (widgetEvent.EventSource == m_gameCreateButton)
		{
            SelectGameController.OnGameCreateClicked();
		}
        else if (widgetEvent.EventSource == m_gameDeleteButton)
		{
            SelectGameController.OnGameDeleteClicked();
		}
        else if (widgetEvent.EventSource == m_gameSelectButton)
		{
            SelectGameController.OnGameSelectClicked();
		}
        else if (widgetEvent.EventSource == m_gameScrollList && widgetEvent.EventType == WidgetEvent.eEventType.listSelectionChanged)
		{
            WidgetEvent.ListSelectionChangedEventParameters eventParameters= 
                (WidgetEvent.ListSelectionChangedEventParameters)widgetEvent.EventParameters;

            OnSelectedGameChanged(eventParameters.ListIndex);
		}
	}
}
