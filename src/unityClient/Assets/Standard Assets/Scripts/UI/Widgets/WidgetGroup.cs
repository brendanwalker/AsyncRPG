using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Environment;

public class WidgetGroup : IWidget, IWidgetEventListener
{
    private IWidgetEventListener m_widgetEventListener;
    private WidgetGroup m_parentWidgetGroup;
    private List<IWidget> m_childWidgets;
    private Point2d m_localPosition;
    private Point2d m_worldPosition;

    public WidgetGroup(WidgetGroup parentGroup, float width, float height, float x, float y)
    {
        m_widgetEventListener = null;
        m_childWidgets = new List<IWidget>() { };
        m_parentWidgetGroup = parentGroup;
        m_localPosition = new Point2d();
        m_worldPosition = new Point2d();
        Width = width;
        Height = height;
        Visible = true;

        if (m_parentWidgetGroup != null)
        {
            m_parentWidgetGroup.AddWidget(this);
        }

        SetLocalPosition(x, y);
    }

    public WidgetGroup ParentWidgetGroup
    {
        get { return m_parentWidgetGroup; }
    }

	public int ChildWidgetCount
	{
		get { return m_childWidgets.Count; }
	}
		
	public IWidget GetChildWidget(int index)
	{
		return m_childWidgets[index];
	}

    public void AddWidget(IWidget widget)
    {
        m_childWidgets.Add(widget);
    }

    public bool RemoveWidget(IWidget widget)
    {
        return m_childWidgets.Remove(widget);
    }

    public IWidget FindChildIWidgetContainingPoint(Point2d point)
    {
        IWidget containingWidget = null;
        bool hasDegenerateSize = (this.Width <= 0.0f || this.Height <= 0.0f);

        // Only consider this group if this group contains the point or if it's zero-sized
        // AND if the group actually has children in it
        if (m_childWidgets.Count > 0 &&
            (this.ContainsWorldPositionPoint(point) || hasDegenerateSize))
        {
            // We go through the child list backwards because the last child is on top
            for (int childWidgetIndex = m_childWidgets.Count - 1; childWidgetIndex >= 0; childWidgetIndex--)
            {
                IWidget childWidget = m_childWidgets[childWidgetIndex];

                if (childWidget.Visible)
                {
                    if (childWidget is WidgetGroup)
                    {
                        containingWidget = ((WidgetGroup)childWidget).FindChildIWidgetContainingPoint(point);
                    }
                    else if (childWidget.ContainsWorldPositionPoint(point))
                    {
                        containingWidget = childWidget;
                    }
                }

                if (containingWidget != null)
                {
                    break;
                }
            }

            // If no children contain the point, and this group is a window widget
            // then say that this group contains the point
            if (containingWidget == null && this is WindowWidget)
            {
                containingWidget = this;
            }
        }

        return containingWidget;
    }

	// IWidgetEventListener
	public void SetWidgetEventListener(IWidgetEventListener listener)
	{
		m_widgetEventListener = listener;
	}		
		
	public virtual void OnWidgetEvent(WidgetEvent widgetEvent)
	{
		if (m_widgetEventListener != null)
		{
			m_widgetEventListener.OnWidgetEvent(widgetEvent);
		}
		else if (m_parentWidgetGroup != null)
		{
			m_parentWidgetGroup.OnWidgetEvent(widgetEvent);
		}
	}
		
	// IWidget	
	public bool Visible { get; set; }		

	public float Width { get; set; }	

	public float Height { get; set; }
		
	public Point2d LocalPosition
	{
		get { return new Point2d(m_localPosition); }
	}
				
	public float LocalX
	{
		get { return m_localPosition.x; }
	}
		
	public float LocalY
	{
		get { return m_localPosition.y; }
	}	
		
	public void SetLocalPosition(float x, float y)
	{
		m_localPosition = new Point2d(x, y);			
		UpdateWorldPosition();
	}	
		
	public Point2d WorldPosition
	{
		get { return new Point2d(m_worldPosition); }
	}		
		
	public float WorldX
	{
		get { return m_worldPosition.x; }
	}
		
	public float WorldY
	{
		get { return m_worldPosition.y; }
	}

    public bool ContainsWorldPositionPoint(Point2d point)
    {
        float minX = this.WorldX;
        float minY = this.WorldY;
        float maxX = minX + this.Width;
        float maxY = minY + this.Height;
        bool containsPoint= (point.x > minX && point.x < maxX && point.y > minY && point.y < maxY);

        return containsPoint;
    }

    public virtual void UpdateWorldPosition()
	{
		if (m_parentWidgetGroup != null)
		{
			m_worldPosition = m_parentWidgetGroup.WorldPosition.Offset(m_localPosition.x, m_localPosition.y);
		}
		else
		{
            m_worldPosition.Set(m_localPosition.x, m_localPosition.y);
		}
			
			
		for (int childIndex = 0; childIndex < m_childWidgets.Count; childIndex++ )
		{
			IWidget widget = m_childWidgets[childIndex];
								
			widget.UpdateWorldPosition();
		}			
	}						
		
    public virtual void OnGUI()
    {
        if (Visible)
        {
            foreach (IWidget widget in m_childWidgets)
            {
                widget.OnGUI();
            }
        }
    }

	public virtual void OnDestroy()
	{	
		for (int childIndex = 0; childIndex < m_childWidgets.Count; childIndex++ )
		{
			IWidget widget = m_childWidgets[childIndex];
															
			widget.OnDestroy();				
		}
			
		// Clear references to the parent and children
		m_parentWidgetGroup = null;
		m_childWidgets = new List<IWidget>();
	}		
}
