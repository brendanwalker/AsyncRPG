using UnityEngine;
using System.Collections;
using System;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Web;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Web.Modules;
using System.Collections.Generic;

public class AsyncRPGServer : MonoBehaviour 
{
	enum eState
	{
		stopped,
		starting,
		started
	}

    public bool DebugEnabled;
    public string UriAddress;
    public string MapDataDirectory;
    public string MobDataDirectory;
    public string DBDataDirectory;
    public string DBConnectionString;

	public string EmailAddress;
	public string EmailHost;
	public int EmailPort;
	public string EmailUsername;
	public string EmailPassword;

    private static AsyncRPGServer m_gameServerInstance;
    private Logger m_logger;
	private List<Logger.LogDelegate> m_logListeners;
	private AsyncDatabaseInitializeRequest m_dbInitRequest;
    private WebServer m_webServer;
	private eState m_state;

    // Game Server Interface
    public static AsyncRPGServer GetInstance()
    {
        return m_gameServerInstance;
    }

	public bool IsServerInitializing()
	{
		return m_state == eState.starting;
	}

    public bool IsServerActive()
    {
		return m_state == eState.started;
    }

    public void StartServer()
    {
        if (m_state == eState.stopped)
        {
			DatabaseManagerConfig dbConfig = 
				new DatabaseManagerConfig(DBConnectionString, MobDataDirectory, MapDataDirectory);

			// Start initializing the database
			m_state = eState.starting;
			m_dbInitRequest = new AsyncDatabaseInitializeRequest(dbConfig, m_logger);
			m_dbInitRequest.Execute();
        }
    }

    public void StopServer()
    {
		if (m_state != eState.stopped)
        {
			// Wait for the init request to finish first, in case it's running
			while (m_dbInitRequest.IsStarted() && !m_dbInitRequest.IsComplete())
			{
				System.Threading.Thread.Sleep(250);
			}

            // Stop listening for connections
            m_webServer.Stop();

            // Forget about the web server
            m_webServer = null;
			m_state= eState.stopped;
        }
    }

    private void InitializeConstants()
    {
        ApplicationConstants.IsDebuggingEnabled = DebugEnabled;

		// Set the optional e-mail account constants
		MailConstants.WEB_SERVICE_EMAIL_ADDRESS = EmailAddress;		
		MailConstants.WEB_SERVICE_EMAIL_HOST = EmailHost;		
		MailConstants.WEB_SERVICE_EMAIL_PORT = EmailPort;
		MailConstants.WEB_SERVICE_EMAIL_USERNAME = EmailUsername;		
		MailConstants.WEB_SERVICE_EMAIL_PASSWORD = EmailPassword;

        // Set the data source directories
        ApplicationConstants.MAPS_DIRECTORY = MapDataDirectory;
        ApplicationConstants.MOBS_DIRECTORY = MobDataDirectory;

        // Set the connection string used to access the database
        ApplicationConstants.CONNECTION_STRING = DBConnectionString;

        // Set the location of the |DataDirectory| value (used in the connection string)
        AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory + DBDataDirectory);
    }

	public void AddServerLogListener(
		Logger.LogDelegate listener)
	{
		m_logListeners.Add(listener);
	}

	public void RemoveServerLogListener(
		Logger.LogDelegate listener)
	{
		m_logListeners.Remove(listener);
	}

	private void OnServerLogMessage(
		string message)
	{
		Debug.Log(message);

		foreach (Logger.LogDelegate listener in m_logListeners)
		{
			listener(message);
		}
	}

    // Unity Callbacks
    public void Start()
    {
        m_gameServerInstance = this;

		m_logListeners = new List<Logger.LogDelegate>();
        m_logger = new Logger(this.OnServerLogMessage);

        // Only need to do this once on creation
        InitializeConstants();

		m_state= eState.stopped;
		m_dbInitRequest= null;
    }

    public void OnDestroy()
    {
        StopServer();
        m_gameServerInstance = null;
    }

    public void Update()
    {
		// See if we have finished initializing the database
		if (m_state == eState.starting && m_dbInitRequest.IsComplete())
		{
			if (m_dbInitRequest.HasSucceeded())
			{
				// Initialize the web server
				m_webServer =
					new WebServer(
						UriAddress,
						m_logger,
						new Type[] {
						typeof(AccountModule),
						typeof(AdminModule),
						typeof(CharacterModule),
						typeof(EnergyTankModule),
						typeof(GameModule),
						typeof(WorldModule)
					});
				
				// Start listening for connections
				m_webServer.Start();

				// Server has successfully started!
				m_state= eState.started;
			}
			else
			{
				// Crap, guess we aren't running a server locally.
				m_state= eState.stopped;
			}
		}
    }

    public void OnGUI()
    {
        // All game UI is on depth >= 1
        GUI.depth = 1;
    }
}
