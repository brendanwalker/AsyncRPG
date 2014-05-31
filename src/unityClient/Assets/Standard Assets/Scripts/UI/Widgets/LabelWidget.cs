using UnityEngine;
using System.Collections;

[System.Serializable]
public class LabelStyle
{
	public float Width;
	public float Height;
	public int FontSize= 12;
	public TextAnchor Alignment= TextAnchor.UpperLeft;
}

public class LabelWidget : Widget 
{
    public string Text { get; set; }
    public int FontSize { get; set; }
    public TextAnchor Alignment { get; set; }
    public Color Color { get; set; }

	public LabelWidget(WidgetGroup parentGroup, LabelStyle style, float x, float y, string text) :
		base(parentGroup, style.Width, style.Height, x, y)
	{
		Text = text;
		FontSize = style.FontSize;
		Alignment = style.Alignment;
		Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	}

    public LabelWidget(WidgetGroup parentGroup, float width, float height, float x, float y, string text) :
        base(parentGroup, width, height, x, y)
    {
        Text = text;
        FontSize = 12;
        Alignment = TextAnchor.UpperLeft;
        Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    public override void OnGUI()
    {
        if (Visible)
        {
            Color originalColor = GUI.color;

            GUIStyle style = GUI.skin.GetStyle("Label");
            style.alignment = Alignment;
            style.fontSize = FontSize;

            GUI.color = Color;
            GUI.Label(new Rect(m_worldPosition.x, m_worldPosition.y, Width, Height), Text, style);
            GUI.color = originalColor;
        }
    }
}
