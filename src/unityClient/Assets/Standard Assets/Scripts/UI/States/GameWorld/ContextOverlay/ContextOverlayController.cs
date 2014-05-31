using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

public class ContextOverlayController
{
    private GameWorldController m_gameWorldController;
    private ContextOverlayModel m_contextOverlayModel;
    private ContextOverlayView m_contextOverlayView;

    public ContextOverlayModel Model 
    { 
        get { return m_contextOverlayModel; }
    }

    public ContextOverlayView View 
    { 
        get { return m_contextOverlayView; }
    }

    public GameWorldController ParentController
    {
        get { return m_gameWorldController; }
    }

    public ContextOverlayController(GameWorldController gameWorldController)
    {
		m_gameWorldController = gameWorldController;
		m_contextOverlayModel= new ContextOverlayModel(this);
		m_contextOverlayView = new ContextOverlayView(this);
    }

	// Use this for initialization
    public void Start(WidgetGroup parentWidgetGroup) 
    {
        m_contextOverlayView.Start(parentWidgetGroup);
        m_contextOverlayModel.Start();
	}
	
    public void OnDestroy()
    {
        // $TODO Free any pending queries on the model
        m_contextOverlayView.OnDestroy();
    }

    // Update is called once per frame
    public void Update()
    {
        m_contextOverlayView.Update();
        m_contextOverlayModel.Update();
    }

    // Update is called once per frame
    public void OnGUI()
    {
        m_contextOverlayView.OnGUI();
    }

	// Game World Controller Events
	public void OnRoomLoaded(RoomData room)
	{
		m_contextOverlayView.CreateHotspots(room);
	}
		
	// Model Events
		
	// View Events
	public void OnPortalClicked(Point2d entryPixelPoint, RoomPortal portal)
	{
		Point3d entryWorldPoint = GameConstants.ConvertPixelPositionToRoomPosition(entryPixelPoint);
			
		m_gameWorldController.OnPortalClicked(entryWorldPoint, portal);
	}
		
	public void OnEnergyTankClicked(Point2d entryPixelPoint, EnergyTankData energyTankData)
	{
		Point3d entryWorldPoint = GameConstants.ConvertPixelPositionToRoomPosition(entryPixelPoint);
			
		m_gameWorldController.OnEnergyTankClicked(entryWorldPoint, energyTankData);		
	}
}
