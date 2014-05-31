using UnityEngine;
using System.Collections;

[System.Serializable]
public class GamePanelStyle
{
    public float Width;
    public float Height;
    public Texture Background;
	public float BorderWidth= 10;
	public float WidgetSpacing= 6;
}

public class GamePanelWidget : WidgetGroup 
{
	private const float BORDER_WIDTH = 10;
	private const float STATS_LABEL_WIDTH = 70;
	private const float STATS_LABEL_HEIGHT = 20;

    private ImageWidget m_background;
    private LabelWidget m_nameLabel;
    private LabelWidget m_nameText;
    private LabelWidget m_ownerLabel;
    private LabelWidget m_ownerText;
    private LabelWidget m_charactersLabel;
    private LabelWidget m_charactersText;

    public GamePanelWidget(WidgetGroup parentGroup, GamePanelStyle gamePanelStyle, float x, float y)
        : base(parentGroup, gamePanelStyle.Width, gamePanelStyle.Height, x, y)
    {
        float statsX = BORDER_WIDTH;
        float statsY = BORDER_WIDTH;

        // Background for the character info
        m_background = new ImageWidget(this, gamePanelStyle.Width, gamePanelStyle.Height, gamePanelStyle.Background, 0, 0);
						
		// Game name
		m_nameLabel = new LabelWidget(this, STATS_LABEL_WIDTH, STATS_LABEL_HEIGHT, statsX, statsY, "Name:");
		m_nameLabel.Visible = false;
			
		m_nameText = new LabelWidget(this, STATS_LABEL_WIDTH, STATS_LABEL_HEIGHT, statsX + STATS_LABEL_WIDTH, statsY, "");
		m_nameText.Visible = false;
		
		// Game Owner
		statsY += STATS_LABEL_HEIGHT;
		m_ownerLabel = new LabelWidget(this, STATS_LABEL_WIDTH, STATS_LABEL_HEIGHT, statsX, statsY, "Owner:");
		m_ownerLabel.Visible = false;
			
		m_ownerText = new LabelWidget(this, STATS_LABEL_WIDTH, STATS_LABEL_HEIGHT, statsX + STATS_LABEL_WIDTH, statsY, "");
		m_ownerText.Visible = false;
			
		// Characters
		statsY += STATS_LABEL_HEIGHT;
        m_charactersLabel = new LabelWidget(this, STATS_LABEL_WIDTH, STATS_LABEL_HEIGHT, statsX, statsY, "Characters:");
		m_charactersLabel.Visible = false;
			
		statsY += STATS_LABEL_HEIGHT;
		m_charactersText = new LabelWidget(this, m_background.Width - 2*BORDER_WIDTH, STATS_LABEL_HEIGHT, statsX, statsY, "");
		m_charactersText.Visible = false;
    }

	public void HideGameData()
	{
		m_nameLabel.Visible= false;
		m_nameText.Visible= false;
		m_ownerLabel.Visible= false;	
		m_ownerText.Visible = false;
		m_charactersLabel.Visible = false;
		m_charactersText.Visible = false;
	}
		
	public void ShowGameData(GameResponseEntry gameEntry)
	{		
		m_nameLabel.Visible= true;
		m_nameText.Visible = true;
		m_nameText.Text = gameEntry.game_name;
						
		m_ownerLabel.Visible= true;	
		m_ownerText.Visible= true;
		m_ownerText.Text= gameEntry.owner_account_name;			
			
		m_charactersLabel.Visible = true;
		m_charactersText.Visible = true;
			
		string characterNames = "";
		foreach (string characterName in gameEntry.character_names)
		{
			characterNames += ("  "+characterName + "\n");
		}
			
		m_charactersText.Text = characterNames;
	}
}
