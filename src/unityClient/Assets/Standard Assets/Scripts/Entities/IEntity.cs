using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public interface IEntity 
{
	PathfindingComponent PathfindingInterface { get; }
	SteeringComponent SteeringInterface { get; }
		
	Point3d Position { get; }
	Vector2d Facing { get; }
	RoomKey CurrentRoomKey { get; }
		
	void AddToGameWorld(GameWorldController gameWorldController);	
	void RemoveFromGameWorld(GameWorldController gameWorldController);
		
	void RequestMoveTo(Point3d point, float angle, PathComputer.OnPathComputedCallback onPathComputed);
	void SnapTo(Point3d point3d, float angle);
	void Update();
}
