using UnityEngine;
using System.Collections;

[System.Serializable]
public class WindowStyle
{
    public float WindowWidth;
    public float WindowHeight;
    public float TitleBarHeight;
}

public class WindowWidget : WidgetGroup
{
    private string m_title;

    public WindowWidget(WidgetGroup parentGroup, string title, WindowStyle style, float x, float y)
        : base(parentGroup, style.WindowWidth, style.WindowHeight, x, y)
    {
        m_title = title;
    }

    private void WindowOnGUICallback(int windowID)
    {
        for (int childIndex = 0; childIndex < this.ChildWidgetCount; ++childIndex)
        {
            IWidget widget = this.GetChildWidget(childIndex);
            widget.OnGUI();
        }
    }

    public override void OnGUI()
    {
        if (Visible)
        {
            GUI.Window(
                0, 
                new Rect(WorldX, WorldY, Width, Height), 
                WindowOnGUICallback, 
                m_title);
        }
    }    

	// IWidgetEventListener
    public override void OnWidgetEvent(WidgetEvent widgetEvent)
    {
		if (widgetEvent.EventType == WidgetEvent.eEventType.mouseDrag)
		{
            WidgetEvent.MouseDragEventParameters dragEvent=
                widgetEvent.EventParameters as WidgetEvent.MouseDragEventParameters;

            this.SetLocalPosition(this.LocalX + dragEvent.deltaX, this.LocalY + dragEvent.deltaY);
		}
        else
        {
            base.OnWidgetEvent(widgetEvent);
        }
    }
}
