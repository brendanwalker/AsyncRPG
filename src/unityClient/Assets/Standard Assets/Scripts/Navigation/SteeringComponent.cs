using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

public class SteeringComponent 
{
	private IEntity m_entity;
	private Vector2d m_throttle; // Pixel-Space Throttle
	private Vector2d m_facing;
		
	public SteeringComponent(IEntity entity) 
	{
		m_entity = entity;
        m_throttle = new Vector2d();
        m_facing = new Vector2d();	
			
		Reset();
	}		

	public Vector2d Throttle
	{
		get { return m_throttle; }
	}
		
	public Vector2d Facing
	{
		get { return m_facing; }
	}
				
	public void Update()
	{
		PathfindingComponent pathFindingInterface = m_entity.PathfindingInterface;

        if (pathFindingInterface != null)
        {
            if (!pathFindingInterface.IsPathComplete)
            {
                Point3d targetRoomPosition = pathFindingInterface.CurrentWaypoint.StepPoint;
                Point2d targetPixelPosition = GameConstants.ConvertRoomPositionToPixelPosition(targetRoomPosition);
                Point2d currentPixelPosition = GameConstants.ConvertRoomPositionToPixelPosition(m_entity.Position);
                Vector2d offsetToTarget = targetPixelPosition - currentPixelPosition;

                // Throttle full speed at the target
                m_throttle.Copy(offsetToTarget);
                m_throttle.Normalize();

                // Always face the direction we're moving
                m_facing.Copy(offsetToTarget);
                m_facing.NormalizeWithDefault(Vector2d.WORLD_DOWN);
            }
            else
            {
                // Leave the facing alone, clear the throttle
                m_throttle.Copy(Vector2d.ZERO_VECTOR);
            }
        }
        else
        {
            Reset();
        }
	}		
		
	private void Reset()
	{
		m_throttle.Copy(Vector2d.ZERO_VECTOR);
		m_facing.Copy(Vector2d.WORLD_DOWN);
	}		
}