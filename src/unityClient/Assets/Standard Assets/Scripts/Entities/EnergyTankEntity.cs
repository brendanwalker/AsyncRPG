using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class EnergyTankEntity : IEntity
{
    private Point3d _position;		
    private int _energyTankId;
    private EnergyTankData _energyTankData;		
    private EnergyTankWidget _energyTankWidget;
		
    public EnergyTankEntity(int energyTankId) 
    {
        SessionData sessionData = SessionData.GetInstance();
					
        _energyTankId = energyTankId;
        _energyTankData = sessionData.CurrentGameData.CurrentRoom.GetEnergyTankById(_energyTankId);			
        _position = new Point3d(_energyTankData.position);			
        _energyTankWidget = null;
    }

    // Accessors
    public EnergyTankWidget MyEnergyTankWidget
    {
        get { return _energyTankWidget; }
    }
		
    public EnergyTankData MyEnergyTankData
    {			
        get { return _energyTankData; }
    }
		
    public bool IsActive
    {
        get { return _energyTankWidget != null; }
    }
		
    public uint Energy 
    {
        get { return _energyTankData.energy; }
        
        set 
        {
            _energyTankData.energy = value;
            _energyTankWidget.Energy = value;
        }
    }

    public GameConstants.eFaction Ownership
    {
        get { return _energyTankData.ownership; }
        
        set 
        {  
            _energyTankData.ownership = value;
            _energyTankWidget.Ownership = value;
        }
    }
		
    // IEntity
    public PathfindingComponent PathfindingInterface
    {
        get { return null; }
    }
		
    public SteeringComponent SteeringInterface
    {
        get { return null; }
    }
		
    public Point3d Position
    {
        get { return _position; }
    }
		
    public Vector2d Facing
    {
        get { return Vector2d.WORLD_DOWN; }
    }
		
    public RoomKey CurrentRoomKey
    {
        get { return MyEnergyTankData.room_key; }
    }
		
    public void AddToGameWorld(GameWorldController gameWorldController)
    {
        _energyTankWidget = gameWorldController.View.AddEnergyTankWidget(_energyTankData);		
        gameWorldController.Model.AddEnergyTankEntity(this);
    }
		
    public void RemoveFromGameWorld(GameWorldController gameWorldController)
    {
        gameWorldController.Model.RemoveEnergyTankEntity(this);
        gameWorldController.View.RemoveEnergyTankWidget(_energyTankWidget);
			
        _energyTankWidget = null;
    }
		
    public void RequestMoveTo(Point3d point, float angle, PathComputer.OnPathComputedCallback onPathComputed)
    {
        SnapTo(point, angle);
        onPathComputed(PathComputer.eResult.success);
    }
		
    public void SnapTo(Point3d point, float angle)
    {
        Point2d pixelPosition = GameConstants.ConvertRoomPositionToPixelPosition(point);
			
        // Update the character data
        _energyTankData.position.Set(point);
			
        // Snap our position
        _position.Set(point);
        _energyTankWidget.SetLocalPosition(pixelPosition.x, pixelPosition.y);
    }
		
    public void Update()
    {
        // Do nothing.
    }
}