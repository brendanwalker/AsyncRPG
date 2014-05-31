using UnityEngine;
using System.Collections;
using LitJson;
using System;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

public class GameEvent_EnergyTankDrained : GameEvent 
{		
    public int EnergyTankId { get; private set; }
    public int DrainerId { get; private set; }
    public GameConstants.eFaction DrainerFaction { get; private set; }
    public int EnergyDrained { get; private set; }
		
	public GameEvent_EnergyTankDrained() : base()
	{		
		EventType = GameEvent.eEventType.energy_tank_drained;
		EnergyTankId = -1;
		DrainerId = -1;
		DrainerFaction = GameConstants.eFaction.neutral;
	}	
			
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);
			
		EnergyTankId = JsonUtilities.ParseInt(parameters, "energy_tank_id");
		DrainerId = JsonUtilities.ParseInt(parameters, "drainer_id");
		DrainerFaction = JsonUtilities.ParseEnum<GameConstants.eFaction>(parameters, "drainer_faction");
		EnergyDrained = JsonUtilities.ParseInt(parameters, "energy_drained");
	}
		
	protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);
			
		parameters["energy_tank_id"] = EnergyTankId;
		parameters["drainer_id"] = DrainerId;
		parameters["drainer_faction"] = (int)DrainerFaction;
		parameters["energy_drained"] = EnergyDrained;
	}	
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
		string drainerName = "";
			
		if (DrainerFaction == GameConstants.eFaction.ai)
		{
			MobData mobData= gameWorldController.Model.GetMobData(DrainerId);
			MobType mobType= MobTypeManager.GetMobTypeByName(mobData.mob_type_name);				
				
			drainerName = mobType.Name;
		}
		else if (DrainerFaction == GameConstants.eFaction.player)
		{
			drainerName = gameWorldController.Model.GetCharacterData(DrainerId).character_name;			
		}
			
		return base.ToChatString(gameWorldController) + " Energy Tank drained by "+drainerName;
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);
			
		EnergyTankEntity energyTankEntity = gameWorldController.Model.GetEnergyTankEntity(EnergyTankId);
			
		energyTankEntity.Energy = (uint)Math.Max((int)energyTankEntity.Energy - EnergyDrained, 0);
			
		if (DrainerFaction == GameConstants.eFaction.ai)
		{
			MobEntity mobEntity = gameWorldController.Model.GetMobEntity(DrainerId);
				
			mobEntity.Energy = mobEntity.Energy + EnergyDrained;
		}
		else if (DrainerFaction == GameConstants.eFaction.player)
		{
			CharacterEntity characterEntity = gameWorldController.Model.GetCharacterEntity(DrainerId);
				
			characterEntity.Energy = characterEntity.Energy + EnergyDrained;
		}			
			
		// Completes immediately
		if (onComplete != null)
		{
			onComplete();
		}
	}

	public override void UndoEvent(GameWorldController gameWorldController)
	{
		EnergyTankEntity energyTankEntity = gameWorldController.Model.GetEnergyTankEntity(EnergyTankId);
			
		energyTankEntity.Energy = energyTankEntity.Energy + (uint)EnergyDrained;
			
		if (DrainerFaction == GameConstants.eFaction.ai)
		{
			MobEntity mobEntity = gameWorldController.Model.GetMobEntity(DrainerId);
				
			mobEntity.Energy = mobEntity.Energy - EnergyDrained;
		}
		else if (DrainerFaction == GameConstants.eFaction.player)
		{
            CharacterEntity characterEntity = gameWorldController.Model.GetCharacterEntity(DrainerId);
				
			characterEntity.Energy = characterEntity.Energy - EnergyDrained;
		}				
			
		base.UndoEvent(gameWorldController);
	}			
}