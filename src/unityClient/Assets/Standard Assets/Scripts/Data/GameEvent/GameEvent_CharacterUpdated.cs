using LitJson;
using UnityEngine;
using System.Collections;

public class GameEvent_CharacterUpdated : GameEvent 
{
	public int CharacterID { get; set; }
		
	public GameEvent_CharacterUpdated() : base()
	{	
		EventType = GameEvent.eEventType.character_updated;
        CharacterID = -1;
	}
		
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);

        CharacterID = (int)parameters["character_id"];
	}

    protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);

        parameters["character_id"] = CharacterID;
	}			
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
		string resultString = "";
        CharacterData characterData = gameWorldController.Model.GetCharacterData(CharacterID);
			
		if (characterData != null)
		{
			string characterName = characterData.character_name;
				
			resultString = base.ToChatString(gameWorldController) + characterName + " updated";
		}
		else
		{
			resultString = base.ToChatString(gameWorldController) + "another player updated";
		}
			
		return resultString;
	}		
}