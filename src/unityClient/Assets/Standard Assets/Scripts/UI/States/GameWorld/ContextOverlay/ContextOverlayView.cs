using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

[System.Serializable]
public class ContextOverlayStyle
{
    public Texture2D DefaultCursorTexture;
    public Vector2 DefaultCursorHotspot;

    public Texture2D WalkCursorTexture;
    public Vector2 WalkCursorHotspot;

    public Texture2D DoorLeftCursorTexture;
    public Vector2 DoorLeftCursorHotspot;

    public Texture2D DoorRightCursorTexture;
    public Vector2 DoorRightCursorHotspot;

    public Texture2D DoorUpCursorTexture;
    public Vector2 DoorUpCursorHotspot;

    public Texture2D DoorDownCursorTexture;
    public Vector2 DoorDownCursorHotspot;

    public Texture2D HackEnergyTankCursorTexture;
    public Vector2 HackEnergyTankCursorHotspot;

    public Texture2D DrainEnergyTankCursorTexture;
    public Vector2 DrainEnergyTankCursorHotspot;
}

public class ContextOverlayView : IWidgetEventListener
{	
	private const float DEFAULT_CURSOR_SCALE = 1;
	private const float LARGE_CURSOR_SCALE = 2;
    
    private ContextOverlayStyle m_style;

	public enum eMouseCursorState
    {
        defaultCursor,
        walk,
        door_left,
        door_right,
        door_up,
        door_down,
        hack_energy_tank,
        drain_energy_tank        
    }

    public enum eHotspotType
    {
        portal,
        energy_tank
    }

    public class HotspotInfo
    {
        public eHotspotType hotspotType;
        public object hotspotEntity;
    }

    public HotspotWidget CurrentHotspot { get; private set; }
				
	private ContextOverlayController m_contextOverlayController;	
	private WidgetGroup m_rootWidgetGroup;
	private eMouseCursorState m_mouseCursorState;
	private List<HotspotWidget> m_hotspotWidgets;
	private NavRef m_currentNavRef;

    public NavRef CurrentNavRef
    {
        get { return m_currentNavRef; }
    }
		
	public ContextOverlayView(ContextOverlayController contextOverlayController) 
	{
		m_contextOverlayController = contextOverlayController;
		m_hotspotWidgets = new List<HotspotWidget>();
		m_currentNavRef = new NavRef(-1, null);
        CurrentHotspot = null;
        m_style= contextOverlayController.ParentController.contextOverlayStyle;
	}
		
	public void Start(WidgetGroup parentGroup)
	{
		m_rootWidgetGroup = new WidgetGroup(parentGroup, Screen.width, Screen.height, 0.0f, 0.0f);
		m_rootWidgetGroup.SetWidgetEventListener(this);
			
		m_mouseCursorState = eMouseCursorState.defaultCursor;
	}
		
	public void OnDestroy()
	{
		SetMouseCursorState(eMouseCursorState.defaultCursor);
		m_rootWidgetGroup.OnDestroy();
	}
		
	public void Update()
	{			
	}

    public void OnGUI()
    {
        m_rootWidgetGroup.OnGUI();
    }
	
	private Texture2D GetTextureForCursorState()
    {
        Texture2D cursorTexture= null;

        switch(m_mouseCursorState)
        {
        case eMouseCursorState.defaultCursor:
            cursorTexture= m_style.DefaultCursorTexture;
            break;
        case eMouseCursorState.walk:
            cursorTexture= m_style.WalkCursorTexture;
            break;
        case eMouseCursorState.door_left:
            cursorTexture= m_style.DoorLeftCursorTexture;
            break;
        case eMouseCursorState.door_right:
            cursorTexture= m_style.DoorRightCursorTexture;
            break;
        case eMouseCursorState.door_up:
            cursorTexture= m_style.DoorUpCursorTexture;
            break;
        case eMouseCursorState.door_down:
            cursorTexture= m_style.DoorDownCursorTexture;
            break;
        case eMouseCursorState.hack_energy_tank:
            cursorTexture= m_style.HackEnergyTankCursorTexture;
            break;
        case eMouseCursorState.drain_energy_tank: 
            cursorTexture= m_style.DrainEnergyTankCursorTexture;
            break;
        }

        return cursorTexture;
    }

