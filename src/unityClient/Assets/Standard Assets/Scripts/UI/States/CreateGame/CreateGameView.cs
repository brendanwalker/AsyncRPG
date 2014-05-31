using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CreateGameView : IWidgetEventListener
{
    public GamePanelStyle gamePanelStyle;
    public ButtonStyle buttonStyle;
    public CheckBoxStyle chekBoxStyle;

    public CreateGameController CreateGameController { get; set; }

    private const float STATS_LABEL_HEIGHT = 20;
    private const float BORDER_WIDTH = 6;

    private WidgetGroup m_rootWidgetGroup;
	private TextEntryWidget m_gameNameTextField;		
	private LabelWidget m_ownerNameLabel;
	private TextEntryWidget m_IRCServerTextField;
	private TextEntryWidget m_IRCPortTextField;
	private CheckBoxWidget m_IRCEnabledToggle;
    private CheckBoxWidget m_IRCEncryptionEnabledToggle;
	private LabelWidget m_statusLabel;
	private ButtonWidget m_createButton;
    private ButtonWidget m_cancelButton;

    public void Start()
    {
        SessionData sessionData = SessionData.GetInstance();

        // Create the root widget group
        m_rootWidgetGroup = new WidgetGroup(null, gamePanelStyle.Width, gamePanelStyle.Height, 0.0f, 0.0f);
        m_rootWidgetGroup.SetWidgetEventListener(this);
			
		// Background for the game info
		ImageWidget gamePanel = 
            new ImageWidget(
                m_rootWidgetGroup, 
                gamePanelStyle.Width, 
                gamePanelStyle.Height, 
                gamePanelStyle.Background, 
				0.0f, 0.0f);

		float statsLabelWidth = (gamePanel.Width - 2 * BORDER_WIDTH) / 2 - 3;
		float statsX = 10;
		float statsY = 10;
			
		// Owner Name
		LabelWidget ownerNameLabel = 
            new LabelWidget(m_rootWidgetGroup, statsLabelWidth, STATS_LABEL_HEIGHT, statsX, statsY, "Owner Name:");
		ownerNameLabel.Alignment = TextAnchor.UpperRight;
		m_ownerNameLabel = 
            new LabelWidget(
                m_rootWidgetGroup, statsLabelWidth, STATS_LABEL_HEIGHT, 
                statsX + statsLabelWidth, statsY, sessionData.UserName);
        m_ownerNameLabel.Text = "";
			
		// Game name
		statsY += STATS_LABEL_HEIGHT;
		LabelWidget gameNameLabel = 
            new LabelWidget(m_rootWidgetGroup, statsLabelWidth, STATS_LABEL_HEIGHT, statsX, statsY, "Game Name:");
        gameNameLabel.Alignment = TextAnchor.UpperRight;
		m_gameNameTextField = new TextEntryWidget(m_rootWidgetGroup, 
			statsLabelWidth, STATS_LABEL_HEIGHT, statsX+statsLabelWidth, statsY, "");
					
		// IRC Server
		statsY += STATS_LABEL_HEIGHT;
		LabelWidget IRCServerLabel =
            new LabelWidget(m_rootWidgetGroup, statsLabelWidth, STATS_LABEL_HEIGHT, statsX, statsY, "IRC Server:");
        IRCServerLabel.Alignment = TextAnchor.UpperRight;
		m_IRCServerTextField = new TextEntryWidget(m_rootWidgetGroup, 
			statsLabelWidth, STATS_LABEL_HEIGHT, statsX+statsLabelWidth, statsY, "");
		m_IRCServerTextField.Text = ServerConstants.DEFAULT_IRC_SERVER;

		// IRC Port
		statsY += STATS_LABEL_HEIGHT;
		LabelWidget IRCPortLabel =
            new LabelWidget(m_rootWidgetGroup, statsLabelWidth, STATS_LABEL_HEIGHT, statsX, statsY, "IRC Port:");
        IRCPortLabel.Alignment = TextAnchor.UpperRight;
		m_IRCPortTextField = new TextEntryWidget(m_rootWidgetGroup, 
			statsLabelWidth, STATS_LABEL_HEIGHT, statsX + statsLabelWidth, statsY, "");
		m_IRCPortTextField.Restrict = @"[^0-9]";
        m_IRCPortTextField.MaxLength = 6;
        m_IRCPortTextField.Text = ServerConstants.DEFAULT_IRC_PORT.ToString();
				
		// IRC Enabled
		statsY += STATS_LABEL_HEIGHT;
		LabelWidget IRCEnabledLabel =
            new LabelWidget(m_rootWidgetGroup, statsLabelWidth, STATS_LABEL_HEIGHT, statsX, statsY, "IRC Enabled:");
        IRCEnabledLabel.Alignment = TextAnchor.UpperRight;
		m_IRCEnabledToggle = new CheckBoxWidget(m_rootWidgetGroup, chekBoxStyle, statsX + statsLabelWidth, statsY);
		m_IRCEnabledToggle.Enabled = true;

		// IRC Encryption Enabled
		statsY += STATS_LABEL_HEIGHT;
        LabelWidget IRCEncryptionEnabledLabel =
            new LabelWidget(m_rootWidgetGroup, statsLabelWidth, STATS_LABEL_HEIGHT, statsX, statsY, "IRC Encryption Enabled:");
        IRCEncryptionEnabledLabel.Alignment = TextAnchor.UpperRight;
        m_IRCEncryptionEnabledToggle = new CheckBoxWidget(m_rootWidgetGroup, chekBoxStyle, statsX + statsLabelWidth, statsY);
		m_IRCEncryptionEnabledToggle.Enabled = true;
				
		// Creation status
        m_statusLabel = new LabelWidget(m_rootWidgetGroup, gamePanel.Width, STATS_LABEL_HEIGHT, 0.0f, 0.0f, "");
		m_statusLabel.SetLocalPosition(0, gamePanel.Height - m_statusLabel.Height - BORDER_WIDTH);
        m_statusLabel.Alignment = TextAnchor.UpperCenter;
			
		// Create button
        m_createButton = new ButtonWidget(m_rootWidgetGroup, buttonStyle, 0, 0, "Create");
		m_createButton.SetLocalPosition(gamePanel.Width/3 - m_createButton.Width/2, m_statusLabel.LocalY - m_createButton.Height - 5);
			
		// Cancel button
        m_cancelButton = new ButtonWidget(m_rootWidgetGroup, buttonStyle, 0, 0, "Cancel");
		m_cancelButton.SetLocalPosition((2*gamePanel.Width)/3 - m_cancelButton.Width/2, m_statusLabel.LocalY - m_cancelButton.Height - 5);			
			
		// Center the group info widgets
        m_rootWidgetGroup.SetLocalPosition(Screen.width / 2 - gamePanel.Width / 2, Screen.height / 2 - gamePanel.Height / 2);
    }

    public void OnGUI()
    {
        m_rootWidgetGroup.OnGUI();
    }

    public void OnDestroy()
    {
        m_rootWidgetGroup.OnDestroy();
    }

	public void SetStatusText(string status)
	{
		m_statusLabel.Text = status;
	}
		
	public string GetGameName()
	{
        return m_gameNameTextField.Text;
	}
		
	public string GetIrcServer()
	{
        return m_IRCServerTextField.Text;
	}
		
	public uint GetIrcPort()
	{
        return uint.Parse(m_IRCPortTextField.Text);
	}
		
	public bool GetIrcEnabled()
	{
        return m_IRCEnabledToggle.Enabled;
	}
		
	public bool GetIrcEncryptionEnabled()
	{
        return m_IRCEncryptionEnabledToggle.Enabled;
	}

    // IWidgetEventListener
    public void OnWidgetEvent(WidgetEvent widgetEvent)
    {
        if (widgetEvent.EventSource == m_createButton)
        {
            CreateGameController.OnGameCreateClicked();
        }
        else if (widgetEvent.EventSource == m_cancelButton)
        {
            CreateGameController.OnCancelClicked();
        }
    }
}
