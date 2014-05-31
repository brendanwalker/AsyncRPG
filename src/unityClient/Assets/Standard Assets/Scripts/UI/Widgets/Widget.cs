using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Environment;
using System;

public class WidgetLayoutCursor
{
	public float X { get; private set; }
	public float Y { get; private set; }
	public float Kerning { get; set; }
	public float Leading { get; set; }
	public float LineHeight { get; private set; }

	private float m_initialX;

	public WidgetLayoutCursor(float x, float y)
	{
		this.X= x;
		this.Y= y;
		this.Kerning= 5;
		this.Leading= 5;
		this.LineHeight= 0;

		this.m_initialX= x;
	}

	public T Advance<T>(T widget) where T : Widget
	{
		this.X+= widget.Width + this.Kerning;
		this.LineHeight= Math.Max(this.LineHeight, widget.Height);

		return widget;
	}

	public void NewLine()
	{
		this.X= this.m_initialX;
		this.Y+= this.LineHeight + this.Leading;
		this.LineHeight= 0;
	}
}

public class Widget : IWidget, IWidgetEventListener
{
    protected WidgetGroup m_parentGroup;
    protected Point2d m_localPosition;
    protected Point2d m_worldPosition;

    public Widget(WidgetGroup parentGroup, float width, float height, float x, float y)
    {
        m_localPosition = new Point2d();
        m_worldPosition = new Point2d();
        Width = width;
        Height = height;
        Visible = true;

        m_parentGroup = parentGroup;
        if (m_parentGroup != null)
        {
            m_parentGroup.AddWidget(this);
        }

        SetLocalPosition(x, y);
    }

    public WidgetGroup ParentGroup
    {
        get { return m_parentGroup; }
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
        bool containsPoint = (point.x > minX && point.x < maxX && point.y > minY && point.y < maxY);

        return containsPoint;
    }

    public virtual void UpdateWorldPosition()
    {
        if (m_parentGroup != null)
        {
            m_worldPosition = m_parentGroup.WorldPosition.Offset(m_localPosition.x, m_localPosition.y);
        }
        else
        {
            m_worldPosition.Set(m_localPosition);
        }
    }

    public virtual void OnGUI()
    {
    }

    public virtual void OnDestroy()
    {
        m_parentGroup = null;
    }

    // IWidgetEventListener
    public virtual void OnWidgetEvent(WidgetEvent widgetEvent)
    {
        if (m_parentGroup != null)
        {
            m_parentGroup.OnWidgetEvent(widgetEvent);
        }
    }
}
