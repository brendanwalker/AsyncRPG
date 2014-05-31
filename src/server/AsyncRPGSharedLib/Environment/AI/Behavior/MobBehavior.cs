using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncRPGSharedLib.Environment.AI.Behavior
{
    class MobBehavior
    {
        protected int m_priority;
        protected MobBehavior m_parent;
        protected  MobBehavior[] m_children;

        public MobBehavior(MobBehavior parent)
        {
            m_priority = -1;
            m_parent = parent;
            m_children = null;
        }

        public int Priority
        {
            get { return m_priority; }
            set { m_priority = value; }
        }

        public MobBehavior Parent
        {
            get { return m_parent; }
        }

        public MobBehavior[] Children
        {
            get { return m_children; }
        }

        public bool IsLeafBehavior
        {
            get { return m_children == null || m_children.Length == 0; }
        }

        public bool HasChildren
        {
            get { return m_children != null && m_children.Length > 0; }
        }

        public MobBehavior GetFirstChildOfType(Type behaviorType)
        {
            MobBehavior matchingBehavior = null;

            if (behaviorType == null)
            {
                return null;
            }
            else if (this.GetType() == behaviorType)
            {
                matchingBehavior= this;
            }
            else if (m_children != null)
            {
                foreach (MobBehavior childBehavior in m_children)
                {
                    matchingBehavior= childBehavior.GetFirstChildOfType(behaviorType);

                    if (matchingBehavior != null)
                    {
                        break;
                    }
                }
            }

            return matchingBehavior;
        }

        public MobBehavior GetFirstChildOfName(String behaviorName)
        {
            MobBehavior matchingBehavior = null;

            if (behaviorName == "")
            {
                matchingBehavior = null;
            }
            else if (this.GetType().Name == behaviorName)
            {
                matchingBehavior = this;
            }
            else if (m_children != null)
            {
                foreach (MobBehavior childBehavior in m_children)
                {
                    matchingBehavior = childBehavior.GetFirstChildOfName(behaviorName);

                    if (matchingBehavior != null)
                    {
                        break;
                    }
                }
            }

            return matchingBehavior;
        }


        public virtual bool CanActivate(MobUpdateContext context)
        {
            return true;
        }

        public virtual void Perform(MobUpdateContext context)
        {
            // Nothing to do
        }
    }

    class MobRootBehavior : MobBehavior
    {
        public MobRootBehavior()
            : base(null)
        {
            this.m_children = new MobBehavior[] {
                new MobSelfPreserveBehavior(this),
                new MobCombatBehavior(this),
                new MobSearchBehavior(this),
                new MobPatrolBehavior(this)
            };
        }

        public override bool CanActivate(MobUpdateContext context)
        {
            return true;
        }
    }
}
