using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

[System.Serializable]
public class CreateCharacterView : IWidgetEventListener
{
    public Texture panelTexture;
    public float panelWidth;
    public float panelHeight;
    public float portraitWidth;
    public float portraitHeight;
    public float labelWidth;
    public float labelHeight;
    public ButtonStyle buttonStyle;

    public CreateCharacterController CreateCharacterController { get; set; }

    private const float BORDER_WIDTH = 6;
    private const float PORTRAIT_X = 5;
    private const float PORTRAIT_Y = 5;

    private WidgetGroup m_rootWidgetGroup;
    private List<ImageWidget> m_portraits;
    private TextEntryWidget m_nameTextField;
    private LabelWidget m_genderLabel;
    private LabelWidget m_archetypeLabel;
    private LabelWidget m_statusLabel;
	private ButtonWidget m_createButton;
    private ButtonWidget m_cancelButton;
    private ButtonWidget m_nextPortraitButton;
    private ButtonWidget m_previousPortraitButton;

    public void Start()
    {
        // Create the root widget group
        m_rootWidgetGroup = 
            new WidgetGroup(
                null, 
                panelWidth, panelHeight, 
                Screen.width/2 - panelWidth/2, Screen.height/2 - panelHeight/2);
        m_rootWidgetGroup.SetWidgetEventListener(this);
			
		// Background for the game info
		new ImageWidget(m_rootWidgetGroup, panelWidth, panelHeight, panelTexture, 0.0f, 0.0f);

		// Character portraits
		m_portraits = new List<ImageWidget>();
		for (int portraitIndex = 0; portraitIndex < ClientGameConstants.GetPortraitCount(); portraitIndex++ )
		{
            ImageWidget portrait = new ImageWidget(
                m_rootWidgetGroup,
				portraitWidth, portraitHeight,
                Resources.Load<Texture>(ClientGameConstants.GetResourceNameForPicture((uint)portraitIndex)),
                PORTRAIT_X, PORTRAIT_Y);
				
			portrait.Visible = false;
			m_portraits.Add(portrait);
		}

        float statsX = PORTRAIT_X + portraitWidth + BORDER_WIDTH;
        float statsY = PORTRAIT_Y;

        // Character Name
        new LabelWidget(m_rootWidgetGroup, labelWidth, labelHeight, statsX, statsY, "Name:");
        m_nameTextField =
            new TextEntryWidget(m_rootWidgetGroup, labelWidth, labelHeight, statsX + labelWidth, statsY, "");
        m_nameTextField.Restrict = @"[^0-9A-Za-z]";
        m_nameTextField.MaxLength = 12;

        // Character Gender
        statsY += labelHeight;
        new LabelWidget(m_rootWidgetGroup, labelWidth, labelHeight, statsX, statsY, "Gender:");
        m_genderLabel = new LabelWidget(m_rootWidgetGroup, labelWidth, labelHeight, statsX+labelWidth, statsY, "");

        // Character Archetype
        statsY += labelHeight;
        new LabelWidget(m_rootWidgetGroup, labelWidth, labelHeight, statsX, statsY, "Archetype:");
        m_archetypeLabel = new LabelWidget(m_rootWidgetGroup, labelWidth, labelHeight, statsX + labelWidth, statsY, "");

        // Creation Status Label
        m_statusLabel = new LabelWidget(m_rootWidgetGroup, panelWidth, panelHeight, 0, panelHeight - labelHeight, "");
        m_statusLabel.Alignment = TextAnchor.UpperCenter;

        // Create Button
        m_createButton = 
            new ButtonWidget(
                m_rootWidgetGroup, 
                buttonStyle, 
                panelWidth / 3 - buttonStyle.Width / 2, m_statusLabel.LocalY - buttonStyle.Height - 5,
                "Create");

        // Cancel Button
        m_cancelButton =
            new ButtonWidget(
                m_rootWidgetGroup,
                buttonStyle,
                (2*panelWidth) / 3 - buttonStyle.Width / 2, m_statusLabel.LocalY - buttonStyle.Height - 5,
                "Cancel");

        // Previous Portrait Button
        m_previousPortraitButton=
            new ButtonWidget(
                m_rootWidgetGroup,
                buttonStyle,
                m_createButton.LocalX, m_createButton.LocalY - buttonStyle.Height - 5,
                "<");

        // Next Portrait Button
        m_nextPortraitButton =
            new ButtonWidget(
                m_rootWidgetGroup,
                buttonStyle,
                m_cancelButton.LocalX, m_cancelButton.LocalY - buttonStyle.Height - 5,
                ">");
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

    public string GetCharacterName()
    {
        return m_nameTextField.Text;
    }

    public void SetPortraitId(uint portraitId)
    {
        for (int portraitIdx = 0; portraitIdx < ClientGameConstants.GetPortraitCount(); portraitIdx++)
        {
            m_portraits[portraitIdx].Visible = (portraitIdx == portraitId);
        }

        m_nameTextField.Text = ClientGameConstants.GetDefaultNameForPicture(portraitId);
        m_genderLabel.Text = GameConstants.GetGenderString(ClientGameConstants.GetGenderForPicture(portraitId));
        m_archetypeLabel.Text = GameConstants.GetArchetypeString(ClientGameConstants.GetArchetypeForPicture(portraitId));
    }
		
    // IWidgetEventListener
    public void OnWidgetEvent(WidgetEvent widgetEvent)
    {
        if (widgetEvent.EventSource == m_createButton)
        {
            CreateCharacterController.OnCreateCharacterClicked();
        }
        else if (widgetEvent.EventSource == m_cancelButton)
        {
            CreateCharacterController.OnCancelClicked();
        }
        else if (widgetEvent.EventSource == m_previousPortraitButton)
        {
            CreateCharacterController.OnPreviousPortraitClicked();
        }
        else if (widgetEvent.EventSource == m_nextPortraitButton)
        {
            CreateCharacterController.OnNextPortraitClicked();
        }
    }
}
