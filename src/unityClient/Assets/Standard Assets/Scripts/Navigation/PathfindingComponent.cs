using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class PathfindingComponent 
{
    private delegate bool WaypointRule(PathfindingComponent pathfindingInterface);

	private static WaypointRule[] WAYPOINT_RULES = new WaypointRule[] {PathfindingComponent.WaypointWithinToleranceRule};
		
	public const float DESTINATION_TOLERANCE = 0.1f;
	public const float WAYPOINT_TOLERANCE = 0.1f;		
	
	public enum ePathFindingState
    {
        completed,
        in_progress
    }
		
	private IEntity m_entity;
	private Point3d m_destinationPoint;
    private Vector2d m_destinationFacing;
	private ePathFindingState m_state;
		
	private List<PathStep> m_pathSteps;
	private int m_pathStepIndex;
	private PathComputer.OnPathComputerComplete m_pathCompleteCallback;
		
	public PathfindingComponent(IEntity entity) 
	{
		m_entity = entity;
        m_destinationPoint = new Point3d(entity.Position);
        m_destinationFacing = new Vector2d(entity.Facing);
		m_pathCompleteCallback = null;
			
		ClearPath();
	}
		
	public void ClearPath()
	{		
		m_pathSteps = new List<PathStep>();
		m_pathStepIndex = 0;						
		m_destinationPoint.Set(m_entity.Position);
        m_destinationFacing.Copy(m_entity.Facing);
			
		OnPathCompleted(null);
	}
		
	public void Update()
	{			
		if (m_state == ePathFindingState.in_progress)
		{
			EvaluateWaypointRules();
		}
	}	
		
	// Accessors
	public PathStep CurrentWaypoint
	{
        get 
        {
		    PathStep currentStep = null;
			
		    if (m_pathStepIndex < m_pathSteps.Count)
		    {
			    currentStep = m_pathSteps[m_pathStepIndex];
		    }
		
		    return currentStep;
        }
	}
		
	public bool IsLastStep
	{
		get 
        { 
            return m_pathStepIndex == m_pathSteps.Count - 1; 
        }
	}
		
	public bool IsPathComplete
	{
		get 
        {
            return m_state == ePathFindingState.completed;
        }
	}

    public Point3d DestinationPosition
    {
        get { return m_destinationPoint; }
        set { m_destinationPoint.Set(value); }
    }

    public Vector2d DestinationFacing
    {
        get { return m_destinationFacing; }
        set { m_destinationFacing.Copy(value); }
    }
		
	// Requests
	public bool SubmitPathRequest(
        Point3d point,
        Vector2d facing,
        PathComputer.OnPathComputerComplete pathCompletedCallback) 
	{
		bool success = false;			

		m_pathSteps = new List<PathStep>();
		m_pathStepIndex = 0;
		m_pathCompleteCallback = pathCompletedCallback;
			
		m_destinationPoint.Set(point);
        m_destinationFacing.Copy(facing);
			
		if (Point3d.DistanceSquared(m_destinationPoint, m_entity.Position) > MathConstants.EPSILON_SQUARED)
		{			
			success = 
                PathfindingSystem.AddPathRequest(
                    m_entity.CurrentRoomKey, 
                    m_entity.Position, 
                    m_destinationPoint, 
			        (PathComputer pathResult) =>
			        {					
				        if (pathResult.ResultCode == PathComputer.eResult.success)
				        {
					        m_state = ePathFindingState.in_progress;
                            m_pathSteps = pathResult.FinalPath;
				        }
				        else 
				        {
					        OnPathCompleted(pathResult);
				        }
			        });
		}
		else 
		{
			OnPathCompleted(null);
			success = true;
		}
			
		return success;
	}		
		
	// Events
	private void OnPathCompleted(PathComputer pathResult)
	{
		m_state = ePathFindingState.completed;
			
		if (m_pathCompleteCallback != null)
		{
			m_pathCompleteCallback(pathResult);
			m_pathCompleteCallback = null;
		}
	}
		
	// Way point Rules
	private void EvaluateWaypointRules()
	{
		bool keepEvaluating = true;
			
		while (keepEvaluating)
		{
			keepEvaluating = false;
				
			for (int ruleIndex = 0; ruleIndex <  WAYPOINT_RULES.Length; ruleIndex++)
			{
				WaypointRule rule = WAYPOINT_RULES[ruleIndex];
				bool hitWaypoint = rule(this);
					
				if (hitWaypoint)
				{
					if (IsLastStep)
					{
						OnPathCompleted(null);
					}
					else
					{
						m_pathStepIndex++;
						keepEvaluating = true;
					}
						
					break;
				}
			}
		}
	}
		
	private static bool WaypointWithinToleranceRule(PathfindingComponent pathfindingInterface)
	{
		float distanceToWaypointSquared = 
            Point3d.DistanceSquared(
                pathfindingInterface.m_entity.Position, 
                pathfindingInterface.CurrentWaypoint.StepPoint);
		float toleranceSquared = 
            pathfindingInterface.IsLastStep ? 
            DESTINATION_TOLERANCE * DESTINATION_TOLERANCE : 
            WAYPOINT_TOLERANCE * WAYPOINT_TOLERANCE;
		bool hitWaypoint= (distanceToWaypointSquared <= toleranceSquared);
			
		return hitWaypoint;
	}
}