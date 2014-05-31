using System;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.Environment.AI.Behavior
{
    // Search Behaviors
    class MobSearchBehavior : MobBehavior
    {
        public MobSearchBehavior(MobBehavior parent) :
            base(parent)
        {
            m_children = new MobBehavior[] {
                new MobInvestigateEnemyStimuli(this),
                new MobInvestigateFriendlyStimuli(this),
            };
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }

    class MobInvestigateFriendlyStimuli : MobBehavior
    {
        public MobInvestigateFriendlyStimuli(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }

    class MobInvestigateEnemyStimuli : MobBehavior
    {
        public MobInvestigateEnemyStimuli(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            MobAIPerceptionState perception= context.mob.AIState.perception_data;
            EntityProp prop= perception.GetPlayerTargetProp();
            bool canActivate = (prop != null && prop.propStatus == EntityProp.ePropStatus.orphaned);

            return canActivate;
        }

        public override void Perform(MobUpdateContext context)
        {
            MobAIPerceptionState perception = context.mob.AIState.perception_data;
            EntityProp prop = perception.GetPlayerTargetProp();

            if (prop.distance >= WorldConstants.ROOM_TILE_SIZE)
            {
                context.MobPostDialog("I though I saw someone over here");
                context.MoveMob(prop.GetPosition());
            }
        }
    }
}
