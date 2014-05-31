using LitJson;
using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class GameEvent_CharacterPortaled : GameEvent 
{
	public int CharacterID { get; private set; }
    public RoomKey ToRoomKey { get; private set; }
	public RoomKey FromRoomKey { get; private set; }		
    public Point3d FromPosition { get; private set; }
	public float FromAngle { get; private set; }		
	public Point3d ToPosition { get; private set; }
	public float ToAngle { get; private set; }

    public GameEvent_CharacterPortaled()
        : base()
	{
		EventType = GameEvent.eEventType.character_portaled;
			
		CharacterID = -1;
		FromRoomKey = new RoomKey();
        ToRoomKey = new RoomKey();
			
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
			
		this.FromRoomKey = new RoomKey(
            game_id,
            JsonUtilities.ParseInt(parameters, "from_room_x"),
		    JsonUtilities.ParseInt(parameters, "from_room_y"),
		    JsonUtilities.ParseInt(parameters, "from_room_z"));
        this.ToRoomKey = new RoomKey(
            game_id,
            JsonUtilities.ParseInt(parameters, "to_room_x"),
            JsonUtilities.ParseInt(parameters, "to_room_y"),
            JsonUtilities.ParseInt(parameters, "to_room_z"));
			
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

        parameters["from_room_x"] = FromRoomKey.x;
        parameters["from_room_y"] = FromRoomKey.y;
        parameters["from_room_z"] = FromRoomKey.z;

        parameters["to_room_x"] = ToRoomKey.x;
        parameters["to_room_y"] = ToRoomKey.y;
        parameters["to_room_z"] = ToRoomKey.z;
			
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

        return base.ToChatString(gameWorldController) + characterName + " moved " + facing +
            " to room at (" + ToRoomKey.x + ", " + ToRoomKey.y + ", " + ToRoomKey.z + ")";
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);
			
		CharacterData character = gameWorldController.Model.GetCharacterData(CharacterID);

        // Update the room key and position on the character that changed rooms
        character.room_x = ToRoomKey.x;
        character.room_y = ToRoomKey.y;
        character.room_z = ToRoomKey.z;
        character.x = ToPosition.x;
        character.y = ToPosition.y;
        character.z = ToPosition.z;		
	
		// Character is the current game character
		if (CharacterID == gameWorldController.Model.CurrentCharacterID)
		{
			// Request the static room data for the room the character is portal-ing to
			// event application won't advance while waiting on room data to load from the server.
			// Once we get the new room data we rebuild everything.
			gameWorldController.Model.RequestRoomData(ToRoomKey, onComplete);
		}
		// Character is some other game character
		else 
		{
			// Get the character entity for the character id, if any exists 
            CharacterEntity entity = gameWorldController.Model.GetCharacterEntity(CharacterID);
				
			// Character portal-ed to the current room.
			// Make sure the character exists and is at the correct location.		
			if (gameWorldController.Model.CurrentGame.CurrentRoomKey.Equals(ToRoomKey))
			{	
				if (entity == null)
				{
                    entity = new CharacterEntity(CharacterID);
					entity.AddToGameWorld(gameWorldController);
				}
					
				entity.SnapTo(ToPosition, ToAngle);
			}
			// Character left the current room.
			// Make sure the character entity is cleaned up.
			else
			{
				if (entity != null)
				{
					entity.RemoveFromGameWorld(gameWorldController);
				}
			}
				
			if (onComplete != null)
			{
				onComplete();
			}				
		}	
	}

	public override void UndoEvent(GameWorldController gameWorldController)
	{
        CharacterData character = gameWorldController.Model.GetCharacterData(CharacterID);

        // Update the room key and position on the character that changed rooms
        character.room_x = FromRoomKey.x;
        character.room_y = FromRoomKey.y;
        character.room_z = FromRoomKey.z;
        character.x = FromPosition.x;
        character.y = FromPosition.y;
        character.z = FromPosition.z;

        // Character is the current game character
        if (CharacterID == gameWorldController.Model.CurrentCharacterID)
        {
            // Request the static room data for the room the character portal-ed from
            // event application won't advance while waiting on room data to load from the server.
            // Once we get the new room data we rebuild everything.
            gameWorldController.Model.RequestRoomData(FromRoomKey, null);
        }
        // Character is some other game character
        else
        {
            // Get the character entity for the character id, if any exists 
            CharacterEntity entity = gameWorldController.Model.GetCharacterEntity(CharacterID);

            // Character portal-ed from the current room.
            // Make sure the character exists and is at the correct location.		
            if (gameWorldController.Model.CurrentGame.CurrentRoomKey.Equals(FromRoomKey))
            {
                if (entity == null)
                {
                    entity = new CharacterEntity(CharacterID);
                    entity.AddToGameWorld(gameWorldController);
                }

                entity.SnapTo(FromPosition, FromAngle);
            }
            // Character from some other room than the current one.
            // Make sure the character entity is cleaned up.
            else
            {
                if (entity != null)
                {
                    entity.RemoveFromGameWorld(gameWorldController);
                }
            }
        }

        base.UndoEvent(gameWorldController);
	}	
}