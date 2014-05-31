using UnityEngine;
using System.Collections;

[System.Serializable]
public class LoginPanelStyle
{
	public GamePanelStyle gamePanelStyle;
	public ButtonStyle buttonStyle;
	public CheckBoxStyle checkBoxStyle;
	public LabelStyle labelStyle;
	public TextEntryStyle textEntryStyle;
	public ScrollTextStyle scrollTextStyle;
}

[System.Serializable]
public class LoginView  : IWidgetEventListener
{
	public LoginPanelStyle loginPanelStyle;

    public LoginController loginController { get; set; }
	
	private WidgetEventDispatcher m_widgetEventDispatcher;
	private WidgetGroup m_rootWidgetGroup;
	private WidgetGroup m_loginPanelWidgetGroup;
	private WidgetGroup m_serverPanelWidgetGroup;
	private LabelWidget m_localHostLabel;
	private LabelWidget m_loginStatusLabel;
	private TextEntryWidget m_serverText;
	private TextEntryWidget m_usernameText;
	private TextEntryWidget m_passwordText;
	private ScrollTextWidget m_serverStatusText;
	private ButtonWidget m_createAccountButton;
	private ButtonWidget m_loginButton;
	private ButtonWidget m_serverOkButton;
	private CheckBoxWidget m_localHostCheckBox;

	public bool IsLocallyHostedServer()
	{
		return m_localHostCheckBox.Enabled;
	}

	public string GetServerAddress()
	{
		return !IsLocallyHostedServer() ? m_serverText.Text : "localhost";
	}

    public string GetUserName()
    {
		return m_usernameText.Text;
    }

    public string GetPassword()
    {
		return m_passwordText.Text;
    }

