using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Environment;

public class WidgetEventDispatcher
{
    private enum eMouseButton
    {
        left= 0,
        right= 1,
        middle= 2
    }

    private WidgetGroup m_rootWidgetGroup;
    private Point2d m_mousePosition;
    private bool m_mouseIsDragging;
    private bool m_mouseWasDown;
    private IWidget m_mouseDownIWidget;
    private IWidget m_mouseOverIWidget;

    public WidgetEventDispatcher()
    {
        m_mousePosition = new Point2d();
        m_rootWidgetGroup = null;
        m_mouseOverIWidget = null;
        m_mouseDownIWidget = null;
        m_mouseIsDragging = false;
        m_mouseWasDown = false;
    }

    public IWidget MouseOverWidget
    {
        get { return m_mouseOverIWidget; }
    }

    public bool LogEnabled
    {
        get
        {
            return Debug.isDebugBuild ? DebugRegistry.TestToggle("ui.event.log") : false;
        }
    }

    public void Start(WidgetGroup widgetGroup)
    {
        Point2d newMousePosition = GetMousePosition();

        m_rootWidgetGroup = widgetGroup;           

        m_mousePosition.Set(newMousePosition.x, newMousePosition.y);
        m_mouseOverIWidget = m_rootWidgetGroup.FindChildIWidgetContainingPoint(m_mousePosition);
        m_mouseIsDragging = false;
        m_mouseWasDown = Input.GetMouseButton((int)eMouseButton.left);

        if (Debug.isDebugBuild)
        {
            DebugRegistry.SetToggle("ui.event.log", false);
        }
    }

    public void Update()
    {
        Point2d mousePosition = GetMousePosition();
        float mouseScrollAmount= Input.GetAxis("Mouse ScrollWheel");
        bool mouseIsDown = Input.GetMouseButton((int)eMouseButton.left);

        if (mousePosition.x != m_mousePosition.x || mousePosition.y != m_mousePosition.y)
        {
            OnMouseMove(mousePosition.x, mousePosition.y);
        }

        if (mouseScrollAmount != 0.0f)
        {
            OnMouseWheel(mouseScrollAmount);
        }

        if (!m_mouseWasDown && mouseIsDown)
        {
            OnMouseDown();
        }
        else if (m_mouseWasDown && !mouseIsDown)
        {
            OnMouseUp();
        }

        m_mouseWasDown = mouseIsDown;
    }

    public void OnDestroy()
    {
        m_mouseOverIWidget = null;
    }

	// Unity Mouse Event Handlers
	private void OnMouseUp()
	{
		m_mouseIsDragging = false;
			
		if (m_mouseOverIWidget != null)
        {
            {
                WidgetEvent.MouseUpEventParameters eventParameters =
                    new WidgetEvent.MouseUpEventParameters
                    {
                        worldX = m_mousePosition.x,
                        worldY = m_mousePosition.y,
                        localX = m_mousePosition.x - m_mouseOverIWidget.WorldX,
                        localY = m_mousePosition.y - m_mouseOverIWidget.WorldY
                    };

                if (LogEnabled)
                {
                    Debug.Log("[MouseUp] on Widget " + m_mouseOverIWidget.GetType().Name);
                }

                // Notify the widget that the mouse was released
                (m_mouseOverIWidget as IWidgetEventListener).OnWidgetEvent(
                    new WidgetEvent(WidgetEvent.eEventType.mouseUp, m_mouseOverIWidget, eventParameters));
            }

            // Notify the widget if we clicked on it
            if (m_mouseOverIWidget == m_mouseDownIWidget)
            {
                WidgetEvent.MouseClickEventParameters eventParameters =
                    new WidgetEvent.MouseClickEventParameters
                    {
                        worldX = m_mousePosition.x,
                        worldY = m_mousePosition.y,
                        localX = m_mousePosition.x - m_mouseOverIWidget.WorldX,
                        localY = m_mousePosition.y - m_mouseOverIWidget.WorldY
                    };

                if (LogEnabled)
                {
                    Debug.Log("[MouseClick] on Widget " + m_mouseOverIWidget.GetType().Name);
                }

                (m_mouseOverIWidget as IWidgetEventListener).OnWidgetEvent(
                    new WidgetEvent(WidgetEvent.eEventType.mouseClick, m_mouseOverIWidget, eventParameters));
            }
        }	
			
		m_mouseDownIWidget = null;
	}		
		