    private Vector2 GetHotspotForCursorState()
    {
        Vector2 hotspot= Vector2.zero;

        switch(m_mouseCursorState)
        {
        case eMouseCursorState.defaultCursor:
            hotspot= m_style.DefaultCursorHotspot;
            break;
        case eMouseCursorState.walk:
            hotspot= m_style.WalkCursorHotspot;
            break;
        case eMouseCursorState.door_left:
            hotspot= m_style.DoorLeftCursorHotspot;
            break;
        case eMouseCursorState.door_right:
            hotspot= m_style.DoorRightCursorHotspot;
            break;
        case eMouseCursorState.door_up:
            hotspot= m_style.DoorUpCursorHotspot;
            break;
        case eMouseCursorState.door_down:
            hotspot= m_style.DoorDownCursorHotspot;
            break;
        case eMouseCursorState.hack_energy_tank:
            hotspot= m_style.HackEnergyTankCursorHotspot;
            break;
        case eMouseCursorState.drain_energy_tank: 
            hotspot= m_style.DrainEnergyTankCursorHotspot;
            break;
        }

        return hotspot;
    }

	private void SetMouseCursorState(eMouseCursorState mouseCursorState)
	{
		if (m_mouseCursorState != mouseCursorState)
		{
            m_mouseCursorState = mouseCursorState;	
            Cursor.SetCursor(GetTextureForCursorState(), GetHotspotForCursorState(), CursorMode.Auto);											
		}
	}
		
	public void CreateHotspots(RoomData room)
	{
		ClearHotspots();
		CreatePortalHotspots(room);
		CreateEnergyTankHotspots(room);
	}
		
	private void ClearHotspots()
	{
		foreach (HotspotWidget hotspot in m_hotspotWidgets)
		{
			m_rootWidgetGroup.RemoveWidget(hotspot);
		}

		m_hotspotWidgets = new List<HotspotWidget>();			
	}
		
	private void CreatePortalHotspots(RoomData room)
	{
		foreach (RoomPortal portalEntry in room.RoomPortals)
		{
			AABB2d boundingBox = new AABB2d();
			Point2d screenP0 = ClientGameConstants.ConvertRoomPositionToScreenPosition(portalEntry.boundingBox.Min);
			Point2d screenP1 = ClientGameConstants.ConvertRoomPositionToScreenPosition(portalEntry.boundingBox.Max);
				
			boundingBox.EnclosePoint(screenP0);
			boundingBox.EnclosePoint(screenP1);
				
			m_hotspotWidgets.Add(
                new HotspotWidget(m_rootWidgetGroup, 
				    "Portal" + portalEntry.portal_id.ToString(), 
				    boundingBox.Extents.i, 
				    boundingBox.Extents.j, 
				    boundingBox.Min.x, 
				    boundingBox.Min.y,
				    new HotspotInfo {hotspotType=eHotspotType.portal, hotspotEntity=portalEntry}));
		}			
	}
		
	private void CreateEnergyTankHotspots(RoomData room)
	{
		foreach (EnergyTankData energyTankData in room.EnergyTankMap.Values)
		{
			AABB2d boundingBox = new AABB2d();
            Point2d screenP0 = ClientGameConstants.ConvertRoomPositionToScreenPosition(energyTankData.boundingBox.Min);
            Point2d screenP1 = ClientGameConstants.ConvertRoomPositionToScreenPosition(energyTankData.boundingBox.Max);
				
			boundingBox.EnclosePoint(screenP0);
			boundingBox.EnclosePoint(screenP1);
				
			m_hotspotWidgets.Add(
                new HotspotWidget(m_rootWidgetGroup, 
				    "EnergyTank" + energyTankData.energy_tank_id.ToString(), 
				    boundingBox.Extents.i, 
				    boundingBox.Extents.j, 
				    boundingBox.Min.x, 
				    boundingBox.Min.y,
				    new HotspotInfo {hotspotType=eHotspotType.energy_tank, hotspotEntity=energyTankData}));
		}			
	}
			
	// Events		
	public void OnWidgetEvent(WidgetEvent widgetEvent)
	{
		if (widgetEvent.EventType == WidgetEvent.eEventType.mouseMove)
		{
			UpdateCurrentNavRef();
		}

        if (widgetEvent.EventSource is HotspotWidget)
        {
            HotspotWidget hotspotWidget = widgetEvent.EventSource as HotspotWidget;

            switch (widgetEvent.EventType)
            {
                case WidgetEvent.eEventType.mouseOver:
                    OnHotspotEntered(hotspotWidget);
                    break;
                case WidgetEvent.eEventType.mouseOut:
                    OnHotspotExited();
                    break;
                case WidgetEvent.eEventType.mouseClick:
                    OnHotspotClicked(hotspotWidget, widgetEvent);
                    break;
            }
        }

        UpdateMouseCursor(widgetEvent);
	}
		
	private void OnHotspotEntered(HotspotWidget hotspotWidget)
	{
        CurrentHotspot = hotspotWidget;
	}
		
	private void OnHotspotExited()
	{
        CurrentHotspot = null;
	}