    public void Start()
    {
		bool isLocalServerRunning= AsyncRPGServer.GetInstance().IsServerActive();
        Screen.showCursor = true;

		// Create the root widget group
		m_rootWidgetGroup = new WidgetGroup(null, Screen.width, Screen.height, 0.0f, 0.0f);
		m_rootWidgetGroup.SetWidgetEventListener(this);
		
		m_widgetEventDispatcher = new WidgetEventDispatcher();
		m_widgetEventDispatcher.Start(m_rootWidgetGroup);

		// Create the widget group to hold the login panel
		m_loginPanelWidgetGroup = 
			new WidgetGroup(
				m_rootWidgetGroup, 
				loginPanelStyle.gamePanelStyle.Width, loginPanelStyle.gamePanelStyle.Height, 
				Screen.width / 2 - loginPanelStyle.gamePanelStyle.Width / 2, 
				Screen.height / 2 - loginPanelStyle.gamePanelStyle.Height / 2);

		// Background for the game info
		new ImageWidget(
			m_loginPanelWidgetGroup, 
			loginPanelStyle.gamePanelStyle.Width, 
			loginPanelStyle.gamePanelStyle.Height, 
			loginPanelStyle.gamePanelStyle.Background, 
			0.0f, 0.0f);

		// Username and Password - Left Aligned
		{
			WidgetLayoutCursor cursor = new WidgetLayoutCursor(
				loginPanelStyle.gamePanelStyle.BorderWidth 
				+ loginPanelStyle.gamePanelStyle.WidgetSpacing,
				loginPanelStyle.gamePanelStyle.BorderWidth 
				+ loginPanelStyle.gamePanelStyle.WidgetSpacing);
			cursor.Kerning= loginPanelStyle.gamePanelStyle.WidgetSpacing;
			cursor.Leading= loginPanelStyle.gamePanelStyle.WidgetSpacing;

			// Server
			cursor.Advance(
				new LabelWidget(
					m_loginPanelWidgetGroup, 
					loginPanelStyle.labelStyle, 
					cursor.X, cursor.Y, 
					"Server:"));
			m_serverText = 
				cursor.Advance(
					new TextEntryWidget(
						m_loginPanelWidgetGroup, 
						loginPanelStyle.textEntryStyle, 
						cursor.X, cursor.Y, 
						"localhost"));
			m_serverText.Visible= !isLocalServerRunning;
			m_localHostLabel=
				new LabelWidget(
					m_loginPanelWidgetGroup, 
					m_serverText.Width, m_serverText.Height,
					m_serverText.LocalX, m_serverText.LocalY, 
					"localhost");
			m_localHostLabel.Visible= isLocalServerRunning;
			cursor.NewLine();

			// Local hosted check box
			cursor.Advance(
				new LabelWidget(
				m_loginPanelWidgetGroup, 
				loginPanelStyle.labelStyle, 
				cursor.X, cursor.Y, 
				"Host Server:"));
			m_localHostCheckBox=
				cursor.Advance(
					new CheckBoxWidget(
						m_loginPanelWidgetGroup, 
						loginPanelStyle.checkBoxStyle, 
						cursor.X, cursor.Y));
			m_localHostCheckBox.Enabled = isLocalServerRunning;
			cursor.NewLine();

			// Username
			cursor.Advance(
				new LabelWidget(
					m_loginPanelWidgetGroup, 
					loginPanelStyle.labelStyle, 
					cursor.X, cursor.Y, 
					"Username:"));
			m_usernameText = 
				cursor.Advance(
					new TextEntryWidget(
						m_loginPanelWidgetGroup, 
						loginPanelStyle.textEntryStyle, 
						cursor.X, cursor.Y, 
						ServerConstants.debugging ? "test" : ""));
			cursor.NewLine();

			// Password
			cursor.Advance(
				new LabelWidget(
					m_loginPanelWidgetGroup, 
					loginPanelStyle.labelStyle, 
					cursor.X, cursor.Y, 
					"Password:"));
			m_passwordText = 
				cursor.Advance(
					new TextEntryWidget(
						m_loginPanelWidgetGroup, 
						loginPanelStyle.textEntryStyle, 
						cursor.X, cursor.Y, 
						ServerConstants.debugging ? "password" : ""));
			m_passwordText.IsPassword= true;
			cursor.NewLine();
		}

		// Buttons - Centered along the bottom
		{
			WidgetLayoutCursor cursor = new WidgetLayoutCursor(
				loginPanelStyle.gamePanelStyle.Width/2
				- loginPanelStyle.gamePanelStyle.WidgetSpacing/2
				- loginPanelStyle.buttonStyle.Width,
				loginPanelStyle.gamePanelStyle.Height
				- loginPanelStyle.buttonStyle.Height
				- loginPanelStyle.labelStyle.Height
				- loginPanelStyle.gamePanelStyle.BorderWidth 
				- loginPanelStyle.gamePanelStyle.WidgetSpacing);

			m_createAccountButton = 
				cursor.Advance(
					new ButtonWidget(
						m_loginPanelWidgetGroup, 
						loginPanelStyle.buttonStyle, 
						cursor.X, cursor.Y,
						"New Account"));
			m_createAccountButton.Visible= false;

			m_loginButton = 
				cursor.Advance(
					new ButtonWidget(
						m_loginPanelWidgetGroup, 
						loginPanelStyle.buttonStyle, 
						cursor.X, cursor.Y,
						"Login"));
			m_loginButton.Visible= true;

			cursor.NewLine();

			m_loginStatusLabel=
				cursor.Advance(
					new LabelWidget(
					m_loginPanelWidgetGroup, 
					loginPanelStyle.gamePanelStyle.Width 
					- 2.0f*loginPanelStyle.gamePanelStyle.BorderWidth, 
					loginPanelStyle.labelStyle.Height, 
					cursor.X, cursor.Y, 
					""));
		}

		// Server Status Panel
		m_serverPanelWidgetGroup = 
			new WidgetGroup(
				m_rootWidgetGroup, 
				loginPanelStyle.gamePanelStyle.Width, loginPanelStyle.gamePanelStyle.Height, 
				Screen.width / 2 - loginPanelStyle.gamePanelStyle.Width / 2, 
				Screen.height / 2 - loginPanelStyle.gamePanelStyle.Height / 2);
		m_serverPanelWidgetGroup.Visible= false;
		
		// Background for the game info
		new ImageWidget(
			m_serverPanelWidgetGroup, 
			loginPanelStyle.gamePanelStyle.Width, 
			loginPanelStyle.gamePanelStyle.Height, 
			loginPanelStyle.gamePanelStyle.Background, 
			0.0f, 0.0f);

		{
			float contentCenter= loginPanelStyle.gamePanelStyle.Width / 2.0f; 
			WidgetLayoutCursor cursor = new WidgetLayoutCursor(
				0.0f,
				loginPanelStyle.gamePanelStyle.BorderWidth 
				+ loginPanelStyle.gamePanelStyle.WidgetSpacing);
			cursor.Kerning= loginPanelStyle.gamePanelStyle.WidgetSpacing;
			cursor.Leading= loginPanelStyle.gamePanelStyle.WidgetSpacing;

			LabelWidget topLabel=
				cursor.Advance(
					new LabelWidget(
						m_serverPanelWidgetGroup, 
						loginPanelStyle.labelStyle, 
						contentCenter - loginPanelStyle.labelStyle.Width / 2.0f, cursor.Y, 
						"[Server]"));
			topLabel.Alignment= TextAnchor.UpperCenter;
			cursor.NewLine();

			m_serverStatusText=
				cursor.Advance(
					new ScrollTextWidget(
					m_serverPanelWidgetGroup,
					loginPanelStyle.scrollTextStyle, 
					contentCenter - loginPanelStyle.scrollTextStyle.Width / 2.0f, cursor.Y, 
					""));
			cursor.NewLine();

			m_serverOkButton=
				cursor.Advance(
					new ButtonWidget(
					m_serverPanelWidgetGroup, 
					loginPanelStyle.buttonStyle, 
					contentCenter - loginPanelStyle.buttonStyle.Width / 2.0f, cursor.Y,
					"Ok"));
			m_serverOkButton.Visible= false;
		}
    }

