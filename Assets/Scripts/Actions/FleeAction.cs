using UnityEngine;

namespace FlatEarth
{
    public class FleeAction : Action
    {
        public FleeAction(int pPriority)
        {
            base.priority = pPriority;
        }

        
        public override bool CanDoAction(CurrentState state)
        {
            if (state.GetState("isAfraid"))
            {
                return true;
            }

            return false;
        }

        public override void Act(Entity entity)
        {
            entity.Flee();
        }
    }
}
