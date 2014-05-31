using UnityEngine;
using System.Collections;
using LitJson;
using System;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

public class GameEvent_EnergyTankHacked : GameEvent
{
    public int EnergyTankId { get; private set; }
    public GameConstants.eFaction EnergyTankFaction { get; private set; }
    public int HackerId { get; private set; }
    public GameConstants.eFaction HackerFaction { get; private set; }

    public GameEvent_EnergyTankHacked()
        : base()
    {
        EventType = GameEvent.eEventType.energy_tank_hacked;
        EnergyTankId = -1;
        EnergyTankFaction = GameConstants.eFaction.neutral;
        HackerId = -1;
        HackerFaction = GameConstants.eFaction.neutral;
    }

    protected override void ParseParameters(JsonData parameters)
    {
        base.ParseParameters(parameters);

        EnergyTankId = JsonUtilities.ParseInt(parameters, "energy_tank_id");
        EnergyTankFaction = JsonUtilities.ParseEnum<GameConstants.eFaction>(parameters, "energy_tank_faction");
        HackerId = JsonUtilities.ParseInt(parameters, "hacker_id");
        HackerFaction = JsonUtilities.ParseEnum<GameConstants.eFaction>(parameters, "hacker_faction");
    }

    protected override void AppendParameters(JsonData parameters)
    {
        base.AppendParameters(parameters);

        parameters["energy_tank_id"] = EnergyTankId;
        parameters["energy_tank_faction"] = (int)EnergyTankFaction;
        parameters["hacker_id"] = HackerId;
        parameters["hacker_faction"] = (int)HackerFaction;
    }

    public override string ToChatString(GameWorldController gameWorldController)
    {
        string hackerName = "";

        if (HackerFaction == GameConstants.eFaction.ai)
        {
            MobData mobData = gameWorldController.Model.GetMobData(HackerId);
            MobType mobType = MobTypeManager.GetMobTypeByName(mobData.mob_type_name);

            hackerName = mobType.Name;
        }
        else if (HackerFaction == GameConstants.eFaction.player)
        {
            hackerName = gameWorldController.Model.GetCharacterData(HackerId).character_name;
        }

        return base.ToChatString(gameWorldController) + " Energy Tank hacked by " + hackerName;
    }

    public override void ApplyEvent(GameWorldController gameWorldController, GameEvent.OnEventCompleteDelegate onComplete)
    {
        base.ApplyEvent(gameWorldController, onComplete);

        EnergyTankEntity energyTankEntity = gameWorldController.Model.GetEnergyTankEntity(EnergyTankId);

        energyTankEntity.Ownership = EnergyTankFaction;

        // Completes immediately
        if (onComplete != null)
        {
            onComplete();
        }
    }

    public override void UndoEvent(GameWorldController gameWorldController)
    {
        EnergyTankEntity energyTankEntity = gameWorldController.Model.GetEnergyTankEntity(EnergyTankId);

        // AI is un-hacking
        if (HackerFaction == GameConstants.eFaction.ai)
        {
            switch (EnergyTankFaction)
            {
                case GameConstants.eFaction.ai:
                    energyTankEntity.Ownership = GameConstants.eFaction.neutral;
                    break;
                case GameConstants.eFaction.neutral:
                    energyTankEntity.Ownership = GameConstants.eFaction.player;
                    break;
                case GameConstants.eFaction.player:
                    energyTankEntity.Ownership = GameConstants.eFaction.player;
                    break;
            }
        }
        // Player is un-hacking
        else if (HackerFaction == GameConstants.eFaction.player)
        {
            switch (EnergyTankFaction)
            {
                case GameConstants.eFaction.player:
                    energyTankEntity.Ownership = GameConstants.eFaction.neutral;
                    break;
                case GameConstants.eFaction.neutral:
                    energyTankEntity.Ownership = GameConstants.eFaction.ai;
                    break;
                case GameConstants.eFaction.ai:
                    energyTankEntity.Ownership = GameConstants.eFaction.ai;
                    break;
            }
        }

        base.UndoEvent(gameWorldController);
    }
}