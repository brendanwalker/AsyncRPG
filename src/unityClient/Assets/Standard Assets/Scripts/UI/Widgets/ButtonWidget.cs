using UnityEngine;
using System.Collections;

[System.Serializable]
public class ButtonStyle
{
    public float Width;
    public float Height;
}

public class ButtonWidget : Widget
{
    public string Label { get; set; }

    public ButtonWidget(WidgetGroup parentGroup, ButtonStyle style, float x, float y, string label) :
        base(parentGroup, style.Width, style.Height, x, y)
    {
        Label = label;
    }

    public override void OnGUI()
    {
        if (Visible)
        {
            if (GUI.Button(new Rect(m_worldPosition.x, m_worldPosition.y, Width, Height), Label))
            {
                m_parentGroup.OnWidgetEvent(new WidgetEvent(WidgetEvent.eEventType.buttonClick, this, null));
            }
        }
    }
}
