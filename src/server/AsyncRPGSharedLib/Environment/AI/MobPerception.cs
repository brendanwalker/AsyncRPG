using System;
using AsyncRPGSharedLib.RequestProcessors;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using System.Collections.Generic;

namespace AsyncRPGSharedLib.Environment.AI
{
    [Serializable]
    public class EntityProp
    {
        [Serializable]
        public enum ePropStatus
        {
            unacknowledged,
            orphaned,
            assumed,
            acknowledged
        }

        [Serializable]
        public enum ePropVisibilityFlags
        {
            seenAtLeastOnce,
            canSee,
            caughtGlimpse,
            stationary
        }

        // Entity properties
        public int target_object_id; // mob_id or character_id or energy_tank_id
        public double position_x, position_y, position_z;
        public GameConstants.eFaction faction;
        public int energy;
        public int health;
        public double distance;

        // Perception computed properties
        public ePropStatus propStatus;
        public int propStatusTurnCount;
        public double salience;
        public TypedFlags<EntityProp.ePropVisibilityFlags> visibilityFlags;

        public EntityProp()
        {
            this.target_object_id = -1;

            this.position_x = 0.0;
            this.position_y = 0.0;
            this.position_z = 0.0;
            this.faction = GameConstants.eFaction.neutral;
            this.energy = 0;
            this.health = 0;
            this.distance = 0.0f;

            this.propStatus = EntityProp.ePropStatus.unacknowledged;
            this.propStatusTurnCount = 0;
            this.salience = 0.0f;
            this.visibilityFlags = new TypedFlags<EntityProp.ePropVisibilityFlags>();
        }

        public EntityProp(
            Mob ownerMob,
            IEnvironmentEntity entity)
        {
            this.target_object_id = entity.ID;
            RefeshEntityProperties(ownerMob, entity);

            this.propStatus = EntityProp.ePropStatus.unacknowledged;
            this.propStatusTurnCount = 0;
            this.salience = 0.0f;
            this.visibilityFlags = new TypedFlags<EntityProp.ePropVisibilityFlags>();
        }

        public void RefeshEntityProperties(
            Mob ownerMob,
            IEnvironmentEntity entity)
        {
            this.position_x = entity.Position.x;
            this.position_y = entity.Position.y;
            this.position_z = entity.Position.z;
            this.faction = entity.Faction;
            this.energy = entity.Energy;
            this.health = entity.Health;
            this.distance = Point2d.Distance(ownerMob.Position, entity.Position);
        }

        public Point3d GetPosition()
        {
            return new Point3d((float)position_x, (float)position_y, (float)position_z);
        }
    }

    [Serializable]
    public class MobAIPerceptionState
    {
        public List<EntityProp> player_props;
        public List<EntityProp> ai_props;
        public List<EntityProp> energy_tank_props;

        private int friendlyTargetPropIndex;
        private int enemyTargetPropIndex;
        private int energyTankTargetPropIndex;

        public MobAIPerceptionState()
        {
            player_props = new List<EntityProp>();
            ai_props = new List<EntityProp>();
            energy_tank_props = new List<EntityProp>();

            friendlyTargetPropIndex = -1;
            enemyTargetPropIndex = -1;
            energyTankTargetPropIndex = -1;
        }

        public EntityProp GetFriendlyTargetProp()
        {
            return (friendlyTargetPropIndex != -1) ? ai_props[friendlyTargetPropIndex] : null;
        }

        public EntityProp GetPlayerTargetProp()
        {
            return (enemyTargetPropIndex != -1) ? player_props[enemyTargetPropIndex] : null;
        }

        public EntityProp GetEnergyTankTargetProp()
        {
            return (energyTankTargetPropIndex != -1) ? energy_tank_props[energyTankTargetPropIndex] : null;
        }

        public void Update(
            MobUpdateContext context)
        {
            // See if there are any new props we should be paying attention to
            PropListsRefresh(context);

            // Update the prop status for each prop based on visibility
            PropsUpdateVisibility(context);

            //TODO: Update the prop status for each prop based on audibility events

            // Compute the new status based on the new visibility / audibility of each props
            PropsUpdateStatus(context);

            // Update salience scores of our targets
            PropsUpdateSalience(context);

            // Pick friendly, enemy, and neutral targets based on salience
            UpdateTargets();
        }

