using UnityEngine;
using System.Collections;
using LitJson;
using System;

public class GameEvent_MobEnergyTankPropSpotted : GameEvent 
{		
    public int MobId { get; private set; }
    public int EnergyTankID { get; private set; }
		
	public GameEvent_MobEnergyTankPropSpotted() : base()
	{		
		EventType = GameEvent.eEventType.mob_energy_tank_prop_spotted;
        EnergyTankID = -1;
	}	
			
	protected override void ParseParameters(JsonData parameters)
	{
		base.ParseParameters(parameters);

        MobId = JsonUtilities.ParseInt(parameters, "mob_id");
        EnergyTankID = JsonUtilities.ParseInt(parameters, "energy_tank_id");
	}
		
	protected override void AppendParameters(JsonData parameters)
	{
		base.AppendParameters(parameters);

        parameters["mob_id"] = MobId;
        parameters["energy_tank_id"] = EnergyTankID;
	}	
		
	public override string ToChatString(GameWorldController gameWorldController)
	{
        MobData mobData = gameWorldController.Model.GetMobData(MobId);
		MobType mobType= MobTypeManager.GetMobTypeByName(mobData.mob_type_name);

        return base.ToChatString(gameWorldController) + mobType.Name + " spotted an energy tank";
	}
		
	public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
	{
		base.ApplyEvent(gameWorldController, onComplete);
			
		//TODO: Spawn "spotted" effect on energy tank
	
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