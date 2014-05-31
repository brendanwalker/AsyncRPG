using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SelectCharacterController : GameState
{
    public SelectCharacterView selectGameView;
    public AudioClip successSound;
    public AudioClip alertSound;
    public AudioClip errorSound;

    private SelectCharacterModel m_selectCharacterModel;
    private bool m_readyForInput;
    private bool m_characterListRefreshRequested;

    public bool IsReadyForInput
    {
        get { return m_readyForInput; }
    }

    public SelectCharacterModel Model
    {
        get { return m_selectCharacterModel; }
    }

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        m_selectCharacterModel = new SelectCharacterModel(this);

        selectGameView.SelectCharacterController = this;
        selectGameView.Start();

        // $TODO Play title music

        // Fade in the background
        m_readyForInput = false;
        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeIn, 1.0f, () =>
        {
            m_readyForInput = true;
        });

        // Request a list of games available on the server
        m_characterListRefreshRequested = true;
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

        if (m_characterListRefreshRequested)
        {
            m_characterListRefreshRequested = false;
            m_selectCharacterModel.RequestCharacterList();
        }
    }

    // Update is called once per frame
    public override void OnGUI()
    {
        base.OnGUI();

        selectGameView.OnGUI();
    }

	// UI Event Handlers
	public void OnCharacterCreateClicked()
	{
        audio.PlayOneShot(successSound, 1.0f);

        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
        {
            GameManager.GetInstance().SetTargetGameState("CreateCharacterScreen");
        });
	}

	public void OnCharacterDeleteClicked()
	{
        audio.PlayOneShot(alertSound, 1.0f);
		m_selectCharacterModel.RequestSelectedCharacterDeletion();
	}
		
	public void OnSelectedCharacterChanged(int listIndex)
	{
		m_selectCharacterModel.SelectedListIndex= listIndex;
	}

	public void OnCharacterSelectClicked()
	{
		if (m_selectCharacterModel.GetCharacterCount() > 0)
		{
            m_selectCharacterModel.RequestBindCharacterToGame();
		}
		else 
		{
			audio.PlayOneShot(errorSound, 1.0f);
		}
	}
		
	// Model Event Handlers
	public void OnCharacterListUpdated()
	{
		selectGameView.RebuildCharacterList(m_selectCharacterModel.GetCharacterList());			
	}

    public void OnCharacterDeleted()
    {
        m_characterListRefreshRequested = true;
    }

    public void OnCharacterBoundToGame()
    {
        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
        {
            GameManager.GetInstance().SetTargetGameState("GameWorldScreen");
        });
    }
		
	public void OnRequestFailed(string reason)
	{
		// Connection failed! Bail back to login screen :(
        // TODO: pass failure reason back into the login screen
        GameManager.GetInstance().SetTargetGameState("LoginScreen");
	}
}
