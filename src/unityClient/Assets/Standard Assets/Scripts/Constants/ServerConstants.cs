using UnityEngine;
using System.Collections;

public class ServerConstants 
{	
    public const string DEFAULT_IRC_SERVER = "irc.freenode.net";
    public const uint DEFAULT_IRC_PORT = 6667;
    public const uint LOWEST_VALID_IRC_PORT = 1024;
    public const uint HIGHEST_VALID_IRC_PORT = 65535;
		
	private static string SERVER_URL = "http://localhost";

    private const uint SERVER_SOCKET = 8080;
    private const uint DEBUG_SERVER_SOCKET = 8080;
				
	private const string ACCOUNT_WEBSERVICE_PATH = "Account.asmx";
	private const string DEBUG_ACCOUNT_WEBSERVICE_PATH = "Account.asmx";

	private const string CHARACTER_WEBSERVICE_PATH = "Character.asmx";
	private const string DEBUG_CHARACTER_WEBSERVICE_PATH = "Character.asmx";
		
	private const string ENERGY_TANK_WEBSERVICE_PATH = "EnergyTank.asmx";
	private const string DEBUG_ENERGY_TANK_WEBSERVICE_PATH = "EnergyTank.asmx";	
		
	private const string GAME_WEBSERVICE_PATH = "Game.asmx";
	private const string DEBUG_GAME_WEBSERVICE_PATH = "Game.asmx";

	private const string WORLD_WEBSERVICE_PATH = "World.asmx";
	private const string DEBUG_WORLD_WEBSERVICE_PATH = "World.asmx";
	
	public static bool debugging
	{
		#if UNITY_EDITOR
        get { return true; }
		#else
		get { return false; }
		#endif
	}
		
	public static uint serverSocket
	{
		get { return ServerConstants.debugging ? DEBUG_SERVER_SOCKET : SERVER_SOCKET; }
	}
		
	public static string serverURL
	{
		set {
			SERVER_URL= value.StartsWith("http://") ? value : "http://"+value;
		}
		get { 
			return SERVER_URL; 
		}
	}
		
	public static string accountWebServicePath
	{
		get { return ServerConstants.debugging ? DEBUG_ACCOUNT_WEBSERVICE_PATH : ACCOUNT_WEBSERVICE_PATH; }
	}

	public static string accountWebServiceURL
	{
		get 
        { 
            return ServerConstants.serverURL + ":" + 
                ServerConstants.serverSocket.ToString() + "/" +
                ServerConstants.accountWebServicePath; 
        }
	}
		
	public static string characterWebServicePath
	{
		get { return ServerConstants.debugging ? DEBUG_CHARACTER_WEBSERVICE_PATH : CHARACTER_WEBSERVICE_PATH; }
	}

	public static string characterWebServiceURL
	{
		get 
        { 
            return ServerConstants.serverURL + ":" + 
                ServerConstants.serverSocket.ToString() + "/" + 
                ServerConstants.characterWebServicePath;
        }
	}

	public static string energyTankWebServicePath
	{
		get 
        {
            return ServerConstants.debugging ? DEBUG_ENERGY_TANK_WEBSERVICE_PATH : ENERGY_TANK_WEBSERVICE_PATH;
        }
	}
		
	public static string energyTankWebServiceURL
	{
		get
        {
            return ServerConstants.serverURL + ":" + 
                ServerConstants.serverSocket.ToString() + "/" + 
                ServerConstants.energyTankWebServicePath;
        }
	}		
		
	public static string gameWebServicePath
	{
		get
        {
            return ServerConstants.debugging ? DEBUG_GAME_WEBSERVICE_PATH : GAME_WEBSERVICE_PATH;
        }
	}
		
	public static string gameWebServiceURL
	{
		get
        {
            return ServerConstants.serverURL + ":" + 
                ServerConstants.serverSocket.ToString() + "/" + 
                ServerConstants.gameWebServicePath;
        }
	}		
						
	public static string worldWebServicePath
	{
		get 
        {
            return ServerConstants.debugging ? DEBUG_WORLD_WEBSERVICE_PATH : WORLD_WEBSERVICE_PATH;
        }
	}
		
	public static string worldWebServiceURL
	{
		get 
        {
            return ServerConstants.serverURL + ":" + 
                ServerConstants.serverSocket.ToString() + "/" + 
                ServerConstants.worldWebServicePath;
        }
	}
		
	// Account Requests
	public static string loginRequestURL 
	{
		get {return ServerConstants.accountWebServiceURL + "/AccountLoginRequest";}
	}
		
	public static string logoutRequestURL 
	{
		get {return ServerConstants.accountWebServiceURL + "/AccountLogoutRequest";}
	}

	public static string createAccountRequestURL 
	{
		get {return ServerConstants.accountWebServiceURL + "/AccountCreateRequest";}
	}
			
	// Character Requests
	public static string characterCreateRequestURL
	{
		get {return ServerConstants.characterWebServiceURL + "/CreateCharacter";}
	}

	public static string characterDeleteRequestURL
	{
		get {return ServerConstants.characterWebServiceURL + "/DeleteCharacter";}
	}
		
	public static string characterListRequestURL 
	{
		get {return ServerConstants.characterWebServiceURL + "/GetCharacterList";}
	}
		
	public static string characterMoveRequestURL 
	{
		get {return ServerConstants.characterWebServiceURL + "/MoveCharacter";}
	}		

	public static string characterPortalRequestURL 
	{
		get {return ServerConstants.characterWebServiceURL + "/PortalCharacter";}
	}
		
	public static string characterPingRequestURL 
	{
		get {return ServerConstants.characterWebServiceURL + "/PingCharacter";}
	}
		
	// Energy Tank Requests
	public static string energyTankHackRequestURL 
	{
		get {return ServerConstants.energyTankWebServiceURL + "/HackEnergyTank";}
	}

	public static string energyTankDrainRequestURL 
	{
		get {return ServerConstants.energyTankWebServiceURL + "/DrainEnergyTank";}
	}
		
	// Game Requests
	public static string pingRequestURL 
	{
		get {return ServerConstants.gameWebServiceURL + "/GameCharacterPing";}
	}				
		
	public static string gameCreateRequestURL
	{
		get {return ServerConstants.gameWebServiceURL + "/CreateGame";}
	}
		
	public static string gameDeleteRequestURL
	{
		get {return ServerConstants.gameWebServiceURL + "/DeleteGame";}
	}
		
	public static string gameListRequestURL 
	{
		get {return ServerConstants.gameWebServiceURL + "/GetGameList";}
	}
				
	public static string bindCharacterToGameRequestURL
	{
		get {return ServerConstants.gameWebServiceURL + "/BindCharacterToGame";}
	}

	// World Requests
	public static string roomDataRequestURL
	{
		get {return ServerConstants.worldWebServiceURL + "/GetRoomData";}
	}
		
	public static string fullGameStateRequestURL
	{
		get {return ServerConstants.worldWebServiceURL + "/WorldGetFullGameStateRequest";}
	}
}