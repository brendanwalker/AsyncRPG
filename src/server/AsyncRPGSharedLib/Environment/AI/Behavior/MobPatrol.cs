using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Utility;

namespace AsyncRPGSharedLib.Environment.AI.Behavior
{
    // Patrol Behaviors
    class MobPatrolBehavior : MobBehavior
    {
        public MobPatrolBehavior(MobBehavior parent) :
            base(parent)
        {
            m_children = new MobBehavior[] {
                new MobDrainEnergyTankBehavior(this),
                new MobHackEnergyTankBehavior(this),
                new MobAimlessWanderBehavior(this),
            };
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return true;
        }
    }

    class MobDrainEnergyTankBehavior : MobBehavior
    {
        public MobDrainEnergyTankBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            EntityProp energyTankProp = context.mob.AIState.perception_data.GetEnergyTankTargetProp();

            return
                energyTankProp != null &&
                context.mob.MobType.Abilities.energy_tank_drain_per_turn > 0 &&
                // Even if we can't use the energy, we can deny the player from having it
                //energyTankProp.energy < context.mob.MobType.MaxEnergy && 
                energyTankProp.propStatus >= EntityProp.ePropStatus.assumed &&
                energyTankProp.faction == GameConstants.eFaction.ai &&
                energyTankProp.energy > 0;
        }

        public override void Perform(MobUpdateContext context)
        {
            EntityProp energyTankProp = context.mob.AIState.perception_data.GetEnergyTankTargetProp();
            EnergyTank energyTank = context.moveRequest.EnergyTanks.Find(e => e.ID == energyTankProp.target_object_id);

            if (energyTank.Faction == GameConstants.eFaction.ai)
            {
                // We can only drain up to what we can take and whats left in the energy tank
                int drainAmount = 
                    Math.Min(energyTank.Energy, context.mob.MobType.Abilities.energy_tank_drain_per_turn);

                context.MobDrainEnergyTank(energyTank, drainAmount);
                context.MobPostDialog("Sweet Sweet Energy...");
            }
            else
            {
                context.MoveMob(energyTank.Position);
                context.MobPostDialog("What?! Someone hacked this! Grrr...");
            }

            // Update the energy on the prop
            energyTankProp.energy = energyTank.Energy;
        }
    }

    class MobHackEnergyTankBehavior : MobBehavior
    {
        public MobHackEnergyTankBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            EntityProp energyTankProp= context.mob.AIState.perception_data.GetEnergyTankTargetProp();

            return 
                energyTankProp != null &&
                energyTankProp.propStatus >= EntityProp.ePropStatus.assumed &&
                energyTankProp.faction != GameConstants.eFaction.ai && 
                energyTankProp.energy > 0;
        }

        public override void Perform(MobUpdateContext context)
        {
            EntityProp energyTankProp = context.mob.AIState.perception_data.GetEnergyTankTargetProp();
            EnergyTank energyTank = context.moveRequest.EnergyTanks.Find(e => e.ID == energyTankProp.target_object_id);

            if (energyTank.Faction == GameConstants.eFaction.player)
            {
                context.MobHackEnergyTank(energyTank);
                context.MobPostDialog("Which team owns one less energy tank? Team Player!");
            }
            else if (energyTank.Faction == GameConstants.eFaction.neutral)
            {
                context.MobHackEnergyTank(energyTank);
                context.MobPostDialog("Energy Tank claimed for the AI Team!");
            }
            else if (energyTank.Faction == GameConstants.eFaction.ai)
            {
                context.MobHackEnergyTank(energyTank);
                context.MobPostDialog("Oh! I guess we already owned this energy tank");
            }

            // Update the faction on the prop
            energyTankProp.faction = energyTank.Faction;
        }
    }

    class MobAimlessWanderBehavior : MobBehavior
    {
        public MobAimlessWanderBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return true;
        }

        public override void Perform(MobUpdateContext context)
        {
            NavMesh navMesh= context.moveRequest.Room.runtime_nav_mesh;
            float max_range= 5.0f; // Compute max range from mob type
            uint[] navCells = navMesh.ComputeNavCellsInRadius(context.mob.Position, max_range, false);
            Random rng = context.mob.AIState.behavior_data.GetMobRandomNumberGenerator();
            RNGUtilities.DeterministicKnuthShuffle(rng, navCells);
            Point3d targetPosition = new Point3d(context.mob.Position);

            // Pick the first random nav cell in range that we have line of sight to
            foreach (uint navCellIndex in navCells)
            {
                Point3d testTarget = navMesh.ComputeNavCellCenter(navCellIndex);
                bool canSee = navMesh.PointCanSeeOtherPoint(context.mob.Position, testTarget);

                if (canSee)
                {
                    targetPosition = testTarget;
                    break;
                }
            }

            // Compute the side effect of actually moving the mob on the update context
            context.MoveMob(targetPosition);
            context.MobPostDialog("Soo bored...");
        }
    }
}