    private void OnHotspotClicked(
        HotspotWidget hotspotWidget,
        WidgetEvent widgetEvent)
    {
        if (m_currentNavRef.IsValid)
        {
            WidgetEvent.MouseClickEventParameters eventParameters =
                widgetEvent.EventParameters as WidgetEvent.MouseClickEventParameters;
            Point2d clickPoint = new Point2d(eventParameters.worldX, eventParameters.worldY);
            HotspotInfo hotspotInfo = hotspotWidget.Userdata as HotspotInfo;

            switch (hotspotInfo.hotspotType)
            {
                case eHotspotType.energy_tank:
                    {
                        EnergyTankData energyTankData = hotspotInfo.hotspotEntity as EnergyTankData;

                        m_contextOverlayController.OnEnergyTankClicked(clickPoint, energyTankData);
                    }
                    break;
                case eHotspotType.portal:
                    {
                        RoomPortal portal = hotspotInfo.hotspotEntity as RoomPortal;

                        m_contextOverlayController.OnPortalClicked(clickPoint, portal);
                    }
                    break;
            }
        }
    }
				
	private void UpdateCurrentNavRef()
	{
        SessionData sessionData = SessionData.GetInstance();
        GameData gameData = sessionData.CurrentGameData;
        RoomData roomData = gameData.GetCachedRoomData(gameData.CurrentRoomKey);

        if (roomData != null)
        {
            AsyncRPGSharedLib.Navigation.NavMesh navMesh = roomData.StaticRoomData.NavMesh;

            Point2d pixelPoint = WidgetEventDispatcher.GetMousePosition();
            Point3d roomPoint = GameConstants.ConvertPixelPositionToRoomPosition(pixelPoint);

            m_currentNavRef = navMesh.ComputeNavRefAtPoint(roomPoint);
        }
	}

    private void UpdateMouseCursor(WidgetEvent widgetEvent)
	{
        if (widgetEvent.EventType == WidgetEvent.eEventType.mouseOver ||
            widgetEvent.EventType == WidgetEvent.eEventType.mouseOut ||
            widgetEvent.EventType == WidgetEvent.eEventType.mouseMove)
        {
            if (widgetEvent.EventSource is HotspotWidget)
            {
                HotspotWidget hotspotWidget = widgetEvent.EventSource as HotspotWidget;
                HotspotInfo hotspotInfo = hotspotWidget.Userdata as HotspotInfo;

                switch (hotspotInfo.hotspotType)
                {
                    case eHotspotType.energy_tank:
                        {
                            EnergyTankData energyTankData = hotspotInfo.hotspotEntity as EnergyTankData;

                            if (energyTankData.ownership != GameConstants.eFaction.player)
                            {
                                // If our faction doesn't control the energy tank, we'll have to hack it
                                SetMouseCursorState(eMouseCursorState.hack_energy_tank);
                            }
                            else if (energyTankData.energy > 0)
                            {
                                // If our faction does control the energy tank, then we can drain it if it has energy
                                SetMouseCursorState(eMouseCursorState.drain_energy_tank);
                            }
                            else
                            {
                                // Otherwise all we can do is walk to it
                                SetMouseCursorState(eMouseCursorState.walk);
                            }
                        }
                        break;
                    case eHotspotType.portal:
                        {
                            Point2d hotspotCenter =
                                hotspotWidget.WorldPosition.Offset(new Vector2d(hotspotWidget.Width / 2.0f, hotspotWidget.Height / 2.0f));
                            Point2d screenCenter = new Point2d(Screen.width / 2.0f, Screen.height / 2.0f);
                            Vector2d vectorToHotspot = hotspotCenter - screenCenter;

                            MathConstants.eDirection currentHotspotDirection = MathConstants.GetDirectionForVector(vectorToHotspot);

                            switch (currentHotspotDirection)
                            {
                                case MathConstants.eDirection.left:
                                    SetMouseCursorState(eMouseCursorState.door_left);
                                    break;
                                case MathConstants.eDirection.right:
                                    SetMouseCursorState(eMouseCursorState.door_right);
                                    break;
                                case MathConstants.eDirection.up:
                                    SetMouseCursorState(eMouseCursorState.door_up);
                                    break;
                                case MathConstants.eDirection.down:
                                    SetMouseCursorState(eMouseCursorState.door_down);
                                    break;
                                default:
                                    SetMouseCursorState(eMouseCursorState.walk);
                                    break;
                            }
                        }
                        break;
                    default:
                        {
                            SetMouseCursorState(eMouseCursorState.defaultCursor);
                        }
                        break;
                }
            }
            else if (widgetEvent.EventSource is TileGridWidget)
            {
                if (m_currentNavRef.IsValid)
                {
                    SetMouseCursorState(eMouseCursorState.walk);
                }
                else
                {
                    SetMouseCursorState(eMouseCursorState.defaultCursor);
                }

            }
            else
            {
                SetMouseCursorState(eMouseCursorState.defaultCursor);
            }
        }
	}
}
