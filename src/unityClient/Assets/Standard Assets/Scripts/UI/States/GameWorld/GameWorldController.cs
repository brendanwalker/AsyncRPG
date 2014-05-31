using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

[RequireComponent(typeof(AudioSource))]
public class GameWorldController : GameState
{
    public GameWorldView gameWorldView;
    public ContextOverlayStyle contextOverlayStyle;
    public ChatWindowStyle chatWindowStyle;
    public AudioClip successSound;
    public AudioClip alertSound;
    public AudioClip errorSound;

    private GameWorldModel m_gameWorldModel;
    private ChatWindowController m_chatWindowController;
    private GameWorldDebug m_gameWorldDebug;
    private ContextOverlayController m_contextOverlayController;
    private bool m_readyForInput;

    public bool IsReadyForInput
    {
        get { return m_readyForInput; }
    }

    public GameWorldModel Model
    {
        get { return m_gameWorldModel; }
    }

    public GameWorldView View
    {
        get { return gameWorldView; }
    }

    public ContextOverlayController OverlayController
    {
        get { return m_contextOverlayController; }
    }

    public override void Start()
    {
        base.Start();

        m_gameWorldModel = new GameWorldModel(this);
        m_gameWorldDebug = new GameWorldDebug(this);
        m_contextOverlayController = new ContextOverlayController(this);
        gameWorldView.GameWorldController = this;
        m_chatWindowController = null;

        gameWorldView.Start();
        m_contextOverlayController.Start(gameWorldView.RootWidgetGroup);
        m_gameWorldModel.Start();
        m_gameWorldDebug.Start();

        // Ask the server for the complete state of the game
        m_gameWorldModel.RequestFullGameState();

        // $TODO Play title music

        // Fade in the background
        m_readyForInput = false;
        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeIn, 1.0f, () =>
        {
            m_readyForInput = true;
        });

    }

    public override void OnDestroy()
    {
        if (m_chatWindowController != null)
        {
            m_chatWindowController.OnDestroy();
            m_chatWindowController = null;
        }

        m_contextOverlayController.OnDestroy();
        m_gameWorldModel.OnDestroy();
        gameWorldView.OnDestroy();
        m_gameWorldDebug.OnDestroy();
        base.OnDestroy();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (m_chatWindowController != null)
        {
            m_chatWindowController.Update();
        }

        m_contextOverlayController.Update();
        m_gameWorldModel.Update();
        m_gameWorldDebug.Update();
        gameWorldView.Update();
    }

    // Update is called once per frame
    public override void OnGUI()
    {
        base.OnGUI();

        gameWorldView.OnGUI();
        m_gameWorldDebug.OnGUI();
    }

	public void SendCharacterUpdatedGameEvent()
	{
        if (m_chatWindowController != null)
        {
            m_chatWindowController.Model.SendCharacterUpdatedGameEvent();
        }
	}

    public void SendChatWindowText(string message)
    {
        if (m_chatWindowController != null)
        {
            m_chatWindowController.OnChatMessage(message);
        }
    }
		
	public void ShowErrorDialog(string message)
	{
        audio.PlayOneShot(errorSound, 1.0f);

        if (m_chatWindowController != null)
        {
            // TODO: Post this to a real dialog
            m_chatWindowController.View.AppendChatText("Error: " + message);
        }
	}

	// View Event Handlers
	public void OnLogoutClicked()
	{
        if (!m_gameWorldModel.IsLogoutRequestPending)
        {
            gameWorldView.SetUIVisible(false);
            m_gameWorldModel.RequestLogout();
        }
	}	

	public void OnFirstEventClicked()
	{
        if (m_gameWorldModel.EventPlaybackMode == GameWorldModel.eGameEventMode.paused)
        {
            // Roll back events to the beginning
            while (m_gameWorldModel.ReverseCurrentEvent(this)) { }

            // Update which event controls should be active
            gameWorldView.RefreshEventControls(m_gameWorldModel);
        }
	}
		
	public void OnPreviousEventClicked()
	{
        if (m_gameWorldModel.EventPlaybackMode == GameWorldModel.eGameEventMode.paused)
        {
            // Step back one event
            m_gameWorldModel.ReverseCurrentEvent(this);

            // Update which event controls should be active
            gameWorldView.RefreshEventControls(m_gameWorldModel);
        }
	}
		
	public void OnPlayPauseClicked()
	{
        if (m_gameWorldModel.EventPlaybackMode == GameWorldModel.eGameEventMode.paused)
        {
            m_gameWorldModel.EventPlaybackMode = GameWorldModel.eGameEventMode.playing;
            gameWorldView.SetEntitiesActive(true);
        }
        else
        {
            m_gameWorldModel.EventPlaybackMode = GameWorldModel.eGameEventMode.paused;
            gameWorldView.SetEntitiesActive(false);
        }

        OnEventPlaybackModeChanged();
	}
		
	public void OnNextEventClicked()
	{
        if (m_gameWorldModel.EventPlaybackMode == GameWorldModel.eGameEventMode.paused)
        {
            // Step ahead one event
            m_gameWorldModel.AdvanceCurrentEvent(this);

            // Update which event controls should be active
            gameWorldView.RefreshEventControls(m_gameWorldModel);
        }			
	}
		
	public void OnLastEventClicked()
	{
        if (m_gameWorldModel.EventPlaybackMode == GameWorldModel.eGameEventMode.paused)
        {
            // Roll back events to the beginning
            while (m_gameWorldModel.AdvanceCurrentEvent(this)) { }
				
            // Update which event controls should be active
            gameWorldView.RefreshEventControls(m_gameWorldModel);
        }			
	}		
		
	public void OnCharacterMoveToRequest(float x, float y)
	{
        Point2d pixelPoint = new Point2d(x, y);
        Point3d roomPoint = GameConstants.ConvertPixelPositionToRoomPosition(pixelPoint);

        if (!m_gameWorldModel.IsAsyncRequestPending &&
            m_gameWorldModel.IsEventCursorAtLastEvent &&
            !m_gameWorldModel.IsEventPlaybackActive &&
            m_gameWorldModel.IsWorldPointOnNavMesh(roomPoint))
        {
            m_gameWorldModel.RequestCharacterMove(roomPoint);
        }
	}
		
	public void OnEventPlaybackModeChanged()
	{
		gameWorldView.RefreshEventControls(m_gameWorldModel);
	}	
		
	// Model Event Handlers
	public void OnGameLoaded()
	{
		GameData currentGame = m_gameWorldModel.CurrentGame;
		CharacterData currentCharacter = currentGame.GetCharacterById( m_gameWorldModel.CurrentCharacterID );
		RoomKey currentRoomKey = currentCharacter.CurrentRoomKey;
		
		// Rebuild all the entities that are part of this room
		OnRoomLoaded(currentRoomKey);

        // Roll back the recent events so that the player can see them played back
        while (m_gameWorldModel.ReverseCurrentEvent(this)) { }

        m_gameWorldModel.EventPlaybackMode = GameWorldModel.eGameEventMode.playing;
        gameWorldView.RefreshEventControls(m_gameWorldModel);

        // Re-Connect to the IRC back-end
        {
            if (m_chatWindowController != null)
            {
                m_chatWindowController.OnDestroy();
                m_chatWindowController = null;
            }

            m_chatWindowController = new ChatWindowController(this);
            m_chatWindowController.Start(gameWorldView.RootWidgetGroup);
        }
	}
		
	public void OnRoomLoaded(RoomKey roomKey)
	{
		RoomData currentRoom = m_gameWorldModel.GetRoomData(roomKey);
			
		// Get a list of all the characters that are in the same room as my character
		List<int> characterIDList = m_gameWorldModel.GetCharacterIDListInRoom(roomKey);

		// Remove any existing character 
        List<int> allCharacterIds = new List<int>(m_gameWorldModel.GetCharacterEntityMap().Keys);
        foreach (int characterEntityId in allCharacterIds)
        {
            CharacterEntity characterEntity= m_gameWorldModel.GetCharacterEntity(characterEntityId);

            characterEntity.RemoveFromGameWorld(this);
        }

        // (Re)Create character entities for each character in the room
        foreach (int characterID in characterIDList)
        {
            CharacterEntity newCharacterEntity = new CharacterEntity(characterID);

            newCharacterEntity.AddToGameWorld(this);
        }			
			
        // Remove any existing mobs
        List<int> allMobIds = new List<int>(m_gameWorldModel.GetMobEntityMap().Keys);
        foreach (int mobEntityId in allMobIds)
        {
            MobEntity mobEntity = m_gameWorldModel.GetMobEntity(mobEntityId);

            mobEntity.RemoveFromGameWorld(this);
        }		
			
        // (Re)Create mob entities for each mob in the room
        foreach (MobData mobData in currentRoom.MobMap.Values)
        {
            MobEntity newMobEntity = new MobEntity(mobData.mob_id);
				
            newMobEntity.AddToGameWorld(this);
        }				
			
        // Remove any existing energy tanks
        List<int> allEnergyTankIds = new List<int>(m_gameWorldModel.GetEnergyTankEntityMap().Keys);
        foreach (int energyTankId in allEnergyTankIds)
        {
            EnergyTankEntity energyTankEntity = m_gameWorldModel.GetEnergyTankEntity(energyTankId);

            energyTankEntity.RemoveFromGameWorld(this);
        }		
			
        // (Re)Create energy tank entities for each energy tank in the room
        foreach (EnergyTankData energyTankData in currentRoom.EnergyTankMap.Values)
        {
            EnergyTankEntity newEnergyTankEntity = new EnergyTankEntity(energyTankData.energy_tank_id);
				
            newEnergyTankEntity.AddToGameWorld(this);
        }				
			
		// generate all of the terrain tiles and static objects in the room
		gameWorldView.RebuildRoom(currentRoom);					
			
		// Notify the overlay of a room reload
		m_contextOverlayController.OnRoomLoaded(currentRoom);			
	}	

    public void OnLogoutComplete()
	{
        audio.PlayOneShot(successSound, 1.0f);

        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
        {
            GameManager.GetInstance().SetTargetGameState("LoginScreen");
        });
	}

	public void OnCharacterActionDisallowed(object request) 
	{
        if (request is AsyncCharacterMoveRequest)
        {
            ShowErrorDialog("Not allowed to move there!");
        }
        else if (request is AsyncCharacterPortalRequest)
        {
            ShowErrorDialog("Not allowed to use that portal!");
        }
        else if (request is AsyncDrainEnergyTankRequest)
        {
            ShowErrorDialog("Not allowed to drain energy tank!");
        }
        else if (request is AsyncHackEnergyTankRequest)
        {
            ShowErrorDialog("Not allowed to hack energy tank!");
        }
        else
        {
            GameManager.GetInstance().SetTargetGameState("LoginScreen");
        }
	}
		
	public void OnRequestFailed(string reason)
	{
		// Connection failed! Bail back to login screen :(
        // TODO: pass failure reason back into the login screen
        GameManager.GetInstance().SetTargetGameState("LoginScreen");
	}

	// Context Overlay Events
	public void OnPortalClicked(Point3d entryPoint, RoomPortal portal)
	{
        if (!m_gameWorldModel.IsAsyncRequestPending &&
            m_gameWorldModel.IsEventCursorAtLastEvent &&
            !m_gameWorldModel.IsEventPlaybackActive &&
            m_gameWorldModel.IsWorldPointOnNavMesh(entryPoint))
        {
            m_gameWorldModel.RequestCharacterPortal(entryPoint, portal);
        }
	}

	public void OnEnergyTankClicked(Point3d entryPoint, EnergyTankData energyTankData)
	{
        if (!m_gameWorldModel.IsAsyncRequestPending &&
            m_gameWorldModel.IsEventCursorAtLastEvent &&
            !m_gameWorldModel.IsEventPlaybackActive &&
            m_gameWorldModel.IsWorldPointOnNavMesh(entryPoint))
        {
            if (energyTankData.ownership != GameConstants.eFaction.player)
            {
                // If we don't control the tank, hack it!
                m_gameWorldModel.RequestHackEnergyTank(entryPoint, energyTankData);
            }
            else if (energyTankData.energy > 0)
            {
                // If it's got energy, drain it!
                m_gameWorldModel.RequestDrainEnergyTank(entryPoint, energyTankData);
            }
            else
            {
                // Just move to it
                m_gameWorldModel.RequestCharacterMove(entryPoint);
            }
        }
	}
		
	// Game Events returned from a server request
	public void OnGameHasNewEvents()
	{
		m_gameWorldModel.EventPlaybackMode = GameWorldModel.eGameEventMode.playing;
		gameWorldView.RefreshEventControls(m_gameWorldModel);
	}
		
	public void OnJoinedIRCChannel()
	{			
		// Tell anyone listening on the IRC channel that the game state changed
		SendCharacterUpdatedGameEvent();
	}
		
	// Game Events posted in IRC
    public void OnGameEventPosted(GameEvent gameEvent)
    {
        GameData currentGame = m_gameWorldModel.CurrentGame;
        bool requestGameStateUpdate = false;

        switch (gameEvent.EventType)
        {
            case GameEvent.eEventType.character_updated:
                requestGameStateUpdate = (gameEvent as GameEvent_CharacterUpdated).CharacterID != currentGame.CharacterID;
                break;
        }

        if (requestGameStateUpdate && !m_gameWorldModel.IsCharacterPingRequestPending)
        {
            // If the game event wasn't caused by us, then something interesting
            // request any events that affect our character
            m_gameWorldModel.RequestCharacterPing();
        }
    }		
}