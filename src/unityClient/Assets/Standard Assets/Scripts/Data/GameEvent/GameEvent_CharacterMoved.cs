using LitJson;
using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class GameEvent_CharacterMoved : GameEvent 
{
	public int CharacterID { get; private set; }        
	public RoomKey CurrentRoomKey { get; private set; }		
    public Point3d FromPosition { get; private set; }
	public float FromAngle { get; private set; }		
	public Point3d ToPosition { get; private set; }
	public float ToAngle { get; private set; }
		
	public GameEvent_CharacterMoved() : base()
	{
		EventType = GameEvent.eEventType.character_moved;
			
		CharacterID = -1;
		CurrentRoomKey = new RoomKey();
			
		FromPosition = new Point3d();
		FromAngle = 0;
			
		ToPosition = new Point3d();
		ToAngle = 0;
	}					
		
	protected override void ParseParameters(JsonData parameters)
	{
        int game_id = SessionData.GetInstance().GameID;
		base.ParseParameters(parameters);
			
		this.CharacterID = JsonUtilities.ParseInt(parameters, "character_id");
			
		this.CurrentRoomKey = new RoomKey(
            game_id,
            JsonUtilities.ParseInt(parameters, "room_x"),
		    JsonUtilities.ParseInt(parameters, "room_y"),
		    JsonUtilities.ParseInt(parameters, "room_z"));
			
        this.FromPosition = new Point3d(
		    JsonUtilities.ParseFloat(parameters, "from_x"),
		    JsonUtilities.ParseFloat(parameters, "from_y"),
		    JsonUtilities.ParseFloat(parameters, "from_z"));
		this.FromAngle = JsonUtilities.ParseFloat(parameters, "from_angle");

        this.ToPosition = new Point3d(
		    JsonUtilities.ParseFloat(parameters, "to_x"),
		    JsonUtilities.ParseFloat(parameters, "to_y"),
		    JsonUtilities.ParseFloat(parameters, "to_z"));
		this.ToAngle = JsonUtilities.ParseFloat(parameters, "to_angle");						
	}
		
	protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);
			
		parameters["character_id"] = CharacterID;
			
		parameters["room_x"] = CurrentRoomKey.x;
		parameters["room_y"] = CurrentRoomKey.y;
		parameters["room_z"] = CurrentRoomKey.z;
			
		parameters["from_x"] = FromPosition.x;
		parameters["from_y"] = FromPosition.y;
		parameters["from_z"] = FromPosition.z;
		parameters["from_angle"] = FromAngle;

		parameters["to_x"] = ToPosition.x;
		parameters["to_y"] = ToPosition.y;
		parameters["to_z"] = ToPosition.z;
		parameters["to_angle"] = ToAngle;				
	}		
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
		string characterName = gameWorldController.Model.GetCharacterData(CharacterID).character_name;
		float distance = (Point3d.Distance(FromPosition, ToPosition));
		MathConstants.eDirection direction = MathConstants.GetDirectionForAngle(ToAngle);
		string facing = "";
			
		switch(direction)
		{
		case MathConstants.eDirection.none:
			facing = "South";
			break;
		case MathConstants.eDirection.right:
			facing = "East";
			break;
		case MathConstants.eDirection.up:
			facing = "North";
			break;
		case MathConstants.eDirection.left:
			facing = "West";
			break;
		case MathConstants.eDirection.down:
			facing = "South";
			break;
		}
			
		return base.ToChatString(gameWorldController) + characterName + " moved "+distance.ToString("F1")+" feet, now facing "+facing;
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);
			
		CharacterEntity entity = gameWorldController.Model.GetCharacterEntity(CharacterID);
			
		if (onComplete != null)
		{
            entity.RequestMoveTo(ToPosition, ToAngle, (PathComputer.eResult resultCode) =>
			{
				if (resultCode != PathComputer.eResult.success)
				{
					Debug.Log("GameEvent_CharacterMoved:apply_event - Failed to compute path to event target, ResultCode=" + resultCode);
                    entity.SnapTo(ToPosition, ToAngle);
				}
					
				onComplete();
			});
		}
		else 
		{
			entity.SnapTo(ToPosition, ToAngle);
		}
	}

	public override void UndoEvent(GameWorldController gameWorldController)
	{
		base.UndoEvent(gameWorldController);

        CharacterEntity entity = gameWorldController.Model.GetCharacterEntity(CharacterID);
			
		entity.SnapTo(FromPosition, FromAngle);
	}	
}