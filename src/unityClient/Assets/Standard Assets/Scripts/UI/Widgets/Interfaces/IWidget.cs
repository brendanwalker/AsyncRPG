using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Environment;

public interface IWidget
{
	bool Visible { get; set; }

	float Width { get; }
	float Height { get; }
		
	Point2d LocalPosition { get; }
	float LocalX { get; }
	float LocalY { get; }

    void SetLocalPosition(float x, float y);

    Point2d WorldPosition { get; }
    float WorldX { get; }
    float WorldY { get; }
    bool ContainsWorldPositionPoint(Point2d point);
    void UpdateWorldPosition();

    void OnGUI();
    void OnDestroy();    
}
