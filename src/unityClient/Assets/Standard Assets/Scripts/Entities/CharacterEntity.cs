using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class CharacterEntity : IEntity
{
    private Point3d m_position;
    private Vector2d m_facing;
		
    private int m_characterId;
    private CharacterData m_characterData;
		
    private CharacterWidget m_characterWidget;
    private PathfindingComponent m_pathfindingComponent;
    private SteeringComponent m_steeringComponent;

    public CharacterEntity(int characterId) 
    {
        SessionData sessionData = SessionData.GetInstance();
					
        m_characterId = characterId;
        m_characterData = sessionData.CurrentGameData.GetCharacterById(m_characterId);
			
        m_position = new Point3d(m_characterData.x, m_characterData.y, m_characterData.z);
        m_facing = MathConstants.GetUnitVectorForAngle(m_characterData.angle);
			
        m_characterWidget = null;
        m_pathfindingComponent = null;
        m_steeringComponent = null;
    }				

    // Accessors
    public CharacterWidget MyCharacterWidget
    {
        get { return m_characterWidget; }
    }
		
    public CharacterData MyCharacterData
    {			
        get { return m_characterData; }
    }
		
    public bool IsActive
    {
        get { return m_characterWidget != null; }
    }
		
    public int Energy
    {
        get { return m_characterData.energy; }
        
        set 
        {
            m_characterData.energy = value;
            m_characterWidget.Energy = value;
        }
    }

    // IEntity
    public PathfindingComponent PathfindingInterface
    {
        get { return m_pathfindingComponent; }
    }
		
    public SteeringComponent SteeringInterface
    {
        get { return m_steeringComponent; }
    }
		
    public Point3d Position
    {
        get { return m_position; }
    }
		
    public Vector2d Facing
    {
        get { return m_facing; }
    }
		
    public RoomKey CurrentRoomKey
    {
        get { return MyCharacterData.CurrentRoomKey; }
    }

    public void AddToGameWorld(GameWorldController gameWorldController)
    {
        m_pathfindingComponent = new PathfindingComponent(this);
        m_steeringComponent = new SteeringComponent(this);

        m_characterWidget = gameWorldController.View.AddCharacterWidget(m_characterData);		
        gameWorldController.Model.AddCharacterEntity(this);
    }
		
    public void RemoveFromGameWorld(GameWorldController gameWorldController)
    {
        gameWorldController.Model.RemoveCharacterEntity(this);
        gameWorldController.View.RemoveCharacterWidget(m_characterWidget);
			
        m_characterWidget = null;
        m_pathfindingComponent = null;
        m_steeringComponent = null;
    }

    public void RequestMoveTo(
        Point3d point, 
        float angle,
        PathComputer.OnPathComputedCallback onPathComputed)
    {	
        // Set our new destination.
        // The position will update as we move along our path 
        m_pathfindingComponent.SubmitPathRequest(
            point, 
            MathConstants.GetUnitVectorForAngle(angle),
            (PathComputer pathResult) =>
        {
            if (pathResult == null)
            {
                // No result means we stopped in out tracks
                m_characterData.x = Position.x;
                m_characterData.y = Position.y;
                m_characterData.z = Position.z;
                m_characterData.angle = angle;
					
                onPathComputed(PathComputer.eResult.success);
            }
            else 
            {
                if (pathResult.ResultCode == PathComputer.eResult.success)
                {
                    // Set the character data immediately to the destination
                    m_characterData.x = point.x;
                    m_characterData.y = point.y;
                    m_characterData.z = point.z;
                    m_characterData.angle = angle;
                }
					
                onPathComputed(pathResult.ResultCode);
            }
        });
    }

    public void SnapTo(Point3d point, float angle)
    {
        Point2d pixelPosition = GameConstants.ConvertRoomPositionToPixelPosition(point);
			
        // Update the character data
        m_characterData.x = point.x;
        m_characterData.y = point.y;
        m_characterData.z = point.z;
        m_characterData.angle = angle;
			
        // Snap our position
        m_position.Set(point);
        m_facing = MathConstants.GetUnitVectorForAngle(angle);
        m_characterWidget.SetLocalPosition(pixelPosition.x, pixelPosition.y);

        // Snap out facing and go to idle
        m_characterWidget.UpdateAnimation(m_facing, Vector2d.ZERO_VECTOR);
			
        // Forget about any path we were running
        m_pathfindingComponent.ClearPath();
    }

    public void Update()
    {
        if (this.IsActive)
        {
            // Update which waypoint the AI is heading toward
            m_pathfindingComponent.Update();
				
            // Compute a throttle and a facing toward the waypoint
            m_steeringComponent.Update();
												
            // Make a local copy of our position
            {
                Point2d pixelPosition = new Point2d(m_characterWidget.LocalX, m_characterWidget.LocalY);
					
                m_position = GameConstants.ConvertPixelPositionToRoomPosition(pixelPosition);
            }

            // Make a local copy of our facing, if we're moving
            if (!m_steeringComponent.Throttle.IsAlmostZero)
            {
                m_facing.Copy(m_steeringComponent.Facing);
            }

            // Update the animation based on our desired facing and velocity
            m_characterWidget.UpdateAnimation(m_facing, m_steeringComponent.Throttle);
        }
    }
}