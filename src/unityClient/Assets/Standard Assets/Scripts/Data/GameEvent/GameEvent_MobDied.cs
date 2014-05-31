using LitJson;
using UnityEngine;
using System.Collections;

public class GameEvent_MobDied : GameEvent 
{
	public int MobId { get; private set; }
	public int KillerCharacterId { get; private set; }
		
	public GameEvent_MobDied() : base()
	{	
		EventType = GameEvent.eEventType.mob_died;
		MobId = -1;
		KillerCharacterId = -1;
	}
			
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);
			
		MobId = JsonUtilities.ParseInt(parameters, "mob_id");
		KillerCharacterId = JsonUtilities.ParseInt(parameters, "killer_character_id");
	}
		
	protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);
			
		parameters["mob_id"] = MobId;
		parameters["killer_character_id"] = KillerCharacterId;
	}
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
		MobData mobData = gameWorldController.Model.GetMobData(MobId);
		MobType mobType = MobTypeManager.GetMobTypeByName(mobData.mob_type_name);				

		return base.ToChatString(gameWorldController) + " "+mobType.Name+" died";
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);

		// TODO: Actually de-spawn the mob sprite
			
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