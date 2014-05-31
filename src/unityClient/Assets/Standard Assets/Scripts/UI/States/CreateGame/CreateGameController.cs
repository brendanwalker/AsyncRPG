using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;

[RequireComponent(typeof(AudioSource))]
public class CreateGameController : GameState
{
    public CreateGameView createGameView;
    public AudioClip successSound;
    public AudioClip alertSound;
    public AudioClip errorSound;

    private CreateGameModel m_createGameModel;
    private bool m_readyForInput;

    public bool IsReadyForInput
    {
        get { return m_readyForInput; }
    }

    public CreateGameModel Model
    {
        get { return m_createGameModel; }
    }

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        m_createGameModel = new CreateGameModel(this);

        createGameView.CreateGameController = this;
        createGameView.Start();

        // Fade in the background
        m_readyForInput = false;
        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeIn, 1.0f, () =>
        {
            m_readyForInput = true;
        });
    }

    public override void OnDestroy()
    {
        // $TODO Free any pending queries on the model
        createGameView.OnDestroy();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    // Update is called once per frame
    public override void OnGUI()
    {
        base.OnGUI();

        createGameView.OnGUI();
    }

    // UI Event Handlers
    public void OnGameCreateClicked()
    {
        audio.PlayOneShot(alertSound, 1.0f);

		if (!m_createGameModel.IsGameCreateRequestPending)
		{
            string name = createGameView.GetGameName();
            string irc_server = createGameView.GetIrcServer();
            uint irc_port = createGameView.GetIrcPort();
            bool irc_enabled = createGameView.GetIrcEnabled();
            bool irc_encryption_enabled = createGameView.GetIrcEncryptionEnabled();
				
			//TODO: Read the dungeon size from the game view
			GameConstants.eDungeonSize dungeon_size = GameConstants.eDungeonSize.small;
				
			//TODO: Read the dungeon difficulty from the game view
			GameConstants.eDungeonDifficulty dungeon_difficulty = GameConstants.eDungeonDifficulty.normal;
				
			if (name.Length == 0)
			{
                createGameView.SetStatusText("Enter a name!");
			}
			else if (irc_server.Length == 0)
			{
                createGameView.SetStatusText("Enter an IRC server address!");
			}
			else if (irc_port < ServerConstants.LOWEST_VALID_IRC_PORT || irc_port > ServerConstants.HIGHEST_VALID_IRC_PORT)
			{
                createGameView.SetStatusText("Enter a valid IRC server port!");
			}				
			else 
			{
                createGameView.SetStatusText("Creating...");
				m_createGameModel.CreateGame(
                    name, 
					dungeon_size,
					dungeon_difficulty,
					irc_server, 
					irc_port, 
					irc_enabled, 
					irc_encryption_enabled);					
			}
		}
    }

	public void OnCancelClicked()
	{
		if (!m_createGameModel.IsGameCreateRequestPending)
		{
			ExitMenu();
		}
	}
		
	public void OnGameCreated()
	{
        createGameView.SetStatusText("Success!");
		ExitMenu();			
	}

	private void ExitMenu()
	{
        audio.PlayOneShot(successSound, 1.0f);
        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
        {
            GameManager.GetInstance().SetTargetGameState("SelectGameScreen");
        });
	}

    // Model Event Handlers
    public void OnRequestFailed(string reason)
    {
        // Connection failed! Bail back to login screen :(
        // TODO: pass failure reason back into the login screen
        audio.PlayOneShot(errorSound, 1.0f);
        GameManager.GetInstance().SetTargetGameState("LoginScreen");
    }
}
