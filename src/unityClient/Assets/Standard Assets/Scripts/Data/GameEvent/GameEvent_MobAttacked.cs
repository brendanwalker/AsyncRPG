using LitJson;
using UnityEngine;
using System.Collections;

public class GameEvent_MobAttacked : GameEvent 
{
	public int MobId { get; private set; }
		
	public GameEvent_MobAttacked() : base()
	{	
		EventType = GameEvent.eEventType.mob_attacked;
		MobId = -1;
	}
		
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);
			
		MobId = JsonUtilities.ParseInt(parameters, "mob_id");
	}
		
	protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);
			
		parameters["mob_id"] = MobId;				
	}		
		
	public override string ToChatString(GameWorldController gameWorldController)
	{		
		MobData mobData= gameWorldController.Model.GetMobData(MobId);
		MobType mobType = MobTypeManager.GetMobTypeByName(mobData.mob_type_name);				
			
		// TODO: Display what the mob ID attacked
		return base.ToChatString(gameWorldController)+" "+mobType.Name+" attacked";
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);
			
		// TODO: Actually apply damage to victim
			
		// Completes immediately
		if (onComplete != null)
		{
			onComplete();
		}		
	}

    public override void UndoEvent(GameWorldController gameWorldController)
	{
		base.UndoEvent(gameWorldController);
	}	
}