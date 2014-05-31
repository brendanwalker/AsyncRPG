using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncRPGSharedLib.Environment.AI.Behavior
{
    // Combat Behaviors
    class MobCombatBehavior : MobBehavior
    {
        public MobCombatBehavior(MobBehavior parent) :
            base(parent)
        {
            m_children = new MobBehavior[] {
                new MobRepairFriendBehavior(this),
                new MobGiveEnergyToFriendBehavior(this),
                new MobSpawnHelperBotBehavior(this),
                new MobAttackEnemyBehavior(this)
            };
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }

    class MobRepairFriendBehavior : MobBehavior
    {
        public MobRepairFriendBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }

    class MobGiveEnergyToFriendBehavior : MobBehavior
    {
        public MobGiveEnergyToFriendBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }

    class MobAttackEnemyBehavior : MobBehavior
    {
        public MobAttackEnemyBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }

    class MobSpawnHelperBotBehavior : MobBehavior
    {
        public MobSpawnHelperBotBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }
}
