using UnityEngine;
using System.Collections;
using LitJson;

public class GameEvent_MobSpawned : GameEvent 
{
	private MobData m_mobState;
		
	public GameEvent_MobSpawned() : base()
	{		
		EventType = GameEvent.eEventType.mob_spawned;
		m_mobState = new MobData();			
	}
		
	public MobData MobState 
	{
		get { return m_mobState; }
	}
		
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);
			
		m_mobState = MobData.FromObject( parameters["mob_state"] );
	}	
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
		MobType mobType = MobTypeManager.GetMobTypeByName(m_mobState.mob_type_name);
			
		return base.ToChatString(gameWorldController) + mobType.Name + " spawned";
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{			
		RoomData currentRoom = gameWorldController.Model.CurrentGame.CurrentRoom;
			
		base.ApplyEvent(gameWorldController, onComplete);			
			
		if (currentRoom.RoomKey.Equals(m_mobState.CurrentRoomKey))
		{
			// Add the mob state to the current game state
			currentRoom.SetMobById(m_mobState.mob_id, m_mobState);
							
			// Create a new mob entity using the mob data just set in the game state
			{
				MobEntity mobEntity = new MobEntity(m_mobState.mob_id);
					
				mobEntity.AddToGameWorld(gameWorldController);
			}
		}
			
		// Completes immediately
		if (onComplete != null)
		{
			onComplete();
		}		
	}

	public override void UndoEvent(GameWorldController gameWorldController)
	{
		RoomData currentRoom = gameWorldController.Model.CurrentGame.CurrentRoom;
			
		if (currentRoom.RoomKey.Equals(m_mobState.CurrentRoomKey))
		{
            MobEntity mobEntity = gameWorldController.Model.GetMobEntity(m_mobState.mob_id);
				
			// Clean up the entity 
			mobEntity.RemoveFromGameWorld(gameWorldController);
				
			// Remove the mob data from the current game state
			currentRoom.SetMobById(m_mobState.mob_id, null);		
		}
			
		base.UndoEvent(gameWorldController);
	}			
}