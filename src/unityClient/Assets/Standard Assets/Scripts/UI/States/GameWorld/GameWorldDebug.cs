using UnityEngine;
using System.Collections;
using System;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class GameWorldDebug 
{
	private GameWorldController _gameWorldController;
	private PathComputer _pathComputer;
    private LabelWidget _pathStatusLabel;
    private LabelWidget _widgetLabel;
    private LabelWidget _positionLabel;
    private LabelWidget _visibilityStatusLabel;
		
	public GameWorldDebug(GameWorldController gameWorldController) 
	{
		_gameWorldController = gameWorldController;
		_pathComputer = new PathComputer();
			
		DebugRegistry.SetToggle("entity.pathfinding.render_debug_path", false);
		DebugRegistry.SetToggle("entity.pathfinding.render_nav_mesh", false);
        DebugRegistry.SetToggle("entity.pathfinding.render_visibility", false);
        DebugRegistry.SetToggle("ui.outline_hover_widget", false);
        DebugRegistry.SetToggle("ui.show_cursor_position", false);
	}

    public void Start()
    {
        _pathStatusLabel = new LabelWidget(null, 150, 100, 0.0f, 0.0f, "");
        _pathStatusLabel.Visible = false;

		_visibilityStatusLabel = new LabelWidget(null, 150, 100, 0.0f, 0.0f, "");
        _visibilityStatusLabel.Visible = false;

		_widgetLabel = new LabelWidget(null, 150, 20, 0.0f, 0.0f, "");
        _widgetLabel.Color = Color.red;
        _widgetLabel.Visible = false;

        _positionLabel = new LabelWidget(null, 150, 300, 100, 0, "");
        _positionLabel.Color = Color.red;
        _positionLabel.Visible = false;
    }
		
	public void OnDestroy()
	{
        _pathStatusLabel.OnDestroy();
        _visibilityStatusLabel.OnDestroy();
        _widgetLabel.OnDestroy();
        _positionLabel.OnDestroy();
	}
		
	public void Update()
	{
        if (DebugRegistry.TestToggle("entity.pathfinding.*"))
        {
            if (DebugRegistry.TestToggle("entity.pathfinding.render_debug_path"))
            {
                DebugDrawTestPath();
            }

            if (DebugRegistry.TestToggle("entity.pathfinding.render_visibility"))
            {
                DebugDrawTestVisibility();
            }

            if (DebugRegistry.TestToggle("entity.pathfinding.render_nav_mesh"))
            {
                DebugDrawNavMesh();
            }
        }
        else
        {
            _pathStatusLabel.Visible = false;
            _visibilityStatusLabel.Visible = false;
        }

        if (DebugRegistry.TestToggle("ui.outline_hover_widget"))
        {
            DebugDrawHoverWidget();
        }
        else
        {
            _widgetLabel.Visible = false;
        }

        if (DebugRegistry.TestToggle("ui.show_cursor_position"))
        {
            DebugCursorPosition();
        }
        else
        {
            _positionLabel.Visible = false;
        }
	}

    public void OnGUI()
    {
        _pathStatusLabel.OnGUI();
        _visibilityStatusLabel.OnGUI();
        _widgetLabel.OnGUI();
        _positionLabel.OnGUI();
    }
		
	private void DebugDrawTestPath()
	{
		int characterID = _gameWorldController.Model.CurrentCharacterID;
		RoomKey roomKey = _gameWorldController.Model.CurrentGame.CurrentRoomKey;
		CharacterEntity entity= _gameWorldController.Model.GetCharacterEntity(characterID);
			
		if (entity != null)
		{
			Point2d mousePixelPos = GetMousePixelPosition();
			Point3d mouseWorldPos = GameConstants.ConvertPixelPositionToRoomPosition(mousePixelPos);

			// Draw the target nav cell
			string targetLabel= "";
			AsyncRPGSharedLib.Navigation.NavMesh navMesh = PathfindingSystem.GetNavMesh(roomKey);

			if (navMesh != null)
			{
				NavRef navRef= navMesh.ComputeNavRefAtPoint(mouseWorldPos);
					
				if (navRef.IsValid)
				{
					Point3d centerWorldPos = navMesh.ComputeNavCellCenter((uint)navRef.NavCellIndex);
					Point2d centerPixelPos = GameConstants.ConvertRoomPositionToPixelPosition(centerWorldPos);										
					float halfWidth= (float)GameConstants.NAV_MESH_PIXEL_SIZE / 2.0f;

                    Vector3 boxUL=
                        ClientGameConstants.ConvertPixelPositionToVertexPosition(
                            centerPixelPos.x - halfWidth, centerPixelPos.y - halfWidth, 0.0f);
                    Vector3 boxUR=
                        ClientGameConstants.ConvertPixelPositionToVertexPosition(
                            centerPixelPos.x + halfWidth, centerPixelPos.y - halfWidth, 0.0f);
                    Vector3 boxLR=
                        ClientGameConstants.ConvertPixelPositionToVertexPosition(
                            centerPixelPos.x + halfWidth, centerPixelPos.y + halfWidth, 0.0f);
                    Vector3 boxLL=
                        ClientGameConstants.ConvertPixelPositionToVertexPosition(
                            centerPixelPos.x - halfWidth, centerPixelPos.y + halfWidth, 0.0f);

                    Debug.DrawLine(boxUL, boxUR, Color.blue);
                    Debug.DrawLine(boxUR, boxLR, Color.blue);
                    Debug.DrawLine(boxLR, boxLL, Color.blue);
                    Debug.DrawLine(boxLL, boxUL, Color.blue);
						
					targetLabel = "\nNavCell=" + navRef.NavCellIndex.ToString();
				}

                // Attempt to compute a path from the active player to the mouse
                _pathComputer.BlockingPathRequest(navMesh, roomKey, entity.Position, mouseWorldPos);

                // Update the path status label
                {
                    if (_pathComputer.ResultCode == PathComputer.eResult.success)
                    {
                        _pathStatusLabel.Text = "VALID" + targetLabel;
                        _pathStatusLabel.Color = Color.green;
                    }
                    else
                    {
                        _pathStatusLabel.Text = _pathComputer.ResultCode + targetLabel;
                        _pathStatusLabel.Color = Color.red;
                    }

                    _pathStatusLabel.SetLocalPosition(mousePixelPos.x, mousePixelPos.y);
                    _pathStatusLabel.Visible = true;
                }

                // Render the raw path
                for (int stepIndex = 1; stepIndex < _pathComputer.FinalPath.Count; stepIndex++)
                {
                    Point3d previousPoint = _pathComputer.FinalPath[stepIndex - 1].StepPoint;
                    Point3d currentPoint = _pathComputer.FinalPath[stepIndex].StepPoint;
                    Vector3 previousPixelPoint = ClientGameConstants.ConvertRoomPositionToVertexPosition(previousPoint);
                    Vector3 currentPixelPoint = ClientGameConstants.ConvertRoomPositionToVertexPosition(currentPoint);

                    Debug.DrawLine(previousPixelPoint, currentPixelPoint, Color.green);
                }
			}			
		}
	}

    private void DebugDrawTestVisibility()
    {
        int characterID = _gameWorldController.Model.CurrentCharacterID;
        RoomKey roomKey = _gameWorldController.Model.CurrentGame.CurrentRoomKey;
        CharacterEntity entity = _gameWorldController.Model.GetCharacterEntity(characterID);

        if (entity != null)
        {
            Point3d startWorldPos = entity.Position;

            Point2d endPixelPos = GetMousePixelPosition();
            Point3d endWorldPos = GameConstants.ConvertPixelPositionToRoomPosition(endPixelPos);

            // Draw the target nav cell
            AsyncRPGSharedLib.Navigation.NavMesh navMesh = PathfindingSystem.GetNavMesh(roomKey);

            if (navMesh != null)
            {
                NavRef startNavRef = navMesh.ComputeNavRefAtPoint(startWorldPos);
                NavRef endNavRef = navMesh.ComputeNavRefAtPoint(endWorldPos);

                if (startNavRef.IsValid && endNavRef.IsValid)
                {
                    bool canSee = navMesh.NavRefCanSeeOtherNavRef(startNavRef, endNavRef);
                    Color debugColor = canSee ? Color.green : Color.red;

                    // Draw a box around the nav cell
                    {
                        Point3d endCenterWorldPos = navMesh.ComputeNavCellCenter((uint)endNavRef.NavCellIndex);
                        Point2d endCenterPixelPos = GameConstants.ConvertRoomPositionToPixelPosition(endCenterWorldPos);
                        float halfWidth = (float)GameConstants.NAV_MESH_PIXEL_SIZE / 2.0f;

                        Vector3 boxUL =
                            ClientGameConstants.ConvertPixelPositionToVertexPosition(
                                endCenterPixelPos.x - halfWidth, endCenterPixelPos.y - halfWidth, 0.0f);
                        Vector3 boxUR =
                            ClientGameConstants.ConvertPixelPositionToVertexPosition(
                                endCenterPixelPos.x + halfWidth, endCenterPixelPos.y - halfWidth, 0.0f);
                        Vector3 boxLR =
                            ClientGameConstants.ConvertPixelPositionToVertexPosition(
                                endCenterPixelPos.x + halfWidth, endCenterPixelPos.y + halfWidth, 0.0f);
                        Vector3 boxLL =
                            ClientGameConstants.ConvertPixelPositionToVertexPosition(
                                endCenterPixelPos.x - halfWidth, endCenterPixelPos.y + halfWidth, 0.0f);

                        Debug.DrawLine(boxUL, boxUR, debugColor);
                        Debug.DrawLine(boxUR, boxLR, debugColor);
                        Debug.DrawLine(boxLR, boxLL, debugColor);
                        Debug.DrawLine(boxLL, boxUL, debugColor);
                    }

                    // Update the visibility status label
                    _visibilityStatusLabel.Text = canSee ? "VISIBLE" : "INVISIBLE";
                    _visibilityStatusLabel.SetLocalPosition(endPixelPos.x, endPixelPos.y);
                    _visibilityStatusLabel.Color = debugColor;
                    _visibilityStatusLabel.Visible = true;

                    // Render the ray-cast line
                    {
                        Vector3 startVertex = ClientGameConstants.ConvertRoomPositionToVertexPosition(startWorldPos);
                        Vector3 endVertex = ClientGameConstants.ConvertRoomPositionToVertexPosition(endWorldPos);

                        Debug.DrawLine(startVertex, endVertex, debugColor);
                    }
                }
            }
        }
    }
		
	private void DebugDrawNavMesh()
	{
		RoomKey roomKey = _gameWorldController.Model.CurrentGame.CurrentRoomKey;
		AsyncRPGSharedLib.Navigation.NavMesh navMesh = PathfindingSystem.GetNavMesh(roomKey);
			
		if (navMesh != null)
		{
            for (uint navCellIndex = 0; navCellIndex < navMesh.GetNavCellCount(); navCellIndex++)
		    {
                int connectivityId = navMesh.GetNavCellConnectivityID(navCellIndex);

                if (connectivityId != AsyncRPGSharedLib.Navigation.NavMesh.EMPTY_NAV_CELL)
			    {
				    for (MathConstants.eDirection direction = MathConstants.eDirection.first; 
                        direction < MathConstants.eDirection.count; 
                        direction++)
				    {
                        if (!navMesh.NavCellHasNeighbor(navCellIndex, direction))
					    {
                            Point3d portalLeft, portalRight;
                            Vector3 portalVertexLeft, portalVertexRight;

                            navMesh.ComputePointsOnNavCellSide(navCellIndex, direction, out portalLeft, out portalRight);
                            portalVertexLeft = ClientGameConstants.ConvertRoomPositionToVertexPosition(portalLeft);
                            portalVertexRight = ClientGameConstants.ConvertRoomPositionToVertexPosition(portalRight);
							
                            Debug.DrawLine(
                                portalVertexLeft,
                                portalVertexRight,
                                Color.yellow, 
                                0.0f, // duration 
                                false); // depth test
					    }
				    }
			    }
		    }
		}
	}

    private void DebugDrawHoverWidget()
    {
        IWidget widget= _gameWorldController.View.WidgetEventDispatcher.MouseOverWidget;

        if (widget != null)
        {
            Vector3 boxUL =
                ClientGameConstants.ConvertPixelPositionToVertexPosition(
                    widget.WorldX, widget.WorldY, 0.0f);
            Vector3 boxUR =
                ClientGameConstants.ConvertPixelPositionToVertexPosition(
                    widget.WorldX + widget.Width, widget.WorldY, 0.0f);
            Vector3 boxLR =
                ClientGameConstants.ConvertPixelPositionToVertexPosition(
                    widget.WorldX + widget.Width, widget.WorldY + widget.Height, 0.0f);
            Vector3 boxLL =
                ClientGameConstants.ConvertPixelPositionToVertexPosition(
                    widget.WorldX, widget.WorldY + widget.Height, 0.0f);

            Debug.DrawLine(boxUL, boxUR, Color.red);
            Debug.DrawLine(boxUR, boxLR, Color.red);
            Debug.DrawLine(boxLR, boxLL, Color.red);
            Debug.DrawLine(boxLL, boxUL, Color.red);

            _widgetLabel.SetLocalPosition(widget.WorldX + (widget.Width / 2.0f), widget.WorldY + (widget.Height / 2.0f));
            _widgetLabel.Text = widget.GetType().Name;
            _widgetLabel.Visible = true;
        }
    }

    private void DebugCursorPosition()
    {
        Point2d pixelPosition= GetMousePixelPosition();
        Point2d screenPosition = ClientGameConstants.ConvertPixelPositionToScreenPosition(pixelPosition);
        Point3d roomPosition = GameConstants.ConvertPixelPositionToRoomPosition(pixelPosition);
        Vector3 vertexPosition = ClientGameConstants.ConvertPixelPositionToVertexPosition(pixelPosition.x, pixelPosition.y, 0.0f);
        NavRef navRef = _gameWorldController.OverlayController.View.CurrentNavRef;

        _positionLabel.Text = 
            string.Format(
                "Pixel: {0},{1}\nScreen: {2}, {3}\nRoom: {4}, {5}, {6}\nVertex: {7}, {8}, {9}\nNavRef: {10}",
                pixelPosition.x, pixelPosition.y,
                screenPosition.x, screenPosition.y,
                roomPosition.x, roomPosition.y, roomPosition.z,
                vertexPosition.x, vertexPosition.y, vertexPosition.z,
                navRef.NavCellIndex);
        _positionLabel.Visible = true;
    }

    private static Point2d GetMousePixelPosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        mousePosition.y = Screen.height - mousePosition.y;

        return new Point2d(mousePosition.x, mousePosition.y);
    }
}
