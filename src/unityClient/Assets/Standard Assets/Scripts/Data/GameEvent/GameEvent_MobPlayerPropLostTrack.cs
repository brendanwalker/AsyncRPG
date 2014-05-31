using UnityEngine;
using System.Collections;
using LitJson;
using System;
using AsyncRPGSharedLib.Environment;

public class GameEvent_MobPlayerPropLostTrack : GameEvent 
{		
    public int MobId { get; private set; }
    public int CharacterID { get; private set; }
    public Point3d Position { get; private set; }
		
	public GameEvent_MobPlayerPropLostTrack() : base()
	{		
		EventType = GameEvent.eEventType.mob_player_prop_lost_track;
        CharacterID = -1;
        Position = new Point3d();
	}	
			
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);

        MobId = JsonUtilities.ParseInt(parameters, "mob_id");
        CharacterID = JsonUtilities.ParseInt(parameters, "character_id");
        Position = new Point3d(
            JsonUtilities.ParseFloat(parameters, "x"),
            JsonUtilities.ParseFloat(parameters, "y"),
            JsonUtilities.ParseFloat(parameters, "z"));
	}
		
	protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);

        parameters["mob_id"] = MobId;
        parameters["character_id"] = CharacterID;
        parameters["x"] = Position.x;
        parameters["y"] = Position.y;
        parameters["z"] = Position.z;
	}	
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
        MobData mobData = gameWorldController.Model.GetMobData(MobId);
		MobType mobType= MobTypeManager.GetMobTypeByName(mobData.mob_type_name);
        string characterName = gameWorldController.Model.GetCharacterData(CharacterID).character_name;

        return base.ToChatString(gameWorldController) + mobType.Name + " lost track of " + characterName;
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);
			
		//TODO: Spawn "lost track of" effect
	
		// Completes immediately
		if (onComplete != null)
		{
			onComplete();
		}
	}

	public override void UndoEvent(GameWorldController gameWorldController)
	{	
	    // Nothing to do		
		base.UndoEvent(gameWorldController);
	}			
}