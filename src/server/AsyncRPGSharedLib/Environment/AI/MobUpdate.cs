using System;
using System.Collections.Generic;
using System.Linq;
using AsyncRPGSharedLib.RequestProcessors;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;

namespace AsyncRPGSharedLib.Environment.AI
{
    public class MobUpdateContext
    {
        public AIMoveRequestProcessor moveRequest;
        public Mob mob;
        public EntityPath path;
        public List<Mob> otherMobs;
        public List<GameEventParameters> output_game_events;

        public MobUpdateContext(
            AIMoveRequestProcessor moveRequest,
            Mob mob)
        {
            this.moveRequest = moveRequest;
            this.otherMobs = moveRequest.MobContexts.Where(c => c.mob.ID != mob.ID).Select(c => c.mob).ToList();
            this.mob = mob;
            this.path = null;

            output_game_events = new List<GameEventParameters>();
        }

        public void ComputeMove()
        {
            mob.AIState.perception_data.Update(this);
            mob.AIState.behavior_data.Update(this);
            mob.IsDirty = true;
        }

        public bool MoveMob(
            Point3d targetPosition)
        {
            bool success= false;

            if (path == null)
            {
                PathComputer pathComputer = new PathComputer();
                
                success =
                    pathComputer.BlockingPathRequest(
                        moveRequest.Room.runtime_nav_mesh,
                        moveRequest.Room.room_key,
                        new Point3d(mob.Position),
                        new Point3d(targetPosition));

                if (success)
                {
                    RoomKey roomKey = moveRequest.Room.room_key;
                    PathStep lastPathStep = pathComputer.FinalPath[pathComputer.FinalPath.Count - 1];
                    PathStep secondLastPathStep = pathComputer.FinalPath[pathComputer.FinalPath.Count - 2];
                    Vector3d lastPathHeading = lastPathStep.StepPoint - secondLastPathStep.StepPoint;
                    float targetAngle = MathConstants.GetAngleForVector(lastPathHeading);

                    path =
                        new EntityPath()
                        {
                            entity_id = mob.ID,
                            path = pathComputer.FinalPath
                        };

                    // Post an event that we moved
                    output_game_events.Add(
                        new GameEvent_MobMoved()
                        {
                            mob_id = mob.ID,
                            room_x = roomKey.x,
                            room_y = roomKey.y,
                            room_z = roomKey.z,
                            from_x = mob.Position.x,
                            from_y = mob.Position.y,
                            from_z = mob.Position.z,
                            from_angle = mob.Angle,
                            to_x = targetPosition.x,
                            to_y = targetPosition.y,
                            to_z = targetPosition.z,
                            to_angle = targetAngle
                        });

                    // Update the mob position and facing
                    mob.Position = targetPosition;
                    mob.Angle = targetAngle;

                    // TODO: Update the mob energy based on the distance traveled     
                }
            }

            return success;
        }

        public bool MobDrainEnergyTank(EnergyTank energyTank, int desiredDrainAmount)
        {
            bool success = true;

            float min_distance_squared= WorldConstants.ROOM_TILE_SIZE*WorldConstants.ROOM_TILE_SIZE;

            // Move to the energy tank first if it's too far away
            if (Point3d.DistanceSquared(mob.Position, energyTank.Position) > min_distance_squared)
            {
                success = MoveMob(energyTank.Position);
            }

            if (success)
            {
                int energyDrained = Math.Min(energyTank.Energy, desiredDrainAmount);

                // Drain the energy from the tank
                energyTank.Energy -= energyDrained;

                // Give energy to the mob
                mob.Energy = Math.Min(mob.Energy + energyDrained, mob.MobType.MaxEnergy);

                // Post an event that we moved
                output_game_events.Add(
                    new GameEvent_EnergyTankDrained()
                    {
                        energy_tank_id = energyTank.ID,
                        drainer_id = mob.ID,
                        drainer_faction = GameConstants.eFaction.ai,
                        energy_drained = energyDrained
                    });
            }

            return success;
        }

        public bool MobHackEnergyTank(EnergyTank energyTank)
        {
            bool success = true;

            float min_distance_squared = WorldConstants.ROOM_TILE_SIZE * WorldConstants.ROOM_TILE_SIZE;

            // Move to the energy tank first if it's too far away
            if (Point3d.DistanceSquared(mob.Position, energyTank.Position) > min_distance_squared)
            {
                success = MoveMob(energyTank.Position);
            }

            if (success)
            {
                // If the energy tank's faction was player it's now neutral
                // If the energy tank's faction was neutral it's now ai
                GameConstants.eFaction newFaction =
                    (energyTank.Faction == GameConstants.eFaction.player) 
                    ? GameConstants.eFaction.neutral : GameConstants.eFaction.ai;

                // Update the faction on the energy tank
                energyTank.Faction = newFaction;

                // Post an event that we moved
                output_game_events.Add(
                    new GameEvent_EnergyTankHacked()
                    {
                        energy_tank_id = energyTank.ID,
                        energy_tank_faction = newFaction,
                        hacker_id = mob.ID,
                        hacker_faction = GameConstants.eFaction.ai
                    });
            }

            return success;
        }

        public void MobPostDialog(string dialogLine)
        {
            output_game_events.Add(
                new GameEvent_MobDialog()
                {
                    mob_id = mob.ID,
                    dialog = dialogLine
                });
        }
    }

}
