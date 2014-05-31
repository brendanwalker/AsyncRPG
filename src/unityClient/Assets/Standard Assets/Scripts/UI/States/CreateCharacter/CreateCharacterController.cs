using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class CreateCharacterController : GameState
{
    public CreateCharacterView createCharacterView;
    public AudioClip successSound;
    public AudioClip alertSound;
    public AudioClip errorSound;

    private CreateCharacterModel m_createCharacterModel;
    private bool m_readyForInput;

    public bool IsReadyForInput
    {
        get { return m_readyForInput; }
    }

    public CreateCharacterModel Model
    {
        get { return m_createCharacterModel; }
    }

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        m_createCharacterModel = new CreateCharacterModel(this);

        createCharacterView.CreateCharacterController = this;
        createCharacterView.Start();
        createCharacterView.SetPortraitId(m_createCharacterModel.GetPictureId());

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
        createCharacterView.OnDestroy();
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

        createCharacterView.OnGUI();
    }

    // UI Event Handlers
	public void OnPreviousPortraitClicked()
	{
        if (!m_createCharacterModel.IsCharacterCreateRequestPending)
		{
            m_createCharacterModel.SelectPreviousPicture();
            createCharacterView.SetPortraitId(m_createCharacterModel.GetPictureId());
		}
	}

	public void OnNextPortraitClicked()
	{
        if (!m_createCharacterModel.IsCharacterCreateRequestPending)
		{
            m_createCharacterModel.SelectNextPicture();
            createCharacterView.SetPortraitId(m_createCharacterModel.GetPictureId());
		}			
	}

    public void OnCreateCharacterClicked()
    {
        audio.PlayOneShot(alertSound, 1.0f);

		if (!m_createCharacterModel.IsCharacterCreateRequestPending)
		{
			string name = createCharacterView.GetCharacterName();
				
			if (name.Length > 0)
			{
				createCharacterView.SetStatusText("Creating...");
				m_createCharacterModel.CreateCharacter(name);					
			}
			else
			{
				createCharacterView.SetStatusText("Enter a name!");
			}
		}
    }

	public void OnCancelClicked()
	{
        if (!m_createCharacterModel.IsCharacterCreateRequestPending)
		{
			ExitMenu();
		}
	}
		
	public void OnCharacterCreated()
	{
        createCharacterView.SetStatusText("Success!");
		ExitMenu();			
	}

	private void ExitMenu()
	{
        audio.PlayOneShot(successSound, 1.0f);
        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
        {
            GameManager.GetInstance().SetTargetGameState("SelectCharacterScreen");
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
