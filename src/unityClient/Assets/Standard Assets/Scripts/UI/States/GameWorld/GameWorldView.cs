using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[System.Serializable]
public class GameWorldView : IWidgetEventListener
{
    public ButtonStyle buttonStyle;
    public CharacterWidgetStyle characterStyle;
    public MobWidgetStyle mobStyle;
    public EnergyTankWidgetStyle energyTankStyle;

    public GameWorldController GameWorldController { get; set; }

    private WidgetEventDispatcher m_widgetEventDispatcher;
	private WidgetGroup m_rootWidgetGroup;
    private WidgetGroup m_backgroundGroup;
    private WidgetGroup m_entityGroup;
    private WidgetGroup m_foregroundGroup;

    private TileGridWidget m_backgroundTiles;
    private TileGridWidget m_wallTiles;
    private TileGridWidget m_backgroundObjectTiles;
    private TileGridWidget m_foregroundObjectTiles;

    private WidgetGroup m_foregroundUIGroup;	
    private ButtonWidget m_logoutButton;
	private ButtonWidget m_firstEventButton;
	private ButtonWidget m_previousEventButton;
	private ButtonWidget m_playPauseEventButton;
	private ButtonWidget m_nextEventButton;
	private ButtonWidget m_lastEventButton;

    public void Start()
    {
		// Create the root widget group
		m_rootWidgetGroup = new WidgetGroup(null, Screen.width, Screen.height, 0.0f, 0.0f);
		m_rootWidgetGroup.SetWidgetEventListener(this);

        m_widgetEventDispatcher = new WidgetEventDispatcher();
        m_widgetEventDispatcher.Start(m_rootWidgetGroup);

        // Create the background
        m_backgroundGroup = new WidgetGroup(m_rootWidgetGroup, Screen.width, Screen.height, 0.0f, 0.0f);
        m_backgroundTiles = null;
        m_wallTiles = null;
        m_backgroundObjectTiles = null;

        // Create the foreground object group
		m_entityGroup = new WidgetGroup(m_rootWidgetGroup, Screen.width, Screen.height, 0.0f, 0.0f);

        // Create the foreground tile group
		m_foregroundGroup = new WidgetGroup(m_rootWidgetGroup, Screen.width, Screen.height, 0.0f, 0.0f);
        m_foregroundObjectTiles = null;

        // Create the foreground UI
		m_foregroundUIGroup = new WidgetGroup(m_rootWidgetGroup, Screen.width, Screen.height, 0.0f, 0.0f);
			
		// Create game button
        m_logoutButton = new ButtonWidget(m_foregroundUIGroup, buttonStyle, 0, 0, "Logout");

		// Create the event navigation buttons
		{
			float layoutX = 0;
            float layoutY = Screen.height - buttonStyle.Height;

            m_firstEventButton = new ButtonWidget(m_foregroundUIGroup, buttonStyle, layoutX, layoutY, "<<");
				
			layoutX += m_firstEventButton.Width;
            m_previousEventButton = new ButtonWidget(m_foregroundUIGroup, buttonStyle, layoutX, layoutY, "<");

            layoutX += m_previousEventButton.Width;
            m_playPauseEventButton = new ButtonWidget(m_foregroundUIGroup, buttonStyle, layoutX, layoutY, "||");

            layoutX += m_playPauseEventButton.Width;
            m_nextEventButton = new ButtonWidget(m_foregroundUIGroup, buttonStyle, layoutX, layoutY, ">");

            layoutX += m_nextEventButton.Width;
            m_lastEventButton = new ButtonWidget(m_foregroundUIGroup, buttonStyle, layoutX, layoutY, ">>");
		}
    }

    public void OnGUI()
    {
        m_rootWidgetGroup.OnGUI();
    }

    public void OnDestroy()
    {
        m_widgetEventDispatcher.OnDestroy();
        m_rootWidgetGroup.SetWidgetEventListener(null);
        m_rootWidgetGroup.OnDestroy();
    }

    public void Update()
    {
        m_widgetEventDispatcher.Update();
    }

