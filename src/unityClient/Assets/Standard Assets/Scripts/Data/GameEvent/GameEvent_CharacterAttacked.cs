using LitJson;
using UnityEngine;
using System.Collections;

public class GameEvent_CharacterAttacked : GameEvent 
{
	public int CharacterId { get; private set;}
    public int MobId { get; private set; }
		
	public GameEvent_CharacterAttacked() : base() 
	{	
		EventType = GameEvent.eEventType.character_attacked;			
		CharacterId = -1;
        MobId = -1;
	}

    protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);
			
		CharacterId = (int)parameters["character_id"];
		MobId = (int)parameters["mob_id"];
	}
		
	protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);
			
		parameters["character_id"] = CharacterId;
		parameters["mob_id"] = MobId;
	}			

	public override string ToChatString(GameWorldController gameWorldController)
	{
		MobData mobData= gameWorldController.Model.GetMobData(MobId);
		MobType mobType = MobTypeManager.GetMobTypeByName(mobData.mob_type_name);		
		string characterName = gameWorldController.Model.GetCharacterData(CharacterId).character_name;
			
		return base.ToChatString(gameWorldController) + characterName + " attacked "+mobType.Name;
	}

    public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);
			
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