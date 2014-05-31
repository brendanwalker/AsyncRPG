using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using AsyncRPGSharedLib.Environment;

[System.Serializable]
public class TextEntryStyle
{
	public float Width;
	public float Height;
	public int FontSize= 12;
	public TextAnchor Alignment= TextAnchor.UpperLeft;
}

public class TextEntryWidget : Widget
{
    public string Text { get; set; }
    public int FontSize { get; set; }
    public string Restrict { get; set; }
    public TextAnchor Alignment { get; set; }
    public Color Color { get; set; }
    public int MaxLength { get; set; }
    public bool IsPassword { get; set; }
    public bool EnableReturnSignal { get; set; }
    public bool EnableTabSignal { get; set; }

	public TextEntryWidget(WidgetGroup parentGroup, TextEntryStyle style, float x, float y, string text) :
		base(parentGroup, style.Width, style.Height, x, y)
	{
		Text = text;
		FontSize = style.FontSize;
		Alignment = TextAnchor.UpperLeft;
		Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		MaxLength = 255;
		Restrict = "";
		EnableReturnSignal = false;
		EnableTabSignal = false;
	}

    public TextEntryWidget(WidgetGroup parentGroup, float width, float height, float x, float y, string text) :
        base(parentGroup, width, height, x, y)
    {
        Text = text;
        FontSize = 12;
        Alignment = TextAnchor.UpperLeft;
        Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        MaxLength = 255;
        Restrict = "";
        EnableReturnSignal = false;
        EnableTabSignal = false;
    }

    public override void OnGUI()
    {
        if (Visible)
        {
            Point2d widgetPosition = (ParentGroup is WindowWidget) ? m_localPosition : m_worldPosition;
            Color originalColor = GUI.color;

            GUIStyle style = GUI.skin.GetStyle("TextField");
            style.alignment = Alignment;
            style.fontSize = FontSize;

            GUI.color = Color;
            if (IsPassword)
            {
                Text = 
                    GUI.PasswordField(
                        new Rect(widgetPosition.x, widgetPosition.y, Width, Height), 
                        Text, 
                        '*', 
                        MaxLength, 
                        style);
            }
            else
            {
                Text = 
                    GUI.TextField(
                        new Rect(widgetPosition.x, widgetPosition.y, Width, Height), 
                        Text, 
                        MaxLength, 
                        style);
            }
            GUI.color = originalColor;

            if (Restrict.Length > 0)
            {
                Text = Regex.Replace(Text, Restrict, "");
            }

            if (EnableReturnSignal && 
                Event.current.isKey && 
                //Event.current.type == EventType.KeyDown &&
                Event.current.keyCode == KeyCode.Return)
            {
                Text = Regex.Replace(Text, @"\n|\r|\r\n", "");
                base.OnWidgetEvent(new WidgetEvent(WidgetEvent.eEventType.textInputReturn, this, null));
            }

            if (EnableTabSignal && 
                Event.current.isKey &&
                Event.current.type == EventType.KeyDown &&
                Event.current.keyCode == KeyCode.Tab)
            {
                Text = Regex.Replace(Text, @"\t", "");
                base.OnWidgetEvent(new WidgetEvent(WidgetEvent.eEventType.textInputTab, this, null));
            }
        }
    }
}
