using UnityEngine;
using System.Collections;

public class ImageWidget : Widget
{
    public Texture Image { get; set; }
    public ScaleMode ScaleMode { get; set; }
    public bool AlphaBlend { get; set; }
    public Color Color { get; set; }

    public ImageWidget(WidgetGroup parentGroup, float width, float height, Texture image, float x, float y) :
        base(parentGroup, width, height, x, y)
    {
        Image = image;
        Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    public override void OnGUI()
    {
        if (Visible)
        {
            Color originalColor = GUI.color;

            GUI.color = Color;
            GUI.DrawTexture(new Rect(m_worldPosition.x, m_worldPosition.y, Width, Height), Image, ScaleMode, AlphaBlend);
            GUI.color = originalColor;
        }
    }
}
