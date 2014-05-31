using UnityEngine;
using System.Collections;

[System.Serializable]
public class CheckBoxStyle
{
    public float Width;
    public float Height;
    public Texture ButtonSkin;
}

public class CheckBoxWidget : Widget
{
    public bool Enabled { get; set; }

    private Texture m_buttonSkin;

    public CheckBoxWidget(WidgetGroup parentGroup, CheckBoxStyle style, float x, float y) :
        base(parentGroup, style.Width, style.Height, x, y)
    {
        m_buttonSkin = style.ButtonSkin;
        Enabled = false;
    }

    public override void OnGUI()
    {
        if (Visible)
        {
            bool newEnabled = GUI.Toggle(new Rect(m_worldPosition.x, m_worldPosition.y, Width, Height), Enabled, m_buttonSkin);

            if (newEnabled != Enabled)
            {
                Enabled = newEnabled;

                m_parentGroup.OnWidgetEvent(new WidgetEvent(WidgetEvent.eEventType.checkboxToggle, this, null));
            }
        }
    }
}
