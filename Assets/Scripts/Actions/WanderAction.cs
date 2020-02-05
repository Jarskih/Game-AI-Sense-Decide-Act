using UnityEngine;

namespace FlatEarth
{
    public class WanderAction : Action
    {
        public WanderAction(int pPriority)
        {
            base.priority = pPriority;
        }

        public override bool CanDoAction(CurrentState state)
        {
            if (state.GetState("isHungry"))
            {
                return false;
            }

            if (state.GetState("isAfraid"))
            {
                return false;
            }

            return true;
        }

        public override void Act(Entity entity)
        {
            entity.Wander();
        }
    }
}