using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class ScrollTextStyle
{
	public float Width;
	public float Height;
	public int FontSize= 12;
	public TextAnchor Alignment= TextAnchor.UpperLeft;
}

public class ScrollTextWidget : Widget 
{
    public string Text { get; set; }
    public int FontSize { get; set; }
    public TextAnchor Alignment { get; set; }
    public Color Color { get; set; }
    public float ScrollRate { get; set; }

    private Vector2 m_scrollPosition;
    private bool m_scrollToEnd;

	public ScrollTextWidget(WidgetGroup parentGroup, ScrollTextStyle style, float x, float y, string text) :
		base(parentGroup, style.Width, style.Height, x, y)
	{
		Text = text;
		FontSize = style.FontSize;
		Alignment = style.Alignment;
		Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		ScrollRate = 1.0f;
		
		m_scrollPosition= new Vector2(0.0f, 0.0f);
		m_scrollToEnd = false;
	}

    public ScrollTextWidget(WidgetGroup parentGroup, float width, float height, float x, float y, string text) :
        base(parentGroup, width, height, x, y)
    {
        Text = text;
        FontSize = 12;
        Alignment = TextAnchor.UpperLeft;
        Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        ScrollRate = 1.0f;

        m_scrollPosition= new Vector2(0.0f, 0.0f);
        m_scrollToEnd = false;
    }

	public void ClearText()
	{
		this.Text = "";
	}
		
	public void AppendLine(string line)
	{
		this.Text+= (line + "\n");
        m_scrollToEnd = true;
	}			
			
	public void Scroll(float delta)
	{
        m_scrollPosition.y = Math.Max(m_scrollPosition.y + ((float)delta) * ScrollRate, 0.0f);
        m_scrollToEnd = false;
	}

    public override void OnGUI()
    {
        if (Visible)
        {
            bool parentGroupIsWindow = ParentGroup is WindowWidget;

            Color originalColor = GUI.color;

            GUIStyle style = GUI.skin.GetStyle("Label");
            style.alignment = Alignment;
            style.fontSize = FontSize;

            if (m_scrollToEnd)
            {
                GUIContent guiContext = new GUIContent(this.Text);

                m_scrollPosition.y = style.CalcHeight(guiContext, this.Width);
                m_scrollToEnd = false;
            }

            if (!parentGroupIsWindow)
            {             
                GUI.BeginGroup(new Rect(m_worldPosition.x, m_worldPosition.y, this.Width, this.Height));
            }

            m_scrollPosition = 
                GUILayout.BeginScrollView(
                    m_scrollPosition, 
                    GUILayout.Width(this.Width), 
                    GUILayout.Height(this.Height));

            GUI.color = Color;
            GUILayout.Label(Text, style);
            GUI.color = originalColor;

            GUILayout.EndScrollView();

            if (!parentGroupIsWindow)
            {
                GUI.EndGroup();
            }
        }
    }

	// IWidgetEventListener
	public override void OnWidgetEvent(WidgetEvent widgetEvent)
	{
		if (widgetEvent.EventType == WidgetEvent.eEventType.mouseWheel)
		{
            WidgetEvent.MousWheelEventParameters wheelEvent = 
                (WidgetEvent.MousWheelEventParameters)widgetEvent.EventParameters;

			Scroll(wheelEvent.delta);
		}
		else
		{
            base.OnWidgetEvent(widgetEvent);
		}
	}
}
