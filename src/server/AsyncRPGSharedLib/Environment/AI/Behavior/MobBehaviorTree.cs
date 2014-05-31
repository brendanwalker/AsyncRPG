using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncRPGSharedLib.Environment.AI.Behavior
{
    [Serializable]
    public class MobAIBehaviorState
    {
        public string active_behavior_stack = "";
        public int random_seed = 0;

        public MobAIBehaviorState()
        {
            active_behavior_stack = "";
            random_seed = 0;
        }

        public MobAIBehaviorState(MobSpawner spawner)
        {
            active_behavior_stack = "";
            random_seed = (spawner != null) ? spawner.RandomSeed : 0;
        }

        public void Update(
            MobUpdateContext context)
        {
            MobBehaviorTree.GetInstance().Update(context);
        }

        public Random GetMobRandomNumberGenerator()
        {
            Random rng = new Random(random_seed);

            random_seed = rng.Next();

            return rng;
        }
    }

    class MobBehaviorTree
    {
        private static MobBehaviorTree m_instance;
        private MobBehavior m_root;

        private MobBehaviorTree()
        {
            m_root = new MobRootBehavior();
        }

        public static MobBehaviorTree GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new MobBehaviorTree();
            }

            return m_instance;
        }

        public void Update(
            MobUpdateContext context)
        {
            // Get the current behavior running, if any
            MobAIBehaviorState behaviorState = context.mob.AIState.behavior_data;

            behaviorState.active_behavior_stack = "";
            ActivateBehaviors(context, m_root);

            //currentBehavior.GetType().Name;
        }

        private bool ActivateBehaviors(
            MobUpdateContext context,
            MobBehavior behavior)
        {
            bool foundBehaviorToActivate = false;

            // Can the behavior be activated
            if (behavior.CanActivate(context))
            {
                foundBehaviorToActivate = true;

                // Perform the behavior we can activate
                behavior.Perform(context);

                // Add the behavior to the list of behaviors that was activated
                if (context.mob.AIState.behavior_data.active_behavior_stack.Length > 0)
                {
                    context.mob.AIState.behavior_data.active_behavior_stack += ".";
                }
                context.mob.AIState.behavior_data.active_behavior_stack += behavior.GetType().Name;

                // Try to activate children behaviors
                if (behavior.Children != null)
                {
                    foreach (MobBehavior childBehavior in behavior.Children)
                    {
                        // Stop searching if this child behavior could be activated
                        if (ActivateBehaviors(context, childBehavior))
                        {
                            break;
                        }
                    }
                }
            }

            return foundBehaviorToActivate;
        }
    }
}
