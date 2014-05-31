using UnityEngine;
using System.Collections;
using LitJson;
using System;

public class GameEvent_MobDialog : GameEvent 
{		
    public int MobId { get; private set; }
    public string Dialog { get; private set; }
		
	public GameEvent_MobDialog() : base()
	{		
		EventType = GameEvent.eEventType.mob_dialog;
        Dialog = "";
	}	
			
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);

        MobId = JsonUtilities.ParseInt(parameters, "mob_id");
        Dialog = JsonUtilities.ParseString(parameters, "dialog");
	}
		
	protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);

        parameters["energy_tank_id"] = MobId;
        parameters["drainer_id"] = Dialog;
	}	
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
		string speakerName = "";
			
        MobData mobData = gameWorldController.Model.GetMobData(MobId);
		MobType mobType= MobTypeManager.GetMobTypeByName(mobData.mob_type_name);

        speakerName = mobType.Name;

        return base.ToChatString(gameWorldController) + speakerName + " said \"" + Dialog + "\"";
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);

        MobEntity entity = gameWorldController.Model.GetMobEntity(MobId);

        entity.PostDialogEvent(Dialog);
	
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