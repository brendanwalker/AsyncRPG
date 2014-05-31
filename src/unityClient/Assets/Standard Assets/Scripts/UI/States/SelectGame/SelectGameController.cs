using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SelectGameController : GameState
{
    public SelectGameView selectGameView;
    public AudioClip successSound;
    public AudioClip alertSound;
    public AudioClip errorSound;

    private SelectGameModel m_selectGameModel;
    private bool m_readyForInput;
    private bool m_gameListRefreshRequested;

    public bool IsReadyForInput
    {
        get { return m_readyForInput; }
    }

    public SelectGameModel Model
    {
        get { return m_selectGameModel; }
    }

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        m_selectGameModel = new SelectGameModel(this);

        selectGameView.SelectGameController = this;
        selectGameView.Start();

        // $TODO Play title music

        // Fade in the background
        m_readyForInput = false;
        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeIn, 1.0f, () =>
        {
            m_readyForInput = true;
        });

        // Request a list of games available on the server
        m_gameListRefreshRequested = true;
    }

    public override void OnDestroy()
    {
        // $TODO Free any pending queries on the model
        selectGameView.OnDestroy();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (m_gameListRefreshRequested)
        {
            m_gameListRefreshRequested = false;
            m_selectGameModel.RequestGameList();
        }
    }

    // Update is called once per frame
    public override void OnGUI()
    {
        base.OnGUI();

        selectGameView.OnGUI();
    }

	// UI Event Handlers
	public void OnGameCreateClicked()
	{
        audio.PlayOneShot(successSound, 1.0f);

        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
        {
            GameManager.GetInstance().SetTargetGameState("CreateGameScreen");
        });
	}

	public void OnGameDeleteClicked()
	{
        audio.PlayOneShot(alertSound, 1.0f);
		m_selectGameModel.RequestSelectedGameDeletion();
	}
		
	public void OnSelectedGameChanged(int listIndex)
	{
		m_selectGameModel.SelectedListIndex= listIndex;
	}

	public void OnGameSelectClicked()
	{
		if (m_selectGameModel.GetGameCount() > 0)
		{
            Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
            {
                GameManager.GetInstance().SetTargetGameState("SelectCharacterScreen");
            });		
		}
		else 
		{
			audio.PlayOneShot(errorSound, 1.0f);
		}
	}
		
	// Model Event Handlers
	public void OnGameListUpdated()
	{
		selectGameView.RebuildGameList(m_selectGameModel.GetGameList());			
	}

    public void OnGameDeleted()
    {
        m_gameListRefreshRequested = true;
    }
		
	public void OnRequestFailed(string reason)
	{
		// Connection failed! Bail back to login screen :(
        // TODO: pass failure reason back into the login screen
        GameManager.GetInstance().SetTargetGameState("LoginScreen");
	}
}
