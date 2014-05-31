using UnityEngine;
using System.Collections;

[System.Serializable]
public class GameThumbnailStyle
{
    public float Width;
    public float Height;
    public float LabelHeight;
    public Texture Background;
}

public class GameThumbnailWidget : WidgetGroup 
{
    private const float BORDER_WIDTH = 6;

    public GameThumbnailWidget(WidgetGroup parentGroup, GameThumbnailStyle style, GameResponseEntry gameInfo, float x, float y)
        : base(parentGroup, style.Width, style.Height, x, y)
    {
        new ImageWidget(this, style.Width, style.Height, style.Background, 0.0f, 0.0f);

        LabelWidget gameNameLabel = 
            new LabelWidget(
                this, 
                40, style.LabelHeight, 
                BORDER_WIDTH, BORDER_WIDTH, 
                "Game:");
        gameNameLabel.Alignment = TextAnchor.UpperRight;

        LabelWidget gameOwnerLabel = 
            new LabelWidget(
                this, 
                40, style.LabelHeight, 
                BORDER_WIDTH, gameNameLabel.LocalY + gameNameLabel.Height, 
                "Owner:");
        gameOwnerLabel.Alignment = TextAnchor.UpperRight;

        {
            float textWidth = style.Width - gameNameLabel.Width - 2 * BORDER_WIDTH;
            float textX = BORDER_WIDTH + gameNameLabel.Width;

            LabelWidget gameNameText = 
                new LabelWidget(
                    this, 
                    textWidth, style.LabelHeight, 
                    textX, BORDER_WIDTH, 
                    gameInfo.game_name);
            gameNameText.Alignment = TextAnchor.UpperLeft;

            LabelWidget gameOwnerText = 
                new LabelWidget(
                    this, 
                    textWidth, style.LabelHeight, 
                    textX, gameNameText.LocalY + gameNameText.Height, 
                    gameInfo.owner_account_name);
            gameOwnerText.Alignment = TextAnchor.UpperLeft;
        }
			
		this.Visible = false;
    }
}