	public void RebuildRoom(RoomData room)
	{
        if (m_backgroundTiles == null)
        {
            m_backgroundTiles = new TileGridWidget(m_backgroundGroup, "BackgroundTiles", Screen.width, Screen.height, 3, 0.0f, 0.0f);
        }

        if (m_wallTiles == null)
        {
			m_wallTiles = new TileGridWidget(m_backgroundGroup, "WallTiles", Screen.width, Screen.height, 2, 0.0f, 0.0f);
        }

        if (m_backgroundObjectTiles == null)
        {
			m_backgroundObjectTiles = new TileGridWidget(m_backgroundGroup, "BackgroundObjects", Screen.width, Screen.height, 1, 0.0f, 0.0f);
        }

        if (m_foregroundObjectTiles == null)
        {
			m_foregroundObjectTiles = new TileGridWidget(m_foregroundGroup, "ForegroundObjects", Screen.width, Screen.height, 0, 0.0f, 0.0f);
        }

        // Covert the tile IDs over to sprite sheet indices
        RoomTemplate roomTemplate = room.StaticRoomData.RoomTemplate;

        TileGridTemplate floorGrid = roomTemplate.FloorGridTemplate;
        m_backgroundTiles.LoadMap(
            floorGrid.TileIndices,
            floorGrid.RowCount, floorGrid.ColomnCount,
            floorGrid.TileSetImageName, floorGrid.TileWidth, floorGrid.TileHeight);

        TileGridTemplate wallGrid = roomTemplate.WallGridTemplate;
        m_wallTiles.LoadMap(
            wallGrid.TileIndices,
            wallGrid.RowCount, wallGrid.ColomnCount,
            wallGrid.TileSetImageName, wallGrid.TileWidth, wallGrid.TileHeight);

        TileGridTemplate backgroundObjGrid = roomTemplate.BackgroundObjectsTemplate;
        m_backgroundObjectTiles.LoadMap(
            backgroundObjGrid.TileIndices,
            backgroundObjGrid.RowCount, backgroundObjGrid.ColomnCount,
            backgroundObjGrid.TileSetImageName, backgroundObjGrid.TileWidth, backgroundObjGrid.TileHeight);

        TileGridTemplate foregroundObjGroup = roomTemplate.BackgroundObjectsTemplate;
        m_foregroundObjectTiles.LoadMap(
            foregroundObjGroup.TileIndices,
            foregroundObjGroup.RowCount, foregroundObjGroup.ColomnCount,
            foregroundObjGroup.TileSetImageName, foregroundObjGroup.TileWidth, foregroundObjGroup.TileHeight); 				
				
		//TODO: rebuildRoom: Use animated tile entities from the room template 
		/*var tileIdGrid:Array = room.staticRoomData.backgroundTileGrid;
		var tileIndices:Array = new Array();
		for (var listIndex:uint = 0; listIndex < tileIdGrid.length; listIndex++)
		{
			var tileId:uint = tileIdGrid[listIndex];
			var tileDescriptor:TileDescriptor= currentTileSet.getTileDescriptorById(tileId);
			var sheetIndex:int = tileDescriptor.sheetIndex;

			// Create an animated tile for any tile that has more than one frame
			if (tileDescriptor.frameCount > 1)
			{
				var tileColomn:uint = listIndex % room.staticRoomData.tileColomns;
				var tileRow:uint = uint(listIndex / room.staticRoomData.tileColomns);
				var tileX:Number = Number(tileColomn) * currentTileSet.TileWidth;
				var tileY:Number = Number(tileRow) * currentTileSet.TileHeight;
					
				new AnimatedTileWidget(m_backgroundGroup, currentTileSet, tileDescriptor, tileX, tileY);
			}
				
			tileIndices.push(sheetIndex);
		}*/			
	}

    public CharacterWidget AddCharacterWidget(CharacterData characterData)
    {
        // Constructor adds the widget to the entity widget group
        return new CharacterWidget(m_entityGroup, characterStyle, characterData);
    }

    public void RemoveCharacterWidget(CharacterWidget characterWidget)
    {
        m_entityGroup.RemoveWidget(characterWidget);
        characterWidget.OnDestroy();
    }

    public MobWidget AddMobWidget(MobData mobData)
    {
        // Constructor adds the widget to the entity widget group
        return new MobWidget(m_entityGroup, mobStyle, mobData);
    }