	private void OnMouseDown()
	{
		m_mouseIsDragging = true;
			
		// Notify the widget of a drag event
		if (m_mouseOverIWidget != null)
		{				
			WidgetEvent.MouseDownEventParameters eventParameters =
                new WidgetEvent.MouseDownEventParameters { 
				    worldX= m_mousePosition.x, 
                    worldY= m_mousePosition.y, 
				    localX= m_mousePosition.x - m_mouseOverIWidget.WorldX, 
                    localY= m_mousePosition.y - m_mouseOverIWidget.WorldY };

            if (LogEnabled)
            {
                Debug.Log("[MouseDown] on Widget " + m_mouseOverIWidget.GetType().Name);
            }

			(m_mouseOverIWidget as IWidgetEventListener).OnWidgetEvent(
				new WidgetEvent(WidgetEvent.eEventType.mouseDown, m_mouseOverIWidget, eventParameters));
					
			m_mouseDownIWidget = m_mouseOverIWidget;
		}			
	}
		
	private void OnMouseMove(float newMouseX, float newMouseY)
	{
		float mouseDeltaX = newMouseX - m_mousePosition.x;
		float mouseDeltaY = newMouseY - m_mousePosition.y;

        //Debug.Log("Mouse: " + newMouseX.ToString() + ", " + newMouseY.ToString());
			
		m_mousePosition.Set(newMouseX, newMouseY);
			
		// Notify the widget of a drag event
		if (m_mouseOverIWidget != null)
		{	
			if (m_mouseIsDragging)
			{
                if (LogEnabled)
                {
                    Debug.Log("[MouseDrag] on Widget " + m_mouseOverIWidget.GetType().Name);
                }

				(m_mouseOverIWidget as IWidgetEventListener).OnWidgetEvent(
					new WidgetEvent(
                        WidgetEvent.eEventType.mouseDrag, 
                        m_mouseOverIWidget, 
                        new WidgetEvent.MouseDragEventParameters { deltaX= mouseDeltaX, deltaY= mouseDeltaY } ));
			}
			else
			{
                if (LogEnabled)
                {
                    Debug.Log("[MouseMove] on Widget " + m_mouseOverIWidget.GetType().Name);
                }

                (m_mouseOverIWidget as IWidgetEventListener).OnWidgetEvent(
					new WidgetEvent(
                        WidgetEvent.eEventType.mouseMove, 
                        m_mouseOverIWidget, 
                        new WidgetEvent.MouseMoveEventParameters { deltaX= mouseDeltaX, deltaY= mouseDeltaY } ));					
			}
		}

        {
            IWidget newMouseOverWidget = m_rootWidgetGroup.FindChildIWidgetContainingPoint(m_mousePosition);
			
		    // See if we have moved over a new widget
		    if (newMouseOverWidget != m_mouseOverIWidget)
		    {
			    // Notify the old widget the cursor left
			    if (m_mouseOverIWidget != null)
			    {
                    if (LogEnabled)
                    {
                        Debug.Log("[MouseOut] on Widget " + m_mouseOverIWidget.GetType().Name);
                    }

				    (m_mouseOverIWidget as IWidgetEventListener).OnWidgetEvent(
					    new WidgetEvent(
                            WidgetEvent.eEventType.mouseOut, 
                            m_mouseOverIWidget, 
							null));
			    }
					
			    // Notify the new mouse over widget we entered
			    if (newMouseOverWidget != null)
			    {
                    if (LogEnabled)
                    {
                        Debug.Log("[MouseOver] on Widget " + newMouseOverWidget.GetType().Name);
                    }

				    (newMouseOverWidget as IWidgetEventListener).OnWidgetEvent(
					    new WidgetEvent(
                            WidgetEvent.eEventType.mouseOver, 
                            newMouseOverWidget, 
							null));
			    }
				
			    // Remember the new mouse over widget
			    m_mouseOverIWidget = newMouseOverWidget;
		    }
        }
	}
		
	private void OnMouseWheel(float scrollDelta)
	{
		if (m_mouseOverIWidget != null)
		{
            if (LogEnabled)
            {
                Debug.Log("[MouseWheel] on Widget " + m_mouseOverIWidget.GetType().Name);
            }

			(m_mouseOverIWidget as IWidgetEventListener).OnWidgetEvent(
				new WidgetEvent(
                    WidgetEvent.eEventType.mouseWheel, 
                    m_mouseOverIWidget, 
                    new WidgetEvent.MousWheelEventParameters {delta= scrollDelta} ));
		}
	}

    public static Point2d GetMousePosition()
    {
        Vector3 unityMousePosition = Input.mousePosition;
        Point2d gameMousePosition = new Point2d(unityMousePosition.x, Screen.height - unityMousePosition.y);

        return gameMousePosition;
    }
}