        // -- Top Level Update Functions --
        private void PropListsRefresh(
            MobUpdateContext context)
        {
            // Create props for any Entity that we don't already have a prop for.
            // Remove props for any Entity that no longer exists.
            player_props = MobAIPerceptionState.PropListsRefresh(context.mob, context.moveRequest.Players, player_props);
            ai_props = MobAIPerceptionState.PropListsRefresh(context.mob, context.otherMobs, ai_props);
            energy_tank_props = MobAIPerceptionState.PropListsRefresh(context.mob, context.moveRequest.EnergyTanks, energy_tank_props);
        }

        private void PropsUpdateVisibility(
            MobUpdateContext context)
        {
            UpdatePlayerPropVisibility(context);
            UpdateAIPropVisibility(context);
            UpdateEnergyTankPropVisibility(context);
        }

        private void PropsUpdateStatus(
            MobUpdateContext context)
        {
            Mob ownerMob = context.mob;

            player_props.ForEach(prop => MobAIPerceptionState.UpdatePropStatus(ownerMob, prop));
            ai_props.ForEach(prop => MobAIPerceptionState.UpdatePropStatus(ownerMob, prop));
            energy_tank_props.ForEach(prop => MobAIPerceptionState.UpdatePropStatus(ownerMob, prop));
        }

        private void PropsUpdateSalience(
            MobUpdateContext context)
        {
            Mob ownerMob = context.mob;

            // Compute a salience score for all props
            player_props.ForEach(prop => prop.salience = MobAIPerceptionState.ComputePropSalience(PlayerPropEvaluators, ownerMob, prop));
            ai_props.ForEach(prop => prop.salience = MobAIPerceptionState.ComputePropSalience(AIPropEvaluators, ownerMob, prop));
            energy_tank_props.ForEach(prop => prop.salience = MobAIPerceptionState.ComputePropSalience(EnergyTankPropEvaluators, ownerMob, prop));
        }

        private void UpdateTargets()
        {
            // Compute a target based on the most salient prop
            enemyTargetPropIndex = MobAIPerceptionState.RefreshSalienceTarget(player_props);
            friendlyTargetPropIndex = MobAIPerceptionState.RefreshSalienceTarget(ai_props);
            energyTankTargetPropIndex = MobAIPerceptionState.RefreshSalienceTarget(energy_tank_props);
        }

        // -- Prop List Helpers --
        private static List<EntityProp> PropListsRefresh<TEnvironmentEntity>(
            Mob ownerMob,
            List<TEnvironmentEntity> currentEntities,
            List<EntityProp> oldProps) 
                where TEnvironmentEntity : IEnvironmentEntity
        {
            List<EntityProp> updatedProps = new List<EntityProp>();

            // Mark/Add existing props and Create new props
            foreach (IEnvironmentEntity entity in currentEntities)
            {
                int existingPropIndex = oldProps.FindIndex(prop => prop.target_object_id == entity.ID);
                EntityProp existingProp = (existingPropIndex != -1) ? oldProps[existingPropIndex] : null;

                if (existingProp != null)
                {
                    updatedProps.Add(existingProp);
                }
                else
                {
                    updatedProps.Add(new EntityProp(ownerMob, entity));
                }
            }

            return updatedProps;
        }

        // -- Prop Visibility Helpers --
        private void UpdatePlayerPropVisibility(
            MobUpdateContext context)
        {
            Mob ownerMob = context.mob;
            NavMesh navMesh = context.moveRequest.Room.runtime_nav_mesh;

            foreach (EntityProp prop in player_props)
            {
                Point3d oldPropPosition = prop.GetPosition();

                // Find the player associated with the player id
                Player player= context.moveRequest.Players.Find(p => p.ID == prop.target_object_id);
                
                // Can the mob see the player at their current location
                bool canSee= CanMobSeePoint(navMesh, ownerMob, player.Position);

                TypedFlags<EntityProp.ePropVisibilityFlags> oldVisibilityFlags= 
                    new TypedFlags<EntityProp.ePropVisibilityFlags>(prop.visibilityFlags);

                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.canSee, canSee);
                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.caughtGlimpse, canSee);

