using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncRPGSharedLib.Environment.AI.Behavior
{
    // Self Preservation Behaviors
    class MobSelfPreserveBehavior : MobBehavior
    {
        public MobSelfPreserveBehavior(MobBehavior parent) :
            base(parent)
        {
            m_children = new MobBehavior[] {
                new MobTakeEnergyFromTankBehavior(this),
                new MobRequestRepairBehavior(this),
                new MobRequestEnergyBehavior(this),
                new MobFleeBehavior(this)
            };
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }

    class MobTakeEnergyFromTankBehavior : MobBehavior
    {
        public MobTakeEnergyFromTankBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }

    class MobRequestRepairBehavior : MobBehavior
    {
        public MobRequestRepairBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }

    class MobRequestEnergyBehavior : MobBehavior
    {
        public MobRequestEnergyBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }

    class MobFleeBehavior : MobBehavior
    {
        public MobFleeBehavior(MobBehavior parent) :
            base(parent)
        {
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return false;
        }
    }
}
