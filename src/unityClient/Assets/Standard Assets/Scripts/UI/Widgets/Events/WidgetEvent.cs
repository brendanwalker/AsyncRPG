using UnityEngine;
using System.Collections;

public class WidgetEvent
{
    public enum eEventType
    {
        // General UI Events
        buttonClick,
        checkboxToggle,
        listSelectionChanged,
        textInputReturn,        // Return/Enter hit in a text box
        textInputTab,           // Tab hit in a text entry box

        // Mouse Events
        mouseClick,
        mouseDrag,
        mouseMove,
        mouseUp,
        mouseDown,
        mouseOver,
        mouseOut,
        mouseWheel
    }

    public class MouseUpEventParameters
    {
        public float worldX { get; set; }
        public float worldY { get; set; }
        public float localX { get; set; }
        public float localY { get; set; }
    }

    public class MouseDownEventParameters
    {
        public float worldX { get; set; }
        public float worldY { get; set; }
        public float localX { get; set; }
        public float localY { get; set; }
    }

    public class MouseClickEventParameters
    {
        public float worldX { get; set; }
        public float worldY { get; set; }
        public float localX { get; set; }
        public float localY { get; set; }
    }

    public class MouseDragEventParameters
    {
        public float deltaX { get; set; }
        public float deltaY { get; set; }
    }

    public class MouseMoveEventParameters
    {
        public float deltaX { get; set; }
        public float deltaY { get; set; }
    }

    public class MousWheelEventParameters
    {
        public float delta { get; set; }
    }

    public class ListSelectionChangedEventParameters
    {
        public int ListIndex { get; set; }
    }

    public IWidget EventSource { get; private set; }
    public eEventType EventType { get; private set; }
    public object EventParameters { get; private set; }

    public WidgetEvent(eEventType eventType, IWidget eventSource, object eventParameters)
    {
        EventType = eventType;
        EventSource = eventSource;
        EventParameters = eventParameters;
    }
}