	public void OnGUI()
	{
		m_rootWidgetGroup.OnGUI();
	}

	public void Update()
	{
		bool isReadyForInput= loginController.IsReadyForInput;

		if (m_loginPanelWidgetGroup.Visible)
		{
			m_loginButton.Visible= isReadyForInput;
			m_createAccountButton.Visible= isReadyForInput;
			m_localHostCheckBox.Visible= isReadyForInput;
		}
		else if (m_serverPanelWidgetGroup.Visible)
		{
			if (!m_serverOkButton.Visible && isReadyForInput)
			{
				loginController.OnServerStartupComplete();
			}

			m_serverOkButton.Visible= isReadyForInput;
		}
	}

	public void OnServerLogMessage(string message)
	{
		m_serverStatusText.AppendLine(message);
	}

	public void OnDestroy()
	{
		m_rootWidgetGroup.OnDestroy();
	}

	public void SetLoginStatus(string message)
	{
		m_loginStatusLabel.Text= message;
	}

	private void SetServerStatusVisible(bool visible)
	{
		if (!m_serverPanelWidgetGroup.Visible && visible)
		{
			// Hide the login panel
			m_loginPanelWidgetGroup.Visible= false;
			
			// Reset and show the server status panel
			m_serverStatusText.Text= "";
			m_serverOkButton.Visible= false;
			m_serverPanelWidgetGroup.Visible= true;

			// Start listening for log messages
			AsyncRPGServer.GetInstance().AddServerLogListener(this.OnServerLogMessage);
		}
		else if (m_serverPanelWidgetGroup.Visible && !visible)
		{
			m_serverPanelWidgetGroup.Visible= false;
			m_loginPanelWidgetGroup.Visible= true;

			// Stop listening for log messages
			AsyncRPGServer.GetInstance().RemoveServerLogListener(this.OnServerLogMessage);
		}
	}

	// IWidgetEventListener
	public void OnWidgetEvent(WidgetEvent widgetEvent)
	{
		if (widgetEvent.EventSource == m_createAccountButton)
		{
			loginController.OnCreateAccountClicked();
		}
		else if (widgetEvent.EventSource == m_loginButton)
		{
			loginController.OnLoginClicked();
		}
		else if (widgetEvent.EventSource == m_localHostCheckBox)
		{
			bool hostServerLocally= m_localHostCheckBox.Enabled;

			m_localHostLabel.Visible= hostServerLocally;
			m_serverText.Visible= !hostServerLocally;

			// If we're turning on the server,
			// show initialization status in the server status panel
			SetServerStatusVisible(hostServerLocally);

			// The the login controller about the change in server hosting
			loginController.OnLocalHostToggled(hostServerLocally);
		}
		else if (widgetEvent.EventSource == m_serverOkButton)
		{
			// Hide the server status panel
			SetServerStatusVisible(false);
		}
	}


}