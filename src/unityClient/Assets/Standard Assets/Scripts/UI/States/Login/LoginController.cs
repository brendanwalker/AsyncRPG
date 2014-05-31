using UnityEngine;
using System.Collections;
using LitJson;

[RequireComponent(typeof(AudioSource))]
public class LoginController : GameState 
{
    public LoginView loginView;
    public AudioClip successSound;

    private LoginModel m_loginModel;
    private bool m_readyForInput;

    public bool IsReadyForInput
    {
        get 
		{ 
			return m_readyForInput && !AsyncRPGServer.GetInstance().IsServerInitializing(); 
		}
    }

	// Use this for initialization
	public override void Start () 
    {
        base.Start();

        m_loginModel = new LoginModel(this);
        
        loginView.loginController = this;
		loginView.Start();

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
        // $TODO Free any pending queries on the model
    }
	
	// Update is called once per frame
    public override void Update() 
    {
        base.Update();

		loginView.Update();
	}

    // Update is called once per frame
    public override void OnGUI()
    {
        base.OnGUI();

        loginView.OnGUI();
    }

    public void OnLoginClicked()
    {
        if (!m_loginModel.LoginRequestPending)
        {
			// Set the server address before attempting login
			string serverAddress= loginView.GetServerAddress().Trim();

			if (serverAddress.Length > 0)
			{
				ServerConstants.serverURL= serverAddress;

	            m_loginModel.RequestLogin(loginView.GetUserName(), loginView.GetPassword());
				loginView.SetLoginStatus("Authenticating...");
			}
			else
			{
				loginView.SetLoginStatus("Please specify a server address");
			}
        }
    }

    public void OnCreateAccountClicked()
    {
        audio.PlayOneShot(successSound, 1.0f);

        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
        {
            GameManager.GetInstance().SetTargetGameState("CreateAccountScreen");
        });
    }

    public void OnLoginSucceeded()
    {
        loginView.SetLoginStatus("Success!");
        audio.PlayOneShot(successSound, 1.0f);

        Fader.AttachFaderTo(this.gameObject, Color.black, Fader.eFadeType.fadeOut, 1.0f, () =>
        {
            GameManager.GetInstance().SetTargetGameState("SelectGameScreen");
        });

        Debug.Log("Login Succeeded!");
    }		

	public void OnLocalHostToggled(
		bool hostServerLocally)
	{
		AsyncRPGServer server= AsyncRPGServer.GetInstance();

		Debug.Log("Active: "+server.IsServerActive()+", Initializing: "+server.IsServerInitializing());
		if (hostServerLocally && !server.IsServerActive() && !server.IsServerInitializing())
		{
			Debug.Log("Start Server!");
			server.StartServer();
		}
		else if (!hostServerLocally && (server.IsServerActive() || server.IsServerInitializing()))
		{
			Debug.Log("Stop Server!");
			server.StopServer();
		}
	}

	public void OnServerStartupComplete()
	{
		audio.PlayOneShot(successSound, 1.0f);
	}
		
    public void OnLoginFailed(string reason)
    {
        //_loginView.setLoginButtonVisible(true);
		loginView.SetLoginStatus(reason);

        Debug.Log("Login Failed: "+reason);
    }
}
