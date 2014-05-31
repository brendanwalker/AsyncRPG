using LitJson;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class GameWorldModel
{
    public enum eGameEventMode
    {
        playing,
        paused
    }

    private GameWorldController m_gameWorldController;
	
	private Dictionary<int, CharacterEntity> m_characterEntities; // character id -> CharacterEntity
	private Dictionary<int, MobEntity> m_mobEntities; // mob_id -> MobEntity
	private Dictionary<int, EnergyTankEntity> m_energyTankEntities; // energy_tank_id -> EnergyTankEntity

    public GameWorldModel(GameWorldController gameWorldController)
    {
        m_gameWorldController = gameWorldController;

        EventPlaybackMode = eGameEventMode.paused;
        IsWaitingForEventCompletion = false;

        m_characterEntities= new Dictionary<int, CharacterEntity>();
        m_mobEntities = new Dictionary<int, MobEntity>();
        m_energyTankEntities= new Dictionary<int, EnergyTankEntity>();

        IsFullGameStateRequestPending= false;
        IsRoomDataRequestPending= false;
        IsCharacterMoveRequestPending= false;
        IsCharacterPingRequestPending= false;
        IsCharacterPortalRequestPending= false;
        IsHackEnergyTankRequestPending= false;
        IsDrainEnergyTankRequestPending= false;
        IsLogoutRequestPending = false;
    }

    public void Start()
    {
    }

    public void OnDestroy()
    {
        //$TODO - Free any pending queries
    }

    public void Update()
    {
        eGameEventMode oldEventPlaybackMode = EventPlaybackMode;

        // Play the next game event
        if (EventPlaybackMode == eGameEventMode.playing && !IsWaitingForEventCompletion)
        {
            if (!IsEventCursorAtLastEvent)
            {
                IsWaitingForEventCompletion = true;

                CurrentGame.AdvanceEventCursor(
                m_gameWorldController,
                () =>
                {
                    IsWaitingForEventCompletion = false;
                });
            }
            else
            {
                EventPlaybackMode = eGameEventMode.paused;
            }
        }

        if (oldEventPlaybackMode != EventPlaybackMode)
        {
            m_gameWorldController.OnEventPlaybackModeChanged();
        }

        // Update all active entities
        //if (EventPlaybackMode != eGameEventMode.paused)
        {
            foreach (CharacterEntity characterEntity in m_characterEntities.Values)
            {
                characterEntity.Update();
            }

            foreach (MobEntity mobEntity in m_mobEntities.Values)
            {
                mobEntity.Update();
            }

            foreach (EnergyTankEntity energyTankEntity in m_energyTankEntities.Values)
            {
                energyTankEntity.Update();
            }
        }
    }

	// Events
    public bool IsWaitingForEventCompletion { get; set; }
    public eGameEventMode EventPlaybackMode { get; set; }

    public bool IsEventCursorAtFirstEvent
    {
        get { return CurrentGame.IsEventCursorAtFistEvent; }
    }

    public bool IsEventCursorAtLastEvent
    {
        get { return CurrentGame.IsEventCursorAtLastEvent; }
    }
		
	public bool IsEventPlaybackActive
	{
		get { return EventPlaybackMode == eGameEventMode.playing; }
	}

    public bool AdvanceCurrentEvent(GameWorldController gameWorldController)
    {
        // null complete callback tells the event to immediately jump to the result
        return CurrentGame.AdvanceEventCursor(gameWorldController, null);
    }

    public bool ReverseCurrentEvent(GameWorldController gameWorldController)
    {
        return CurrentGame.ReverseEventCursor(gameWorldController);
    }

	// Entities			
    public void AddCharacterEntity(CharacterEntity characterEntity)
    {
        if (m_characterEntities.ContainsKey(characterEntity.MyCharacterData.character_id))
        {
            m_characterEntities[characterEntity.MyCharacterData.character_id]= characterEntity;			
        }
        else
        {
            m_characterEntities.Add(characterEntity.MyCharacterData.character_id, characterEntity);			
        }
    }
		
    public void RemoveCharacterEntity(CharacterEntity characterEntity)
    {
        m_characterEntities.Remove(characterEntity.MyCharacterData.character_id);			
    }

    public void AddMobEntity(MobEntity mobEntity)
    {
        if (m_mobEntities.ContainsKey(mobEntity.MyMobData.mob_id))
        {
            m_mobEntities[mobEntity.MyMobData.mob_id]= mobEntity;
        }
        else
        {
            m_mobEntities.Add(mobEntity.MyMobData.mob_id, mobEntity);
        }
    }
		
    public void RemoveMobEntity(MobEntity mobEntity)
    {
        m_mobEntities.Remove(mobEntity.MyMobData.mob_id);			
    }
		
    public void AddEnergyTankEntity(EnergyTankEntity energyTankEntity)
    {
        if (m_energyTankEntities.ContainsKey(energyTankEntity.MyEnergyTankData.energy_tank_id))
        {
            m_energyTankEntities[energyTankEntity.MyEnergyTankData.energy_tank_id]= energyTankEntity;
        }
        else
        {
            m_energyTankEntities.Add(energyTankEntity.MyEnergyTankData.energy_tank_id, energyTankEntity);
        }
    }
		
    public void RemoveEnergyTankEntity(EnergyTankEntity energyTankEntity)
    {
        m_energyTankEntities.Remove(energyTankEntity.MyEnergyTankData.energy_tank_id);			
    }		
		
	// Game State Accessors
	public GameData CurrentGame
	{
		get { return SessionData.GetInstance().CurrentGameData; }
	}
		
	public int CurrentCharacterID
	{
        get { return CurrentGame.CharacterID; }						
	}
		
    public CharacterEntity GetCharacterEntity(int characterID)
    {
        return m_characterEntities[characterID];
    }

    public Dictionary<int, CharacterEntity> GetCharacterEntityMap()
    {
        return m_characterEntities;
    }
		
    public CharacterData GetCharacterData(int characterID)
    {
        return CurrentGame.GetCharacterById(characterID);
    }
		
    public MobEntity GetMobEntity(int mobID) 
    {
        return m_mobEntities[mobID];
    }

    public Dictionary<int, MobEntity> GetMobEntityMap()
    {
        return m_mobEntities;
    }
		
    public MobData GetMobData(int mobID)
    {
        return CurrentGame.CurrentRoom.GetMobById(mobID);
    }
						
    public EnergyTankEntity GetEnergyTankEntity(int energyTankID)
    {
        return m_energyTankEntities[energyTankID];
    }

    public Dictionary<int, EnergyTankEntity> GetEnergyTankEntityMap()
    {
        return m_energyTankEntities;
    }
		
    public EnergyTankData GetEnergyTankData(int energyTankID)
    {
        return CurrentGame.CurrentRoom.GetEnergyTankById(energyTankID);
    }
		
    public RoomData GetRoomData(RoomKey roomKey)
    {
        return CurrentGame.GetCachedRoomData(roomKey);
    }
		
    public List<int> GetCharacterIDListInRoom(RoomKey currentRoomKey)
    {
        List<int> characterDataList = new List<int>();
			
        foreach (CharacterData testCharacter in CurrentGame.CharacterMap.Values)
        {
            if (testCharacter.CurrentRoomKey.Equals(currentRoomKey))
            {
                characterDataList.Add(testCharacter.character_id);
            }
        }
			
        return characterDataList;
    }

	// Utilities
	public bool IsWorldPointOnNavMesh(Point3d testPoint)
	{
		GameData gameData = CurrentGame;
		RoomData roomData = gameData.GetCachedRoomData(gameData.CurrentRoomKey);
		bool pointOnNavMesh = false;
			
		if (roomData != null)
		{
			AsyncRPGSharedLib.Navigation.NavMesh navMesh = roomData.StaticRoomData.NavMesh;
            NavRef navRef = navMesh.ComputeNavRefAtPoint(testPoint);
				
			if (navRef != null && navRef.IsValid)
			{
				pointOnNavMesh= true;
			}
		}
			
		return pointOnNavMesh;
	}

	// Requests
    public bool IsFullGameStateRequestPending { get; private set; }
    public bool IsRoomDataRequestPending { get; private set; }
    public bool IsCharacterMoveRequestPending { get; private set; }
    public bool IsCharacterPingRequestPending { get; private set; }
    public bool IsCharacterPortalRequestPending { get; private set; }
    public bool IsHackEnergyTankRequestPending { get; private set; }
    public bool IsDrainEnergyTankRequestPending { get; private set; }
    public bool IsLogoutRequestPending { get; private set; }

	public bool IsAsyncRequestPending
	{
        get
        {
            return IsFullGameStateRequestPending
                || IsRoomDataRequestPending
                || IsCharacterMoveRequestPending
                || IsCharacterPortalRequestPending
                || IsHackEnergyTankRequestPending
                || IsDrainEnergyTankRequestPending
                || IsLogoutRequestPending;
        }
	}		

	public void RequestFullGameState()
	{
		if (!IsFullGameStateRequestPending)
		{
			AsyncJSONRequest fullGameStateRequest = AsyncJSONRequest.Create(m_gameWorldController.gameObject);
            Dictionary<string, object> request = new Dictionary<string, object>();
				
			request["character_id"] = SessionData.GetInstance().CharacterID;
				
			IsFullGameStateRequestPending = true;

            fullGameStateRequest.POST(
                ServerConstants.fullGameStateRequestURL,
                request,
                (AsyncJSONRequest asyncRequest) =>
			{
				if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
				{
					JsonData response = asyncRequest.GetResult();
                    string responseResult = (string)response["result"];
						
					if (responseResult == "Success")
					{
                        GameData currentGameData = SessionData.GetInstance().CurrentGameData;
							
						// Parse the full world state into the session data
						currentGameData.ParseFullGameStateResponse(response);
													
						Debug.Log("Game State Loaded");
                        m_gameWorldController.OnGameLoaded();							
					}
					else 
					{
                        Debug.LogError("Game State Request Failed: " + responseResult);
                        m_gameWorldController.OnRequestFailed(responseResult);
					}
				}
				else 
				{
                    Debug.LogError("Game State Request Failed: " + asyncRequest.GetFailureReason());
                    m_gameWorldController.OnRequestFailed("Connection Failed!");
				}
					
				IsFullGameStateRequestPending = false;					
			});				
		}
	}

    public void RequestRoomData(RoomKey roomKey, GameEvent.OnEventCompleteDelegate onComplete)
    {
        if (!IsRoomDataRequestPending)
        {
            // Cached room data is available
            if (SessionData.GetInstance().CurrentGameData.HasRoomData(roomKey))
            {
                // Update which room we're currently in
                SessionData.GetInstance().CurrentGameData.CurrentRoomKey = roomKey;							
												
                // Notify the controller 
                Debug.Log("Using cached room data");
                m_gameWorldController.OnRoomLoaded(roomKey);
				
                // Notify the caller
                if (onComplete != null)
                {
                    onComplete();
                }					
            }
            // Have to request room data from the server
            else
            {
                AsyncJSONRequest roomDataRequest = AsyncJSONRequest.Create(m_gameWorldController.gameObject);
                Dictionary<string, object> request = new Dictionary<string, object>();
					
                request["game_id"] = SessionData.GetInstance().GameID;
                request["room_x"] = roomKey.x;
                request["room_y"] = roomKey.y;
                request["room_z"] = roomKey.z;
					
                IsRoomDataRequestPending = true;

                roomDataRequest.POST(
                    ServerConstants.roomDataRequestURL, 
                    request,
                    (AsyncJSONRequest asyncRequest) =>
                {
                    if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
                    {
                        JsonData response = asyncRequest.GetResult();
                        string responseResult = (string)response["result"];
							
                        if (responseResult == "Success")
                        {
                            SessionData sessionData = SessionData.GetInstance();
                            GameData currentGame= sessionData.CurrentGameData;
                            RoomData roomData = RoomData.FromObject(response);
								
                            // Add the room data to the room cache
                            currentGame.SetCachedRoomData(roomData.RoomKey, roomData);
								
                            // Update which room we're currently in
                            currentGame.CurrentRoomKey = roomKey;							
															
                            // Notify the controller 
                            Debug.Log("Room Loaded");
                            m_gameWorldController.OnRoomLoaded(roomData.RoomKey);
                        }
                        else 
                        {
                            Debug.Log("Room Data Request Failed: " + responseResult);
                            m_gameWorldController.OnRequestFailed(responseResult);
                        }
                    }
                    else 
                    {
                        Debug.Log("Room Data Request Failed: " + asyncRequest.GetFailureReason());
                        m_gameWorldController.OnRequestFailed("Connection Failed!");
                    }
						
                    // Notify the caller
                    if (onComplete != null)
                    {
                        onComplete();
                    }
						
                    IsRoomDataRequestPending = false;					
                });
            }
        }
    }

    public void RequestLogout()
    {
        if (!IsLogoutRequestPending)
        {
            AsyncJSONRequest gameListRequest = AsyncJSONRequest.Create(m_gameWorldController.gameObject);
            Dictionary<string, object> request = new Dictionary<string, object>();

            request["username"] = SessionData.GetInstance().UserName;

            IsLogoutRequestPending = true;

            gameListRequest.POST(
                ServerConstants.logoutRequestURL,
                request,
                (AsyncJSONRequest asyncRequest) =>
            {
                if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
                {
                    JsonData response = asyncRequest.GetResult();
                    string responseResult = (string)response["result"];

                    if (responseResult== "Success")
                    {
                        m_gameWorldController.OnLogoutComplete();
                    }
                    else
                    {
                        m_gameWorldController.OnRequestFailed(responseResult);
                        Debug.LogError("Logout Request Failed: " + asyncRequest.GetFailureReason());
                    }
                }
                else
                {
                    m_gameWorldController.OnRequestFailed("Connection Failure!");
                    Debug.LogError("Logout Request Failed: " + asyncRequest.GetFailureReason());
                }

                IsLogoutRequestPending = false;
            });
        }
    }

    public void RequestCharacterMove(Point3d roomPosition)
    {
        Debug.Log("GameWorldModel:requestCharacterMove - Requesting move to (" + roomPosition.x + ", " + roomPosition.y + ")");
			
        if (!IsCharacterMoveRequestPending)
        {
            GameData gameData = SessionData.GetInstance().CurrentGameData;
            CharacterEntity entity = GetCharacterEntity(CurrentCharacterID);
            AsyncCharacterMoveRequest characterMoveRequest = null;
				
            IsCharacterMoveRequestPending = true;
				
            characterMoveRequest = new AsyncCharacterMoveRequest(m_gameWorldController.gameObject, entity, roomPosition);
            characterMoveRequest.Execute(
                (AsyncCharacterMoveRequest asyncRequest) =>
            {
                IsCharacterMoveRequestPending = false;
										
                switch (asyncRequest.ResultCode)
                {					
                case AsyncCharacterMoveRequest.eResult.success:
                    {
                        Debug.Log("GameWorldModel:requestCharacterMove - Move Succeeded!");

                        // Tell anyone listening on IRC that the game state changed
                        m_gameWorldController.SendCharacterUpdatedGameEvent();
                    }
                    break;
                case AsyncCharacterMoveRequest.eResult.failed_path:
                case AsyncCharacterMoveRequest.eResult.failed_server_denied:
                    {
                        Debug.LogError("GameWorldModel:requestCharacterMove - "+asyncRequest.ResultDetails);
                        m_gameWorldController.OnCharacterActionDisallowed(asyncRequest);
                    }
                    break;						
                case AsyncCharacterMoveRequest.eResult.failed_server_connection:
                    {
                        Debug.LogError("GameWorldModel:requestCharacterMove - "+asyncRequest.ResultDetails);
                        m_gameWorldController.OnRequestFailed("Connection Failed!");
                    }
                    break;
                }
					
                // Parse any incoming game events
                if (asyncRequest.ServerResponse != null)
                {
                    gameData.ParseEventResponse(asyncRequest.ServerResponse);
                }
					
                // If there were new events, notify the controller so that 
                // it can start playing the events back
                if (!gameData.IsEventCursorAtLastEvent)
                {
                    m_gameWorldController.OnGameHasNewEvents();
                }					
            });
        }
    }	

    public void RequestCharacterPortal(Point3d entryPoint, RoomPortal portal)
    {
        Debug.Log("GameWorldModel:requestCharacterPortal - Entering portal_id="
            +portal.portal_id+" at position ("+ entryPoint.x + ", " + entryPoint.y + ")");
			
        if (!IsCharacterPortalRequestPending)
        {
            GameData gameData = SessionData.GetInstance().CurrentGameData;
            CharacterEntity entity = GetCharacterEntity(CurrentCharacterID);
            AsyncCharacterPortalRequest characterPortalRequest = null;
				
            IsCharacterPortalRequestPending = true;
								
            characterPortalRequest = new AsyncCharacterPortalRequest(m_gameWorldController.gameObject, entity, entryPoint, portal);
            characterPortalRequest.Execute(
                (AsyncCharacterPortalRequest asyncRequest) => 
            {
                IsCharacterPortalRequestPending = false;
										
                switch (asyncRequest.ResultCode)
                {					
                case AsyncCharacterPortalRequest.eResult.success:
                    {
                        Debug.Log("GameWorldModel:requestCharacterPortal - Portal Succeeded!");
							
                        // Tell anyone listening on IRC that the game state changed
                        m_gameWorldController.SendCharacterUpdatedGameEvent();
							
                        // Parse any incoming game events
                        if (asyncRequest.ServerResponse != null)
                        {
                            gameData.ParseEventResponse(asyncRequest.ServerResponse);
                        }
							
                        // If there were new events, notify the controller so that 
                        // it can start playing the events back
                        if (!gameData.IsEventCursorAtLastEvent)
                        {
                            m_gameWorldController.OnGameHasNewEvents();
                        }								
                    }
                    break;
                case AsyncCharacterPortalRequest.eResult.failed_path:
                case AsyncCharacterPortalRequest.eResult.failed_server_denied:
                    {
                        Debug.LogError("GameWorldModel:requestCharacterPortal - " + asyncRequest.ResultDetails);
							
                        m_gameWorldController.OnCharacterActionDisallowed(asyncRequest);
                    }
                    break;
                case AsyncCharacterPortalRequest.eResult.failed_server_connection:
                    {
                        Debug.LogError("GameWorldModel:requestCharacterPortal - " + asyncRequest.ResultDetails);
							
                        m_gameWorldController.OnRequestFailed("Connection Failed!");
                    }
                    break;
                }				
            });
        }
    }

    public void RequestHackEnergyTank(Point3d entryPoint, EnergyTankData energyTankData)
    {
        Debug.Log("GameWorldModel:requestHackEnergyTank - Hacking energy tank id="
            +energyTankData.energy_tank_id+" at position ("+ entryPoint.x + ", " + entryPoint.y + ")");
			
        if (!IsHackEnergyTankRequestPending)
        {
            GameData gameData = SessionData.GetInstance().CurrentGameData;
            CharacterEntity entity = GetCharacterEntity(CurrentCharacterID);
            AsyncHackEnergyTankRequest hackEnergyTankRequest = null;
				
            IsHackEnergyTankRequestPending = true;
								
            hackEnergyTankRequest = 
                new AsyncHackEnergyTankRequest(m_gameWorldController.gameObject, entity, entryPoint, energyTankData);

            hackEnergyTankRequest.Execute(
                (AsyncHackEnergyTankRequest asyncRequest) =>
            {
                IsHackEnergyTankRequestPending = false;
										
                switch (asyncRequest.ResultCode)
                {					
                case AsyncHackEnergyTankRequest.eResult.success:
                    {
                        Debug.Log("GameWorldModel:requestHackEnergyTank - Hack Succeeded!");
							
                        // Tell anyone listening on IRC that the game state changed
                        m_gameWorldController.SendCharacterUpdatedGameEvent();
							
                        // Parse any incoming game events
                        if (asyncRequest.ServerResponse != null)
                        {
                            gameData.ParseEventResponse(asyncRequest.ServerResponse);
                        }
							
                        // If there were new events, notify the controller so that 
                        // it can start playing the events back
                        if (!gameData.IsEventCursorAtLastEvent)
                        {
                            m_gameWorldController.OnGameHasNewEvents();
                        }								
                    }
                    break;
                case AsyncHackEnergyTankRequest.eResult.failed_path:
                case AsyncHackEnergyTankRequest.eResult.failed_server_denied:
                    {
                        Debug.LogError("GameWorldModel:requestHackEnergyTank - " + asyncRequest.ResultDetails);
							
                        m_gameWorldController.OnCharacterActionDisallowed(asyncRequest);
                    }
                    break;						
                case AsyncHackEnergyTankRequest.eResult.failed_server_connection:
                    {
                        Debug.LogError("GameWorldModel:requestHackEnergyTank - " + asyncRequest.ResultDetails);
							
                        m_gameWorldController.OnRequestFailed("Connection Failed!");
                    }
                    break;
                }				
            });
        }			
    }

    public void RequestDrainEnergyTank(Point3d entryPoint, EnergyTankData energyTankData)
    {
        Debug.Log("GameWorldModel:requestDrainEnergyTank - Draining energy tank id="
            +energyTankData.energy_tank_id+" at position ("+ entryPoint.x + ", " + entryPoint.y + ")");
			
        if (!IsDrainEnergyTankRequestPending)
        {
            GameData gameData = SessionData.GetInstance().CurrentGameData;
            CharacterEntity entity = GetCharacterEntity(CurrentCharacterID);
            AsyncDrainEnergyTankRequest drainEnergyTankRequest = null;
				
            IsDrainEnergyTankRequestPending = true;
								
            drainEnergyTankRequest = 
                new AsyncDrainEnergyTankRequest(m_gameWorldController.gameObject, entity, entryPoint, energyTankData);
            drainEnergyTankRequest.Execute(
                (AsyncDrainEnergyTankRequest asyncRequest) =>
            {
                IsDrainEnergyTankRequestPending = false;
										
                switch (asyncRequest.ResultCode)
                {
                case AsyncDrainEnergyTankRequest.eResult.success:
                    {
                        Debug.Log("GameWorldModel:requestDrainEnergyTank - Drain Succeeded!");
							
                        // Tell anyone listening on IRC that the game state changed
                        m_gameWorldController.SendCharacterUpdatedGameEvent();
							
                        // Parse any incoming game events
                        if (asyncRequest.ServerResponse != null)
                        {
                            gameData.ParseEventResponse(asyncRequest.ServerResponse);
                        }
							
                        // If there were new events, notify the controller so that 
                        // it can start playing the events back
                        if (!gameData.IsEventCursorAtLastEvent)
                        {
                            m_gameWorldController.OnGameHasNewEvents();
                        }								
                    }
                    break;
                case AsyncDrainEnergyTankRequest.eResult.failed_path:
                case AsyncDrainEnergyTankRequest.eResult.failed_server_denied:
                    {
                        Debug.LogError("GameWorldModel:requestDrainEnergyTank - " + asyncRequest.ResultDetails);
							
                        m_gameWorldController.OnCharacterActionDisallowed(asyncRequest);
                    }
                    break;						
                case AsyncDrainEnergyTankRequest.eResult.failed_server_connection:
                    {
                        Debug.LogError("GameWorldModel:requestDrainEnergyTank - " + asyncRequest.ResultDetails);
							
                        m_gameWorldController.OnRequestFailed("Connection Failed!");
                    }
                    break;
                }				
            });
        }			
    }

	public void RequestCharacterPing()
	{
		if (!IsCharacterPingRequestPending)
		{
            AsyncJSONRequest characterPingRequest= AsyncJSONRequest.Create(m_gameWorldController.gameObject);
            Dictionary<string, object> request = new Dictionary<string, object>();
				
			request["character_id"] = SessionData.GetInstance().CharacterID;
				
			IsCharacterPingRequestPending = true;

            characterPingRequest.POST(
                ServerConstants.characterPingRequestURL, 
                request,
                (AsyncJSONRequest asyncRequest) =>
			{
				if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
				{
					JsonData response = asyncRequest.GetResult();
                    string responseResult = JsonUtilities.ParseString(response, "result");
						
					if (responseResult == "Success")
					{
                        GameData gameData = SessionData.GetInstance().CurrentGameData;
							
						// Parse the game events into the session data
						Debug.Log("Game Events Received");
						gameData.ParseEventResponse(response);
													
						// If there were new events, notify the controller so that 
						// it can start playing the events back							
						if (!gameData.IsEventCursorAtLastEvent)
						{
                            m_gameWorldController.OnGameHasNewEvents();
						}							
					}
					else 
					{
                        Debug.LogError("Character Ping Request Failed: " + responseResult);
                        m_gameWorldController.OnRequestFailed(responseResult);
					}
				}
				else 
				{
					Debug.LogError("Game Ping Request Failed: " + asyncRequest.GetFailureReason());
                    m_gameWorldController.OnRequestFailed("Connection Failed!");
				}
					
				IsCharacterPingRequestPending = false;					
			});				
		}			
	}

}
