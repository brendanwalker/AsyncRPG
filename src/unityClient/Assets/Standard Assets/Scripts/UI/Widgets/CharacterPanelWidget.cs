using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;

[System.Serializable]
public class CharacterPanelStyle
{
    public float BackgroundWidth;
    public float BackgroundHeight;
    public Texture Background;
    public float PortraitWidth;
    public float PortraitHeight;
    public float LabelWidth;
    public float LabelHeight;
}

public class CharacterPanelWidget : WidgetGroup 
{
    private WidgetGroup m_characterDataGroup;
    private List<ImageWidget> m_portraits;
	private LabelWidget m_nameText;
	private LabelWidget m_genderText;
	private LabelWidget m_archetypeText;
	private LabelWidget m_levelText;
	private LabelWidget m_gameText;	

    public CharacterPanelWidget(WidgetGroup parentGroup, CharacterPanelStyle style, float x, float y)
        : base(parentGroup, style.BackgroundWidth, style.BackgroundHeight, x, y)
    {
        // Background for the character info
        new ImageWidget(this, style.BackgroundWidth, style.BackgroundHeight, style.Background, 0, 0);

        // Create a group to hold the data
        m_characterDataGroup = new WidgetGroup(this, style.BackgroundWidth, style.BackgroundHeight, 0.0f, 0.0f);

        // Character Portraits
        m_portraits = new List<ImageWidget>();
        for (int portraitIndex = 0; portraitIndex < ClientGameConstants.GetPortraitCount(); portraitIndex++)
        {
            ImageWidget portrait =
                new ImageWidget(
                    m_characterDataGroup, 
                    style.PortraitWidth, 
                    style.PortraitHeight, 
                    Resources.Load(ClientGameConstants.GetResourceNameForPicture((uint)portraitIndex)) as Texture, 
                    5.0f, 5.0f);

            portrait.Visible = false;
            m_portraits.Add(portrait);
        }

        {
            float statsX = style.PortraitWidth + 10.0f;
            float statsY = 5.0f;

			// Character name
            new LabelWidget(m_characterDataGroup, style.LabelWidth, style.LabelHeight, statsX, statsY, "Name:");
            m_nameText = 
                new LabelWidget(m_characterDataGroup, style.LabelWidth, style.LabelHeight, statsX + style.LabelWidth, statsY, "");
		
			// Character gender
			statsY += style.LabelHeight;
            new LabelWidget(m_characterDataGroup, style.LabelWidth, style.LabelHeight, statsX, statsY, "Gender:");
            m_genderText = 
                new LabelWidget(m_characterDataGroup, style.LabelWidth, style.LabelHeight, statsX + style.LabelWidth, statsY, "");

			// Character archetype
			statsY += style.LabelHeight;
            new LabelWidget(m_characterDataGroup, style.LabelWidth, style.LabelHeight, statsX, statsY, "Archetype:");
            m_archetypeText = 
                new LabelWidget(m_characterDataGroup, style.LabelWidth, style.LabelHeight, statsX + style.LabelWidth, statsY, "");
			
			// Level
			statsY += style.LabelHeight;
            new LabelWidget(m_characterDataGroup, style.LabelWidth, style.LabelHeight, statsX, statsY, "Level:");
            m_levelText = 
                new LabelWidget(m_characterDataGroup, style.LabelWidth, style.LabelHeight, statsX + style.LabelWidth, statsY, "");
			
			// Game
			statsY += style.LabelHeight;
            new LabelWidget(m_characterDataGroup, style.LabelWidth, style.LabelHeight, statsX, statsY, "Game:");
            m_gameText = 
                new LabelWidget(m_characterDataGroup, style.LabelWidth, style.LabelHeight, statsX + style.LabelWidth, statsY, "");
        }

        // By default this isn't visible
        HideCharacterData();
    }

	public void HideCharacterData()
	{
        m_characterDataGroup.Visible = false;		
	}
		
	public void ShowCharacterData(CharacterData characterEntry)
	{
        for (int portraitIndex = 0; portraitIndex < ClientGameConstants.GetPortraitCount(); portraitIndex++)
		{
			m_portraits[portraitIndex].Visible = (portraitIndex == characterEntry.picture_id);
		}

        m_nameText.Text = characterEntry.character_name;
        m_genderText.Text = GameConstants.GetGenderString(ClientGameConstants.GetGenderForPicture((uint)characterEntry.picture_id));
        m_archetypeText.Text = GameConstants.GetArchetypeString(ClientGameConstants.GetArchetypeForPicture((uint)characterEntry.picture_id));
        m_levelText.Text = characterEntry.power_level.ToString();
        m_gameText.Text = characterEntry.game_name;

        m_characterDataGroup.Visible = true;
	}
}