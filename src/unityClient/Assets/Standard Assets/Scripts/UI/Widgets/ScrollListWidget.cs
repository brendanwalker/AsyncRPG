using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ScrollListStyle
{
    public float Width;
    public float Height;
    public Texture Background;
    public ButtonStyle ButtonStyle;
}

public class ScrollListWidget : WidgetGroup
{
    public delegate IWidget WidgetFactory(ScrollListWidget scrollListWidget, object dataSourceEntry);

	private const float BORDER_WIDTH = 6;
	private const float ENTRY_SPACING = 5;
		
	private WidgetFactory m_widgetFactory;
		
	private int m_scrollIndex;
	private ImageWidget m_scrollFrame;
	private ButtonWidget m_scrollUpButton;
	private ButtonWidget m_scrollDownButton;
	private List<IWidget> m_widgetList;

    public ScrollListWidget(
        WidgetGroup parentGroup, 
        WidgetFactory widgetFactory,
        ScrollListStyle style,
        float x, 
        float y)
        : base(parentGroup, style.Width, style.Height, x, y)
    {
		m_widgetFactory = widgetFactory;
		m_widgetList = new List<IWidget>();
		m_scrollIndex = 0;

        m_scrollFrame = new ImageWidget(this, style.Width, style.Height, style.Background, 0.0f, 0.0f);
			
		m_scrollUpButton = 
            new ButtonWidget(this, style.ButtonStyle, 0, 0, "Previous");
		m_scrollUpButton.SetLocalPosition(
            m_scrollFrame.Width / 2 - m_scrollUpButton.Width / 2, 
            BORDER_WIDTH);
		m_scrollUpButton.Visible = false;

		m_scrollDownButton =
            new ButtonWidget(this, style.ButtonStyle, 0, 0, "Next");
		m_scrollDownButton.SetLocalPosition(
            m_scrollUpButton.LocalX, 
            m_scrollFrame.Height - m_scrollDownButton.Height - BORDER_WIDTH);
		m_scrollDownButton.Visible = false;
    }

	// IWidgetEventListener
	public override void OnWidgetEvent(WidgetEvent widgetEvent)
	{
		bool handled = false;
			
		if (widgetEvent.EventType == WidgetEvent.eEventType.buttonClick)
		{
			if (widgetEvent.EventSource == m_scrollUpButton)
			{
				OnScrollUpClicked();
				handled = true;
			}
			else if (widgetEvent.EventSource == m_scrollDownButton)
			{
				OnScrollDownClicked();
				handled = true;
			}
		}

		if (!handled) 
		{
            base.OnWidgetEvent(widgetEvent);
		}
	}

	// WidgetScrollList	
	public int Length
	{
		get { return m_widgetList.Count; }
	}
		
	public float ScrollFrameWidth
	{
		get { return m_scrollFrame.Width - BORDER_WIDTH * 2; }
	}
		
	public void SetListData(List<object> dataSourceList)
	{			
		foreach (IWidget widget in m_widgetList)
		{
			base.RemoveWidget(widget);
		}
			
		m_widgetList = new List<IWidget>();
			
		foreach (object dataSourceEntry in dataSourceList)
		{	
			IWidget widget = m_widgetFactory(this, dataSourceEntry);					
			
			m_widgetList.Add(widget);				
		}
			
		if (dataSourceList.Count > 0)
		{
			SelectEntry(0);
		}
		else 
		{
			UpdateScrollButtons();
		}			
	}
	
		
	// Private Functions	
	private void SelectEntry(int listIndex)
	{
		m_scrollIndex = listIndex;
			
		foreach (IWidget listEntry in m_widgetList)
		{
			listEntry.Visible= false;
		}
			
		float yOffset = m_scrollUpButton.LocalY + m_scrollUpButton.Height + ENTRY_SPACING;
		for (int index = listIndex; index < m_widgetList.Count; index++)
		{
			IWidget listEntry = m_widgetList[index];		
		
			listEntry.SetLocalPosition(m_scrollFrame.LocalX + BORDER_WIDTH, yOffset);					
			listEntry.Visible= (listEntry.LocalY + listEntry.Height < m_scrollDownButton.LocalY);
			yOffset += listEntry.Height + ENTRY_SPACING;			
		}
			
		UpdateScrollButtons();
			
		base.OnWidgetEvent(
            new WidgetEvent(
                WidgetEvent.eEventType.listSelectionChanged, 
                this, 
                new WidgetEvent.ListSelectionChangedEventParameters {ListIndex= m_scrollIndex}));
	}
		
	private void UpdateScrollButtons()
	{
		m_scrollDownButton.Visible = false;
		m_scrollUpButton.Visible = false;

		if (m_widgetList.Count > 1)
		{
			if (m_scrollIndex > 0)
			{
				m_scrollUpButton.Visible = true;
			}
				
			if (m_scrollIndex + 1 < m_widgetList.Count)
			{
				m_scrollDownButton.Visible = true;
			}				
		}
	}
		
	private void OnScrollUpClicked()
	{
		if (m_scrollIndex > 0)
		{
			SelectEntry(m_scrollIndex - 1);
            //TODO Play scroll sound
		}
		else 
		{
            //TODO Play bump sound
			//FlxG.play(ResourceManager.getInstance().getSoundClass("bump"));
		}
	}
		
	private void OnScrollDownClicked()
	{
		if (m_scrollIndex + 1 < m_widgetList.Count)
		{
			SelectEntry(m_scrollIndex + 1);
            //TODO Play scroll sound
		}
		else 
		{
            //TODO Play bump sound
		}
	}
}
