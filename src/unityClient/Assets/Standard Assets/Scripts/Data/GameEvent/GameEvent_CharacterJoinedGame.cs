using LitJson;
using UnityEngine;
using System.Collections;

public class GameEvent_CharacterJoinedGame : GameEvent 
{		
	private CharacterData _characterState;
		
	public GameEvent_CharacterJoinedGame() : base()
	{
		EventType = GameEvent.eEventType.character_joined_game;
        _characterState = new CharacterData();
	}
		
	public CharacterData CharacterState 
	{
		get { return _characterState; }
	}
		
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);
			
		_characterState = CharacterData.FromObject( parameters["character_state"] );
	}
		
	/*protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);
			
		parameters["character_state"] = _characterState.ToObject();
	}*/					
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
		return base.ToChatString(gameWorldController) + _characterState.character_name + " joined the game";
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{			
		GameData currentGame = gameWorldController.Model.CurrentGame;
			
		if (currentGame.CharacterID != _characterState.character_id)
		{
			base.ApplyEvent(gameWorldController, onComplete);			
				
			// Add the character state to the current game state
			currentGame.SetCharacterById(_characterState.character_id, _characterState);
							
			// Create a new character entity using the character data just set in the game state
			{
				CharacterEntity characterEntity = new CharacterEntity(_characterState.character_id);
					
				characterEntity.AddToGameWorld(gameWorldController);
			}
		}
		else 
		{
			Debug.Log("Ignoring apply CharacterJoinedGame event for primary character_id=" + _characterState.character_id);
		}
			
		// Completes immediately
		if (onComplete != null)
		{
			onComplete();
		}		
	}

	public override void UndoEvent(GameWorldController gameWorldController)
	{
		GameData currentGame = gameWorldController.Model.CurrentGame;
			
		if (currentGame.CharacterID != _characterState.character_id)
		{
            CharacterEntity characterEntity = gameWorldController.Model.GetCharacterEntity(_characterState.character_id);
				
			// Clean up the entity 
			characterEntity.RemoveFromGameWorld(gameWorldController);
				
			// Remove the character data from the current game state
			currentGame.SetCharacterById(_characterState.character_id, null);			
				
			base.UndoEvent(gameWorldController);
		}
		else 
		{
			Debug.Log("Ignoring undo CharacterJoinedGame event for primary character_id=" + _characterState.character_id);
		}			
	}	
}