using LitJson;
using UnityEngine;
using System.Collections;

public class GameEvent_CharacterLeftGame : GameEvent 
{
	private CharacterData _characterState;
		
	public GameEvent_CharacterLeftGame() : base()
	{	
		EventType = GameEvent.eEventType.character_left_game;
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
		
	/*protected override void appendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);
			
		parameters["character_state"] = _characterState.ToObject();
	}*/		
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
		return base.ToChatString(gameWorldController) + _characterState.character_name + " left the game";
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{
		GameData currentGame = gameWorldController.Model.CurrentGame;
			
		if (currentGame.CharacterID != _characterState.character_id)
		{
			base.ApplyEvent(gameWorldController, onComplete);
				
			// Clean up the entity 
			{
				CharacterEntity characterEntity = gameWorldController.Model.GetCharacterEntity(_characterState.character_id);
					
				characterEntity.RemoveFromGameWorld(gameWorldController);
			}
				
			// Remove the character data from the current game state
			currentGame.SetCharacterById(_characterState.character_id, null);		
		}
		else 
		{
			Debug.Log("Ignoring apply CharacterLeftGame event for primary character_id=" + _characterState.character_id);
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
			// Add the character state back into to the current game state
			gameWorldController.Model.CurrentGame.SetCharacterById(_characterState.character_id, _characterState);
				
			// Create a new character entity using the character data just set in the game state
			{
                CharacterEntity characterEntity = new CharacterEntity(_characterState.character_id);
					
				characterEntity.AddToGameWorld(gameWorldController);
			}
				
			base.UndoEvent(gameWorldController);
		}
		else 
		{
			Debug.Log("Ignoring undo CharacterLeftGame event for primary character_id=" + _characterState.character_id);
		}
	}				
}