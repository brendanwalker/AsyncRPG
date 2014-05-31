using UnityEngine;
using System.Collections;
using LitJson;
using System;
using AsyncRPGSharedLib.Environment;

public class GameEvent_MobAIPropSpotted : GameEvent 
{		
    public int MobId { get; private set; }
    public int SpottedMobID { get; private set; }
    public Point3d Position { get; private set; }
		
	public GameEvent_MobAIPropSpotted() : base()
	{		
		EventType = GameEvent.eEventType.mob_ai_prop_spotted;
        SpottedMobID = -1;
        Position = new Point3d();
	}	
			
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);

        MobId = JsonUtilities.ParseInt(parameters, "mob_id");
        SpottedMobID = JsonUtilities.ParseInt(parameters, "spotted_mob_id");
        Position = new Point3d(
            JsonUtilities.ParseFloat(parameters, "x"),
            JsonUtilities.ParseFloat(parameters, "y"),
            JsonUtilities.ParseFloat(parameters, "z"));
	}
		
	protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);

        parameters["mob_id"] = MobId;
        parameters["spotted_mob_id"] = SpottedMobID;
        parameters["x"] = Position.x;
        parameters["y"] = Position.y;
        parameters["z"] = Position.z;
	}	
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
        MobData mobData = gameWorldController.Model.GetMobData(MobId);
		MobType mobType= MobTypeManager.GetMobTypeByName(mobData.mob_type_name);

        MobData spottedMobData = gameWorldController.Model.GetMobData(SpottedMobID);
        MobType spottedMobType = MobTypeManager.GetMobTypeByName(spottedMobData.mob_type_name);

        return base.ToChatString(gameWorldController) + mobType.Name + " spotted " + spottedMobType.Name;
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);
			
		//TODO: Spawn "spotted" effect
	
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