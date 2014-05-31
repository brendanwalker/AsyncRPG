using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class CreateAccountController : GameState
{
    public CreateAccountView createAccountView;
    public AudioClip successSound;

    private CreateAccountModel m_createAccountModel;
    private bool m_readyForInput;

    public bool IsReadyForInput
    {
        get { return m_readyForInput; }
    }

    public CreateAccountModel Model
    {
        get { return m_createAccountModel; }
    }

    public string Status
    {
        get { return m_createAccountModel.Status; }
        set { m_createAccountModel.Status = value; }
    }

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        m_createAccountModel = new CreateAccountModel(this);

        createAccountView.createAccountController = this;

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

        createAccountView.OnGUI();
    }

	public void OnCreateAccountClicked()
	{
        if (createAccountView.GetUserName().Length == 0)
		{
            Status = "Please specify a username";
		}
        else if (createAccountView.GetPassword().Length == 0)
		{
            Status = "Please specify a password";
		}
        else if (!createAccountView.DoPasswordsMatch())
		{
            Status = "Passwords don't match";
		}
        else if (createAccountView.GetEmailAddress().Length == 0)
		{
			Status = "Please specify an e-mail address";
		}
        else if (!m_createAccountModel.IsCreateAccountRequestPending)
		{
            m_createAccountModel.RequestCreateAccount(
                createAccountView.GetUserName(),
                createAccountView.GetPassword(), 
                createAccountView.GetEmailAddress());
			//_createAccountView.setCreateAccountButtonVisible(false);
            Status = "Creating Account...";
		}			
	}
		
	public void OnCancelClicked()
	{
		audio.PlayOneShot(successSound, 1.0f);

        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
        {
            GameManager.GetInstance().SetTargetGameState("LoginScreen");
        });
	}		

	public void OnCreateAccountSucceeded(string result)
	{
		Status = result;
						
		audio.PlayOneShot(successSound, 1.0f);

        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
        {
            GameManager.GetInstance().SetTargetGameState("LoginScreen");
        });
	}		
		
	public void OnCreateAccountFailed(string reason)
	{
        Status= reason;
	}
}
