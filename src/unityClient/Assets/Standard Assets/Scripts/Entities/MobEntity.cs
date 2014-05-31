using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class MobEntity : IEntity
{
    private const float DIALOG_DISPLAY_DURATION = 2.0f;

    private Point3d m_position;
    private Vector2d m_facing;
    private float m_dialogTimer;

    private int m_mobId;
    private MobData m_mobData;

    private MobWidget m_mobWidget;
    private PathfindingComponent m_pathfindingComponent;
    private SteeringComponent m_steeringComponent;

    public MobEntity(int characterId)
    {
        SessionData sessionData = SessionData.GetInstance();

        m_mobId = characterId;
        m_mobData = sessionData.CurrentGameData.CurrentRoom.GetMobById(m_mobId);

        m_position = new Point3d(m_mobData.x, m_mobData.y, m_mobData.z);
        m_facing = MathConstants.GetUnitVectorForAngle(m_mobData.angle);
        m_dialogTimer = -1.0f;

        m_mobWidget = null;
        m_pathfindingComponent = null;
        m_steeringComponent = null;
    }

    // Accessors
    public MobWidget MyMobWidget
    {
        get { return m_mobWidget; }
    }

    public MobData MyMobData
    {
        get { return m_mobData; }
    }

    public bool IsActive
    {
        get { return m_mobWidget != null; }
    }

    public int Energy
    {
        get { return m_mobData.energy; }

        set
        {
            m_mobData.energy = value;
            m_mobWidget.Energy = value;
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
        get { return MyMobData.CurrentRoomKey; }
    }

    public void AddToGameWorld(GameWorldController gameWorldController)
    {
        m_pathfindingComponent = new PathfindingComponent(this);
        m_steeringComponent = new SteeringComponent(this);

        m_mobWidget = gameWorldController.View.AddMobWidget(m_mobData);
        gameWorldController.Model.AddMobEntity(this);
    }

    public void RemoveFromGameWorld(GameWorldController gameWorldController)
    {
        gameWorldController.Model.RemoveMobEntity(this);
        gameWorldController.View.RemoveMobWidget(m_mobWidget);

        m_mobWidget = null;
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
                    m_mobData.x = Position.x;
                    m_mobData.y = Position.y;
                    m_mobData.z = Position.z;
                    m_mobData.angle = angle;

                    onPathComputed(PathComputer.eResult.success);
                }
                else
                {
                    if (pathResult.ResultCode == PathComputer.eResult.success)
                    {
                        // Set the character data immediately to the destination
                        m_mobData.x = point.x;
                        m_mobData.y = point.y;
                        m_mobData.z = point.z;
                        m_mobData.angle = angle;
                    }

                    onPathComputed(pathResult.ResultCode);
                }
            });
    }

    public void SnapTo(Point3d point, float angle)
    {
        Point2d pixelPosition = GameConstants.ConvertRoomPositionToPixelPosition(point);

        // Update the character data
        m_mobData.x = point.x;
        m_mobData.y = point.y;
        m_mobData.z = point.z;
        m_mobData.angle = angle;

        // Snap our position and facing
        m_position.Set(point);
        m_facing = MathConstants.GetUnitVectorForAngle(angle);
        m_mobWidget.SetLocalPosition(pixelPosition.x, pixelPosition.y);

        // Snap out facing and go to idle
        m_mobWidget.UpdateAnimation(m_facing, Vector2d.ZERO_VECTOR);

        // Forget about any path we were running
        m_pathfindingComponent.ClearPath();
    }

    public void PostDialogEvent(string dialogLine)
    {
        m_mobWidget.ShowDialog(dialogLine);
        m_dialogTimer = Time.time;
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
                Point2d pixelPosition = new Point2d(m_mobWidget.LocalX, m_mobWidget.LocalY);

                m_position = GameConstants.ConvertPixelPositionToRoomPosition(pixelPosition);
            }

            // Make a local copy of our facing, if we're moving
            if (!m_steeringComponent.Throttle.IsAlmostZero)
            {
                m_facing.Copy(m_steeringComponent.Facing);
            }

            // Update the animation based on our desired facing and velocity
            m_mobWidget.UpdateAnimation(m_facing, m_steeringComponent.Throttle);

            // Hide the vision cone while we' re moving
            m_mobWidget.SetVisionConeVisible(m_steeringComponent.Throttle.IsAlmostZero);

            // Hide the dialog if it timed out
            if (m_dialogTimer >= 0.0f && (Time.time - m_dialogTimer) >= DIALOG_DISPLAY_DURATION)
            {
                m_mobWidget.HideDialog();
                m_dialogTimer = -1.0f;
            }
        }
    }
}