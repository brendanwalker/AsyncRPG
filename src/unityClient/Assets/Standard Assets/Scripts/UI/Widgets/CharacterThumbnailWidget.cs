using UnityEngine;
using System;
using System.Collections;
using AsyncRPGSharedLib.Common;

[System.Serializable]
public class CharacterThumbnailStyle
{
    public float BackgroundWidth;
    public float BackgroundHeight;
    public Texture Background;
    public float PortraitWidth;
    public float PortraitHeight;
    public float LabelWidth;
    public float LabelHeight;
}

public class CharacterThumbnailWidget : WidgetGroup 
{
    public CharacterThumbnailWidget(
        WidgetGroup parentGroup, 
        CharacterThumbnailStyle style, 
        CharacterData characterData)
        : base(parentGroup, style.BackgroundWidth, style.BackgroundHeight, 0, 0)
    {
        new ImageWidget(this, style.BackgroundWidth, style.BackgroundHeight, style.Background, 0.0f, 0.0f);

		// Background for the character info
        string portraitResource = ClientGameConstants.GetResourceNameForPicture((uint)characterData.picture_id);
        ImageWidget portrait=
            new ImageWidget(
                this,
                style.PortraitWidth, style.PortraitHeight,
                Resources.Load<Texture>(portraitResource), 
                5, 5);
					
		// Character name
        LabelWidget nameLabel=
            new LabelWidget(
                this,
                style.LabelWidth, style.LabelHeight,
                portrait.LocalX + portrait.Width + 3, 0,
                characterData.character_name);
			
		// Character info
        string archetype = GameConstants.GetArchetypeString(ClientGameConstants.GetArchetypeForPicture((uint)characterData.picture_id));
        string infoString = "Lvl " + characterData.power_level.ToString() + " " + archetype;
        new LabelWidget(
            this,
            style.LabelWidth, style.LabelHeight,
            portrait.LocalX + portrait.Width + 3, 
            nameLabel.LocalY + nameLabel.Height,
            infoString);
			
		this.Visible = false;
    }
}
