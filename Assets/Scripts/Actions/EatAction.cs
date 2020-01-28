using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace FlatEarth
{
    public class EatAction : Action
    {
        public EatAction(int pPriority)
        {
            base.priority = pPriority;
        }
        
        public override bool CanDoAction(CurrentState state)
        {
            bool canDoIt = state.GetState("isHungry");

            if (state.GetState("isAfraid"))
            {
                canDoIt = false;
            }
            
            return canDoIt;
        }

        public override void Act(Entity entity)
        {
            entity.Eat();
        }
    }
}