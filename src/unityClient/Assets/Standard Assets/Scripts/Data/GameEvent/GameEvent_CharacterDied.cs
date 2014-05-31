using LitJson;
using UnityEngine;
using System.Collections;

public class GameEvent_CharacterDied : GameEvent 
{
    public int CharacterId { get; private set; }
    public int KillerMobId { get; private set; }
		
	public GameEvent_CharacterDied() : base()
	{
		EventType = GameEvent.eEventType.character_died;
		CharacterId = -1;
		KillerMobId = -1;
	}
				
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);
			
		CharacterId = (int)parameters["character_id"];
		KillerMobId = (int)parameters["killer_mob_id"];
	}
		
	protected override void AppendParameters(JsonData parameters) 
	{
		base.AppendParameters(parameters);
			
		parameters["character_id"] = CharacterId;
		parameters["killer_mob_id"] = KillerMobId;
	}	
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
		MobData mobData= gameWorldController.Model.GetMobData(KillerMobId);
		MobType mobType = MobTypeManager.GetMobTypeByName(mobData.mob_type_name);		
		string characterName = gameWorldController.Model.GetCharacterData(CharacterId).character_name;
			
		return base.ToChatString(gameWorldController) + characterName + " killed by "+mobType.Name;
	}
		
	override public void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);
			
		// TODO: Actually apply de-spawn the player sprite
			
		// Completes immediately
		if (onComplete != null)
		{
			onComplete();
		}
	}

    override public void UndoEvent(GameWorldController gameWorldController)
	{
		base.UndoEvent(gameWorldController);
	}			
}