                if (canSee)
                {
                    // If so, we get to pull the entity properties
                    prop.RefeshEntityProperties(ownerMob, player);
                    prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.seenAtLeastOnce, true);
                }
                else
                {
                    EntityPath entityPath= context.moveRequest.PlayerPaths.Find(p => p.entity_id == prop.target_object_id);

                    if (entityPath != null)
                    {
                        // Find the last place we saw the player along their path, if at all
                        for (int pathStepIndex = entityPath.path.Count - 1; pathStepIndex >= 0; pathStepIndex--)
                        {
                            PathStep pathStep= entityPath.path[pathStepIndex];

                            if (CanMobSeePoint(navMesh, ownerMob, pathStep.StepPoint))
                            {
                                prop.position_x = pathStep.StepPoint.x;
                                prop.position_y = pathStep.StepPoint.y;
                                prop.position_z = pathStep.StepPoint.z;
                                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.caughtGlimpse, true);
                                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.seenAtLeastOnce, true);
                                break;
                            }
                        }                            
                    }
                }

                // Post an event if we just spotted a player
                if (!oldVisibilityFlags.Test(EntityProp.ePropVisibilityFlags.canSee) &&
                    (prop.visibilityFlags.Test(EntityProp.ePropVisibilityFlags.canSee) ||
                     prop.visibilityFlags.Test(EntityProp.ePropVisibilityFlags.caughtGlimpse)))
                {
                    context.output_game_events.Add(
                        new GameEvent_MobPlayerPropSpotted()
                        {
                            mob_id = ownerMob.ID,
                            character_id= prop.target_object_id,
                            x = prop.position_x,
                            y = prop.position_y,
                            z = prop.position_z
                        });
                }
                // Post an event if we just lost sight
                else if (oldVisibilityFlags.Test(EntityProp.ePropVisibilityFlags.canSee) &&
                        !prop.visibilityFlags.Test(EntityProp.ePropVisibilityFlags.canSee))
                {
                    context.output_game_events.Add(
                        new GameEvent_MobPlayerPropLostTrack()
                        {
                            mob_id = ownerMob.ID,
                            character_id = prop.target_object_id,
                            x = prop.position_x,
                            y = prop.position_y,
                            z = prop.position_z
                        });
                }
            }
        }

        private void UpdateAIPropVisibility(
            MobUpdateContext context)
        {
            Mob ownerMob = context.mob;
            NavMesh navMesh = context.moveRequest.Room.runtime_nav_mesh;

            foreach (EntityProp prop in ai_props)
            {
                Point3d oldPropPosition = prop.GetPosition();

                // Find the player associated with the player id
                MobUpdateContext otherMobContext = context.moveRequest.MobContexts.Find(m => m.mob.ID == prop.target_object_id);
                Mob otherMob= otherMobContext.mob;

                // Can the mob see the other mob at their current location
                bool canSee = CanMobSeePoint(navMesh, ownerMob, otherMob.Position);

                TypedFlags<EntityProp.ePropVisibilityFlags> oldVisibilityFlags = 
                    new TypedFlags<EntityProp.ePropVisibilityFlags>(prop.visibilityFlags);

                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.canSee, canSee);
                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.caughtGlimpse, canSee);

                if (canSee)
                {
                    // If so, we get to pull the entity properties
                    prop.RefeshEntityProperties(ownerMob, otherMob);
                    prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.seenAtLeastOnce, true);
                }
                else
                {
                    EntityPath entityPath = otherMobContext.path;

                    if (entityPath != null)
                    {
                        // Find the last place we saw the player along their path, if at all
                        for (int pathStepIndex = entityPath.path.Count - 1; pathStepIndex >= 0; pathStepIndex--)
                        {
                            PathStep pathStep = entityPath.path[pathStepIndex];

                            if (CanMobSeePoint(navMesh, ownerMob, pathStep.StepPoint))
                            {
                                prop.position_x = pathStep.StepPoint.x;
                                prop.position_y = pathStep.StepPoint.y;
                                prop.position_z = pathStep.StepPoint.z;
                                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.caughtGlimpse, true);
                                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.seenAtLeastOnce, true);
                                break;
                            }
                        }
                    }
                }

                // Post an event if we just spotted another mob that we've never seen before
                if (!oldVisibilityFlags.Test(EntityProp.ePropVisibilityFlags.canSee) &&
                    !oldVisibilityFlags.Test(EntityProp.ePropVisibilityFlags.seenAtLeastOnce) &&
                    (prop.visibilityFlags.Test(EntityProp.ePropVisibilityFlags.canSee) ||
                     prop.visibilityFlags.Test(EntityProp.ePropVisibilityFlags.caughtGlimpse)))
                {
                    context.output_game_events.Add(
                        new GameEvent_MobAIPropSpotted()
                        {
                            mob_id = ownerMob.ID,
                            spotted_mob_id = prop.target_object_id,
                            x = prop.position_x,
                            y = prop.position_y,
                            z = prop.position_z
                        });
                }
            }
        }

        private void UpdateEnergyTankPropVisibility(
            MobUpdateContext context)
        {
            Mob ownerMob = context.mob;
            NavMesh navMesh = context.moveRequest.Room.runtime_nav_mesh;

            foreach (EntityProp prop in energy_tank_props)
            {
                Point3d oldPropPosition = prop.GetPosition();

                // Find the energy tank associated with the player id
                EnergyTank energyTank = context.moveRequest.EnergyTanks.Find(e => e.ID == prop.target_object_id);

                // Can the mob see the player at their current location
                bool canSee = CanMobSeePoint(navMesh, ownerMob, energyTank.Position);

                TypedFlags<EntityProp.ePropVisibilityFlags> oldVisibilityFlags = 
                    new TypedFlags<EntityProp.ePropVisibilityFlags>(prop.visibilityFlags);

                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.canSee, canSee);
                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.caughtGlimpse, canSee);
                prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.stationary, true);

                if (canSee)
                {
                    // If so, we get to pull the entity properties
                    prop.RefeshEntityProperties(ownerMob, energyTank);
                    prop.visibilityFlags.Set(EntityProp.ePropVisibilityFlags.seenAtLeastOnce, true);
                }

                // Update the status of the prop based on the current visibility
                UpdatePropStatus(ownerMob, prop);

                // Post an event if we just spotted an energy that we've never seen before
                if (!oldVisibilityFlags.Test(EntityProp.ePropVisibilityFlags.canSee) &&
                    !oldVisibilityFlags.Test(EntityProp.ePropVisibilityFlags.seenAtLeastOnce) &&
                    (prop.visibilityFlags.Test(EntityProp.ePropVisibilityFlags.canSee) ||
                     prop.visibilityFlags.Test(EntityProp.ePropVisibilityFlags.caughtGlimpse)))
                {
                    context.output_game_events.Add(
                        new GameEvent_MobEnergyTankPropSpotted()
                        {
                            mob_id = ownerMob.ID,
                            energy_tank_id = prop.target_object_id
                        });
                }
            }
        }

        public static bool CanMobSeeProp(
            NavMesh navMesh,
            Mob mob,
            EntityProp prop)
        {
            return CanMobSeePoint(navMesh, mob, prop.GetPosition());
        }

        public static bool CanMobSeePoint(
            NavMesh navMesh,
            Mob mob, 
            Point3d point)
        {
            bool canSee = false;

            if (IsPointInMobVisionCone(mob, point))
            {
                NavRef mobNavRef = navMesh.ComputeNavRefAtPoint(mob.Position);
                NavRef propNavRef = navMesh.ComputeNavRefAtPoint(point);

                canSee = navMesh.NavRefCanSeeOtherNavRef(mobNavRef, propNavRef);
            }

            return canSee;
        }

        public static bool IsPropInMobVisionCone(
            Mob mob,
            EntityProp prop)
        {
            return IsPointInMobVisionCone(mob, prop.GetPosition());
        }

        public static bool IsPointInMobVisionCone(
            Mob mob,
            Point3d point)
        {
            MobType mobType= mob.MobType;
            
            Vector2d mobToPoint = (point - mob.Position).ToVector2d();
            Vector2d facingVector = MathConstants.GetUnitVectorForAngle(mob.Angle);
            
            float cosHalfAngle = (float)Math.Cos(mob.MobType.VisionCone.GetHalfAngle());
            float cosAngleToProp = facingVector.Dot(mobToPoint);
            float pointDistanceSquared = Point2d.DistanceSquared(point, mob.Position);
            float minVisionCircleDistanceSquared = WorldConstants.ROOM_TILE_SIZE * WorldConstants.ROOM_TILE_SIZE;
            float minVisionConeDistanceSquared = (float)(mobType.VisionCone.distance * mobType.VisionCone.distance);

            bool isInVisionCone =
                (pointDistanceSquared <= minVisionCircleDistanceSquared) ||
                ((cosAngleToProp > cosHalfAngle) && (pointDistanceSquared <= minVisionConeDistanceSquared));

            return isInVisionCone;
        }

        // -- Prop Status Helpers --
        static private void UpdatePropStatus(
            Mob mob,
            EntityProp prop)
        {
            MobType mobType = mob.MobType;
            EntityProp.ePropStatus newStatus = prop.propStatus;

            bool canSee = prop.visibilityFlags.Test(EntityProp.ePropVisibilityFlags.canSee);
            bool caughtGlipse = prop.visibilityFlags.Test(EntityProp.ePropVisibilityFlags.caughtGlimpse);
            bool stationary = prop.visibilityFlags.Test(EntityProp.ePropVisibilityFlags.stationary);

            switch (prop.propStatus)
            {
                case EntityProp.ePropStatus.unacknowledged:
                    if (canSee)
                    {
                        newStatus = EntityProp.ePropStatus.acknowledged;
                    }
                    else
                    {
                        if (caughtGlipse)
                        {
                            newStatus = stationary ? EntityProp.ePropStatus.assumed : EntityProp.ePropStatus.unacknowledged;
                        }
                        else
                        {
                            newStatus = EntityProp.ePropStatus.unacknowledged;
                        }
                    }
                    break;
                case EntityProp.ePropStatus.orphaned:
                    if (canSee)
                    {
                        newStatus = EntityProp.ePropStatus.acknowledged;
                    }
                    else
                    {
                        if (caughtGlipse)
                        {
                            // Still orphaned, but reset the orphan timeout
                            newStatus = EntityProp.ePropStatus.orphaned;
                            prop.propStatusTurnCount = 0;
                        }
                        else
                        {
                            if (prop.propStatusTurnCount >= mobType.Perception.orphan_turn_timeout)
                            {
                                newStatus = EntityProp.ePropStatus.unacknowledged;
                            }
                            else
                            {
                                newStatus = EntityProp.ePropStatus.orphaned;
                            }
                        }
                    }
                    break;
                case EntityProp.ePropStatus.assumed:
                    // Don't care about catching glimpse because we assume we know where the prop is
                    newStatus = canSee ? EntityProp.ePropStatus.acknowledged : EntityProp.ePropStatus.assumed;
                    break;
                case EntityProp.ePropStatus.acknowledged:
                    if (canSee)
                    {
                        newStatus = EntityProp.ePropStatus.acknowledged;
                    }
                    else
                    {
                        // Don't care about catching glimpse because we're going to orphaned or assumed regardless
                        newStatus = stationary ? EntityProp.ePropStatus.assumed : EntityProp.ePropStatus.orphaned;
                    }
                    break;
            }

            if (newStatus != prop.propStatus)
            {
                prop.propStatus = newStatus;
                prop.propStatusTurnCount = 0;
            }
            else
            {
                prop.propStatusTurnCount++;
            }
        }

        // -- Prop Salience Helpers --
        private static float ComputePropSalience(
            SalienceEvaluatorSet evaluatorSet,
            Mob ownerMob,
            EntityProp prop)
        {
            MobType mobType = ownerMob.MobType;
            float netSalienceScore = 0.0f;

            // Any prop that we don't know about automatically gets a score of zero
            if (prop.propStatus != EntityProp.ePropStatus.unacknowledged)
            {
                float weightTotal = 0.0f;

                // Distance
                {
                    float distanceScore = evaluatorSet.DistanceEvaluator(ownerMob, prop);

                    netSalienceScore += distanceScore * (float)mobType.Perception.saliance_distance_weight;
                    weightTotal += (float)mobType.Perception.saliance_distance_weight;
                }

                // Energy
                {
                    float energyScore = evaluatorSet.EnergyEvaluator(ownerMob, prop);

                    netSalienceScore += energyScore * (float)mobType.Perception.saliance_energy_weight;
                    weightTotal += (float)mobType.Perception.saliance_energy_weight;
                }

                // Health
                {
                    float healthScore = evaluatorSet.HealthEvaluator(ownerMob, prop);

                    netSalienceScore += healthScore * (float)mobType.Perception.saliance_health_weight;
                    weightTotal += (float)mobType.Perception.saliance_health_weight;
                }

                // Status
                {
                    float statusScore = evaluatorSet.StatusEvaluator(ownerMob, prop);

                    netSalienceScore += statusScore * (float)mobType.Perception.saliance_status_weight;
                    weightTotal += (float)mobType.Perception.saliance_status_weight;
                }

                if (weightTotal > 0.0f)
                {
                    netSalienceScore /= weightTotal;
                }
            }

            return netSalienceScore;
        }

        private class SalienceEvaluatorSet
        {
            public delegate float SalienceEvaluator(Mob ownerMob, EntityProp prop);

            public SalienceEvaluator DistanceEvaluator;
            public SalienceEvaluator EnergyEvaluator;
            public SalienceEvaluator HealthEvaluator;
            public SalienceEvaluator StatusEvaluator;
        }

        private static SalienceEvaluatorSet PlayerPropEvaluators = new SalienceEvaluatorSet()
        {
            DistanceEvaluator = SaliencePreferCloserDistance,
            EnergyEvaluator = SaliencePreferLowerEnergy,
            HealthEvaluator = SaliencePreferLowerHealth,
            StatusEvaluator = SaliencePreferMoreVisibleStatus
        };

        private static SalienceEvaluatorSet AIPropEvaluators = new SalienceEvaluatorSet()
        {
            DistanceEvaluator = SaliencePreferCloserDistance,
            EnergyEvaluator = SaliencePreferLowerEnergy,
            HealthEvaluator = SaliencePreferLowerHealth,
            StatusEvaluator = SaliencePreferMoreVisibleStatus
        };

        private static SalienceEvaluatorSet EnergyTankPropEvaluators = new SalienceEvaluatorSet()
        {
            DistanceEvaluator = SaliencePreferCloserDistance,
            EnergyEvaluator = SaliencePreferHigherEnergy,
            HealthEvaluator = SaliencePreferHigherHealth,
            StatusEvaluator = SaliencePreferMoreVisibleStatus
        };

        private static float SaliencePreferCloserDistance(
            Mob ownerMob,
            EntityProp prop)
        {
            double maxDistance = ownerMob.MobType.VisionCone.distance;

            // judge based off of my max vision distance
            return (float)Math.Max(Math.Min(1.0 - (prop.distance / maxDistance), 1.0), 0.0);
        }

        private static float SaliencePreferHigherEnergy(
            Mob ownerMob,
            EntityProp prop)
        {
            float maxEnergy = ownerMob.MobType.MaxEnergy; // judge based off of my max energy

            return Math.Max(Math.Min((float)prop.energy / maxEnergy, 1.0f), 0.0f);
        }

        private static float SaliencePreferLowerEnergy(
            Mob ownerMob,
            EntityProp prop)
        {
            float maxEnergy = ownerMob.MobType.MaxEnergy; // judge based off of my max energy

            return Math.Max(Math.Min(1.0f - ((float)prop.energy / maxEnergy), 1.0f), 0.0f);
        }

        private static float SaliencePreferHigherHealth(
            Mob ownerMob,
            EntityProp prop)
        {
            float maxHealth = ownerMob.MobType.MaxHealth; // judge based off of my max energy

            return Math.Max(Math.Min((float)prop.health / maxHealth, 1.0f), 0.0f);
        }

        private static float SaliencePreferLowerHealth(
            Mob ownerMob,
            EntityProp prop)
        {
            float maxHealth = ownerMob.MobType.MaxHealth; // judge based off of my max energy

            return Math.Max(Math.Min(1.0f - ((float)prop.health / maxHealth), 1.0f), 0.0f);
        }

        private static float SaliencePreferMoreVisibleStatus(
            Mob ownerMob,
            EntityProp prop)
        {
            float statusScore = 0.0f;

            switch (prop.propStatus)
            {
                case EntityProp.ePropStatus.acknowledged:
                    statusScore = 1.0f;
                    break;
                case EntityProp.ePropStatus.assumed:
                    statusScore = 0.75f;
                    break;
                case EntityProp.ePropStatus.orphaned:
                    statusScore = 0.5f;
                    break;
            }

            return statusScore;
        }

        // -- Prop Target Helpers --
        public static int RefreshSalienceTarget(
            List<EntityProp> props)
        {
            int bestTargetPropIndex = -1;
            double bestSalianceScore = -1.0;

            for (int propIndex = 0; propIndex < props.Count; propIndex++)
            {
                EntityProp prop = props[propIndex];

                if (prop.propStatus != EntityProp.ePropStatus.unacknowledged &&
                    prop.salience > bestSalianceScore)
                {
                    bestTargetPropIndex = propIndex;
                    bestSalianceScore = prop.salience;
                }
            }

            return bestTargetPropIndex;
        }
    }
}