    public void RemoveMobWidget(MobWidget mobWidget)
    {
        m_entityGroup.RemoveWidget(mobWidget);
        mobWidget.OnDestroy();
    }		

    public EnergyTankWidget AddEnergyTankWidget(EnergyTankData energyTankData)
    {
        // Constructor adds the widget to the entity widget group
        return new EnergyTankWidget(m_entityGroup, energyTankStyle, energyTankData);
    }

    public void RemoveEnergyTankWidget(EnergyTankWidget energyTankWidget)
    {
        m_entityGroup.RemoveWidget(energyTankWidget);
        energyTankWidget.OnDestroy();
    }	
		
	public void SetEntitiesActive(bool flag)
	{
		m_entityGroup.Visible = flag;
	}
		
    public void SetUIVisible(bool visibile)
    {
        m_foregroundUIGroup.Visible = visibile;
    }

    public void RefreshEventControls(GameWorldModel gameWorldModel)
    {
        if (gameWorldModel.EventPlaybackMode == GameWorldModel.eGameEventMode.paused)
        {
            m_firstEventButton.Visible = !gameWorldModel.IsEventCursorAtFirstEvent;
            m_previousEventButton.Visible = !gameWorldModel.IsEventCursorAtFirstEvent;
            m_nextEventButton.Visible = !gameWorldModel.IsEventCursorAtLastEvent;
            m_lastEventButton.Visible = !gameWorldModel.IsEventCursorAtLastEvent;

            // When paused, only show the play/pause button with the "play" label
            m_playPauseEventButton.Visible = !gameWorldModel.IsEventCursorAtLastEvent || gameWorldModel.IsWaitingForEventCompletion;
            m_playPauseEventButton.Label = "|>";				
        }
        else
        {
            m_firstEventButton.Visible = false;
            m_previousEventButton.Visible = false;
            m_nextEventButton.Visible = false;
            m_lastEventButton.Visible = false;

            // When playing, only show the play/pause button with the "pause" label
            m_playPauseEventButton.Visible = !gameWorldModel.IsEventCursorAtLastEvent;
            m_playPauseEventButton.Label = "||";				
        }		
    }

	// UI Accessors	
	public WidgetGroup RootWidgetGroup
	{
		get { return m_rootWidgetGroup; }
	}

    public WidgetEventDispatcher WidgetEventDispatcher
    {
        get { return m_widgetEventDispatcher; }
    }

    public ButtonWidget LogoutButton
	{
		get { return m_logoutButton; }
	}

	// Events
	public void OnWidgetEvent(WidgetEvent widgetEvent)
	{
		switch (widgetEvent.EventType)
		{
		case WidgetEvent.eEventType.buttonClick:
			{
				if (widgetEvent.EventSource == m_logoutButton)
				{
					GameWorldController.OnLogoutClicked();
				}					
					
				if (widgetEvent.EventSource == m_firstEventButton)
				{
					GameWorldController.OnFirstEventClicked();
				}
					
				if (widgetEvent.EventSource == m_previousEventButton)
				{
					GameWorldController.OnPreviousEventClicked();
				}
					
				if (widgetEvent.EventSource == m_playPauseEventButton)
				{
					GameWorldController.OnPlayPauseClicked();
				}
					
				if (widgetEvent.EventSource == m_nextEventButton)
				{
					GameWorldController.OnNextEventClicked();
				}
					
				if (widgetEvent.EventSource == m_lastEventButton)
				{
					GameWorldController.OnLastEventClicked();
				}
			} break;
				
		case WidgetEvent.eEventType.mouseClick:
			{
                if (m_foregroundObjectTiles != null && widgetEvent.EventSource == m_foregroundObjectTiles)
                {
                    WidgetEvent.MouseClickEventParameters eventParameters =
                        widgetEvent.EventParameters as WidgetEvent.MouseClickEventParameters;
                    GameWorldController.OnCharacterMoveToRequest(eventParameters.localX, eventParameters.localY);
                }
			} break;

        case WidgetEvent.eEventType.mouseOver:
        case WidgetEvent.eEventType.mouseOut:
		case WidgetEvent.eEventType.mouseMove:
			{
                // Forward mouse events on to the overlay view for cursor updates
                GameWorldController.OverlayController.View.OnWidgetEvent(widgetEvent);
			} break;
		}
	}
}
